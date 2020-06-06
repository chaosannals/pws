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
                        PhpCgiProcess[] trash = processes.Where(p => p.IsReusable && p.IdleRate > 0.99).ToArray();
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

        public void Dispatch(TcpClient source)
        {
            if (!ticker.Enabled)
            {
                ticker.Start();
            }
            PhpCgiProcess worker = null;
            lock (processes)
            {
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
            }
            worker.Start(source);
        }

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
