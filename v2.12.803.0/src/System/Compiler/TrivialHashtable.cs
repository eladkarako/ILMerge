namespace System.Compiler
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class TrivialHashtable
    {
        private int count;
        private HashEntry[] entries;
        private const int InitialSize = 4;

        public TrivialHashtable()
        {
            this.entries = new HashEntry[4];
        }

        public TrivialHashtable(int expectedEntries)
        {
            int num = 0x10;
            expectedEntries = expectedEntries << 1;
            while ((num < expectedEntries) && (num > 0))
            {
                num = num << 1;
            }
            if (num < 0)
            {
                num = 4;
            }
            this.entries = new HashEntry[num];
        }

        private TrivialHashtable(HashEntry[] entries, int count)
        {
            this.entries = entries;
            this.count = count;
        }

        public TrivialHashtable Clone() => 
            new TrivialHashtable((HashEntry[]) this.entries.Clone(), this.count);

        private void Expand()
        {
            HashEntry[] entries = this.entries;
            int length = entries.Length;
            int num2 = length * 2;
            if (num2 > 0)
            {
                HashEntry[] entryArray2 = new HashEntry[num2];
                int num3 = 0;
                for (int i = 0; i < length; i++)
                {
                    int key = entries[i].Key;
                    if (key > 0)
                    {
                        object obj2 = entries[i].Value;
                        int index = key & (num2 - 1);
                        int num7 = entryArray2[index].Key;
                        while (true)
                        {
                            if (num7 == 0)
                            {
                                entryArray2[index].Value = obj2;
                                entryArray2[index].Key = key;
                                num3++;
                                break;
                            }
                            index++;
                            if (index >= num2)
                            {
                                index = 0;
                            }
                            num7 = entryArray2[index].Key;
                        }
                    }
                }
                this.entries = entryArray2;
                this.count = num3;
            }
        }

        public int Count =>
            this.count;

        public object this[int key]
        {
            get
            {
                if (key <= 0)
                {
                    throw new ArgumentException(ExceptionStrings.KeyNeedsToBeGreaterThanZero, "key");
                }
                HashEntry[] entries = this.entries;
                int length = entries.Length;
                int index = key & (length - 1);
                int num3 = entries[index].Key;
                object obj2 = null;
                while (num3 != key)
                {
                    if (num3 == 0)
                    {
                        return obj2;
                    }
                    index++;
                    if (index >= length)
                    {
                        index = 0;
                    }
                    num3 = entries[index].Key;
                }
                return entries[index].Value;
            }
            set
            {
                if (key <= 0)
                {
                    throw new ArgumentException(ExceptionStrings.KeyNeedsToBeGreaterThanZero, "key");
                }
                HashEntry[] entries = this.entries;
                int length = entries.Length;
                int index = key & (length - 1);
                int num3 = entries[index].Key;
            Label_0035:
                if ((num3 == key) || (num3 == 0))
                {
                    entries[index].Value = value;
                    if (num3 == 0)
                    {
                        if (value != null)
                        {
                            entries[index].Key = key;
                            if (++this.count > (length / 2))
                            {
                                this.Expand();
                            }
                        }
                        return;
                    }
                    if (value == null)
                    {
                        entries[index].Key = -1;
                    }
                }
                else
                {
                    index++;
                    if (index >= length)
                    {
                        index = 0;
                    }
                    num3 = entries[index].Key;
                    goto Label_0035;
                }
            }
        }

        public IEnumerable Values
        {
            get
            {
                for (int i = 0; i < this.entries.Length; i++)
                {
                    if (this.entries[i].Key != 0)
                    {
                        yield return this.entries[i].Value;
                    }
                }
            }
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct HashEntry
        {
            public int Key;
            public object Value;
        }
    }
}

