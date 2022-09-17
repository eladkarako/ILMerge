namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class TrivialHashtable<T> where T: struct
    {
        private int count;
        private HashEntry<T>[] entries;
        private const int InitialSize = 4;

        public TrivialHashtable()
        {
            this.entries = new HashEntry<T>[4];
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
            this.entries = new HashEntry<T>[num];
        }

        private TrivialHashtable(HashEntry<T>[] entries, int count)
        {
            this.entries = entries;
            this.count = count;
        }

        public TrivialHashtable<T> Clone() => 
            new TrivialHashtable<T>((HashEntry<T>[]) this.entries.Clone(), this.count);

        private void Expand()
        {
            HashEntry<T>[] entries = this.entries;
            int length = entries.Length;
            int num2 = length * 2;
            if (num2 > 0)
            {
                HashEntry<T>[] entryArray2 = new HashEntry<T>[num2];
                int num3 = 0;
                for (int i = 0; i < length; i++)
                {
                    int key = entries[i].Key;
                    if (key > 0)
                    {
                        T local = entries[i].Value;
                        int index = key & (num2 - 1);
                        int num7 = entryArray2[index].Key;
                        while (true)
                        {
                            if (num7 == 0)
                            {
                                entryArray2[index].Value = local;
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

        public bool TryGetValue(int key, out T result)
        {
            if (key <= 0)
            {
                throw new ArgumentException(ExceptionStrings.KeyNeedsToBeGreaterThanZero, "key");
            }
            HashEntry<T>[] entries = this.entries;
            int length = entries.Length;
            int index = key & (length - 1);
            int num3 = entries[index].Key;
            while (true)
            {
                if (num3 == key)
                {
                    result = entries[index].Value;
                    return true;
                }
                if (num3 == 0)
                {
                    break;
                }
                index++;
                if (index >= length)
                {
                    index = 0;
                }
                num3 = entries[index].Key;
            }
            result = default(T);
            return false;
        }

        public int Count =>
            this.count;

        public T this[int key]
        {
            get
            {
                T local;
                this.TryGetValue(key, out local);
                return local;
            }
            set
            {
                if (key <= 0)
                {
                    throw new ArgumentException(ExceptionStrings.KeyNeedsToBeGreaterThanZero, "key");
                }
                HashEntry<T>[] entries = this.entries;
                int length = entries.Length;
                int index = key & (length - 1);
                int num3 = entries[index].Key;
            Label_0035:
                if ((num3 == key) || (num3 == 0))
                {
                    entries[index].Value = value;
                    if (num3 == 0)
                    {
                        entries[index].Key = key;
                        if (++this.count > (length / 2))
                        {
                            this.Expand();
                        }
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

        [StructLayout(LayoutKind.Sequential)]
        private struct HashEntry
        {
            public int Key;
            public T Value;
        }
    }
}

