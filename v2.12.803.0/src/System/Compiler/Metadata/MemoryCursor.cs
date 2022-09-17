namespace System.Compiler.Metadata
{
    using System;
    using System.Compiler;
    using System.Text;

    internal sealed class MemoryCursor
    {
        private unsafe byte* buffer;
        internal readonly int Length;
        private unsafe byte* pb;

        internal unsafe MemoryCursor(MemoryCursor c)
        {
            this.buffer = c.buffer;
            this.pb = c.pb;
            this.Length = c.Length;
        }

        internal MemoryCursor(MemoryMappedFile memoryMap) : this(memoryMap.Buffer, memoryMap.Length)
        {
        }

        internal unsafe MemoryCursor(byte* buffer, int length) : this(buffer, length, 0)
        {
        }

        internal unsafe MemoryCursor(byte* buffer, int length, int position)
        {
            this.buffer = buffer;
            this.pb = buffer + position;
            this.Length = length;
        }

        internal unsafe void Align(int size)
        {
            int num = this.Position & (size - 1);
            if (num != 0)
            {
                this.pb += size - num;
            }
        }

        internal unsafe byte Byte(int i) => 
            this.pb[i];

        internal unsafe byte* GetBuffer() => 
            this.buffer;

        internal unsafe short Int16(int i) => 
            *(((short*) (this.pb + (i * 2))));

        internal unsafe int Int32(int i) => 
            *(((int*) (this.pb + (i * 4))));

        internal string ReadASCII() => 
            this.ReadASCII(-1);

        internal unsafe string ReadASCII(int bytesToRead)
        {
            int num = bytesToRead;
            if (bytesToRead == -1)
            {
                num = 0x80;
            }
            byte* pb = this.pb;
            char[] chArray = new char[num];
            int length = 0;
            byte num3 = 0;
        Label_0040:
            while (length < num)
            {
                pb++;
                num3 = pb[0];
                if (num3 == 0)
                {
                    break;
                }
                chArray[length++] = (char) num3;
            }
            if (bytesToRead == -1)
            {
                if (num3 != 0)
                {
                    char[] chArray2 = new char[num *= 2];
                    for (int i = 0; i < length; i++)
                    {
                        chArray2[i] = chArray[i];
                    }
                    chArray = chArray2;
                    goto Label_0040;
                }
                this.pb = pb;
            }
            else
            {
                this.pb += bytesToRead;
            }
            return new string(chArray, 0, length);
        }

        internal unsafe bool ReadBoolean()
        {
            byte* pb = this.pb;
            bool flag = *((bool*) pb);
            this.pb = pb + 1;
            return flag;
        }

        internal unsafe byte ReadByte()
        {
            byte* pb = this.pb;
            byte num = pb[0];
            this.pb = pb + 1;
            return num;
        }

        internal unsafe byte[] ReadBytes(int c)
        {
            byte[] buffer = new byte[c];
            byte* pb = this.pb;
            for (int i = 0; i < c; i++)
            {
                pb++;
                buffer[i] = pb[0];
            }
            this.pb = pb;
            return buffer;
        }

        internal unsafe char ReadChar()
        {
            byte* pb = this.pb;
            char ch = *((char*) pb);
            this.pb = pb + 2;
            return ch;
        }

        internal int ReadCompressedInt()
        {
            byte num = this.ReadByte();
            if ((num & 0x80) == 0)
            {
                return num;
            }
            if ((num & 0x40) == 0)
            {
                return (((num & 0x3f) << 8) | this.ReadByte());
            }
            if (num == 0xff)
            {
                return -1;
            }
            return (((((num & 0x3f) << 0x18) | (this.ReadByte() << 0x10)) | (this.ReadByte() << 8)) | this.ReadByte());
        }

        internal unsafe double ReadDouble()
        {
            byte* pb = this.pb;
            double num = *((double*) pb);
            this.pb = pb + 8;
            return num;
        }

        internal unsafe Identifier ReadIdentifierFromSerString()
        {
            byte* pointer = this.pb + 1;
            byte num = this.pb[0];
            uint length = 0;
            if ((num & 0x80) == 0)
            {
                length = num;
            }
            else if ((num & 0x40) == 0)
            {
                pointer++;
                length = (uint) (((num & 0x3f) << 8) | pointer[0]);
            }
            else
            {
                pointer++;
                pointer++;
                pointer++;
                length = (uint) (((((num & 0x3f) << 0x18) | (pointer[0] << 0x10)) | (pointer[0] << 8)) | pointer[0]);
            }
            this.pb = pointer + ((byte*) length);
            return Identifier.For(pointer, length);
        }

        internal unsafe short ReadInt16()
        {
            byte* pb = this.pb;
            short num = *((short*) pb);
            this.pb = pb + 2;
            return num;
        }

        internal unsafe int ReadInt32()
        {
            byte* pb = this.pb;
            int num = *((int*) pb);
            this.pb = pb + 4;
            return num;
        }

        internal unsafe long ReadInt64()
        {
            byte* pb = this.pb;
            long num = *((long*) pb);
            this.pb = pb + 8;
            return num;
        }

        internal int ReadReference(int refSize)
        {
            if (refSize == 2)
            {
                return this.ReadUInt16();
            }
            return this.ReadInt32();
        }

        internal unsafe sbyte ReadSByte()
        {
            byte* pb = this.pb;
            sbyte num = *((sbyte*) pb);
            this.pb = pb + 1;
            return num;
        }

        internal unsafe float ReadSingle()
        {
            byte* pb = this.pb;
            float num = *((float*) pb);
            this.pb = pb + 4;
            return num;
        }

        internal unsafe ushort ReadUInt16()
        {
            byte* pb = this.pb;
            ushort num = *((ushort*) pb);
            this.pb = pb + 2;
            return num;
        }

        internal unsafe uint ReadUInt32()
        {
            byte* pb = this.pb;
            uint num = *((uint*) pb);
            this.pb = pb + 4;
            return num;
        }

        internal unsafe ulong ReadUInt64()
        {
            byte* pb = this.pb;
            ulong num = *((ulong*) pb);
            this.pb = pb + 8;
            return num;
        }

        internal unsafe string ReadUTF16()
        {
            string str = new string((char*) this.pb);
            this.pb += (str.Length + 1) * 2;
            return str;
        }

        internal unsafe string ReadUTF16(int charsToRead)
        {
            char* pb = (char*) this.pb;
            char[] chArray = new char[charsToRead];
            for (int i = 0; i < charsToRead; i++)
            {
                pb++;
                chArray[i] = pb[0];
            }
            this.pb = (byte*) pb;
            return new string(chArray, 0, charsToRead);
        }

        internal unsafe string ReadUTF8()
        {
            byte* pb = this.pb;
            StringBuilder builder = new StringBuilder();
            byte num = 0;
            while (true)
            {
                pb++;
                num = pb[0];
                if (num == 0)
                {
                    break;
                }
                if ((num & 0x80) == 0)
                {
                    builder.Append((char) num);
                }
                else
                {
                    char ch;
                    pb++;
                    byte num2 = pb[0];
                    if (num2 == 0)
                    {
                        builder.Append((char) num);
                        break;
                    }
                    if ((num & 0x20) == 0)
                    {
                        ch = (char) (((num & 0x1f) << 6) | (num2 & 0x3f));
                    }
                    else
                    {
                        uint num4;
                        pb++;
                        byte num3 = pb[0];
                        if (num3 == 0)
                        {
                            builder.Append((char) ((num << 8) | num2));
                            break;
                        }
                        if ((num & 0x10) == 0)
                        {
                            num4 = (uint) ((((num & 15) << 12) | ((num2 & 0x3f) << 6)) | (num3 & 0x3f));
                        }
                        else
                        {
                            pb++;
                            byte num5 = pb[0];
                            if (num5 == 0)
                            {
                                builder.Append((char) ((num << 8) | num2));
                                builder.Append((char) num3);
                                break;
                            }
                            num4 = (uint) (((((num & 7) << 0x12) | ((num2 & 0x3f) << 12)) | ((num3 & 0x3f) << 6)) | (num5 & 0x3f));
                        }
                        if ((num4 & 0xffff0000) == 0)
                        {
                            ch = (char) num4;
                        }
                        else
                        {
                            builder.Append((char) ((num4 >> 10) | 0xd800));
                            ch = (char) ((num4 & 0x3ff) | 0xdc00);
                        }
                    }
                    builder.Append(ch);
                }
            }
            this.pb = pb;
            return builder.ToString();
        }

        internal unsafe string ReadUTF8(int bytesToRead)
        {
            char[] chArray = new char[bytesToRead];
            byte* pb = this.pb;
            this.pb += bytesToRead;
            int length = 0;
            while (bytesToRead > 0)
            {
                pb++;
                byte num2 = pb[0];
                bytesToRead--;
                if (((num2 & 0x80) == 0) || (bytesToRead == 0))
                {
                    chArray[length++] = (char) num2;
                }
                else
                {
                    char ch;
                    pb++;
                    byte num3 = pb[0];
                    bytesToRead--;
                    if ((num2 & 0x20) == 0)
                    {
                        ch = (char) (((num2 & 0x1f) << 6) | (num3 & 0x3f));
                    }
                    else
                    {
                        uint num5;
                        if (bytesToRead == 0)
                        {
                            chArray[length++] = (char) ((num2 << 8) | num3);
                            break;
                        }
                        pb++;
                        byte num4 = pb[0];
                        bytesToRead--;
                        if ((num2 & 0x10) == 0)
                        {
                            num5 = (uint) ((((num2 & 15) << 12) | ((num3 & 0x3f) << 6)) | (num4 & 0x3f));
                        }
                        else
                        {
                            if (bytesToRead == 0)
                            {
                                chArray[length++] = (char) ((num2 << 8) | num3);
                                chArray[length++] = (char) num4;
                                break;
                            }
                            pb++;
                            byte num6 = pb[0];
                            bytesToRead--;
                            num5 = (uint) (((((num2 & 7) << 0x12) | ((num3 & 0x3f) << 12)) | ((num4 & 0x3f) << 6)) | (num6 & 0x3f));
                        }
                        if ((num5 & 0xffff0000) == 0)
                        {
                            ch = (char) num5;
                        }
                        else
                        {
                            chArray[length++] = (char) ((num5 >> 10) | 0xd800);
                            ch = (char) ((num5 & 0x3ff) | 0xdc00);
                        }
                    }
                    chArray[length++] = ch;
                }
            }
            if ((length > 0) && (chArray[length - 1] == '\0'))
            {
                length--;
            }
            return new string(chArray, 0, length);
        }

        internal unsafe void SkipByte(int c)
        {
            this.pb += c;
        }

        internal unsafe void SkipInt16(int c)
        {
            this.pb += c * 2;
        }

        internal unsafe void SkipInt32(int c)
        {
            this.pb += c * 4;
        }

        internal unsafe void SkipUInt16(int c)
        {
            this.pb += c * 2;
        }

        internal unsafe ushort UInt16(int i) => 
            *(((ushort*) (this.pb + (i * 2))));

        internal int Position
        {
            get => 
                ((int) ((long) ((this.pb - this.buffer) / 1)));
            set
            {
                this.pb = this.buffer + value;
            }
        }
    }
}

