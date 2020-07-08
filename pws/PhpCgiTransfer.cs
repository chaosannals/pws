using System;
using System.Net.Sockets;

namespace Pws
{
    /// <summary>
    /// 传输
    /// </summary>
    public class PhpCgiTransfer
    {
        public TcpClient Source { get; set; }
        public TcpClient Target { get; set; }

        /// <summary>
        /// 传输
        /// </summary>
        public void Transfer()
        {
            NetworkStream requester = Source.GetStream();
            NetworkStream responser = Target.GetStream();
            byte[] buffer = new byte[8192];
            DateTime begin = DateTime.Now;
            FastCgiMessager fcm = new FastCgiMessager();
            try
            {
                while (true)
                {
                    // 转发请求信息。
                    if (requester.DataAvailable)
                    {
                        DateTime start = DateTime.Now;
                        int count = requester.Read(buffer, 0, buffer.Length);
                        responser.Write(buffer, 0, count);
                        fcm.Gain(buffer, count);
                        while (true)
                        {
                            FastCgiMessage m = fcm.Pop();
                            if (m == null) break;
                            m.ToString().Log();
                        }
                        TimeSpan d = DateTime.Now.Subtract(start);
                    }

                    // 接收响应内容。
                    if (responser.DataAvailable)
                    {
                        DateTime start = DateTime.Now;
                        int count = responser.Read(buffer, 0, buffer.Length);
                        requester.Write(buffer, 0, count);
                        // 一旦开始接收就直到结束为止。
                        while (count > 0)
                        {
                            count = responser.Read(buffer, 0, buffer.Length);
                            requester.Write(buffer, 0, count);
                        }
                        TimeSpan d = DateTime.Now.Subtract(start);
                        break; // 响应内容接收完毕，退出。
                    }
                }
            }
            finally
            {
                TimeSpan d = DateTime.Now.Subtract(begin);
                "转发耗时 {0:N} ms".Log(d.TotalMilliseconds);
                // 结束时回收资源。
                requester.Dispose();
                responser.Dispose();
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            Target.Close();
            Source.Close();
        }
    }
}
