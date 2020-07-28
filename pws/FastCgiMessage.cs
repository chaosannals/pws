﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pws
{
    /// <summary>
    /// 请求信息
    /// </summary>
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
            StringBuilder builder = new StringBuilder();
            builder.Append("ID: ");
            builder.Append(Header.RequestId.ToString());
            builder.Append(" Length: ");
            builder.Append(Header.ContentLength.ToString());
            builder.Append(" Type: ");
            builder.Append(Header.Type.ToString());
            return builder.ToString();
        }
    }
}
