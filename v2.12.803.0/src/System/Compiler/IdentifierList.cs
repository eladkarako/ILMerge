namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class IdentifierList
    {
        private int count;
        private Identifier[] elements;

        public IdentifierList()
        {
            this.elements = new Identifier[8];
        }

        public IdentifierList(int capacity)
        {
            this.elements = new Identifier[capacity];
        }

        public void Add(Identifier element)
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
                Identifier[] identifierArray = new Identifier[num4];
                for (int i = 0; i < length; i++)
                {
                    identifierArray[i] = this.elements[i];
                }
                this.elements = identifierArray;
            }
            this.elements[index] = element;
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

        public Identifier this[int index]
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
            private readonly IdentifierList list;
            public Enumerator(IdentifierList list)
            {
                this.index = -1;
                this.list = list;
            }

            public Identifier Current =>
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

