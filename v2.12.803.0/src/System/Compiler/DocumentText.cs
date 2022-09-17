namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Text;

    internal sealed class DocumentText
    {
        public unsafe byte* AsciiStringPtr;
        public int Length;
        public string Source;
        public ISourceText TextProvider;

        public DocumentText(ISourceText textProvider)
        {
            if (textProvider != null)
            {
                this.TextProvider = textProvider;
                this.Length = textProvider.Length;
            }
        }

        public unsafe DocumentText(ISourceTextBuffer textProvider)
        {
            if (textProvider != null)
            {
                this.TextProvider = textProvider;
                this.AsciiStringPtr = textProvider.Buffer;
                this.Length = textProvider.Length;
            }
        }

        public DocumentText(string source)
        {
            if (source != null)
            {
                this.Source = source;
                this.Length = source.Length;
            }
        }

        public unsafe bool Equals(string str, int position, int length)
        {
            if (str == null)
            {
                return false;
            }
            if (str.Length != length)
            {
                return false;
            }
            if ((position < 0) || ((position + length) > this.Length))
            {
                return false;
            }
            byte* asciiStringPtr = this.AsciiStringPtr;
            if (asciiStringPtr != null)
            {
                int index = position;
                for (int j = 0; j < length; j++)
                {
                    if (asciiStringPtr[index] != str[j])
                    {
                        return false;
                    }
                    index++;
                }
                return true;
            }
            string source = this.Source;
            if (source != null)
            {
                int num3 = position;
                for (int k = 0; k < length; k++)
                {
                    if (source[num3] != str[k])
                    {
                        return false;
                    }
                    num3++;
                }
                return true;
            }
            ISourceText textProvider = this.TextProvider;
            if (textProvider == null)
            {
                return false;
            }
            int num5 = position;
            for (int i = 0; i < length; i++)
            {
                if (textProvider[num5] != str[i])
                {
                    return false;
                }
                num5++;
            }
            return true;
        }

        public unsafe bool Equals(int offset, DocumentText text, int textOffset, int length)
        {
            if (((offset < 0) || (length < 0)) || ((offset + length) > this.Length))
            {
                return false;
            }
            if (((textOffset < 0) || (text == null)) || ((textOffset + length) > text.Length))
            {
                return false;
            }
            byte* asciiStringPtr = this.AsciiStringPtr;
            if (asciiStringPtr != null)
            {
                byte* numPtr2 = text.AsciiStringPtr;
                if (numPtr2 != null)
                {
                    int num = offset;
                    int num2 = textOffset;
                    int num3 = offset + length;
                    while (num < num3)
                    {
                        if (asciiStringPtr[num] != numPtr2[num2])
                        {
                            return false;
                        }
                        num++;
                        num2++;
                    }
                    return true;
                }
                string str = text.Source;
                if (str != null)
                {
                    int num4 = offset;
                    int num5 = textOffset;
                    int num6 = offset + length;
                    while (num4 < num6)
                    {
                        if (asciiStringPtr[num4] != str[num5])
                        {
                            return false;
                        }
                        num4++;
                        num5++;
                    }
                    return true;
                }
                ISourceText text2 = text.TextProvider;
                if (text2 == null)
                {
                    return false;
                }
                int index = offset;
                int num8 = textOffset;
                int num9 = offset + length;
                while (index < num9)
                {
                    if (asciiStringPtr[index] != text2[num8])
                    {
                        return false;
                    }
                    index++;
                    num8++;
                }
                return true;
            }
            string source = this.Source;
            if (source != null)
            {
                byte* numPtr3 = text.AsciiStringPtr;
                if (numPtr3 != null)
                {
                    int num10 = offset;
                    int num11 = textOffset;
                    int num12 = offset + length;
                    while (num10 < num12)
                    {
                        if (source[num10] != numPtr3[num11])
                        {
                            return false;
                        }
                        num10++;
                        num11++;
                    }
                    return true;
                }
                string str3 = text.Source;
                if (str3 != null)
                {
                    int num13 = offset;
                    int num14 = textOffset;
                    int num15 = offset + length;
                    while (num13 < num15)
                    {
                        if (source[num13] != str3[num14])
                        {
                            return false;
                        }
                        num13++;
                        num14++;
                    }
                    return true;
                }
                ISourceText text3 = text.TextProvider;
                if (text3 == null)
                {
                    return false;
                }
                int num16 = offset;
                int num17 = textOffset;
                int num18 = offset + length;
                while (num16 < num18)
                {
                    if (source[num16] != text3[num17])
                    {
                        return false;
                    }
                    num16++;
                    num17++;
                }
                return true;
            }
            ISourceText textProvider = this.TextProvider;
            if (textProvider == null)
            {
                return false;
            }
            byte* numPtr4 = text.AsciiStringPtr;
            if (numPtr4 != null)
            {
                int num19 = offset;
                int num20 = textOffset;
                int num21 = offset + length;
                while (num19 < num21)
                {
                    if (textProvider[num19] != numPtr4[num20])
                    {
                        return false;
                    }
                    num19++;
                    num20++;
                }
                return true;
            }
            string str4 = text.Source;
            if (str4 != null)
            {
                int num22 = offset;
                int num23 = textOffset;
                int num24 = offset + length;
                while (num22 < num24)
                {
                    if (textProvider[num22] != str4[num23])
                    {
                        return false;
                    }
                    num22++;
                    num23++;
                }
                return true;
            }
            ISourceText text5 = text.TextProvider;
            if (text5 == null)
            {
                return false;
            }
            int num25 = offset;
            int num26 = textOffset;
            int num27 = offset + length;
            while (num25 < num27)
            {
                if (textProvider[num25] != text5[num26])
                {
                    return false;
                }
                num25++;
                num26++;
            }
            return true;
        }

        public unsafe string Substring(int position, int length)
        {
            if (((position >= 0) && (length >= 0)) && ((position + length) <= (this.Length + 1)))
            {
                if ((position + length) > this.Length)
                {
                    length = this.Length - position;
                }
                if (this.AsciiStringPtr != null)
                {
                    return new string((sbyte*) this.AsciiStringPtr, position, length, Encoding.ASCII);
                }
                if (this.Source != null)
                {
                    return this.Source.Substring(position, length);
                }
                if (this.TextProvider != null)
                {
                    return this.TextProvider.Substring(position, length);
                }
            }
            return "";
        }

        public char this[int position]
        {
            get
            {
                if ((position >= 0) && (position < this.Length))
                {
                    if (this.AsciiStringPtr != null)
                    {
                        return *(((char*) (this.AsciiStringPtr + position)));
                    }
                    if (this.Source != null)
                    {
                        return this.Source[position];
                    }
                    if (this.TextProvider != null)
                    {
                        return this.TextProvider[position];
                    }
                }
                return '\0';
            }
        }
    }
}

