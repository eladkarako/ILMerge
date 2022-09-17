namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class TrivialHashtableUsingWeakReferences
    {
        private int count;
        private HashEntry[] entries;

        public TrivialHashtableUsingWeakReferences()
        {
            this.entries = new HashEntry[0x10];
        }

        public TrivialHashtableUsingWeakReferences(int expectedEntries)
        {
            int num = 0x10;
            expectedEntries = expectedEntries << 1;
            while ((num < expectedEntries) && (num > 0))
            {
                num = num << 1;
            }
            if (num < 0)
            {
                num = 0x10;
            }
            this.entries = new HashEntry[num];
        }

        private TrivialHashtableUsingWeakReferences(HashEntry[] entries, int count)
        {
            this.entries = entries;
            this.count = count;
        }

        public TrivialHashtableUsingWeakReferences Clone() => 
            new TrivialHashtableUsingWeakReferences((HashEntry[]) this.entries.Clone(), this.count);

        private void Contract()
        {
            HashEntry[] entries = this.entries;
            int length = entries.Length;
            int num2 = length / 2;
            if (num2 >= 0x10)
            {
                HashEntry[] entryArray2 = new HashEntry[num2];
                int num3 = 0;
                for (int i = 0; i < length; i++)
                {
                    int key = entries[i].Key;
                    if (key > 0)
                    {
                        WeakReference reference = entries[i].Value;
                        if ((reference != null) && reference.IsAlive)
                        {
                            int index = key & (num2 - 1);
                            int num7 = entryArray2[index].Key;
                            while (true)
                            {
                                if (num7 == 0)
                                {
                                    entryArray2[index].Value = reference;
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
                }
                this.entries = entryArray2;
                this.count = num3;
            }
        }

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
                        WeakReference reference = entries[i].Value;
                        if ((reference != null) && reference.IsAlive)
                        {
                            int index = key & (num2 - 1);
                            int num7 = entryArray2[index].Key;
                            while (true)
                            {
                                if (num7 == 0)
                                {
                                    entryArray2[index].Value = reference;
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
                }
                this.entries = entryArray2;
                this.count = num3;
            }
        }

        private void WeedOutCollectedEntries()
        {
            HashEntry[] entries = this.entries;
            int length = entries.Length;
            HashEntry[] entryArray2 = new HashEntry[length];
            int num2 = 0;
            for (int i = 0; i < length; i++)
            {
                int key = entries[i].Key;
                if (key > 0)
                {
                    WeakReference reference = entries[i].Value;
                    if ((reference != null) && reference.IsAlive)
                    {
                        int index = key & (length - 1);
                        int num6 = entryArray2[index].Key;
                        while (true)
                        {
                            if (num6 == 0)
                            {
                                entryArray2[index].Value = reference;
                                entryArray2[index].Key = key;
                                num2++;
                                break;
                            }
                            index++;
                            if (index >= length)
                            {
                                index = 0;
                            }
                            num6 = entryArray2[index].Key;
                        }
                    }
                }
            }
            this.entries = entryArray2;
            this.count = num2;
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
                object target = null;
                while (true)
                {
                    if (num3 == key)
                    {
                        WeakReference reference = entries[index].Value;
                        if (reference != null)
                        {
                            target = reference.Target;
                            if (target != null)
                            {
                                return target;
                            }
                            this.WeedOutCollectedEntries();
                            while ((this.count < (length / 4)) && (length > 0x10))
                            {
                                this.Contract();
                                length = this.entries.Length;
                            }
                        }
                        return null;
                    }
                    if (num3 == 0)
                    {
                        return target;
                    }
                    index++;
                    if (index >= length)
                    {
                        index = 0;
                    }
                    num3 = entries[index].Key;
                }
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
                    if (value == null)
                    {
                        entries[index].Value = null;
                    }
                    else
                    {
                        entries[index].Value = new WeakReference(value);
                    }
                    if (num3 == 0)
                    {
                        if (value != null)
                        {
                            entries[index].Key = key;
                            if (++this.count > (length / 2))
                            {
                                this.Expand();
                                while ((this.count < (length / 4)) && (length > 0x10))
                                {
                                    this.Contract();
                                    length = this.entries.Length;
                                }
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

        [StructLayout(LayoutKind.Sequential)]
        private struct HashEntry
        {
            public int Key;
            public WeakReference Value;
        }
    }
}

