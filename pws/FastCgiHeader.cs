using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pws
{
    public class FastCgiHeader
    {
        public byte Version;
        public byte Type;
        public byte RequestIdB1;
        public byte RequestIdB0;
        public byte ContentLengthB1;
        public byte ContentLengthB0;
        public byte PaddingLength;
        public byte Reserved;

        public FastCgiHeader(byte[] data)
        {
            Version = data[0];
            Type = data[1];
            RequestIdB1 = data[2];
            RequestIdB0 = data[3];
            ContentLengthB1 = data[4];
            ContentLengthB0 = data[5];
            PaddingLength = data[6];
            Reserved = data[7];
        }

        public int RequestId
        {
            get
            {
                return (RequestIdB1 << 8) | RequestIdB0;
            }
        }

        public int ContentLength
        {
            get
            {
                return (ContentLengthB1 << 8) | ContentLengthB0;
            }
        }
    }
}
