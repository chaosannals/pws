using System;
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
        private PhpCgiServerDispatcher dispatcher;

        /// <summary>
        /// 初始化代理
        /// </summary>
        /// <param name="port"></param>
        public PhpCgiServerProxy(short port=9000)
        {
            IPAddress address = IPAddress.Parse("0.0.0.0");
            listener = new TcpListener(address, port);
            dispatcher = new PhpCgiServerDispatcher();
            thread = null;
            ticker = new System.Timers.Timer();
            ticker.Elapsed += new ElapsedEventHandler((sender, args) =>
            {
                try
                {
                    Work();
                }
                catch (Exception e)
                {
                    e.ToString().Log();
                }
            });
            ticker.Interval = 2000;
        }

        /// <summary>
        /// 启动代理
        /// </summary>
        public void Start()
        {
            listener.Start();
            ticker.Enabled = true; // 会调用一次 Elapsed 委托
            ticker.Start();
        }

        /// <summary>
        /// 停止代理
        /// </summary>
        public void Stop()
        {
            ticker.Stop();
            if (thread != null && thread.IsAlive)
            {
                thread.Abort();
                thread = null;
            }
            listener.Stop();
            dispatcher.Stop();
        }

        public void Work()
        {
            if (thread == null || !thread.IsAlive)
            {
                "启动监听线程".Log();
                thread = new Thread(() =>
                {
                    try
                    {
                        while (true)
                        {
                            TcpClient source = listener.AcceptTcpClient();
                            source.SendTimeout = 300000;
                            source.ReceiveTimeout = 300000;
                            dispatcher.Dispatch(source);
                        }
                    }
                    catch (Exception e)
                    {
                        e.ToString().Log();
                    }
                    finally
                    {
                        "线程关闭".Log();
                        thread = null;
                    }
                });
                thread.Start();
            }
        }
    }
}
