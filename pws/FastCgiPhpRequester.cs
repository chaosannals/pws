using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace Pws
{
    /// <summary>
    /// 请求者
    /// </summary>
    public class FastCgiPhpRequester
    {
        public TcpClient Source { get; private set; }

        public FastCgiPhpRequester(TcpClient source)
        {
            Source = source;
        }

        public IEnumerable<int> Transfer(TcpClient target)
        {
            byte[] buffer = new byte[8192];
            NetworkStream requester = Source.GetStream();
            NetworkStream responser = target.GetStream();
            while (true)
            {
                if (requester.DataAvailable)
                {
                    int count = requester.Read(buffer, 0, buffer.Length);
                    responser.Write(buffer, 0, count);
                }
                if (responser.DataAvailable)
                {
                    int count = responser.Read(buffer, 0, buffer.Length);
                    requester.Write(buffer, 0, count);
                }
                yield return 0;
            }
        }
    }
}
