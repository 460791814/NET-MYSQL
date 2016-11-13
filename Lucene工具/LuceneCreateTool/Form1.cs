using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DBUtility;
using Model;
using System.Data.SqlClient;
using System.Xml;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using System.Configuration;
using System.IO;
using Lucene.Net.Analysis.PanGu;
using System.Threading;

namespace LuceneCreateTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            dataCount = GetCount();
            label1.Text = "数据库还剩" + dataCount + "条数据未处理！";
        }
        int handler = 500;
        System.Timers.Timer timer1;  //计时器
        private int TotalCount = 0;
        private void btnStrat_Click(object sender, EventArgs e)
        {
            //ShowLog("程序定时开启成功");
            //timer1 = new System.Timers.Timer();
            //timer1.Interval = 150000;  //设置计时器事件间隔执行时间              
            //timer1.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Elapsed);
            //timer1.Enabled = true;
            //this.btnStrat.Enabled = false;
            Thread t = new Thread(AddToIndex);
            t.IsBackground = true;
            t.Start();
        }
        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            AddToIndex();
            //执行SQL语句或其他操作  

        }

        StringBuilder deleteSql = new StringBuilder();
        int dataCount = 0;
        private void AddToIndex()
        {
            deleteSql.Length = 0;//清空StringBuilder
            try
            {
                if (TotalCount > 300) {
                    this.listBox1.Items.Clear();
                }
             
                List<E_Soft> list = SelectSoft();
                if (list.Count > handler-10)
            {


                
                if (list != null && list.Count > 0)
                {
                    CreateIndexByData(list);

                    //  DeleteData();//删除数据

                }
                AddToIndex();
            }
            else {
                ShowLog("数据库未处理的数据少于"+handler+"条，请稍后再试");
            }
            }
            catch (Exception e)
            {
                System.Timers.Timer t = new System.Timers.Timer(Convert.ToInt32(this.txtTime.Text) * 1000);   //实例化Timer类，设置间隔时间为10000毫秒；   
                t.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Elapsed); //到达时间的时候执行事件；   
                t.AutoReset = false;   //设置是执行一次（false）还是一直执行(true)；   
                t.Enabled = true;     //是否执行System.Timers.Timer.Elapsed事件；   


                this.btnStrat.Enabled = true;
                ShowLog("数据库连接中断，程序将在" + this.txtTime.Text + "秒后自动启动！");
                PrintLn("AddToIndex:"+e.Message);
            }

        }
       
        public void CreateIndexByData(List<E_Soft> list)
        {
           
     
            ShowLog("开始批量添加索引");
            string indexPath = ConfigurationManager.AppSettings["Dir"];//索引文档保存位置          
            FSDirectory directory = FSDirectory.Open(new DirectoryInfo(indexPath), new NativeFSLockFactory());
            //IndexReader:对索引库进行读取的类
            bool isExist = IndexReader.IndexExists(directory); //是否存在索引库文件夹以及索引库特征文件
            if (isExist)
            {
                //如果索引目录被锁定（比如索引过程中程序异常退出或另一进程在操作索引库），则解锁
                //Q:存在问题 如果一个用户正在对索引库写操作 此时是上锁的 而另一个用户过来操作时 将锁解开了 于是产生冲突 --解决方法后续
                if (IndexWriter.IsLocked(directory))
                {
                    IndexWriter.Unlock(directory);
                }
            }
            RAMDirectory rmd = new RAMDirectory();
            // IndexWriter rmdwriter = new IndexWriter(rmd, new PanGuAnalyzer(), true);
            //创建向索引库写操作对象  IndexWriter(索引目录,指定使用盘古分词进行切词,最大写入长度限制)
            //补充:使用IndexWriter打开directory时会自动对索引库文件上锁
            IndexWriter writer = new IndexWriter(rmd, new PanGuAnalyzer(), true, IndexWriter.MaxFieldLength.LIMITED);
         
            try
            {


            //--------------------------------遍历数据源 将数据转换成为文档对象 存入索引库
            foreach (var soft in list)
            {
                TotalCount++;
                 
               // Thread.Sleep(200);
                try
                {

               
                Document document = new Document(); //new一篇文档对象 --一条记录对应索引库中的一个文档

                //向文档中添加字段  Add(字段,值,是否保存字段原始值,是否针对该列创建索引)SoftType
                document.Add(new Field("Hash", soft.Hash.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));//--所有字段的值都将以字符串类型保存 因为索引库只存储字符串类型数据
                document.Add(new Field("Length", soft.Length, Field.Store.YES, Field.Index.NOT_ANALYZED));
                document.Add(new Field("FileCount", soft.FileCount.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                document.Add(new Field("SoftType", soft.SoftType.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                document.Add(new Field("Hit", soft.Hit.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                document.Add(new Field("UpdateTime", soft.UpdateTime.ToString("yyyy-MM-dd"), Field.Store.YES, Field.Index.NOT_ANALYZED));
                //Field.Store:表示是否保存字段原值。指定Field.Store.YES的字段在检索时才能用document.Get取出原值  //Field.Index.NOT_ANALYZED:指定不按照分词后的结果保存--是否按分词后结果保存取决于是否对该列内容进行模糊查询


                document.Add(new Field("Name", soft.Name, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));

                //Field.Index.ANALYZED:指定文章内容按照分词后结果保存 否则无法实现后续的模糊查询 
                //WITH_POSITIONS_OFFSETS:指示不仅保存分割后的词 还保存词之间的距离

                document.Add(new Field("Details", GetSoftInro(soft.Details), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
             
                writer.AddDocument(document); //文档写入索引库
              //  writer.Optimize();
                //deleteSql.Append("DELETE FROM T_Soft_Insert WHERE  Hash='"+soft.Hash+"';");
             
                ShowLog("成功添加索引："+soft.Hash);
                dataCount--; 
                label1.Text = "数据库还剩"+dataCount+"条数据未处理！";


                }
                catch (Exception r)
                {

                    PrintLn("for循环:" + r.Message);
                }
            }
              // 在写入内存之前必须得先关闭不然写入内存的数据为空
            writer.Close();  //会自动解锁
          //  ShowLog("索引添加完毕");
            IndexWriter FSwriter = new IndexWriter(directory, new PanGuAnalyzer(), !isExist, IndexWriter.MaxFieldLength.LIMITED);
       
            Lucene.Net.Store.Directory[] DIR = new Lucene.Net.Store.Directory[] { rmd };
            FSwriter.AddIndexesNoOptimize(DIR);//合并数据
            FSwriter.Close();
            int count = SqlHelper.ExecuteNonQuery("DELETE FROM T_Soft_Insert WHERE  Hash in (SELECT TOP "+handler+"  [Hash]  FROM T_Soft_Insert)");
         
            directory.Close(); //不要忘了Close，否则索引结果搜不到
         
            }
            catch (Exception e1)
            {
                writer.Close();//会自动解锁
                directory.Close(); //不要忘了Close，否则索引结果搜不到
                PrintLn("CreateIndexByData:" + e1.Message);
               
            }
        }

        public void EditIndex(E_Soft eSoft)
        {
            ShowLog("更新索引："+eSoft.Hash);

            Term term = new Term("Hash", eSoft.Hash);
            string indexPath = ConfigurationManager.AppSettings["Dir"];//索引文档保存位置          
            FSDirectory directory = FSDirectory.Open(new DirectoryInfo(indexPath), new NativeFSLockFactory());
            //IndexReader:对索引库进行读取的类
            bool isExist = IndexReader.IndexExists(directory); //是否存在索引库文件夹以及索引库特征文件
            if (isExist)
            {
                //如果索引目录被锁定（比如索引过程中程序异常退出或另一进程在操作索引库），则解锁
                //Q:存在问题 如果一个用户正在对索引库写操作 此时是上锁的 而另一个用户过来操作时 将锁解开了 于是产生冲突 --解决方法后续
                if (IndexWriter.IsLocked(directory))
                {
                    IndexWriter.Unlock(directory);
                }
            }

            //创建向索引库写操作对象  IndexWriter(索引目录,指定使用盘古分词进行切词,最大写入长度限制)
            //补充:使用IndexWriter打开directory时会自动对索引库文件上锁
            IndexWriter writer = new IndexWriter(directory, new PanGuAnalyzer(), !isExist, IndexWriter.MaxFieldLength.UNLIMITED);
            Document document = new Document(); //new一篇文档对象 --一条记录对应索引库中的一个文档

            //向文档中添加字段  Add(字段,值,是否保存字段原始值,是否针对该列创建索引)SoftType
            document.Add(new Field("Hash", eSoft.Hash.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));//--所有字段的值都将以字符串类型保存 因为索引库只存储字符串类型数据
            document.Add(new Field("Length", eSoft.Length, Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field("FileCount", eSoft.FileCount.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field("SoftType", eSoft.SoftType.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field("Hit", eSoft.Hit.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field("UpdateTime", eSoft.UpdateTime.ToString("yyyy-MM-dd"), Field.Store.YES, Field.Index.NOT_ANALYZED));
            //Field.Store:表示是否保存字段原值。指定Field.Store.YES的字段在检索时才能用document.Get取出原值  //Field.Index.NOT_ANALYZED:指定不按照分词后的结果保存--是否按分词后结果保存取决于是否对该列内容进行模糊查询


            document.Add(new Field("Name", eSoft.Name, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));

            //Field.Index.ANALYZED:指定文章内容按照分词后结果保存 否则无法实现后续的模糊查询 
            //WITH_POSITIONS_OFFSETS:指示不仅保存分割后的词 还保存词之间的距离

            document.Add(new Field("Details", GetSoftInro(eSoft.Details), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
            writer.UpdateDocument(term, document);
            writer.Close();
            ShowLog("索引更新完成：" + eSoft.Hash);
        }

        public string GetSoftInro(string str)
        {
            try
            {

      
            List<E_Soft> list = StrToXml(str);
            StringBuilder intro = new StringBuilder();
            if (list.Count >= 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    intro.Append(list[i].Name + "<br/>");
                }
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    intro.Append(list[i].Name + "<br/>");
                }
            }
            return intro.ToString();
            }
            catch (Exception e1)
            {

                PrintLn("GetSoftInro:"+e1.Message);
                return "";
            }
        }

        /// <summary>
        /// 解析XML
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public List<E_Soft> StrToXml(string str)
        {
            // byte[] buff = System.Text.Encoding.Default.GetBytes(str);


            //stream.Write(buff, 0, buff.Length);
            List<E_Soft> list = new List<E_Soft>();
            XmlDocument xdoc = new XmlDocument();

            xdoc.LoadXml(str);

            foreach (XmlElement birthday in xdoc.DocumentElement.ChildNodes)
            {
                E_Soft eSoft = new E_Soft();
                string name = birthday.SelectSingleNode("name").InnerText;
                string length = birthday.SelectSingleNode("length").InnerText;
                eSoft.Name = name + "&nbsp&nbsp&nbsp&nbsp&nbsp" + length;
                list.Add(eSoft);
            }
            return list;
        }

        /// <summary>
        /// 获取资料列表
        /// </summary>
        /// <param name="eSoft"></param>
        /// <returns></returns>
        public List<E_Soft> SelectSoft()
        {
            List<E_Soft> list = new List<E_Soft>();
            string sql = @"SELECT TOP 500 
                                   [Hash]
                                  ,[Name]
                                  ,[Length]
                                  ,[Hit]
                               
                                  ,[FileCount]
                                  ,[SoftType]
                                  ,[Details]
                             
                                
                                  ,[UpdateTime]
                              FROM T_Soft_Insert  ";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(sql))
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        E_Soft _eSoft = new E_Soft();
                     
                        _eSoft.Hash = reader["Hash"].ToString();
                        _eSoft.Name = reader["Name"].ToString();
                        _eSoft.Length = reader["Length"].ToString();
                     
                        _eSoft.Details = reader["Details"].ToString();
                        _eSoft.Hit = Convert.ToInt32(reader["Hit"]);

                        _eSoft.FileCount = Convert.ToInt32(reader["FileCount"]);
                        _eSoft.SoftType = Convert.ToInt32(reader["SoftType"]);
                    
                        _eSoft.UpdateTime = Convert.ToDateTime(reader["UpdateTime"]);
                        list.Add(_eSoft);
                    }

                }
            }
            return list;
        }
        /// <summary>
        /// 获取Update资料列表
        /// </summary>
        /// <param name="eSoft"></param>
        /// <returns></returns>
        public List<E_Soft> SelectUpdateSoft()
        {
            List<E_Soft> list = new List<E_Soft>();
            string sql = @"SELECT TOP 30 
                                   [Hash]
                                  ,[Name]
                                  ,[Length]
                                  ,[Hit]
                               
                                  ,[FileCount]
                                  ,[SoftType]
                                  ,[Details]
                                 
                                
                                  ,[UpdateTime]
                              FROM T_Soft_Update  ";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(sql))
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        E_Soft _eSoft = new E_Soft();

                        _eSoft.Hash = reader["Hash"].ToString();
                        _eSoft.Name = reader["Name"].ToString();
                        _eSoft.Length = reader["Length"].ToString();

                        _eSoft.Details = reader["Details"].ToString();
                        _eSoft.Hit = Convert.ToInt32(reader["Hit"]);

                        _eSoft.FileCount = Convert.ToInt32(reader["FileCount"]);
                        _eSoft.SoftType = Convert.ToInt32(reader["SoftType"]);

                        _eSoft.UpdateTime = Convert.ToDateTime(reader["UpdateTime"]);
                        list.Add(_eSoft);
                    }

                }
            }
            return list;
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="hash"></param>
        public void DeleteData()
        {
            try
            {
               // string sql = "DELETE FROM T_Soft_Insert WHERE  Hash=@Hash";
                if (deleteSql.Length > 0)
                {
                    int count = SqlHelper.ExecuteNonQuery(deleteSql.ToString());
                }
            }
            catch (Exception)
            {

                throw;
            }


        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="hash"></param>
        public void DeleteUpdateData(string hash)
        {
            try
            {
                string sql = "DELETE FROM T_Soft_Update WHERE  Hash=@Hash";
                int count = SqlHelper.ExecuteNonQuery(sql, new SqlParameter("@Hash", hash));
            }
            catch (Exception)
            {

                throw;
            }


        }
        /// <summary>
        /// 获取数量
        /// </summary>
        /// <returns></returns>
        public int GetCount()
        {
            try
            {
                string sql = "select count(*) from T_Soft_Insert";
                object obj = SqlHelper.ExecuteScalar(sql);
                return Convert.ToInt32(obj);
            }
            catch (Exception e)
            {
               
                ShowLog("数据库连接字符串错误，连接数据库失败");
                return 0;
               // throw;
            }
        }
        /// <summary>
        /// 获取数量
        /// </summary>
        /// <returns></returns>
        public int GetUpdateCount()
        {
            try
            {
                string sql = "select count(*) from T_Soft_Update";
                object obj = SqlHelper.ExecuteScalar(sql);
                return Convert.ToInt32(obj);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private const double KBCount = 1024;
        private const double MBCount = KBCount * 1024;
        private const double GBCount = MBCount * 1024;
        private const double TBCount = GBCount * 1024;
        /// <summary>
        /// 计算文件大小函数(保留两位小数),Size为字节大小
        /// </summary>
        /// <param name="Size">初始文件大小</param>
        /// <returns></returns>
        public static string CountSize(long Size)
        {
            string m_strSize = "";
            long FactSize = 0;
            FactSize = Size;
            if (FactSize < KBCount)
                m_strSize = FactSize.ToString("F2") + " Byte";
            else if (FactSize >= KBCount && FactSize < MBCount)
                m_strSize = (FactSize / KBCount).ToString("F2") + " KB";
            else if (FactSize >= MBCount && FactSize < GBCount)
                m_strSize = (FactSize / MBCount).ToString("F2") + " MB";
            else if (FactSize >= GBCount && FactSize < TBCount)
                m_strSize = (FactSize / GBCount).ToString("F2") + " GB";
            else if (FactSize >= TBCount)
                m_strSize = (FactSize / TBCount).ToString("F2") + " TB";
            return m_strSize;
        }

        private void button2_Click(object sender, EventArgs e)
        {

            this.timer1.Enabled = false;
            this.btnStrat.Enabled = true;
            ShowLog("程序结束运行");
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdate_Click(object sender, EventArgs e)
        {

            int dataCount = GetUpdateCount();
           if (dataCount > 30)
           {
 ShowLog("满"+dataCount+"-开始更新索引文件");
                List<E_Soft> list = SelectUpdateSoft();
                if (list != null && list.Count > 0)
                {
                   
                    for (int i = 0; i < list.Count; i++)
                    { 
                        EditIndex(list[i]);
                        DeleteUpdateData(list[i].Hash);//删除数据
                    }
                }
                ShowLog("索引更新完成");
           }
        }
        public void ShowLog(string str)
        {
            this.listBox1.Items.Add(DateTime.Now.ToString() + ":" + str);
        }
        /// <summary>
        /// 保存记录（Log/年-月-日）
        /// </summary>
        /// <param name="sMessage"></param>
        public static void PrintLn(string sMessage)
        {
            try
            {


                DateTime dt = System.DateTime.Now.ToLocalTime();
                sMessage = string.Format("[{0}]: ", dt) + sMessage;
                string sPath = Environment.CurrentDirectory + "/Log/" + DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "/";
                if (System.IO.Directory.Exists(sPath) == false) System.IO.Directory.CreateDirectory(sPath);
                File.AppendAllText(sPath + DateTime.Now.Day.ToString() + ".log", sMessage + "\r\n");
            }
            catch (Exception)
            {


            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }

    }
}
