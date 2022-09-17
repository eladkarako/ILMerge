namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class MethodContractElementList
    {
        private int count;
        private MethodContractElement[] elements;

        public MethodContractElementList()
        {
            this.elements = new MethodContractElement[2];
        }

        public MethodContractElementList(int capacity)
        {
            this.elements = new MethodContractElement[capacity];
        }

        public void Add(MethodContractElement element)
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
                MethodContractElement[] elementArray = new MethodContractElement[num4];
                for (int i = 0; i < length; i++)
                {
                    elementArray[i] = this.elements[i];
                }
                this.elements = elementArray;
            }
            this.elements[index] = element;
        }

        public MethodContractElementList Clone()
        {
            MethodContractElement[] elements = this.elements;
            int count = this.count;
            MethodContractElementList list = new MethodContractElementList(count) {
                count = count
            };
            MethodContractElement[] elementArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                elementArray2[i] = elements[i];
            }
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public MethodContractElement this[int index]
        {
            get => 
                this.elements[index];
            set
            {
                this.elements[index] = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator
        {
            private int index;
            private readonly MethodContractElementList list;
            public Enumerator(MethodContractElementList list)
            {
                this.index = -1;
                this.list = list;
            }

            public MethodContractElement Current =>
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

