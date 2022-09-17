namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class FilterList
    {
        private int count;
        private System.Compiler.Filter[] elements;

        public FilterList()
        {
            this.elements = new System.Compiler.Filter[4];
        }

        public FilterList(int capacity)
        {
            this.elements = new System.Compiler.Filter[capacity];
        }

        public void Add(System.Compiler.Filter element)
        {
            int length = this.elements.Length;
            int index = this.count++;
            if (index == length)
            {
                int num4 = length * 2;
                if (num4 < 4)
                {
                    num4 = 4;
                }
                System.Compiler.Filter[] filterArray = new System.Compiler.Filter[num4];
                for (int i = 0; i < length; i++)
                {
                    filterArray[i] = this.elements[i];
                }
                this.elements = filterArray;
            }
            this.elements[index] = element;
        }

        public FilterList Clone()
        {
            System.Compiler.Filter[] elements = this.elements;
            int count = this.count;
            FilterList list = new FilterList(count) {
                count = count
            };
            System.Compiler.Filter[] filterArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                filterArray2[i] = elements[i];
            }
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public System.Compiler.Filter this[int index]
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
            private readonly FilterList list;
            public Enumerator(FilterList list)
            {
                this.index = -1;
                this.list = list;
            }

            public System.Compiler.Filter Current =>
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

