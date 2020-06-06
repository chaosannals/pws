using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net.Sockets;

namespace Pws
{
    /// <summary>
    /// PHP CGI 服务器
    /// </summary>
    public class PhpCgiServer
    {
        public Process Process { get; private set; }

        public volatile int Sparer;
        public volatile int Counter;
        public short Port { get; private set; }
        public bool HasExited { get { return Process.HasExited; } }

        /// <summary>
        /// PHP CGI 服务器初始化。
        /// </summary>
        /// <param name="port">端口号</param>
        public PhpCgiServer(short port)
        {
            Port = port;
            Sparer = 0;
            Counter = 0;
            string here = AppDomain.CurrentDomain.BaseDirectory;
            Process = new Process();
            Process.StartInfo.FileName = Path.Combine(here, "php-cgi.exe");
            Process.StartInfo.WorkingDirectory = here;
            Process.StartInfo.Arguments = string.Format("-b {0:D}", port);
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
        }

        /// <summary>
        /// 接受一个请求。
        /// </summary>
        /// <param name="task"></param>
        public void Serve(TcpClient source)
        {
            if (HasExited)
            {
                "启动 PHP CGI 服务 {0:D} 端口".Log(Port);
                Process.Start();
                Process.BeginOutputReadLine();
            }
            ++Counter;
            TcpClient target = new TcpClient("127.0.0.1", Port);
            target.SendTimeout = 300000;
            target.ReceiveTimeout = 300000;

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
                    transfer.Close();
                    --Counter;
                }
            }, new PhpCgiTransfer
            {
                Source = source,
                Target = target,
            });
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Stop()
        {
            if (!HasExited)
            {
                Process.Kill();
                Process.Close();
            }
        }
    }
}
