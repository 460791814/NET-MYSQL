using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.IO.Compression;
using System.Threading;
using UMULib;

namespace TorrentTool.Tool
{
  public   class BtDown
    {
        private string pathname = string.Empty;
        public bool webgood = true;
        private int downwebpos = 0;
        private string[] m_strURLList = new string[6];
        private int[] m_timeoutList = new int[4];

        #region 下载到内存中直接使用
        public byte[] DownLoadFileByHashToByte(string hashname)
        {
            byte[] res = null;
            try
            {
                
                m_strURLList[0] = string.Format("https://zoink.it/torrent/{0}.torrent", hashname);
               // m_strURLList[1] = string.Format("http://bt.box.n0808.com/{0}/{1}/{2}.torrent", hashname.Substring(0, 2), hashname.Substring(hashname.Length - 2, 2), hashname);
                m_strURLList[1] = string.Format("http://torcache.net/torrent/{0}.torrent", hashname);
                m_strURLList[2] = string.Format("http://torrage.com/torrent/{0}.torrent", hashname);
                m_strURLList[3] = string.Format("http://torcache.net/torrent/{0}.torrent", hashname);
              
                m_strURLList[4] = string.Format("http://torrent-cache.bitcomet.org:36869/get_torrent?info_hash={0}&size=226920869&key="+GetKey(hashname), hashname.ToLower());
                
                m_strURLList[5] = string.Format("http://torrage.us/torrent/{0}.torrent", hashname);
                //随机从前面两个网站中的一个下载,因为前面两个网站速度快些
                Random r = new Random();
                int suiji= r.Next(0,6);
                //res = DownLoadFileToSaveByte(m_strURLList[downwebpos]);

                //随机打乱三个网址顺序下载,防止从一个网站下载过多被封
                res = DownLoadFileToSaveByte(m_strURLList[suiji]);
                if (res == null||res.Length==7)
                {
                    suiji = r.Next(0, 5);
                        res = DownLoadFileToSaveByte(m_strURLList[suiji]);
                        if (res == null || res.Length == 7)
                        {
                         res=DownLoadFileToSaveByte(m_strURLList[4]);

                        }
                }

                return res;
            }
            catch (Exception e)
            {
               WriteLog.PrintLn("方法DownLoadFileByHashToByte:"+e.Message);
                return null;
            }
        }
        private byte[] DownLoadFileToSaveByte(string strURL)
        {
            Int32 ticktime1 = System.Environment.TickCount;
            byte[] result = null;
            try
            {
                Int32 ticktime2 = 0;
                byte[] buffer = new byte[4096];

                WebRequest wr = WebRequest.Create(strURL);
                wr.ContentType = "application/x-bittorrent";
                wr.Timeout = 3000;
                WebResponse response = wr.GetResponse();
                int readsize = 0;
                {
                    bool gzip = response.Headers["Content-Encoding"] == "gzip";
                    Stream responseStream = gzip ? new GZipStream(response.GetResponseStream(), CompressionMode.Decompress) : response.GetResponseStream();

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        //responseStream.ReadTimeout = timeout1*2;
                        int count = 0;
                        do
                        {
                            count = responseStream.Read(buffer, 0, buffer.Length);
                            memoryStream.Write(buffer, 0, count);
                            readsize += count;
                        } while (count != 0);
                        ticktime2 = System.Environment.TickCount;

                        Thread.Sleep(10);
                        result = memoryStream.ToArray();
                    }
                    Int32 ticktime3 = System.Environment.TickCount;
                    //H31Debug.PrintLn("下载成功" + strURL + ":" + readsize.ToString() + ":" + (ticktime2 - ticktime1).ToString() + "-" + (ticktime3 - ticktime2).ToString());
                }
                wr.Abort();
                return result;
            }
            catch (Exception e)
            {
                Int32 ticktime3 = System.Environment.TickCount;
               // WriteLog.PrintLn("方法DownLoadFileToSaveByte:" + e.Message);
                //H31Debug.PrintLn("下载失败" + strURL + ":" +  (ticktime3 - ticktime1).ToString());
                return null;
            }
        }
        #endregion

        private string GetKey(string hash)
        {
            try
            {

        
            UrlGenerator U = new UrlGenerator();
            string key = U.GenBitCometTorrentKey(hash);
            return key;   
            }
            catch (Exception e)
            {
                WriteLog.PrintLn("方法GetKey:"+e.Message);
                return "";
            }
        }
        #region 下载到文件
        public int DownLoadFileByHashToFile(string hashname)
        {
            try
            {
                if (pathname == string.Empty)
                {
                    string localfile = AppDomain.CurrentDomain.BaseDirectory;
                    pathname = Path.Combine(localfile, "Torrent");
                    if (!Directory.Exists(pathname))
                    {
                        Directory.CreateDirectory(pathname);
                        string tmpFolder = Path.Combine(pathname, "BAD");
                        if (!Directory.Exists(tmpFolder))
                        {
                            Directory.CreateDirectory(tmpFolder);
                        }

                    }
                }

                //检测子文件夹是否存在
                string pathname1 = Path.Combine(pathname, hashname.Substring(hashname.Length - 2, 2));
                if (!Directory.Exists(pathname1))
                {
                    Directory.CreateDirectory(pathname1);
                }
                string filename = string.Format("{0}\\{1}\\{2}.torrent", pathname, hashname.Substring(hashname.Length - 2, 2), hashname);
                if (File.Exists(filename))
                    return 1;
                m_strURLList[3] = string.Format("http://torcache.net/torrent/{0}.torrent", hashname);
                m_strURLList[2] = string.Format("https://zoink.it/torrent/{0}.torrent", hashname);
                m_strURLList[1] = string.Format("http://bt.box.n0808.com/{0}/{1}/{2}.torrent", hashname.Substring(0, 2), hashname.Substring(hashname.Length - 2, 2), hashname);
                m_strURLList[0] = string.Format("http://torrage.com/torrent/{0}.torrent", hashname);
                m_timeoutList[0] = 500;
                m_timeoutList[1] = 500;
                m_timeoutList[2] = 1000;
                m_timeoutList[3] = 1000;

                //随机从一个网址下载
                //downwebpos = (downwebpos + 1) % 2;
                //if (DownLoadFileToSaveFile(m_strURLList[downwebpos], filename) == 1)
                //    return 1;

                //随机打乱三个网址顺序下载,防止从一个网站下载过多被封
                downwebpos = (downwebpos + 1);
                //从三种网址一一测试下载
                if (DownLoadFileToSaveFile(m_strURLList[(downwebpos) % 4], filename, m_timeoutList[(downwebpos) % 4]) == 1)
                    return 1;
                if (DownLoadFileToSaveFile(m_strURLList[(downwebpos + 1) % 4], filename, m_timeoutList[(downwebpos + 1) % 4]) == 1)
                    return 1;

                if (DownLoadFileToSaveFile(m_strURLList[(downwebpos + 2) % 4], filename, m_timeoutList[(downwebpos + 2) % 4]) == 1)
                    return 1;
                if (DownLoadFileToSaveFile(m_strURLList[(downwebpos + 3) % 4], filename, m_timeoutList[(downwebpos + 3) % 4]) == 1)
                    return 1;

                return 0;
            }
            catch (Exception e)
            {
                WriteLog.PrintLn(e.Message);
                return -2;
            }
        }
        private int DownLoadFileToSaveFile(string strURL, string fileName, int timeout1)
        {
            Int32 ticktime1 = System.Environment.TickCount;
            try
            {
                Int32 ticktime2 = 0;
                byte[] buffer = new byte[4096];

                WebRequest wr = WebRequest.Create(strURL);
                wr.ContentType = "application/x-bittorrent";
                wr.Timeout = 5000;
                WebResponse response = wr.GetResponse();
                int readsize = 0;
                {
                    bool gzip = response.Headers["Content-Encoding"] == "gzip";
                    Stream responseStream = gzip ? new GZipStream(response.GetResponseStream(), CompressionMode.Decompress) : response.GetResponseStream();

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        int count = 0;
                        do
                        {
                            count = responseStream.Read(buffer, 0, buffer.Length);
                            memoryStream.Write(buffer, 0, count);
                            readsize += count;
                        } while (count != 0);
                        ticktime2 = System.Environment.TickCount;

                        byte[] result = memoryStream.ToArray();
                        Thread.Sleep(10);
                        using (BinaryWriter writer = new BinaryWriter(new FileStream(fileName, FileMode.Create)))
                        {
                            writer.Write(result);
                        }
                    }
                    Int32 ticktime3 = System.Environment.TickCount;
                    //H31Debug.PrintLn("下载成功" + strURL + ":" + readsize.ToString() + ":" + (ticktime2 - ticktime1).ToString() + "-" + (ticktime3 - ticktime2).ToString());
                }
                return 1;
            }
            catch (WebException e)
            {
                Int32 ticktime3 = System.Environment.TickCount;
                if (e.Status == WebExceptionStatus.Timeout)//文件超时
                {
                    return -2;
                }
                else if (e.Status == WebExceptionStatus.ProtocolError)//文件不存在
                {
                    return -3;
                }
                else
                {
                    WriteLog.PrintLn("下载失败" + strURL + ":" + (ticktime3 - ticktime1).ToString() + e.Status.ToString() + e.Message);
                    return -4;
                }
            }
        }
        #endregion
    }
}
