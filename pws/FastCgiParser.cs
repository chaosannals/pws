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
            }

            // 获取请求内容
            if (
                header != null &&
                stream.Length >= header.MessageLength
            )
            {
                byte[] body = new byte[header.ContentLength];
                stream.Seek(8, SeekOrigin.Begin);
                stream.Read(body, 0, header.ContentLength);
                FastCgiMessage message = new FastCgiMessage(header, body);
                MemoryStream one = new MemoryStream();
                int length = (int)stream.Length - header.MessageLength;
                if (length > 0)
                {
                    one.Write(stream.GetBuffer(), header.MessageLength, length);
                }
                stream = one;
                header = null;
                return message;
            }
            return null;
        }
    }
}
