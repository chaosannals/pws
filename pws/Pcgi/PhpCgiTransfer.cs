using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Pws.Fcgi;

namespace Pws.Pcgi
{
    public class PhpCgiTransfer
    {
        public TcpClient Source { get; private set; }

        public PhpCgiTransfer(TcpClient source)
        {
            Source = source;
        }

        public IEnumerable<FastCgiMessage> Transfer(TcpClient target)
        {
            byte[] buffer = new byte[8192];
            NetworkStream requester = Source.GetStream();
            NetworkStream responser = target.GetStream();
            FastCgiParser sfcp = new FastCgiParser();
            FastCgiParser tfcp = new FastCgiParser();
            while (true)
            {
                // 请求数据传递
                if (requester.DataAvailable)
                {
                    int count = requester.Read(buffer, 0, buffer.Length);
                    responser.Write(buffer, 0, count);
                    sfcp.Gain(buffer, count);
                    while (true)
                    {
                        FastCgiMessage m = sfcp.Pop();
                        if (m == null) break;
                        yield return m;
                    }
                }

                // 响应数据传递
                if (responser.DataAvailable)
                {
                    int count = responser.Read(buffer, 0, buffer.Length);
                    requester.Write(buffer, 0, count);
                    tfcp.Gain(buffer, count);
                    while (true)
                    {
                        FastCgiMessage m = tfcp.Pop();
                        if (m == null) break;
                        yield return m;
                    }
                }
                yield return null;
            }
        }
    }
}
