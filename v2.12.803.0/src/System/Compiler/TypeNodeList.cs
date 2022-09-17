namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class TypeNodeList
    {
        private int count;
        private TypeNode[] elements;

        public TypeNodeList()
        {
            this.elements = new TypeNode[0x20];
        }

        public TypeNodeList(int capacity)
        {
            this.elements = new TypeNode[capacity];
        }

        public TypeNodeList(params TypeNode[] elements)
        {
            if (elements == null)
            {
                elements = new TypeNode[0];
            }
            this.elements = elements;
            this.count = elements.Length;
        }

        public void Add(TypeNode element)
        {
            TypeNode[] elements = this.elements;
            int length = elements.Length;
            int index = this.count++;
            if (index == length)
            {
                int num4 = length * 2;
                if (num4 < 0x20)
                {
                    num4 = 0x20;
                }
                TypeNode[] nodeArray2 = new TypeNode[num4];
                for (int i = 0; i < length; i++)
                {
                    nodeArray2[i] = elements[i];
                }
                this.elements = nodeArray2;
            }
            this.elements[index] = element;
        }

        public TypeNodeList Clone()
        {
            TypeNode[] elements = this.elements;
            int count = this.count;
            TypeNodeList list = new TypeNodeList(count) {
                count = count
            };
            TypeNode[] nodeArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                nodeArray2[i] = elements[i];
            }
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public void Insert(TypeNode element, int index)
        {
            TypeNode[] elements = this.elements;
            int length = this.elements.Length;
            int num3 = this.count++;
            if (index >= num3)
            {
                throw new IndexOutOfRangeException();
            }
            if (num3 == length)
            {
                int num4 = length * 2;
                if (num4 < 0x20)
                {
                    num4 = 0x20;
                }
                TypeNode[] nodeArray2 = new TypeNode[num4];
                for (int i = 0; i < index; i++)
                {
                    nodeArray2[i] = elements[i];
                }
                nodeArray2[index] = element;
                for (int j = index; j < length; j++)
                {
                    nodeArray2[j + 1] = elements[j];
                }
            }
            else
            {
                for (int k = index; k < num3; k++)
                {
                    TypeNode node = elements[k];
                    elements[k] = element;
                    element = node;
                }
                elements[num3] = element;
            }
        }

        public int SearchFor(TypeNode element)
        {
            TypeNode[] elements = this.elements;
            int index = 0;
            int count = this.count;
            while (index < count)
            {
                if (elements[index] == element)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        public int Count =>
            this.count;

        public TypeNode this[int index]
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
            private readonly TypeNodeList list;
            public Enumerator(TypeNodeList list)
            {
                this.index = -1;
                this.list = list;
            }

            public TypeNode Current =>
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

