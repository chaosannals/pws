using System;
using System.IO;

namespace Pws
{
    public class FastCgiMessager
    {
        private FastCgiHeader header;
        private MemoryStream stream;

        public FastCgiMessager()
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
            if (header == null)
            {
                if (stream.Length < 8)
                {
                    return null;
                }
                byte[] buffer = new byte[8];
                stream.Read(buffer, 0, 8);
                header = new FastCgiHeader(buffer);
            }
            if (stream.Length >= header.ContentLength)
            {
                byte[] body = new byte[header.ContentLength];
                FastCgiMessage message = new FastCgiMessage(header, body);
                header = null;
                return message;
            }
            return null;
        }
    }
}
