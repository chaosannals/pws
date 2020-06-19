using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Timers;

namespace Pws
{
    public class PhpCgiProcessDispatcher
    {
        private Timer ticker;
        private List<PhpCgiProcess> processes;

        public PhpCgiProcessDispatcher()
        {
            processes = new List<PhpCgiProcess>();
            ticker = new Timer();
            ticker.Elapsed += (sender, args) =>
            {
                try
                {
                    lock (processes)
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
        public void Dispatch(TcpClient source)
        {
            if (!ticker.Enabled)
            {
                ticker.Start();
            }

            lock (processes)
            {
                PhpCgiProcess worker = null;
                foreach (PhpCgiProcess process in processes)
                {
                    if (process.IsReusable)
                    {
                        worker = process;
                        break;
                    }
                }
                if (worker == null)
                {
                    worker = new PhpCgiProcess();
                    processes.Add(worker);
                }
                worker.Start(source);
            }
        }

        /// <summary>
        /// 停止所有的CGI服务。
        /// </summary>
        public void Stop()
        {
            lock(processes)
            {
                foreach(PhpCgiProcess process in processes)
                {
                    process.Stop();
                }
            }
        }
    }
}
