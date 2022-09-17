namespace System.Compiler
{
    using System;
    using System.IO;

    internal sealed class MemoryStream
    {
        public byte[] Buffer;
        public int Length;
        public int position;

        public MemoryStream() : this(0x40)
        {
        }

        public MemoryStream(int capacity)
        {
            this.Buffer = new byte[capacity];
            this.Length = 0;
            this.position = 0;
        }

        public MemoryStream(byte[] bytes)
        {
            this.Buffer = bytes;
            this.Length = bytes.Length;
            this.position = 0;
        }

        private void Grow(byte[] myBuffer, int n, int m)
        {
            if (myBuffer != null)
            {
                int num = n * 2;
                while (m >= num)
                {
                    num *= 2;
                }
                byte[] buffer2 = this.Buffer = new byte[num];
                for (int i = 0; i < n; i++)
                {
                    buffer2[i] = myBuffer[i];
                }
            }
        }

        public void Seek(long offset, System.Compiler.SeekOrigin loc)
        {
            this.Position = (int) offset;
        }

        public byte[] ToArray()
        {
            int length = this.Length;
            byte[] buffer = this.Buffer;
            if (buffer.Length == length)
            {
                return this.Buffer;
            }
            byte[] buffer2 = new byte[length];
            for (int i = 0; i < length; i++)
            {
                buffer2[i] = buffer[i];
            }
            return buffer2;
        }

        public void Write(byte[] buffer, int index, int count)
        {
            int position = this.position;
            this.Position = position + count;
            byte[] buffer2 = this.Buffer;
            int num2 = 0;
            int num3 = position;
            int num4 = index;
            while (num2 < count)
            {
                buffer2[num3++] = buffer[num4++];
                num2++;
            }
        }

        public void WriteTo(System.Compiler.MemoryStream stream)
        {
            stream.Write(this.Buffer, 0, this.Length);
        }

        public void WriteTo(Stream stream)
        {
            stream.Write(this.Buffer, 0, this.Length);
        }

        public int Position
        {
            get => 
                this.position;
            set
            {
                byte[] myBuffer = this.Buffer;
                int length = myBuffer.Length;
                if (value >= length)
                {
                    this.Grow(myBuffer, length, value);
                }
                if (value > this.Length)
                {
                    this.Length = value;
                }
                this.position = value;
            }
        }
    }
}

