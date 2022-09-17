namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class AttributeList
    {
        private int count;
        private AttributeNode[] elements;

        public AttributeList()
        {
            this.elements = new AttributeNode[4];
        }

        public AttributeList(int capacity)
        {
            this.elements = new AttributeNode[capacity];
        }

        public AttributeList(params AttributeNode[] elements)
        {
            if (elements == null)
            {
                elements = new AttributeNode[0];
            }
            this.elements = elements;
            this.count = elements.Length;
        }

        public void Add(AttributeNode element)
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
                AttributeNode[] nodeArray = new AttributeNode[num4];
                for (int i = 0; i < length; i++)
                {
                    nodeArray[i] = this.elements[i];
                }
                this.elements = nodeArray;
            }
            this.elements[index] = element;
        }

        public AttributeList Clone()
        {
            AttributeNode[] elements = this.elements;
            int count = this.count;
            AttributeList list = new AttributeList(count) {
                count = count
            };
            AttributeNode[] nodeArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                nodeArray2[i] = elements[i];
            }
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public AttributeNode this[int index]
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
            private readonly AttributeList list;
            public Enumerator(AttributeList list)
            {
                this.index = -1;
                this.list = list;
            }

            public AttributeNode Current =>
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

