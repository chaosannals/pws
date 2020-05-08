using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;

namespace pws
{
    /// <summary>
    /// 
    /// </summary>
    public class PhpCgiServerProxy
    {
        private System.Timers.Timer ticker;
        private Thread thread;
        private TcpListener listener;
        private PhpCgiServer[] servers;

        public PhpCgiServerProxy(short port=9000)
        {
            IPAddress address = IPAddress.Parse("0.0.0.0");
            listener = new TcpListener(address, port);
            thread = null;
            ticker = new System.Timers.Timer();
            ticker.Elapsed += new ElapsedEventHandler((sender, args) =>
            {
                foreach (PhpCgiServer server in servers)
                {
                    if (server.Process.HasExited)
                    {
                        server.Process.Start();
                    }
                }
                Work();
            });
            ticker.Interval = 2000;
            ticker.Enabled = true;
            servers = new PhpCgiServer[6];
            for (short i = 0; i < servers.Length; ++i)
            {
                servers[i] = new PhpCgiServer((short)(i + 9001));
            }
        }

        public void Start()
        {
            foreach (PhpCgiServer server in servers)
            {
                server.Process.Start();
                server.Process.BeginOutputReadLine();
            }
            listener.Start();
            Work();
            ticker.Start();
        }

        public void Stop()
        {
            ticker.Stop();
            thread.Abort();
            listener.Stop();
            foreach (PhpCgiServer server in servers)
            {
                server.Process.Kill();
                server.Process.Close();
            }
        }

        public void Work()
        {
            if (thread == null || !thread.IsAlive)
            {
                thread = new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            TcpClient source = listener.AcceptTcpClient();
                            source.SendTimeout = 300000;
                            source.ReceiveTimeout = 300000;
                            "开始请求".Log();
                            Array.Sort(servers, (a, b) =>
                            {
                                return a.Counter - b.Counter;
                            });
                            PhpCgiServer worker = servers[0];
                            worker.Serve(source);
                            string.Format("转发到 {0:D} 端口", worker.Port).Log();
                        }
                        catch (Exception e)
                        {
                            e.ToString().Log();
                        }
                    }
                });
                thread.Start();
            }
        }
    }
}
