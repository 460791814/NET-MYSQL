using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TorrentTool.Tool
{
    public static class Library
    {
        /// <summary>
        /// 判断中日韩
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public static int ISChineseAndEnglist(string title)
        {
            try
            {
                string pattern = @"[\uac00-\ud7ff]+";//判断韩语   
                Match usermatch = Regex.Match(title, pattern, RegexOptions.IgnoreCase);
                if (usermatch.Groups.Count >= 1 && usermatch.Groups[0].Value.Length >= 1)
                    return 3;

                pattern = @"[\u0800-\u4e00]+";//判断日语   
                usermatch = Regex.Match(title, pattern, RegexOptions.IgnoreCase);
                if (usermatch.Groups.Count >= 1 && usermatch.Groups[0].Value.Length >= 1)
                    return 2;

                pattern = @"[\u4e00-\u9fa5]+";//判断汉字
                usermatch = Regex.Match(title, pattern, RegexOptions.IgnoreCase);
                if (usermatch.Groups.Count >= 1 && usermatch.Groups[0].Value.Length >= 1)
                    return 1;

                //判断英文，数字
                byte[] byte_len = System.Text.Encoding.Default.GetBytes(title);
                if (byte_len.Length == title.Length)
                    return 4;

            }
            catch (System.Exception ex)
            {
                WriteLog.PrintLn("方法ISChineseAndEnglist:"+ex.Message);
                return 0;
            }
            return 0;
        }
        /// <summary>
        /// 去掉标题中的网址信息
        /// </summary>
        public static string GetOneGoodString(string title)
        {
            //去掉标题中的网址信息
            string res = title;
            try
            {
                //string pattern = @"\[(.*)([\w-]+://?|(www|bbs)[.])([^\]]*)\]";
                string pattern = @"(\[|\@|\【|\s|\(|\{)(.*)([\w-]+://?|(www|bbs)[.])([^(\]|\@|\】|\)|\})]*)(\]|\@|\】|\)|\})";
                Match usermatch = Regex.Match(title, pattern, RegexOptions.IgnoreCase);
                if (usermatch.Groups.Count > 1)
                {
                    res = res.Replace(usermatch.Groups[0].Value.ToString(), " ");
                    res = res.Trim();
                }
                pattern = @"(\[|\@|\【|\s|\(|\{)(.*)\.(com|edu|gov|mil|net|org|biz|info|name|museum|us|ca|uk|cc|me|cm)([^(\]|\@|\】|\)|\}|\s)]*)(\]|\@|\】|\)|\}|\s)";
                //pattern = @"(\[|\@|\【)(.*)([\w-]+://?|(www|bbs)[.])([^(\]|\@|\】)]*)(\]|\@|\】)";
                usermatch = Regex.Match(res, pattern, RegexOptions.IgnoreCase);
                if (usermatch.Groups.Count > 1)
                {
                    res = res.Replace(usermatch.Groups[0].Value.ToString(), " ");
                    res = res.Trim();
                }
                pattern = @"(www|bbs)(.*)(com|edu|gov|mil|net|org|biz|info|name|museum|us|ca|uk|cc|me|cm)";
                usermatch = Regex.Match(res, pattern, RegexOptions.IgnoreCase);
                if (usermatch.Groups.Count > 1)
                {
                    res = res.Replace(usermatch.Groups[0].Value.ToString(), " ");
                    res = res.Trim();
                }
                if (res.Length <= 5 && res.Length < title.Length)
                {
                    //  int a = 0;
                    res = title;
                }

            }
            catch (System.Exception ex)
            {
                WriteLog.PrintLn("GetOneGoodString:"+ex.Message);
                res = title;
            }
            return res;
        }
        //static string WenDangExt = ".txt .doc .pdf .xls .csv .docx .xlsx .xml";
        //static string MusicExt = ".mp3 .wav  .mod .ra .md  .aac .flac .vqf" ;
        static string DianYingExt = ".flv .rmvb .mkv .avi .mp4 .3gp .rm .vod .mpeg .wmv .ram .asf .mov .dvd .wma .dat .vcd .ts ";
        static string TuPian = ".gif .jpeg .bmp .pcx .png .dxf .tiff .pod .psd .tga";
        /// <summary>
        /// 获取文件类型
        /// </summary>
        /// <param name="TorrentFileInfo"></param>
        /// <returns></returns>
        public static int GetType(IList<TorrentTool.Tool.TorrentFile.TorrentFileInfoClass> TorrentFileInfo)
        {
            int typeId = 0;
            try
            {
              
                if (TorrentFileInfo.Count > 0)
                {
                    for (int i = 0; i < TorrentFileInfo.Count; i++)
                    {
                        string name = TorrentFileInfo[i].Path;
                        if (!string.IsNullOrEmpty(name))
                        {

                            if (name.Contains('.') && name.Contains('�') == false)
                            {
                                name = name.ToLower();
                                string ext = System.IO.Path.GetExtension(name);//文件扩展名

                            
                                if (TuPian.Contains(ext))
                                {
                                    if (typeId < 1)
                                    {
                                        typeId = 1; //图片
                                    }
                                }
                                if (DianYingExt.Contains(ext))
                                {
                                    if (typeId < 2)
                                    {
                                        typeId = 2; //电影
                                    }
                                }

                            }

                        }


                    }
                }
            }
            catch (System.Exception ex)
            {

                WriteLog.PrintLn("方法GetType："+ex.Message);
            }
           
                  
            return typeId;
        }
        private const double KBCount = 1024;
        private const double MBCount = KBCount * 1024;
        private const double GBCount = MBCount * 1024;
        private const double TBCount = GBCount * 1024;

        /// <summary>
        /// 得到适应的大小
        /// </summary>
        /// <param name="path"></param>
        /// <returns>string</returns>
        public static string GetAutoSizeString(double size, int roundCount)
        {
            if (KBCount > size)
            {
                return Math.Round(size, roundCount) + "B";
            }
            else if (MBCount > size)
            {
                return Math.Round(size / KBCount, roundCount) + "KB";
            }
            else if (GBCount > size)
            {
                return Math.Round(size / MBCount, roundCount) + "MB";
            }
            else if (TBCount > size)
            {
                return Math.Round(size / GBCount, roundCount) + "GB";
            }
            else
            {
                return Math.Round(size / TBCount, roundCount) + "TB";
            }
        }
        /// <summary>
        /// 计算文件大小函数(保留两位小数),Size为字节大小
        /// </summary>
        /// <param name="Size">初始文件大小</param>
        /// <returns></returns>
        public static string CountSize(long Size)
        {
            try
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
            catch (Exception e)
            {
                WriteLog.PrintLn("方法CountSize：" + e.Message);
                return "0KB";
            }
        }
    }
}
