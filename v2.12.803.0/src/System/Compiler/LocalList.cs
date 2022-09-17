namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class LocalList
    {
        private int count;
        private Local[] elements;

        public LocalList()
        {
            this.elements = new Local[8];
        }

        public LocalList(int capacity)
        {
            this.elements = new Local[capacity];
        }

        public void Add(Local element)
        {
            int length = this.elements.Length;
            int index = this.count++;
            if (index == length)
            {
                int num4 = length * 2;
                if (num4 < 8)
                {
                    num4 = 8;
                }
                Local[] localArray = new Local[num4];
                for (int i = 0; i < length; i++)
                {
                    localArray[i] = this.elements[i];
                }
                this.elements = localArray;
            }
            this.elements[index] = element;
        }

        public int Count =>
            this.count;

        public Local this[int index]
        {
            get => 
                this.elements[index];
            set
            {
                this.elements[index] = value;
            }
        }

        [Obsolete("Use Count property instead.")]
        public int Length =>
            this.count;

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator
        {
            private int index;
            private readonly LocalList list;
            public Enumerator(LocalList list)
            {
                this.index = -1;
                this.list = list;
            }

            public Local Current =>
                this.list[this.index];
            public bool MoveNext() => 
                (++this.index < this.list.count);

            public void Reset()
            {
                this.index = -1;
            }
        }
    }
}

