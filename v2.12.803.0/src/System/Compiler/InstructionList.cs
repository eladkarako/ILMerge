namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class InstructionList
    {
        private int count;
        private Instruction[] elements;

        public InstructionList()
        {
            this.elements = new Instruction[0x20];
        }

        public InstructionList(int capacity)
        {
            this.elements = new Instruction[capacity];
        }

        public void Add(Instruction element)
        {
            int length = this.elements.Length;
            int index = this.count++;
            if (index == length)
            {
                int num4 = length * 2;
                if (num4 < 0x20)
                {
                    num4 = 0x20;
                }
                Instruction[] instructionArray = new Instruction[num4];
                for (int i = 0; i < length; i++)
                {
                    instructionArray[i] = this.elements[i];
                }
                this.elements = instructionArray;
            }
            this.elements[index] = element;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count
        {
            get => 
                this.count;
            set
            {
                this.count = value;
            }
        }

        public Instruction this[int index]
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
            private readonly InstructionList list;
            public Enumerator(InstructionList list)
            {
                this.index = -1;
                this.list = list;
            }

            public Instruction Current =>
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

