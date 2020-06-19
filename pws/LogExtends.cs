using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;

namespace Pws
{
    /// <summary>
    /// 日志扩展
    /// </summary>
    public static class LogExtends
    {
        private static volatile bool writing = false;
        private static string folder = null;
        private static Timer ticker = new Timer();
        private static List<string> contents = new List<string>();

        /// <summary>
        /// 初始化写入计时器。
        /// </summary>
        static LogExtends()
        {
            ticker.Elapsed += (sender, args) =>
            {
                try
                {
                    if (!writing && contents.Count > 0)
                    {
                        Write();
                    }
                }
                catch (Exception e)
                {
                    e.ToString().Log();
                }
            };
            ticker.Interval = 2000;
            ticker.Start();
        }

        /// <summary>
        /// 日志文件夹
        /// </summary>
        public static string Folder
        {
            get
            {
                if (folder == null)
                {
                    folder = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "pwslog"
                    );
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                }
                return folder;
            }
        }

        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="content"></param>
        public static void Log(this string content, params object[] args)
        {
            string text = string.Format(
                "[{0:S}] - {1:S}\r\n",
                DateTime.Now.ToString("F"),
                string.Format(content, args)
            );
            lock (contents)
            {
                contents.Add(text);
            }
        }

        /// <summary>
        /// 写入文件。
        /// </summary>
        public static void Write()
        {
            writing = true;
            string date = DateTime.Now.ToString("yyyyMMdd");
            string path = Path.Combine(Folder, string.Format("{0:S}.log", date));

            // 大于 2M 的文件先搬移
            FileInfo info = new FileInfo(path);
            if (info.Exists && info.Length > 2000000)
            {
                string time = DateTime.Now.ToString("HHmmss");
                info.MoveTo(Path.Combine(Folder, string.Format("{0:S}-{1:S}.log", date, time)));
            }

            // 写入日志
            using (FileStream stream = File.Open(path, FileMode.OpenOrCreate | FileMode.Append))
            {
                lock(contents)
                {
                    foreach(string text in contents)
                    {
                        byte[] data = Encoding.UTF8.GetBytes(text);
                        stream.Write(data, 0, data.Length);
                    }
                    contents.Clear();
                }
            }
            writing = false;
        }
    }
}
