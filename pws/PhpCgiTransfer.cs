using System;
using System.Net.Sockets;

namespace pws
{
    public class PhpCgiTransfer
    {
        public TcpClient Source { get; set; }
        public TcpClient Target { get; set; }
    }
}
