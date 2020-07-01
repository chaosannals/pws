using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pws
{
    public class FastCgiBeginRequestBody
    {
        public byte RoleB1;
        public byte RoleB0;
        public byte Flags;
        public byte ReservedB4;
        public byte ReservedB3;
        public byte ReservedB2;
        public byte ReservedB1;
        public byte ReservedB0;

        public FastCgiBeginRequestBody(byte[] data)
        {
            RoleB1 = data[0];
            RoleB0 = data[1];
            Flags = data[2];
            ReservedB4 = data[3];
            ReservedB3 = data[4];
            ReservedB2 = data[5];
            ReservedB1 = data[6];
            ReservedB0 = data[7];
        }

        public int Role
        {
            get
            {
                return (RoleB1 << 1) | RoleB0;
            }
        }
    }
}
