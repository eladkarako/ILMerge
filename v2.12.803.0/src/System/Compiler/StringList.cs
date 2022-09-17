namespace System.Compiler
{
    using System;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class StringList
    {
        private int count;
        private string[] elements;

        public StringList()
        {
            this.elements = new string[4];
            this.elements = new string[4];
        }

        public StringList(int capacity)
        {
            this.elements = new string[4];
            this.elements = new string[capacity];
        }

        public StringList(params string[] elements)
        {
            this.elements = new string[4];
            if (elements == null)
            {
                elements = new string[0];
            }
            this.elements = elements;
            this.count = elements.Length;
        }

        public StringList(StringCollection stringCollection)
        {
            this.elements = new string[4];
            int num2 = this.count = (stringCollection == null) ? 0 : stringCollection.Count;
            string[] array = this.elements = new string[num2];
            if (num2 > 0)
            {
                stringCollection.CopyTo(array, 0);
            }
        }

        public void Add(string element)
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
                string[] strArray = new string[num4];
                for (int i = 0; i < length; i++)
                {
                    strArray[i] = this.elements[i];
                }
                this.elements = strArray;
            }
            this.elements[index] = element;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public string this[int index]
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
            private readonly StringList list;
            public Enumerator(StringList list)
            {
                this.index = -1;
                this.list = list;
            }

            public string Current =>
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

