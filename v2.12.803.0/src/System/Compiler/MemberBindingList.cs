namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class MemberBindingList
    {
        private int count;
        private MemberBinding[] elements;

        public MemberBindingList()
        {
            this.elements = new MemberBinding[8];
        }

        public MemberBindingList(int capacity)
        {
            this.elements = new MemberBinding[capacity];
        }

        public void Add(MemberBinding element)
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
                MemberBinding[] bindingArray = new MemberBinding[num4];
                for (int i = 0; i < length; i++)
                {
                    bindingArray[i] = this.elements[i];
                }
                this.elements = bindingArray;
            }
            this.elements[index] = element;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public MemberBinding this[int index]
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
            private readonly MemberBindingList list;
            public Enumerator(MemberBindingList list)
            {
                this.index = -1;
                this.list = list;
            }

            public MemberBinding Current =>
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

