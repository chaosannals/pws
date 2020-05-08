using System;
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
        private PhpCgiServer server;

        public PhpCgiServerProxy(short port=9000)
        {
            IPAddress address = IPAddress.Parse("0.0.0.0");
            listener = new TcpListener(address, port);
            thread = null;
            ticker = new System.Timers.Timer();
            ticker.Elapsed += new ElapsedEventHandler((sender, args) =>
            {
                if (server.Process.HasExited)
                {
                    server.Process.Start();
                }
                Work();
            });
            ticker.Interval = 2000;
            ticker.Enabled = true;
            server = new PhpCgiServer(9001);
        }

        public void Start()
        {
            server.Process.Start();
            server.Process.BeginOutputReadLine();
            listener.Start();
            Work();
            ticker.Start();
        }

        public void Stop()
        {
            ticker.Stop();
            thread.Abort();
            listener.Stop();
            server.Process.Kill();
            server.Process.Close();
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
                            server.Serve(source);
                        }
                        catch (Exception e)
                        {
                            e.ToString().Log();
                        }
                    }
                });
            }
        }
    }
}
