namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class EnsuresList
    {
        private int count;
        private Ensures[] elements;

        public EnsuresList()
        {
            this.elements = new Ensures[2];
        }

        public EnsuresList(int capacity)
        {
            this.elements = new Ensures[capacity];
        }

        public void Add(Ensures element)
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
                Ensures[] ensuresArray = new Ensures[num4];
                for (int i = 0; i < length; i++)
                {
                    ensuresArray[i] = this.elements[i];
                }
                this.elements = ensuresArray;
            }
            this.elements[index] = element;
        }

        public EnsuresList Clone()
        {
            Ensures[] elements = this.elements;
            int count = this.count;
            EnsuresList list = new EnsuresList(count) {
                count = count
            };
            Ensures[] ensuresArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                ensuresArray2[i] = elements[i];
            }
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public Ensures this[int index]
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
            private readonly EnsuresList list;
            public Enumerator(EnsuresList list)
            {
                this.index = -1;
                this.list = list;
            }

            public Ensures Current =>
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

