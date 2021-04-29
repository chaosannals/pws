﻿using System;
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
        public TcpListener Listener { get; private set; }
        private PhpCgiProcessDispatcher dispatcher;
        public short Port { get; private set; }

        /// <summary>
        /// 初始化代理
        /// </summary>
        /// <param name="port"></param>
        public PhpCgiServerProxy(short port=9000)
        {
            Port = port;
            Listener = new TcpListener(IPAddress.Any, Port);
            dispatcher = new PhpCgiProcessDispatcher();
        }

        /// <summary>
        /// 启动代理
        /// </summary>
        public void Start()
        {
            Listener.Start();
            "接受第一个请求".Log();
            Listener.BeginAcceptTcpClient(new AsyncCallback(Accept), Listener);
        }

        /// <summary>
        /// 停止代理
        /// </summary>
        public void Stop()
        {
            Listener.Stop();
            dispatcher.Stop();
        }

        /// <summary>
        /// 代理分发请求
        /// </summary>
        public void Accept(IAsyncResult iar)
        {
            TcpListener listener = iar.AsyncState as TcpListener;
            TcpClient source = listener.EndAcceptTcpClient(iar);

            // 开始接受下一个请求。
            "等待下一个请求".Log();
            listener.BeginAcceptTcpClient(new AsyncCallback(Accept), listener);

            source.SendTimeout = 300000;
            source.ReceiveTimeout = 300000;
            PhpCgiProcess worker = dispatcher.Dispatch();
            ThreadPool.QueueUserWorkItem(e =>
            {
                try
                {
                    worker.Start(source);
                }
                catch (Exception exception)
                {
                    exception.Message.Log();
                }
            });
        }
    }
}
