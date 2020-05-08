using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net.Sockets;

namespace pws
{
    /// <summary>
    /// 
    /// </summary>
    public class PhpCgiServer
    {
        public Process Process { get; private set; }
        public short Port { get; private set; }
        public volatile int Counter;

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
            PhpCgiTransfer task = new PhpCgiTransfer
            {
                Source = source,
                Target = target,
            };
            // 代理任务线程
            ThreadPool.QueueUserWorkItem(param =>
            {
                PhpCgiTransfer transfer = param as PhpCgiTransfer;
                NetworkStream requester = transfer.Source.GetStream();
                NetworkStream responser = transfer.Target.GetStream();
                byte[] buffer = new byte[1024];
                while (true)
                {
                    try
                    {
                        // 转发请求信息。
                        if (requester.DataAvailable)
                        {
                            int count = requester.Read(buffer, 0, buffer.Length);
                            responser.Write(buffer, 0, count);
                        }

                        // 接收响应内容。
                        if (responser.DataAvailable)
                        {
                            int count = responser.Read(buffer, 0, buffer.Length);
                            requester.Write(buffer, 0, count);
                            // 一旦开始接收就直到结束为止。
                            while (count > 0)
                            {
                                count = responser.Read(buffer, 0, buffer.Length);
                                requester.Write(buffer, 0, count);
                            }
                            break; // 响应内容接收完毕，退出。
                        }
                    }
                    catch (Exception e)
                    {
                        e.ToString().Log();
                        break;
                    }
                }
                // 结束时回收资源。
                requester.Dispose();
                responser.Dispose();
                transfer.Source.Close();
                transfer.Target.Close();
                --Counter;
            }, task);
        }
    }
}
