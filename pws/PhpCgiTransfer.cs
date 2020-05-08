using System;
using System.Net.Sockets;

namespace pws
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
            try
            {
                while (true)
                {
                    // 转发请求信息。
                    if (requester.DataAvailable)
                    {
                        int count = requester.Read(buffer, 0, buffer.Length);
                        responser.Write(buffer, 0, count);
                    }

                    // 接收响应内容。
                    if (responser.DataAvailable)
                    {
                        int count = responser.Read(buffer, 0, buffer.Length);
                        requester.Write(buffer, 0, count);
                        // 一旦开始接收就直到结束为止。
                        while (count > 0)
                        {
                            count = responser.Read(buffer, 0, buffer.Length);
                            requester.Write(buffer, 0, count);
                        }
                        break; // 响应内容接收完毕，退出。
                    }
                }
            }
            finally
            {
                // 结束时回收资源。
                requester.Dispose();
                responser.Dispose();
            }
        }
    }
}
