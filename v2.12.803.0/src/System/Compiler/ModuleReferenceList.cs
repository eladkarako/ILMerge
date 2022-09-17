namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class ModuleReferenceList
    {
        private int count;
        private ModuleReference[] elements;

        public ModuleReferenceList()
        {
            this.elements = new ModuleReference[4];
        }

        public ModuleReferenceList(int capacity)
        {
            this.elements = new ModuleReference[capacity];
        }

        public ModuleReferenceList(params ModuleReference[] elements)
        {
            if (elements == null)
            {
                elements = new ModuleReference[0];
            }
            this.elements = elements;
            this.count = elements.Length;
        }

        public void Add(ModuleReference element)
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
                ModuleReference[] referenceArray = new ModuleReference[num4];
                for (int i = 0; i < length; i++)
                {
                    referenceArray[i] = this.elements[i];
                }
                this.elements = referenceArray;
            }
            this.elements[index] = element;
        }

        public ModuleReferenceList Clone()
        {
            ModuleReference[] elements = this.elements;
            int count = this.count;
            ModuleReferenceList list = new ModuleReferenceList(count) {
                count = count
            };
            ModuleReference[] referenceArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                referenceArray2[i] = elements[i];
            }
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public ModuleReference this[int index]
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
            private readonly ModuleReferenceList list;
            public Enumerator(ModuleReferenceList list)
            {
                this.index = -1;
                this.list = list;
            }

            public ModuleReference Current =>
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

