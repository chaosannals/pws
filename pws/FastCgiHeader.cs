using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pws
{
    public struct FastCgiHeader
    {
        public byte Version;
        public byte Type;
        public byte RequestIdB1;
        public byte RequestIdB0;
        public byte ContentLengthB1;
        public byte ContentLengthB0;
        public byte PaddingLength;
        public byte Reserved;
    }
}
