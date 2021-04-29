using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace Pws.Pcgi
{
    public class PhpCgiProcessDispatcher
    {
        private Dictionary<int, PhpCgiProcess> processes;

        public PhpCgiProcessDispatcher()
        {
            processes = new Dictionary<int, PhpCgiProcess>();
        }

        public int FindUsablePort(int start=29000)
        {
            lock (processes)
            {
                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] tcpEndPoints = ipProperties.GetActiveTcpListeners();
                List<int> ports = tcpEndPoints.Select(p => p.Port).Where(p => p > start).ToList<int>();
                ports.AddRange(processes.Keys);
                ports.Sort();
                int result = start;
                int i = 0;
                while (i < ports.Count && result < 60000)
                {
                    if (result != ports[i++]) return result;
                    ++result;
                }
                return 0;
            }
        }
    }
}
