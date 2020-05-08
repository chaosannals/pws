using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;

namespace pws
{
    /// <summary>
    /// PHP CGI 反向代理
    /// </summary>
    public class PhpCgiServerProxy
    {
        private System.Timers.Timer ticker;
        private Thread thread;
        private TcpListener listener;
        private PhpCgiServer[] servers;

        /// <summary>
        /// 初始化代理
        /// </summary>
        /// <param name="port"></param>
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
                if (!listener.Server.IsBound)
                {
                    listener.Start();
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

        /// <summary>
        /// 启动代理
        /// </summary>
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

        /// <summary>
        /// 停止代理
        /// </summary>
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
                    try
                    {
                        while (true)
                        {
                            TcpClient source = listener.AcceptTcpClient();
                            source.SendTimeout = 300000;
                            source.ReceiveTimeout = 300000;
                            "开始请求".Log();
                            // 根据 Counter 分发请求
                            Array.Sort(servers, (a, b) =>
                            {
                                return a.Counter - b.Counter;
                            });
                            PhpCgiServer worker = servers[0];
                            worker.Serve(source);
                            "转发到 {0:D} 端口".Log(worker.Port);
                        }
                    }
                    catch (Exception e)
                    {
                        e.ToString().Log();
                    }
                    finally
                    {
                        listener.Stop();
                    }
                });
                thread.Start();
            }
        }
    }
}
