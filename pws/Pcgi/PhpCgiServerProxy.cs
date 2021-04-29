using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Pws.Pcgi
{
    public class PhpCgiServerProxy
    {
        public int Port { get; private set; }
        public TcpListener Listener { get; private set; }
        public PhpCgiProcessDispatcher Dispatcher { get; private set; }

        public PhpCgiServerProxy(int port=9000)
        {
            Port = port;
            Listener = new TcpListener(IPAddress.Any, Port);
            Dispatcher = new PhpCgiProcessDispatcher();
        }

        public void Start()
        {
            Listener.Start();
            "接受第一个请求".Log();
            Listener.BeginAcceptTcpClient(new AsyncCallback(Accept), Listener);
        }

        public void Stop()
        {
            Listener.Stop();
        }

        private void Accept(IAsyncResult iar)
        {
            TcpListener listener = iar.AsyncState as TcpListener;
            TcpClient source = listener.EndAcceptTcpClient(iar);

            // 开始接受下一个请求。
            "等待下一个请求".Log();
            listener.BeginAcceptTcpClient(new AsyncCallback(Accept), listener);

            // 开始调度并处理。
        }
    }
}
