namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class ModuleList
    {
        private int count;
        private System.Compiler.Module[] elements;

        public ModuleList()
        {
            this.elements = new System.Compiler.Module[4];
        }

        public ModuleList(int capacity)
        {
            this.elements = new System.Compiler.Module[capacity];
        }

        public void Add(System.Compiler.Module element)
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
                System.Compiler.Module[] moduleArray = new System.Compiler.Module[num4];
                for (int i = 0; i < length; i++)
                {
                    moduleArray[i] = this.elements[i];
                }
                this.elements = moduleArray;
            }
            this.elements[index] = element;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public System.Compiler.Module this[int index]
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
            private readonly ModuleList list;
            public Enumerator(ModuleList list)
            {
                this.index = -1;
                this.list = list;
            }

            public System.Compiler.Module Current =>
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

