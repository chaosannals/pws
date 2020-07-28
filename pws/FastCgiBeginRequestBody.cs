using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pws
{
    /// <summary>
    /// 请求开始信息内容
    /// </summary>
    public class FastCgiBeginRequestBody
    {
        public byte ReservedB4;
        public byte ReservedB3;
        public byte ReservedB2;
        public byte ReservedB1;
        public byte ReservedB0;

        public FastCgiRole Role { get; private set; }
        public FastCgiFlag Flags { get; private set; }

        public FastCgiBeginRequestBody(byte[] data)
        {
            Role = (FastCgiRole)((data[0] << 8) | data[1]);
            Flags = (FastCgiFlag)data[2];
            ReservedB4 = data[3];
            ReservedB3 = data[4];
            ReservedB2 = data[5];
            ReservedB1 = data[6];
            ReservedB0 = data[7];
        }
    }
}
