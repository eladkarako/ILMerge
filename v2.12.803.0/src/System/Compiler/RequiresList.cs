namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class RequiresList
    {
        private int count;
        private Requires[] elements;

        public RequiresList()
        {
            this.elements = new Requires[2];
        }

        public RequiresList(int capacity)
        {
            this.elements = new Requires[capacity];
        }

        public void Add(Requires element)
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
                Requires[] requiresArray = new Requires[num4];
                for (int i = 0; i < length; i++)
                {
                    requiresArray[i] = this.elements[i];
                }
                this.elements = requiresArray;
            }
            this.elements[index] = element;
        }

        public RequiresList Clone()
        {
            Requires[] elements = this.elements;
            int count = this.count;
            RequiresList list = new RequiresList(count) {
                count = count
            };
            Requires[] requiresArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                requiresArray2[i] = elements[i];
            }
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public Requires this[int index]
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
            private readonly RequiresList list;
            public Enumerator(RequiresList list)
            {
                this.index = -1;
                this.list = list;
            }

            public Requires Current =>
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

