namespace System.Compiler
{
    using System;
    using System.Text;

    internal sealed class BinaryWriter
    {
        public MemoryStream BaseStream;
        private bool UTF8;

        public BinaryWriter(MemoryStream output)
        {
            this.UTF8 = true;
            this.BaseStream = output;
        }

        public BinaryWriter(MemoryStream output, Encoding encoding)
        {
            this.UTF8 = true;
            this.BaseStream = output;
            this.UTF8 = false;
        }

        public static int GetUTF8ByteCount(string str)
        {
            int num = 0;
            int num2 = 0;
            int num3 = (str == null) ? 0 : str.Length;
            while (num2 < num3)
            {
                char ch = str[num2];
                if (ch < '\x0080')
                {
                    num++;
                }
                else if (ch < 'ࠀ')
                {
                    num += 2;
                }
                else if ((0xd800 <= ch) && (ch <= 0xdbff))
                {
                    num += 2;
                }
                else if ((0xdc00 <= ch) && (ch <= 0xdfff))
                {
                    num += 2;
                }
                else
                {
                    num += 3;
                }
                num2++;
            }
            return num;
        }

        public void Write(bool value)
        {
            MemoryStream baseStream = this.BaseStream;
            int position = baseStream.Position;
            baseStream.Position = position + 1;
            baseStream.Buffer[position] = value ? ((byte) 1) : ((byte) 0);
        }

        public void Write(byte value)
        {
            MemoryStream baseStream = this.BaseStream;
            int position = baseStream.Position;
            baseStream.Position = position + 1;
            baseStream.Buffer[position] = value;
        }

        public void Write(byte[] buffer)
        {
            if (buffer != null)
            {
                this.BaseStream.Write(buffer, 0, buffer.Length);
            }
        }

        public void Write(char ch)
        {
            MemoryStream baseStream = this.BaseStream;
            int position = baseStream.Position;
            if (this.UTF8)
            {
                if (ch < '\x0080')
                {
                    baseStream.Position = position + 1;
                    baseStream.Buffer[position] = (byte) ch;
                }
                else
                {
                    this.Write(new char[] { ch });
                }
            }
            else
            {
                baseStream.Position = position + 2;
                byte[] buffer = baseStream.Buffer;
                buffer[position++] = (byte) ch;
                buffer[position] = (byte) (ch >> 8);
            }
        }

        public void Write(char[] chars)
        {
            if (chars != null)
            {
                MemoryStream baseStream = this.BaseStream;
                int length = chars.Length;
                int position = baseStream.Position;
                if (!this.UTF8)
                {
                    baseStream.Position = position + (length * 2);
                    byte[] buffer2 = baseStream.Buffer;
                    for (int i = 0; i < length; i++)
                    {
                        char ch3 = chars[i];
                        buffer2[position++] = (byte) ch3;
                        buffer2[position++] = (byte) (ch3 >> 8);
                    }
                }
                else
                {
                    baseStream.Position = position + length;
                    byte[] buffer = baseStream.Buffer;
                    for (int j = 0; j < length; j++)
                    {
                        char ch = chars[j];
                        if ((ch & '\x0080') != 0)
                        {
                            int num4 = 0;
                            for (int k = length - (baseStream.Position - position); k < length; k++)
                            {
                                char ch2 = chars[k];
                                if (ch2 < '\x0080')
                                {
                                    baseStream.Position = position + 1;
                                    baseStream.Buffer[position++] = (byte) ch2;
                                }
                                else if (ch2 < 'ࠀ')
                                {
                                    baseStream.Position = position + 2;
                                    buffer = baseStream.Buffer;
                                    buffer[position++] = (byte) (((ch2 >> 6) & '\x001f') | 0xc0);
                                    buffer[position] = (byte) ((ch2 & '?') | 0x80);
                                }
                                else if ((0xd800 <= ch2) && (ch2 <= 0xdbff))
                                {
                                    num4 = (ch2 & 0x3ff) << 10;
                                }
                                else if ((0xdc00 <= ch2) && (ch2 <= 0xdfff))
                                {
                                    num4 |= ch2 & 'Ͽ';
                                    baseStream.Position = position + 4;
                                    buffer = baseStream.Buffer;
                                    buffer[position++] = (byte) (((num4 >> 0x12) & 7) | 240);
                                    buffer[position++] = (byte) (((num4 >> 12) & 0x3f) | 0x80);
                                    buffer[position++] = (byte) (((num4 >> 6) & 0x3f) | 0x80);
                                    buffer[position] = (byte) ((num4 & 0x3f) | 0x80);
                                }
                                else
                                {
                                    baseStream.Position = position + 3;
                                    buffer = baseStream.Buffer;
                                    buffer[position++] = (byte) (((ch2 >> 12) & '\x000f') | 0xe0);
                                    buffer[position++] = (byte) (((ch2 >> 6) & '?') | 0x80);
                                    buffer[position] = (byte) ((ch2 & '?') | 0x80);
                                }
                            }
                            return;
                        }
                        buffer[position++] = (byte) ch;
                    }
                }
            }
        }

        public unsafe void Write(double value)
        {
            MemoryStream baseStream = this.BaseStream;
            int position = baseStream.Position;
            baseStream.Position = position + 8;
            fixed (byte* numRef = baseStream.Buffer)
            {
                numRef[position] = (byte) value;
            }
        }

        public void Write(short value)
        {
            MemoryStream baseStream = this.BaseStream;
            int position = baseStream.Position;
            baseStream.Position = position + 2;
            byte[] buffer = baseStream.Buffer;
            buffer[position++] = (byte) value;
            buffer[position] = (byte) (value >> 8);
        }

        public void Write(int value)
        {
            MemoryStream baseStream = this.BaseStream;
            int position = baseStream.Position;
            baseStream.Position = position + 4;
            byte[] buffer = baseStream.Buffer;
            buffer[position++] = (byte) value;
            buffer[position++] = (byte) (value >> 8);
            buffer[position++] = (byte) (value >> 0x10);
            buffer[position] = (byte) (value >> 0x18);
        }

        public void Write(long value)
        {
            MemoryStream baseStream = this.BaseStream;
            int position = baseStream.Position;
            baseStream.Position = position + 8;
            byte[] buffer = baseStream.Buffer;
            uint num2 = (uint) value;
            uint num3 = (uint) (value >> 0x20);
            buffer[position++] = (byte) num2;
            buffer[position++] = (byte) (num2 >> 8);
            buffer[position++] = (byte) (num2 >> 0x10);
            buffer[position++] = (byte) (num2 >> 0x18);
            buffer[position++] = (byte) num3;
            buffer[position++] = (byte) (num3 >> 8);
            buffer[position++] = (byte) (num3 >> 0x10);
            buffer[position] = (byte) (num3 >> 0x18);
        }

        public void Write(sbyte value)
        {
            MemoryStream baseStream = this.BaseStream;
            int position = baseStream.Position;
            baseStream.Position = position + 1;
            baseStream.Buffer[position] = (byte) value;
        }

        public unsafe void Write(float value)
        {
            MemoryStream baseStream = this.BaseStream;
            int position = baseStream.Position;
            baseStream.Position = position + 4;
            fixed (byte* numRef = baseStream.Buffer)
            {
                numRef[position] = (byte) value;
            }
        }

        public void Write(string str)
        {
            this.Write(str, false);
        }

        public void Write(ushort value)
        {
            MemoryStream baseStream = this.BaseStream;
            int position = baseStream.Position;
            baseStream.Position = position + 2;
            byte[] buffer = baseStream.Buffer;
            buffer[position++] = (byte) value;
            buffer[position] = (byte) (value >> 8);
        }

        public void Write(uint value)
        {
            MemoryStream baseStream = this.BaseStream;
            int position = baseStream.Position;
            baseStream.Position = position + 4;
            byte[] buffer = baseStream.Buffer;
            buffer[position++] = (byte) value;
            buffer[position++] = (byte) (value >> 8);
            buffer[position++] = (byte) (value >> 0x10);
            buffer[position] = (byte) (value >> 0x18);
        }

        public void Write(ulong value)
        {
            MemoryStream baseStream = this.BaseStream;
            int position = baseStream.Position;
            baseStream.Position = position + 8;
            byte[] buffer = baseStream.Buffer;
            uint num2 = (uint) value;
            uint num3 = (uint) (value >> 0x20);
            buffer[position++] = (byte) num2;
            buffer[position++] = (byte) (num2 >> 8);
            buffer[position++] = (byte) (num2 >> 0x10);
            buffer[position++] = (byte) (num2 >> 0x18);
            buffer[position++] = (byte) num3;
            buffer[position++] = (byte) (num3 >> 8);
            buffer[position++] = (byte) (num3 >> 0x10);
            buffer[position] = (byte) (num3 >> 0x18);
        }

        public void Write(string str, bool emitNullTerminator)
        {
            if (str == null)
            {
                this.Write((byte) 0xff);
            }
            else
            {
                int length = str.Length;
                if (!emitNullTerminator)
                {
                    if (this.UTF8)
                    {
                        Ir2md.WriteCompressedInt(this, GetUTF8ByteCount(str));
                    }
                    else
                    {
                        Ir2md.WriteCompressedInt(this, length * 2);
                    }
                }
                MemoryStream baseStream = this.BaseStream;
                int position = baseStream.Position;
                if (!this.UTF8)
                {
                    baseStream.Position = position + (length * 2);
                    byte[] buffer2 = baseStream.Buffer;
                    for (int i = 0; i < length; i++)
                    {
                        char ch3 = str[i];
                        buffer2[position++] = (byte) ch3;
                        buffer2[position++] = (byte) (ch3 >> 8);
                    }
                    if (emitNullTerminator)
                    {
                        baseStream.Position = position + 2;
                        buffer2 = baseStream.Buffer;
                        buffer2[position++] = 0;
                        buffer2[position] = 0;
                    }
                }
                else
                {
                    baseStream.Position = position + length;
                    byte[] buffer = baseStream.Buffer;
                    for (int j = 0; j < length; j++)
                    {
                        char ch = str[j];
                        if (ch >= '\x0080')
                        {
                            int num4 = 0;
                            for (int k = length - (baseStream.Position - position); k < length; k++)
                            {
                                char ch2 = str[k];
                                if (ch2 < '\x0080')
                                {
                                    baseStream.Position = position + 1;
                                    baseStream.Buffer[position++] = (byte) ch2;
                                }
                                else if (ch2 < 'ࠀ')
                                {
                                    baseStream.Position = position + 2;
                                    buffer = baseStream.Buffer;
                                    buffer[position++] = (byte) (((ch2 >> 6) & '\x001f') | 0xc0);
                                    buffer[position++] = (byte) ((ch2 & '?') | 0x80);
                                }
                                else if ((0xd800 <= ch2) && (ch2 <= 0xdbff))
                                {
                                    num4 = (ch2 & 0x3ff) << 10;
                                }
                                else if ((0xdc00 <= ch2) && (ch2 <= 0xdfff))
                                {
                                    num4 |= ch2 & 'Ͽ';
                                    baseStream.Position = position + 4;
                                    buffer = baseStream.Buffer;
                                    buffer[position++] = (byte) (((num4 >> 0x12) & 7) | 240);
                                    buffer[position++] = (byte) (((num4 >> 12) & 0x3f) | 0x80);
                                    buffer[position++] = (byte) (((num4 >> 6) & 0x3f) | 0x80);
                                    buffer[position++] = (byte) ((num4 & 0x3f) | 0x80);
                                }
                                else
                                {
                                    baseStream.Position = position + 3;
                                    buffer = baseStream.Buffer;
                                    buffer[position++] = (byte) (((ch2 >> 12) & '\x000f') | 0xe0);
                                    buffer[position++] = (byte) (((ch2 >> 6) & '?') | 0x80);
                                    buffer[position++] = (byte) ((ch2 & '?') | 0x80);
                                }
                            }
                            if (emitNullTerminator)
                            {
                                baseStream.Position = position + 1;
                                baseStream.Buffer[position] = 0;
                            }
                            return;
                        }
                        buffer[position++] = (byte) ch;
                    }
                    if (emitNullTerminator)
                    {
                        baseStream.Position = position + 1;
                        baseStream.Buffer[position] = 0;
                    }
                }
            }
        }
    }
}

