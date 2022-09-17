namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class Win32ResourceList
    {
        private int count;
        private Win32Resource[] elements;

        public Win32ResourceList()
        {
            this.elements = new Win32Resource[4];
        }

        public Win32ResourceList(int capacity)
        {
            this.elements = new Win32Resource[capacity];
        }

        public void Add(Win32Resource element)
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
                Win32Resource[] resourceArray = new Win32Resource[num4];
                for (int i = 0; i < length; i++)
                {
                    resourceArray[i] = this.elements[i];
                }
                this.elements = resourceArray;
            }
            this.elements[index] = element;
        }

        public Win32ResourceList Clone()
        {
            Win32Resource[] elements = this.elements;
            int count = this.count;
            Win32ResourceList list = new Win32ResourceList(count) {
                count = count
            };
            Win32Resource[] resourceArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                resourceArray2[i] = elements[i];
            }
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public Win32Resource this[int index]
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
            private readonly Win32ResourceList list;
            public Enumerator(Win32ResourceList list)
            {
                this.index = -1;
                this.list = list;
            }

            public Win32Resource Current =>
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

