using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;

namespace Pws
{
    public class PhpCgiProcessDispatcher
    {
        private System.Timers.Timer ticker;
        private volatile object locking = new object();
        private volatile List<PhpCgiProcess> processes;
        private List<int> Ports { get { return processes.Select(p => p.Port).ToList<int>(); } }

        public PhpCgiProcessDispatcher()
        {
            processes = new List<PhpCgiProcess>();
            ticker = new System.Timers.Timer();
            ticker.Elapsed += (sender, args) =>
            {
                try
                {
                    lock (locking)
                    {
                        PhpCgiProcess[] trash = processes.Where(p => p.IsRecyclable).ToArray();
                        processes = processes.Where(p => !trash.Contains(p)).ToList();
                        foreach (PhpCgiProcess process in trash)
                        {
                            process.Stop();
                        }
                    }
                }
                catch (Exception e)
                {
                    e.ToString().Log();
                }
            };
            ticker.Interval = 10000;
        }

        /// <summary>
        /// 调度请求。
        /// </summary>
        /// <param name="source"></param>
        public PhpCgiProcess Dispatch()
        {
            if (!ticker.Enabled)
            {
                ticker.Start();
            }

            lock (locking)
            {
                PhpCgiProcess worker = null;
                DateTime start = DateTime.Now;
                "当前进程数 {0:D}".Log(processes.Count);
                foreach (PhpCgiProcess process in processes)
                {
                    if (process.IsReusable)
                    {
                        "进程 {0:D} 接受调度".Log(process.Port);
                        worker = process;
                        process.IsReusable = false;
                        break;
                    }
                }
                if (worker == null)
                {
                    worker = new PhpCgiProcess(FindUsablePort());
                    processes.Add(worker);
                    "新进程 {0:D} 接受调度".Log(worker.Port);
                }
                TimeSpan d = DateTime.Now.Subtract(start);
                "调度 {0:D} 锁耗时 {1:N} ms".Log(worker.Port, d.TotalMilliseconds);
                return worker;
            }
        }

        /// <summary>
        /// 停止所有的CGI服务。
        /// </summary>
        public void Stop()
        {
            lock(locking)
            {
                foreach(PhpCgiProcess process in processes)
                {
                    process.Stop();
                }
            }
        }

        /// <summary>
        /// 找到可用的端口
        /// </summary>
        /// <returns></returns>
        public int FindUsablePort()
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndPoints = ipProperties.GetActiveTcpListeners();
            List<int> ports = tcpEndPoints.Select(p => p.Port).Where(p => p > 9000).ToList<int>();
            ports.AddRange(Ports);
            ports.Sort();
            int result = 9001;
            int i = 0;
            while (i < ports.Count && result < 60000)
            {
                if (result == ports[i])
                {
                    ++result;
                    ++i;
                } else
                {
                    return result;
                }
            }
            return 0;
        }
    }
}
