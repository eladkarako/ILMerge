namespace System.Compiler
{
    using System;
    using System.Collections;

    internal class ByteArrayKeyComparer : IEqualityComparer, IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            if ((x == null) || (y == null))
            {
                throw new ArgumentNullException();
            }
            byte[] buffer = (byte[]) x;
            byte[] buffer2 = (byte[]) y;
            int length = buffer.Length;
            int num2 = length - buffer2.Length;
            if (num2 != 0)
            {
                return num2;
            }
            for (int i = 0; i < length; i++)
            {
                num2 = buffer[i] - buffer2[i];
                if (num2 != 0)
                {
                    return num2;
                }
            }
            return 0;
        }

        bool IEqualityComparer.Equals(object x, object y)
        {
            if ((x != null) && (y != null))
            {
                return (((IComparer) this).Compare(x, y) == 0);
            }
            return (x == y);
        }

        int IEqualityComparer.GetHashCode(object x)
        {
            byte[] buffer = (byte[]) x;
            int num = 1;
            int index = 0;
            int length = buffer.Length;
            while (index < length)
            {
                num = (num * 0x11) + buffer[index];
                index++;
            }
            return num;
        }
    }
}

