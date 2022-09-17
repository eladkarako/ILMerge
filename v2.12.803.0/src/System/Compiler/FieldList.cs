namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class FieldList
    {
        private int count;
        private Field[] elements;

        public FieldList()
        {
            this.elements = new Field[8];
        }

        public FieldList(int capacity)
        {
            this.elements = new Field[capacity];
        }

        public FieldList(params Field[] elements)
        {
            if (elements == null)
            {
                elements = new Field[0];
            }
            this.elements = elements;
            this.count = elements.Length;
        }

        public void Add(Field element)
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
                Field[] fieldArray = new Field[num4];
                for (int i = 0; i < length; i++)
                {
                    fieldArray[i] = this.elements[i];
                }
                this.elements = fieldArray;
            }
            this.elements[index] = element;
        }

        public FieldList Clone()
        {
            Field[] elements = this.elements;
            int count = this.count;
            FieldList list = new FieldList(count) {
                count = count
            };
            Field[] fieldArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                fieldArray2[i] = elements[i];
            }
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public Field this[int index]
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
            private readonly FieldList list;
            public Enumerator(FieldList list)
            {
                this.index = -1;
                this.list = list;
            }

            public Field Current =>
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

