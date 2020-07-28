using System;
using System.IO;

namespace Pws
{
    public class FastCgiParser
    {
        private FastCgiHeader header;
        private MemoryStream stream;

        public FastCgiParser()
        {
            header = null;
            stream = new MemoryStream();
        }

        public void Gain(byte[] buffer, int size)
        {
            stream.Write(buffer, 0, size);
        }

        public FastCgiMessage Pop()
        {
            // 生成请求头
            if (header == null)
            {
                if (stream.Length < 8)
                {
                    return null;
                }
                byte[] buffer = new byte[8];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(buffer, 0, 8);
                stream.Seek(0, SeekOrigin.End);
                header = new FastCgiHeader(buffer);
                "ID {0} 流长 {1} 容长 {2}".Log(header.RequestId, stream.Length, header.ContentLength);
            }

            // 获取请求内容
            if (
                header != null &&
                stream.Length >= (8 + header.ContentLength)
            )
            {
                byte[] body = new byte[header.ContentLength];
                stream.Seek(8, SeekOrigin.Begin);
                stream.Read(body, 0, header.ContentLength);
                FastCgiMessage message = new FastCgiMessage(header, body);
                MemoryStream one = new MemoryStream();
                int length = (int)stream.Length - header.ContentLength - 8;
                if (length > 0)
                {
                    one.Write(stream.GetBuffer(), 8 + header.ContentLength, length);
                }
                stream = one;
                header = null;
                return message;
            }
            return null;
        }
    }
}
