using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pws.Fcgi
{
    /// <summary>
    /// 请求头
    /// </summary>
    public class FastCgiHeader
    {
        public byte Version { get; private set; }
        public FastCgiType Type { get; private set; }
        public int RequestId { get; private set; }
        public int ContentLength { get; private set; }
        public byte PaddingLength { get; private set; }
        public byte Reserved { get; private set; }

        public int MessageLength { get; private set; }

        public FastCgiHeader(byte[] data)
        {
            Version = data[0];
            Type = (FastCgiType)data[1];
            RequestId = (data[2] << 8) | data[3];
            ContentLength = (data[4] << 8) | data[5];
            PaddingLength = data[6];
            Reserved = data[7];
            MessageLength = 8 + ContentLength + PaddingLength;
        }
    }
}
