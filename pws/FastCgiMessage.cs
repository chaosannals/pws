using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pws
{
    public class FastCgiMessage
    {
        public FastCgiHeader Header;
        public byte[] Body;

        public FastCgiMessage(FastCgiHeader header, byte[] body)
        {
            Header = header;
            Body = body;
        }

        public FastCgiBeginRequestBody AsBeginBody()
        {
            return new FastCgiBeginRequestBody(Body);
        }

        public FastCgiEndRequestBody AsEndBody()
        {
            return new FastCgiEndRequestBody(Body);
        }

        public string AsUTF8Body()
        {
            return Encoding.UTF8.GetString(Body.ToArray());
        }

        public override string ToString()
        {
            return base.ToString();// todo
        }
    }
}
