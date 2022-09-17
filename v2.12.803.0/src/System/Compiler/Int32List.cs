namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class Int32List
    {
        private int count;
        private int[] elements;

        public Int32List()
        {
            this.elements = new int[8];
        }

        public Int32List(int capacity)
        {
            this.elements = new int[capacity];
        }

        public Int32List(params int[] elements)
        {
            if (elements == null)
            {
                elements = new int[0];
            }
            this.elements = elements;
            this.count = elements.Length;
        }

        public void Add(int element)
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
                int[] numArray = new int[num4];
                for (int i = 0; i < length; i++)
                {
                    numArray[i] = this.elements[i];
                }
                this.elements = numArray;
            }
            this.elements[index] = element;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public int this[int index]
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
            private readonly Int32List list;
            public Enumerator(Int32List list)
            {
                this.index = -1;
                this.list = list;
            }

            public int Current =>
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

