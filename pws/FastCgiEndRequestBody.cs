using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pws
{
    /// <summary>
    /// 请求结束内容
    /// </summary>
    public class FastCgiEndRequestBody
    {
        public byte ReservedB2;
        public byte ReservedB1;
        public byte ReservedB0;

        public int AppStatus { get; private set; }
        public FastCgiProtocolStatus ProtocolStatus { get; private set; }

        public FastCgiEndRequestBody(byte[] data)
        {
            AppStatus = (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3];
            ProtocolStatus = (FastCgiProtocolStatus)data[4];
            ReservedB2 = data[5];
            ReservedB1 = data[6];
            ReservedB0 = data[7];
        }
    }
}
