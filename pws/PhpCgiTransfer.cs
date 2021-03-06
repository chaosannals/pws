﻿using System;
using System.Net.Sockets;
using System.Threading;
using Pws.Fcgi;

namespace Pws
{
    /// <summary>
    /// 传输
    /// </summary>
    public class PhpCgiTransfer
    {
        public TcpClient Source { get; set; }
        public TcpClient Target { get; set; }

        /// <summary>
        /// 传输
        /// </summary>
        public void Transfer()
        {
            NetworkStream requester = Source.GetStream();
            NetworkStream responser = Target.GetStream();
            byte[] buffer = new byte[8192];
            DateTime begin = DateTime.Now;
            FastCgiParser fcms = new FastCgiParser();
            FastCgiParser fcmt = new FastCgiParser();
            try
            {
                bool able = true;
                while (able)
                {
                    // 转发请求信息。
                    if (requester.DataAvailable)
                    {
                        DateTime start = DateTime.Now;
                        int count = requester.Read(buffer, 0, buffer.Length);
                        responser.Write(buffer, 0, count);
                        fcms.Gain(buffer, count);
                        while (true)
                        {
                            FastCgiMessage m = fcms.Pop();
                            if (m == null) break;
                            if (m.Header.Type == FastCgiType.AbortRequest)
                            {
                                able = false;
                            }
                            if (m.Header.Type == FastCgiType.BeginRequest)
                            {
                                FastCgiBeginRequestBody body = m.AsBeginBody();
                                "{0} => {1}".Log(m, body);
                            }
                            else
                            {
                                m.ToString().Log();
                            }
                        }
                    }

                    // 接收响应内容。
                    if (responser.DataAvailable)
                    {
                        DateTime start = DateTime.Now;
                        int count = responser.Read(buffer, 0, buffer.Length);
                        requester.Write(buffer, 0, count);
                        fcmt.Gain(buffer, count);

                        // 分析 FastCGI
                        while (true)
                        {
                            FastCgiMessage m = fcmt.Pop();
                            if (m == null) break;
                            if (m.Header.Type == FastCgiType.EndRequest)
                            {
                                able = false;
                                FastCgiEndRequestBody body = m.AsEndBody();
                                "{0} => {1}".Log(m, body);
                            } else
                            {
                                m.ToString().Log();
                            }
                        }
                    }
                }
            }
            finally
            {
                TimeSpan d = DateTime.Now.Subtract(begin);
                "转发耗时 {0:N} ms".Log(d.TotalMilliseconds);
                // 结束时回收资源。
                requester.Dispose();
                responser.Dispose();
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            Target.Close();
            Source.Close();
        }
    }
}
