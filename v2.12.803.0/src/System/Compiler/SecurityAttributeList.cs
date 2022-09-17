namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class SecurityAttributeList
    {
        private int count;
        private SecurityAttribute[] elements;

        public SecurityAttributeList()
        {
            this.elements = new SecurityAttribute[8];
        }

        public SecurityAttributeList(int capacity)
        {
            this.elements = new SecurityAttribute[capacity];
        }

        public void Add(SecurityAttribute element)
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
                SecurityAttribute[] attributeArray = new SecurityAttribute[num4];
                for (int i = 0; i < length; i++)
                {
                    attributeArray[i] = this.elements[i];
                }
                this.elements = attributeArray;
            }
            this.elements[index] = element;
        }

        public SecurityAttributeList Clone()
        {
            SecurityAttribute[] elements = this.elements;
            int count = this.count;
            SecurityAttributeList list = new SecurityAttributeList(count) {
                count = count
            };
            SecurityAttribute[] attributeArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                attributeArray2[i] = elements[i];
            }
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public SecurityAttribute this[int index]
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
            private readonly SecurityAttributeList list;
            public Enumerator(SecurityAttributeList list)
            {
                this.index = -1;
                this.list = list;
            }

            public SecurityAttribute Current =>
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

