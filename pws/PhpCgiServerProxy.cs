using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;

namespace Pws
{
    /// <summary>
    /// PHP CGI 反向代理
    /// </summary>
    public class PhpCgiServerProxy
    {
        private System.Timers.Timer ticker;
        private volatile Thread thread;
        private PhpCgiProcessDispatcher dispatcher;
        private ManualResetEvent manual;
        public short Port { get; private set; }

        /// <summary>
        /// 初始化代理
        /// </summary>
        /// <param name="port"></param>
        public PhpCgiServerProxy(short port=9000)
        {
            Port = port;
            dispatcher = new PhpCgiProcessDispatcher();
            thread = null;
            manual = new ManualResetEvent(false);
            ticker = new System.Timers.Timer();
            ticker.Elapsed += (sender, args) =>
            {
                try
                {
                    Work();
                }
                catch (Exception e)
                {
                    e.ToString().Log();
                }
            };
            ticker.Interval = 2000;
        }

        /// <summary>
        /// 启动代理
        /// </summary>
        public void Start()
        {
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
                thread = null;
                manual.Set();
            }
            dispatcher.Stop();
        }

        /// <summary>
        /// 代理分发请求
        /// </summary>
        public void Work()
        {
            if (thread == null || !thread.IsAlive)
            {
                "启动监听线程".Log();
                thread = new Thread(() =>
                {
                    IPAddress address = IPAddress.Parse("0.0.0.0");
                    TcpListener listener = new TcpListener(address, Port);
                    try
                    {
                        listener.Start();
                        while (thread != null)
                        {
                            manual.Reset();
                            DateTime start = DateTime.Now;
                            listener.BeginAcceptTcpClient(ar =>
                            {
                                TcpListener owner = ar.AsyncState as TcpListener;
                                TcpClient source = owner.EndAcceptTcpClient(ar);
                                source.SendTimeout = 300000;
                                source.ReceiveTimeout = 300000;
                                PhpCgiProcess worker = dispatcher.Dispatch(source);
                                TimeSpan d = DateTime.Now.Subtract(start);
                                "分发解锁 {0:N} ms".Log(d.TotalMilliseconds);
                                manual.Set();
                                ThreadPool.QueueUserWorkItem(e =>
                                {
                                    worker.Start(source);
                                });
                            }, listener);
                            manual.WaitOne();
                            TimeSpan duration = DateTime.Now.Subtract(start);
                            "分发请求耗时 {0:N} ms".Log(duration.TotalMilliseconds);
                        }
                    }
                    catch (Exception e)
                    {
                        e.ToString().Log();
                    }
                    finally
                    {
                        "线程关闭".Log();
                        listener.Stop();
                        thread = null;
                    }
                });
                thread.Start();
            }
        }
    }
}
