using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace pws
{
    public class PhpCgiServerDispatcher
    {
        private PhpCgiServer[] servers;
        public int NowCount { get { return servers.Sum(i => i.HasExited ? 0 : 1); } }
        public int MaxCount { get; private set; }

        public PhpCgiServerDispatcher(int max = 10)
        {
            MaxCount = max;
            servers = new PhpCgiServer[max];
            for (int i = 0; i < max; ++i)
            {
                servers[i] = new PhpCgiServer((short)(9001 + i));
                servers[i].Process.Start();
                servers[i].Process.BeginOutputReadLine();
            }
        }

        public void Dispatch(TcpClient source)
        {
            "开始分发请求".Log();
            // 根据 Counter 分发请求
            Array.Sort(servers, (a, b) =>
            {
                return a.Counter - b.Counter;
            });
            PhpCgiServer worker = servers[0];
            worker.Serve(source);
            "转发到 {0:D} 端口".Log(worker.Port);
        }


        /// <summary>
        /// 停止所有服务。
        /// </summary>
        public void Stop()
        {
            foreach (PhpCgiServer server in servers)
            {
                server.Stop();
            };
        }
    }
}
