using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using TorrentTool.Tool;
using System.Data.SqlClient;
using Model;
using Tool;
using System.Net;
using System.Text.RegularExpressions;
using System.Configuration;
using MySql.Data.MySqlClient;
using Maticsoft.DBUtility;

namespace TorrentTool
{
    public partial class FormBT : Form
    {
        public FormBT()
        {
            InitializeComponent();
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                if (this.folderBrowserDialog1.SelectedPath.Trim() != "")
                    this.filePath.Text = this.folderBrowserDialog1.SelectedPath.Trim();
            }

        }
      
        List<string> list = new List<string>();
        int jiaXCSum = 0;
        int totalCount = 0;
        Thread[] threads = new Thread[50];//最大的线程数量
        DataTable dt = GetTableSchema();
        private void btnClick_Click(object sender, EventArgs e)
        {

            Start();

        }

        public void Start()
        {
            try
            {
                this.btnClick.Enabled = false;

                string path = ConfigurationManager.AppSettings["path"];

                DirectoryInfo theFolder = new DirectoryInfo(path);
                DirectoryInfo[] dirInfo = theFolder.GetDirectories();
                int sum = 0;


                FileInfo[] fileInfo = theFolder.GetFiles();
                foreach (FileInfo NextFile in fileInfo)  //遍历文件
                {
                    sum++;
                    threads[sum] = new Thread(new ParameterizedThreadStart(ThearStrat));
                    threads[sum].IsBackground = true;
                    threads[sum].Start(path + "\\" + NextFile.Name);
                }
                ShowLog("开启线程数：" + sum+"--处理完毕的文件将会自动删除");

            }
            catch (Exception e33)
            {
                System.Timers.Timer t = new System.Timers.Timer(Convert.ToInt32( this.txtTime.Text)*1000);   //实例化Timer类，设置间隔时间为10000毫秒；   
                t.Elapsed += new System.Timers.ElapsedEventHandler(btnClick_Click); //到达时间的时候执行事件；   
                t.AutoReset = false;   //设置是执行一次（false）还是一直执行(true)；   
                t.Enabled = true;     //是否执行System.Timers.Timer.Elapsed事件；   


                this.btnClick.Enabled = true;
                ShowLog("数据库连接中断，程序将在" + this.txtTime.Text + "秒后自动启动！");
                WriteLog.PrintLn("btnClick_Click:" + e33.Message);
            }

        }

        private static BtDown bt = null;
        private static object _lock = new object();
        public static BtDown GetBT()
        {

            if (bt == null)
            {
                lock (_lock)
                {
                    if (bt == null)
                    {
                        bt = new BtDown();
                    }
                }
            }
            return bt;
        }
        public void ThearStrat(object obj)
        {
            string filePath = obj.ToString();
            FileStream fs = new FileStream(filePath, FileMode.Open);

            StreamReader m_streamReader = new StreamReader(fs);
            try
            {





                m_streamReader.BaseStream.Seek(0, SeekOrigin.Begin);

                string strLine = m_streamReader.ReadLine();
                do
                {


                    if (strLine.Length > 40)
                    {
                        strLine = strLine.Replace("\0Hash[", "");
                        strLine = strLine.Substring(0, 40);
                        strLine = strLine.Replace(" ", "");
                    }
                    if (totalCount > 100)
                    {
                        //清空日记
                        this.listLog.Items.Clear();
                        totalCount = 0;
                    }
                    if (strLine.Length == 40)
                    {
                        ReadAndWrite(strLine);
                    }
                    //if (dt != null && dt.Rows.Count > 3) {
                    //    TableValuedToDB(dt);
                    //    dt.Rows.Clear();
                    //}


                    strLine = m_streamReader.ReadLine();


                } while (strLine != null);
                m_streamReader.Close();
                m_streamReader.Dispose();
                fs.Close();
                fs.Dispose();
                DeleteFile(filePath);
                //收尾
                //if (dt != null && dt.Rows.Count > 0)
                //{
                //    TableValuedToDB(dt);
                //    dt.Rows.Clear();

                //}
                ShowLog("处理完毕，线程结束！");
                this.btnClick.Enabled = true;
            }
            catch (Exception e)
            {
                m_streamReader.Close();
                m_streamReader.Dispose();
                fs.Close();
                fs.Dispose();
                WriteLog.PrintLn("方法ThearStrat：" + e.Message);

            }

        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path"></param>
        public void DeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception e)
            {
                WriteLog.PrintLn("方法DeleteFile：" + e.Message);

            }
        }
        /// <summary>
        /// 根据hash去网上搜索种子并下载，解析插入数据库
        /// </summary>
        /// <param name="strHash"></param>
        public void ReadAndWrite(string strHash)
        {

            try
            {
                if (QuChong(strHash))
                {
                    totalCount++;
                    ShowLog("第" + totalCount + "条HASH：" + strHash + "正在处理");
                    byte[] byteTorrent = GetBT().DownLoadFileByHashToByte(strHash);
                    if (byteTorrent != null)
                    {
                        TorrentFile torrent = new TorrentFile(byteTorrent);

                        StringBuilder infoListStr = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                        infoListStr.Append("<a>");
                        long torrentTotalLenth = 0;

                        if (torrent.TorrentFileInfo != null)
                        {
                            for (int i = 0; i < torrent.TorrentFileInfo.Count; i++)
                            {
                                if (torrent.TorrentFileInfo[i].Path != "" && torrent.TorrentFileInfo[i].Path.Contains('�') == false && 0 < torrent.TorrentFileInfo[i].Length)
                                {
                                    infoListStr.Append("<info>");
                                    infoListStr.Append("<name>");
                                    infoListStr.Append("<![CDATA[" + StrSub(torrent.TorrentFileInfo[i].Path, 450) + "]]>");
                                    infoListStr.Append("</name>");
                                    infoListStr.Append("<length>");
                                    infoListStr.Append(Library.CountSize(torrent.TorrentFileInfo[i].Length));
                                    infoListStr.Append("</length>");
                                    infoListStr.Append("</info>");

                                    torrentTotalLenth += torrent.TorrentFileInfo[i].Length;
                                }
                            }
                            infoListStr.Append("</a>");




                            int softType = Library.GetType(torrent.TorrentFileInfo);
                            if (torrent.TorrentName != "" && torrent.TorrentName.Contains('�') == false && 0 < torrentTotalLenth)
                            {

//                                string sql = @"INSERT INTO T_Soft
//                                       (
//                                       [Hash]
//                                       ,[Name]
//                                       ,[Length]
//                                       ,[Hit]
//                                       ,[MonthHit]
//                                       ,[FileCount]
//                                       ,[SoftType]
//                                      ,[Details]
//                                       ,[Area]
//                                       ,[Publisher]
//                                  
//                                       ,[UpdateTime])
//                                 VALUES
//                                       (
//                                        @Hash
//                                       ,@Name
//                                       ,@Length
//                                       ,@Hit
//                                       ,@MonthHit
//                                       ,@FileCount
//                                       ,@SoftType
//                                       ,@Details
//                                       ,@Area
//                                       ,@Publisher
//                                
//                                       ,@UpdateTime)";



//                                int yuYanType = Library.ISChineseAndEnglist(torrent.TorrentName);
//                                SqlParameter[] para = new SqlParameter[]{
//                                       new  SqlParameter("@Hash",strHash)
//                                       ,new  SqlParameter("@Name",StrSub(torrent.TorrentName,450))
//                                       ,new  SqlParameter("@Length",Library.CountSize(torrentTotalLenth))
//                                       ,new  SqlParameter("@Hit",1)
//                                       ,new  SqlParameter("@MonthHit",1)
//                                       ,new  SqlParameter("@FileCount",torrent.TorrentFileInfo.Count)
//                                       ,new  SqlParameter("@SoftType",softType)
//                                       ,new  SqlParameter("@Details",infoListStr.ToString())
//                                       ,new  SqlParameter("@Area",yuYanType)//Library.ISChineseAndEnglist(torrent.TorrentName))
//                                       ,new  SqlParameter("@Publisher",StrSub(torrent.TorrentPublisher,98))
                                 
//                                       ,new  SqlParameter("@UpdateTime",DateTime.Now.ToString())
//                               };

                              //  int count = SqlHelper.ExecuteNonQuery(sql, para);
                                StringBuilder strSql = new StringBuilder();
                                strSql.Append("insert into t_soft(");
                                strSql.Append("Hash,Name,Length,Details,SoftType,FileCount,Hit,Area,Publisher,UpdateTime)");
                                strSql.Append(" values (");
                                strSql.Append("@Hash,@Name,@Length,@Details,@SoftType,@FileCount,@Hit,@Area,@Publisher,@UpdateTime)");
                                MySqlParameter[] parameters = {
					new MySqlParameter("@Hash", MySqlDbType.VarChar,50),
					new MySqlParameter("@Name", MySqlDbType.VarChar,450),
					new MySqlParameter("@Length", MySqlDbType.VarChar,100),
					new MySqlParameter("@Details", MySqlDbType.LongText),
					new MySqlParameter("@SoftType", MySqlDbType.Int32,11),
					new MySqlParameter("@FileCount", MySqlDbType.Int32,11),
					new MySqlParameter("@Hit", MySqlDbType.Int32,11),
					new MySqlParameter("@Area", MySqlDbType.Int32,11),
					new MySqlParameter("@Publisher", MySqlDbType.VarChar,100),
					new MySqlParameter("@UpdateTime", MySqlDbType.DateTime)};
                                parameters[0].Value = strHash;
                                parameters[1].Value = StrSub(torrent.TorrentName, 450);
                                parameters[2].Value = Library.CountSize(torrentTotalLenth);
                                parameters[3].Value = infoListStr.ToString();
                                parameters[4].Value = softType;
                                parameters[5].Value = torrent.TorrentFileInfo.Count;
                                parameters[6].Value = 1;
                                int yuYanType = Library.ISChineseAndEnglist(torrent.TorrentName);
                                parameters[7].Value = yuYanType;
                                parameters[8].Value = StrSub(torrent.TorrentPublisher, 98);
                                parameters[9].Value = DateTime.Now.ToString();

                                int rows = DbHelperMySQL.ExecuteSql(strSql.ToString(), parameters);

                                ShowLog("hash：" + strHash + "--处理成功");


                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {

                WriteLog.PrintLn("方法ReadAndWrite：hash:" + strHash + "Meg:" + e.Message);

            }
        }
        public bool QuChong(string strHash)
        {
            try
            {

                //string sql = "SELECT 1 FROM T_Soft WHERE Hash=@Hash";
                //int count = Convert.ToInt32(SqlHelper.ExecuteScalar(sql, new SqlParameter("@Hash", strHash)));
                //if (count > 0)
                //{
                //    return false;
                //}
                //else
                //{
                //    return true;
                //}
                StringBuilder strSql = new StringBuilder();
                strSql.Append("select count(1) from t_soft");
                strSql.Append(" where Hash=@Hash");
                MySqlParameter[] parameters = {
					new MySqlParameter("@Hash", MySqlDbType.VarChar)
		     	};
                parameters[0].Value = strHash;

                return !DbHelperMySQL.Exists(strSql.ToString(), parameters);
            }
            catch (Exception e)
            {
                WriteLog.PrintLn("方法QuChong：" + e.Message);
                return false;
            }

        }
        /// <summary>
        /// 获取数据库关键词TOP1
        /// </summary>
        /// <returns></returns>
        public E_KeyWord selectKeyWord()
        {
            try
            {
                E_KeyWord ekey = new E_KeyWord();
                string sql = "SELECT TOP 1 [ID],[KeyWord] FROM T_KeyWord where IsSearch=0 ";

                using (SqlDataReader reader = SqlHelper.ExecuteDataReader(sql))
                {

                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            ekey.ID = Convert.ToInt32(reader["ID"]);
                            ekey.KeyWord = reader["KeyWord"].ToString();
                        }
                    }
                }
                return ekey;
            }
            catch (Exception)
            {

                return null;
            }

        }
        /// <summary>
        /// 更新搜索状态
        /// </summary>
        /// <param name="eKey"></param>
        /// <returns></returns>
        public bool UpdateIsSearchForKeyWord(int id)
        {
            string sql = @"update dbo.T_KeyWord set IsSearch=1 where ID=@ID";
            SqlParameter[] para = new SqlParameter[]{
           new SqlParameter("@ID",id),
         
           };
            int count = SqlHelper.ExecuteNonQuery(sql, para.ToArray());

            if (count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        int CurrentPage = 0;
        /// <summary>
        /// 递归抓取写入数据库
        /// </summary>
        /// <param name="KeyWord"></param>
        /// 
        public void WriteToFile(string KeyWord, int id)
        {
            try
            {

                CurrentPage++;
                string str = ToolHtml.GetHtml("http://bt.shousibaocai.com/?s=" + KeyWord + "&p=" + CurrentPage, new CookieContainer());
                string zz = "<li><p class=\"m-title\"><a href=\"/hash/([\\s\\S]*?)\" target=\"_blank\">([\\s\\S]*?)</a></p><div class=\"m-files\"><ul class=\"m-files-ul\">([\\s\\S]*?)</ul></div><p class=\"m-meta\">([\\s\\S]*?)</p></li>";


                str = str.Replace("\n", "");
                Regex r = new Regex(zz);
                if (r.IsMatch(str))
                {
                    var ec = r.Matches(str);
                    foreach (Match item in ec)
                    {

                        string Hash = item.Groups[1].Value;
                        ReadAndWrite(Hash);

                    }
                    UpdateIsSearchForKeyWord(id);
                    WriteToFile(KeyWord, id);
                }
                CurrentPage = 0;
            }
            catch (Exception e)
            {

                ShowLog(e.ToString());
            }
        }
        public string StrSub(string str, int i)
        {
            str = str.Replace("�", "");
            if (str.Length > i)
            {
                return str.Substring(0, i);
            }
            else
            {
                return str;
            }
        }
        /// <summary>
        /// 危险字符串过滤
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string ValidateStr(string str)
        {

            str = str.Replace("&", "&amp");
            str = str.Replace("<", "&it");
            str = str.Replace(">", "&gt");
            str = str.Replace("'", "&apos");
            str = str.Replace("\"", "&quot");


            return str;
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            this.btnClick.Enabled = true;
            try
            {
               

                for (int i = 0; i < threads.Length; i++)
                {
                    threads[i].Abort();
                    threads[i].Join();
                    ShowLog("线程" + i + "已经终止！");
                }
                //关闭线程
              
            }
            catch (Exception e1)
            {

                ShowLog(e1.ToString());
            }
        }



        public void ShowLog(string str)
        {
            this.listLog.Items.Add(DateTime.Now.ToString() + ":" + str);
        }

        private void FormBT_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }



        #region 大数据处理方案
        public static void TableValuedToDB(DataTable dt)
        {
            SqlConnection sqlConn = new SqlConnection(
             ConfigurationManager.ConnectionStrings["sql"].ConnectionString);
            const string TSqlStatement =
             @"insert into [dbo].[T_Soft]([Hash], Name, [Length], Hit, MonthHit, FileCount, SoftType, Details, Area, Publisher, UpdateTime )

select bbb.[Hash], bbb.Name, bbb.[Length], bbb.Hit, bbb.MonthHit, bbb.FileCount, bbb.SoftType, bbb.Details, bbb.Area, bbb.Publisher, bbb.UpdateTime from @TestSoftBt as bbb";
            // where  (select count(1) from [dbo].[T_Soft] as T where  T.[Hash]=bbb.[Hash])=0";
            SqlCommand cmd = new SqlCommand(TSqlStatement, sqlConn);
            SqlParameter catParam = cmd.Parameters.AddWithValue("@TestSoftBt", dt);
            catParam.SqlDbType = SqlDbType.Structured;
            //表值参数的名字叫BulkUdt，在上面的建立测试环境的SQL中有。
            catParam.TypeName = "dbo.SoftBt";
            try
            {
                sqlConn.Open();
                if (dt != null && dt.Rows.Count != 0)
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {

                WriteLog.PrintLn("方法TableValuedToDB：" + ex.Message);
            }
            finally
            {
                sqlConn.Close();
            }
        }
        public static DataTable GetTableSchema()
        {
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[]{
                //[Hash], Name, [Length], Hit, MonthHit, FileCount, SoftType, Details, Area, Publisher, UpdateTime
      new DataColumn("Hash",typeof(string)),
      new DataColumn("Name",typeof(string)),
      new DataColumn("Length",typeof(string)),
      new DataColumn("Hit",typeof(int)),
      new DataColumn("MonthHit",typeof(int)),
      new DataColumn("FileCount",typeof(int)),
      new DataColumn("SoftType",typeof(int)),
      new DataColumn("Details",typeof(string)),
      new DataColumn("Area",typeof(int)),
      new DataColumn("Publisher",typeof(string)),
      new DataColumn("UpdateTime",typeof(string))
            });

            return dt;
        }


        #endregion

    }
}
