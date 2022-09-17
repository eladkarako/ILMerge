namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class NamespaceList
    {
        private int count;
        private Namespace[] elements;

        public NamespaceList()
        {
            this.elements = new Namespace[4];
        }

        public NamespaceList(int capacity)
        {
            this.elements = new Namespace[capacity];
        }

        public void Add(Namespace element)
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
                Namespace[] namespaceArray = new Namespace[num4];
                for (int i = 0; i < length; i++)
                {
                    namespaceArray[i] = this.elements[i];
                }
                this.elements = namespaceArray;
            }
            this.elements[index] = element;
        }

        public NamespaceList Clone()
        {
            Namespace[] elements = this.elements;
            int count = this.count;
            NamespaceList list = new NamespaceList(count) {
                count = count
            };
            Namespace[] namespaceArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                namespaceArray2[i] = elements[i];
            }
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public Namespace this[int index]
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
            private readonly NamespaceList list;
            public Enumerator(NamespaceList list)
            {
                this.index = -1;
                this.list = list;
            }

            public Namespace Current =>
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

