using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pws
{
    public class FastCgiEndRequestBody
    {
        public byte AppStatusB3;
        public byte AppStatusB2;
        public byte AppStatusB1;
        public byte AppStatusB0;
        public byte ProtocolStatus;
        public byte ReservedB2;
        public byte ReservedB1;
        public byte ReservedB0;

        public FastCgiEndRequestBody(byte[] data)
        {
            AppStatusB3 = data[0];
            AppStatusB2 = data[1];
            AppStatusB1 = data[2];
            AppStatusB0 = data[3];
            ProtocolStatus = data[4];
            ReservedB2 = data[5];
            ReservedB1 = data[6];
            ReservedB0 = data[7];
        }

        public int AppStatus
        {
            get
            {
                return (AppStatusB3 << 24) | (AppStatusB2 << 16) | (AppStatusB1 << 8) | AppStatusB0;
            }
        }
    }
}
