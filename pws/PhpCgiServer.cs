using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net.Sockets;

namespace pws
{
    /// <summary>
    /// PHP CGI 服务器
    /// </summary>
    public class PhpCgiServer
    {
        public Process Process { get; private set; }
        public short Port { get; private set; }
        public volatile int Counter;

        /// <summary>
        /// PHP CGI 服务器初始化
        /// </summary>
        /// <param name="port"></param>
        public PhpCgiServer(short port)
        {
            Port = port;
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
            Process.OutputDataReceived += new DataReceivedEventHandler((sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    args.Data.Log();
                }
            });
        }

        /// <summary>
        /// 接受一个请求
        /// </summary>
        /// <param name="task"></param>
        public void Serve(TcpClient source)
        {
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
                    transfer.Source.Close();
                    transfer.Target.Close();
                    --Counter;
                }
            }, new PhpCgiTransfer
            {
                Source = source,
                Target = target,
            });
        }
    }
}
