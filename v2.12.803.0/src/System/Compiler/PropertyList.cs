namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class PropertyList
    {
        private int count;
        private Property[] elements;

        public PropertyList()
        {
            this.elements = new Property[8];
        }

        public PropertyList(int capacity)
        {
            this.elements = new Property[capacity];
        }

        public void Add(Property element)
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
                Property[] propertyArray = new Property[num4];
                for (int i = 0; i < length; i++)
                {
                    propertyArray[i] = this.elements[i];
                }
                this.elements = propertyArray;
            }
            this.elements[index] = element;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public Property this[int index]
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
            private readonly PropertyList list;
            public Enumerator(PropertyList list)
            {
                this.index = -1;
                this.list = list;
            }

            public Property Current =>
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

