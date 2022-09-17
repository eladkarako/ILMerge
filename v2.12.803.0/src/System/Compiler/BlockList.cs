namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class BlockList
    {
        private int count;
        private Block[] elements;

        public BlockList()
        {
            this.elements = new Block[4];
        }

        public BlockList(int n)
        {
            this.elements = new Block[n];
        }

        public void Add(Block element)
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
                Block[] blockArray = new Block[num4];
                for (int i = 0; i < length; i++)
                {
                    blockArray[i] = this.elements[i];
                }
                this.elements = blockArray;
            }
            this.elements[index] = element;
        }

        public BlockList Clone()
        {
            Block[] elements = this.elements;
            int count = this.count;
            BlockList list = new BlockList(count) {
                count = count
            };
            Block[] blockArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                blockArray2[i] = elements[i];
            }
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public Block this[int index]
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
            private readonly BlockList list;
            public Enumerator(BlockList list)
            {
                this.index = -1;
                this.list = list;
            }

            public Block Current =>
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

