namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class AssemblyNodeList
    {
        private int count;
        private AssemblyNode[] elements;

        public AssemblyNodeList()
        {
            this.elements = new AssemblyNode[4];
        }

        public AssemblyNodeList(int capacity)
        {
            this.elements = new AssemblyNode[capacity];
        }

        public AssemblyNodeList(params AssemblyNode[] elements)
        {
            if (elements == null)
            {
                elements = new AssemblyNode[0];
            }
            this.elements = elements;
            this.count = elements.Length;
        }

        public void Add(AssemblyNode element)
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
                AssemblyNode[] nodeArray = new AssemblyNode[num4];
                for (int i = 0; i < length; i++)
                {
                    nodeArray[i] = this.elements[i];
                }
                this.elements = nodeArray;
            }
            this.elements[index] = element;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public AssemblyNode this[int index]
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
            private readonly AssemblyNodeList list;
            public Enumerator(AssemblyNodeList list)
            {
                this.index = -1;
                this.list = list;
            }

            public AssemblyNode Current =>
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

