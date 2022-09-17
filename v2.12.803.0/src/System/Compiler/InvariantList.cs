namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class InvariantList
    {
        private int count;
        private Invariant[] elements;

        public InvariantList()
        {
            this.elements = new Invariant[8];
        }

        public InvariantList(int n)
        {
            this.elements = new Invariant[n];
        }

        public InvariantList(params Invariant[] elements)
        {
            if (elements == null)
            {
                elements = new Invariant[0];
            }
            this.elements = elements;
            this.count = elements.Length;
        }

        public void Add(Invariant element)
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
                Invariant[] invariantArray = new Invariant[num4];
                for (int i = 0; i < length; i++)
                {
                    invariantArray[i] = this.elements[i];
                }
                this.elements = invariantArray;
            }
            this.elements[index] = element;
        }

        public InvariantList Clone()
        {
            Invariant[] elements = this.elements;
            int count = this.count;
            InvariantList list = new InvariantList(count) {
                count = count
            };
            Invariant[] invariantArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                invariantArray2[i] = elements[i];
            }
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count
        {
            get => 
                this.count;
            set
            {
                this.count = value;
            }
        }

        public Invariant this[int index]
        {
            get => 
                this.elements[index];
            set
            {
                this.elements[index] = value;
            }
        }

        [Obsolete("Use Count property instead.")]
        public int Length
        {
            get => 
                this.count;
            set
            {
                this.count = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator
        {
            private int index;
            private readonly InvariantList list;
            public Enumerator(InvariantList list)
            {
                this.index = -1;
                this.list = list;
            }

            public Invariant Current =>
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

