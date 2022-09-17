namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class MethodList
    {
        private int count;
        private Method[] elements;

        public MethodList()
        {
            this.elements = new Method[8];
        }

        public MethodList(int capacity)
        {
            this.elements = new Method[capacity];
        }

        public MethodList(params Method[] elements)
        {
            if (elements == null)
            {
                elements = new Method[0];
            }
            this.elements = elements;
            this.count = elements.Length;
        }

        public void Add(Method element)
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
                Method[] methodArray = new Method[num4];
                for (int i = 0; i < length; i++)
                {
                    methodArray[i] = this.elements[i];
                }
                this.elements = methodArray;
            }
            this.elements[index] = element;
        }

        public MethodList Clone()
        {
            Method[] elements = this.elements;
            int count = this.count;
            MethodList list = new MethodList(count) {
                count = count
            };
            Method[] methodArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                methodArray2[i] = elements[i];
            }
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public Method this[int index]
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
            private readonly MethodList list;
            public Enumerator(MethodList list)
            {
                this.index = -1;
                this.list = list;
            }

            public Method Current =>
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

