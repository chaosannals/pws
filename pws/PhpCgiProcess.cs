using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Linq;

namespace Pws
{
    public class PhpCgiProcess
    {
        public int Port { get; private set; }
        public Process Process { get; private set; }
        public bool IsReusable { get; private set; }

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
            }

            IsReusable = false;
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
                    IsReusable = true;
                    transfer.Close();
                }
            }, new PhpCgiTransfer
            {
                Source = source,
                Target = target,
            });
        }

        public void Stop()
        {
            if (!Process.HasExited)
            {
                Process.Kill();
                Process.Close();
            }
        }

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
