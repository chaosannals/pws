using System;
using System.IO;
using System.Text;

namespace pws
{
    public static class LogExtends
    {
        public static void Log(this string content, string format = null)
        {
            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "service.log"
            );
            using (FileStream stream = File.Open(path, FileMode.OpenOrCreate))
            {
                byte[] data = Encoding.UTF8.GetBytes(
                    DateTime.Now.ToString() + "    " + content
                );
                stream.Write(data, 0, data.Length);
            }
        }
    }
}
