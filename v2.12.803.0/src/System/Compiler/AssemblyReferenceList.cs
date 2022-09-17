namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class AssemblyReferenceList
    {
        private int count;
        private AssemblyReference[] elements;

        public AssemblyReferenceList()
        {
            this.elements = new AssemblyReference[4];
        }

        public AssemblyReferenceList(int capacity)
        {
            this.elements = new AssemblyReference[capacity];
        }

        public void Add(AssemblyReference element)
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
                AssemblyReference[] referenceArray = new AssemblyReference[num4];
                for (int i = 0; i < length; i++)
                {
                    referenceArray[i] = this.elements[i];
                }
                this.elements = referenceArray;
            }
            this.elements[index] = element;
        }

        public AssemblyReferenceList Clone()
        {
            AssemblyReference[] elements = this.elements;
            int count = this.count;
            AssemblyReferenceList list = new AssemblyReferenceList(count) {
                count = count
            };
            AssemblyReference[] referenceArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                referenceArray2[i] = elements[i];
            }
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public AssemblyReference this[int index]
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
            private readonly AssemblyReferenceList list;
            public Enumerator(AssemblyReferenceList list)
            {
                this.index = -1;
                this.list = list;
            }

            public AssemblyReference Current =>
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

