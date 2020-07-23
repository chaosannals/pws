using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace Pws
{
    /// <summary>
    /// PHP FastCGI 进程类
    /// </summary>
    public class FastCgiPhpProcess
    {
        public static string Here { get { return AppDomain.CurrentDomain.BaseDirectory; } }

        private Thread thread;
        private List<FastCgiPhpRequester> transfers;

        public bool Able { get; private set; }
        public int Port { get; private set; }
        public Process Process { get; private set; }
        public TcpClient Responser { get; private set; }
        public int WorkCount { get { return transfers.Count; } }


        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="port"></param>
        public FastCgiPhpProcess(int port)
        {
            Able = true;
            Port = port;
            Process = new Process();
            Process.StartInfo.FileName = Path.Combine(Here, "php-cgi.exe");
            Process.StartInfo.Arguments = string.Format("-b {0:D}", Port);
            Process.StartInfo.WorkingDirectory = Here;
            Process.StartInfo.CreateNoWindow = true;
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process.EnableRaisingEvents = true;
            Process.Exited += (s, e) => { if (Able) Process.Start(); };
            Process.Start();
            Responser = new TcpClient("127.0.0.1", Port);
            Responser.SendTimeout = 30000;
            Responser.ReceiveTimeout = 30000;

            transfers = new List<FastCgiPhpRequester>();
            thread = new Thread(() => {
                while (true)
                {
                    try
                    {
                        foreach (var transfer in transfers)
                        {

                        }
                    }
                    catch (Exception e)
                    {
                        e.Message.Log();
                    }
                    Thread.Sleep(0);
                }
            });
            thread.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        public void Respond(TcpClient source)
        {
            DateTime start = DateTime.Now;
            Guid guid = Guid.NewGuid();
        }

        /// <summary>
        /// 停止进程。
        /// </summary>
        public void Stop()
        {
            try
            {
                Able = false;
                if (!Process.HasExited)
                {
                    Process.Kill();
                    Process.Close();
                }
                if (thread.IsAlive)
                {
                    thread.Abort();
                }
            }
            catch (InvalidOperationException e)
            {
                e.Message.Log();
            }
        }
    }
}
