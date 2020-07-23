using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Pws
{
    public class FastCgiPhpProxy
    {
        public volatile bool able;
        public int Port { get; private set; }
        public Thread Ticker { get; private set; }
        public Thread Worker { get; private set; }
        public FastCgiPhpDispatcher Dispatcher { get; private set; }

        public FastCgiPhpProxy(int port = 9000)
        {
            able = true;
            Port = port;
            Ticker = new Thread(() => {
                if (able && (Worker == null || !Worker.IsAlive)) Tick();
                Thread.Sleep(2000);
            });
            Dispatcher = new FastCgiPhpDispatcher();
        }

        public void Tick()
        {
            Worker = new Thread(() =>
            {
                IPAddress address = IPAddress.Parse("0.0.0.0");
                TcpListener listener = new TcpListener(address, Port);
                ManualResetEvent manual = new ManualResetEvent(false);
                try
                {
                    listener.Start();
                    while (able)
                    {
                        manual.Reset();
                        DateTime start = DateTime.Now;
                        listener.BeginAcceptTcpClient(ar =>
                        {
                            TcpListener owner = ar.AsyncState as TcpListener;
                            TcpClient source = owner.EndAcceptTcpClient(ar);
                            source.SendTimeout = 300000;
                            source.ReceiveTimeout = 300000;

                            // 分发请求
                            var worker = Dispatcher.Dispatch();
                            TimeSpan d = DateTime.Now.Subtract(start);
                            "分发解锁 {0:N} ms".Log(d.TotalMilliseconds);
                            manual.Set();
                            worker.Respond(source);
                        }, listener);
                        manual.WaitOne();
                        TimeSpan duration = DateTime.Now.Subtract(start);
                        "分发请求间隔 {0:N} ms".Log(duration.TotalMilliseconds);
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
                    able = false;
                    Worker = null;
                }
            });
        }

        /// <summary>
        /// 开始代理
        /// </summary>
        public void Start()
        {
            Ticker.Start();
        }

        /// <summary>
        /// 停止代理
        /// </summary>
        public void Stop()
        {
            able = false;
            Ticker.Abort();
            Worker.Abort();
            Dispatcher.Stop();
        }
    }
}
