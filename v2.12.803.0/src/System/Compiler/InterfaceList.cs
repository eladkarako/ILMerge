namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class InterfaceList
    {
        private AttributeList[] attributes;
        private int count;
        private Interface[] elements;

        public InterfaceList()
        {
            this.elements = new Interface[8];
        }

        public InterfaceList(int capacity)
        {
            this.elements = new Interface[capacity];
        }

        public InterfaceList(params Interface[] elements)
        {
            if (elements == null)
            {
                elements = new Interface[0];
            }
            this.elements = elements;
            this.count = elements.Length;
        }

        public void Add(Interface element)
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
                Interface[] interfaceArray = new Interface[num4];
                for (int i = 0; i < length; i++)
                {
                    interfaceArray[i] = this.elements[i];
                }
                this.elements = interfaceArray;
                if (this.attributes != null)
                {
                    AttributeList[] listArray = new AttributeList[num4];
                    for (int j = 0; j < length; j++)
                    {
                        listArray[j] = this.attributes[j];
                    }
                    this.attributes = listArray;
                }
            }
            this.elements[index] = element;
        }

        public void AddAttributes(int index, AttributeList attributes)
        {
            if (this.attributes == null)
            {
                this.attributes = new AttributeList[this.elements.Length];
            }
            this.attributes[index] = attributes;
        }

        public AttributeList AttributesFor(int index)
        {
            if (this.attributes == null)
            {
                return null;
            }
            return this.attributes[index];
        }

        public InterfaceList Clone()
        {
            Interface[] elements = this.elements;
            int count = this.count;
            InterfaceList list = new InterfaceList(count) {
                count = count
            };
            Interface[] interfaceArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                interfaceArray2[i] = elements[i];
            }
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int SearchFor(Interface element)
        {
            Interface[] elements = this.elements;
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

        public int Count
        {
            get => 
                this.count;
            set
            {
                this.count = value;
            }
        }

        public Interface this[int index]
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
            private readonly InterfaceList list;
            public Enumerator(InterfaceList list)
            {
                this.index = -1;
                this.list = list;
            }

            public Interface Current =>
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

