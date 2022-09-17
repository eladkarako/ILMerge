namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class NodeList
    {
        private int count;
        private Node[] elements;

        public NodeList()
        {
            this.elements = new Node[4];
        }

        public NodeList(int capacity)
        {
            this.elements = new Node[capacity];
        }

        public NodeList(params Node[] elements)
        {
            if (elements == null)
            {
                elements = new Node[0];
            }
            this.elements = elements;
            this.count = elements.Length;
        }

        public void Add(Node element)
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
                Node[] nodeArray = new Node[num4];
                for (int i = 0; i < length; i++)
                {
                    nodeArray[i] = this.elements[i];
                }
                this.elements = nodeArray;
            }
            this.elements[index] = element;
        }

        public NodeList Clone()
        {
            Node[] elements = this.elements;
            int count = this.count;
            NodeList list = new NodeList(count) {
                count = count
            };
            Node[] nodeArray2 = list.elements;
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

        public Node this[int index]
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
            private readonly NodeList list;
            public Enumerator(NodeList list)
            {
                this.index = -1;
                this.list = list;
            }

            public Node Current =>
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

