namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class SourceChangeList
    {
        private int count;
        private SourceChange[] elements;

        public SourceChangeList()
        {
            this.elements = new SourceChange[4];
        }

        public SourceChangeList(int capacity)
        {
            this.elements = new SourceChange[capacity];
        }

        public SourceChangeList(params SourceChange[] elements)
        {
            if (elements == null)
            {
                elements = new SourceChange[0];
            }
            this.elements = elements;
            this.count = elements.Length;
        }

        public void Add(SourceChange element)
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
                SourceChange[] changeArray = new SourceChange[num4];
                for (int i = 0; i < length; i++)
                {
                    changeArray[i] = this.elements[i];
                }
                this.elements = changeArray;
            }
            this.elements[index] = element;
        }

        public SourceChangeList Clone()
        {
            SourceChange[] elements = this.elements;
            int count = this.count;
            SourceChangeList list = new SourceChangeList(count) {
                count = count
            };
            SourceChange[] changeArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                changeArray2[i] = elements[i];
            }
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public SourceChange this[int index]
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
            private readonly SourceChangeList list;
            public Enumerator(SourceChangeList list)
            {
                this.index = -1;
                this.list = list;
            }

            public SourceChange Current =>
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

