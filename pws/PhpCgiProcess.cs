﻿using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Linq;

namespace Pws
{
    /// <summary>
    /// PHP-CGI 进程
    /// </summary>
    public class PhpCgiProcess
    {
        public int Port { get; private set; }
        public Process Process { get; private set; }
        public bool IsReusable { get; private set; }

        public double LiveTime
        {
            get
            {
                return DateTime.Now.Subtract(Process.StartTime).TotalMilliseconds;
            }
        }

        public double WorkTime
        {
            get
            {
                return Process.TotalProcessorTime.TotalMilliseconds;
            }
        }

        public double WorkRate
        {
            get
            {
                return WorkRate / LiveTime;
            }
        }

        public double IdleTime
        {
            get
            {
                return LiveTime - WorkTime;
            }
        }

        public double IdleRate
        {
            get
            {
                return IdleTime / LiveTime;
            }
        }

        public bool IsRecyclable
        {
            get
            {
                return IsReusable && LiveTime > 100000 && IdleRate > 0.75;
            }
        }

        public PhpCgiProcess()
        {
            string here = AppDomain.CurrentDomain.BaseDirectory;
            Port = FindUsablePort();
            Process = new Process();
            Process.StartInfo.FileName = Path.Combine(here, "php-cgi.exe");
            Process.StartInfo.Arguments = string.Format("-b {0:D}", Port);
            Process.StartInfo.WorkingDirectory = here;
            Process.StartInfo.CreateNoWindow = true;
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.RedirectStandardError = true;
            Process.EnableRaisingEvents = true;
            Process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    args.Data.Log();
                }
            };
            Process.Start();
        }

        public void Start(TcpClient source)
        {
            if (Process.HasExited)
            {
                Port = FindUsablePort();
                Process.StartInfo.Arguments = string.Format("-b {0:D}", Port);
                Process.BeginOutputReadLine();
                Process.Start();
                "进程（{0:D}）重启".Log(Port);
            }

            IsReusable = false;
            TcpClient target = new TcpClient("127.0.0.1", Port);
            target.SendTimeout = 30000;
            target.ReceiveTimeout = 30000;
            DateTime start = DateTime.Now;
            Guid guid = Guid.NewGuid();
            "进程（{0:D}）处理请求 {1}".Log(Port, guid);

            // 代理任务线程
            ThreadPool.QueueUserWorkItem(param =>
            {
                PhpCgiTransfer transfer = param as PhpCgiTransfer;
                try
                {
                    transfer.Transfer();
                }
                catch (Exception e)
                {
                    e.ToString().Log();
                }
                finally
                {
                    TimeSpan duration = DateTime.Now.Subtract(start);
                    IsReusable = true;
                    transfer.Close();
                    "进程（{0:D}）处理请求 {1} 耗时 {2:N} ms".Log(Port, guid, duration.TotalMilliseconds);
                }
            }, new PhpCgiTransfer
            {
                Source = source,
                Target = target,
            });
        }

        /// <summary>
        /// 停止进程。
        /// </summary>
        public void Stop()
        {
            if (!Process.HasExited)
            {
                Process.Kill();
                Process.Close();
            }
        }

        /// <summary>
        /// 找到可用的端口
        /// </summary>
        /// <returns></returns>
        public static int FindUsablePort()
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndPoints = ipProperties.GetActiveTcpListeners();
            int[] ports = tcpEndPoints.Select(p => p.Port).Where(p => p > 9000).ToArray();
            Array.Sort(ports);
            for (int i = 0; i < ports.Length; ++i)
            {
                int port = ports[i];
                int result = port + 1;
                if (result < ports[i + 1])
                {
                    return result;
                }
            }
            return 0;
        }
    }
}
