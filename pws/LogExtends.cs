using System;
using System.IO;
using System.Text;

namespace pws
{
    /// <summary>
    /// 日志扩展
    /// </summary>
    public static class LogExtends
    {
        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="content"></param>
        public static void Log(this string content)
        {
            // 日志写入目录
            string folder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "pwslog"
            );
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            // 日志路径
            string path = Path.Combine(
                folder,
                string.Format(
                    "{0:S}.log",
                    DateTime.Now.ToString("yyyyMMdd")
                )
            );

            

            // 写入日志
            using (FileStream stream = File.Open(path, FileMode.OpenOrCreate | FileMode.Append))
            {
                string text = string.Format(
                    "[{0:S}]\r\n{1:S}\r\n",
                    DateTime.Now.ToString("F"),
                    content
                );
                byte[] data = Encoding.UTF8.GetBytes(text);
                stream.Write(data, 0, data.Length);
            }
        }
    }
}
