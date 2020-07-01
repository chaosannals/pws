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
    }
}
