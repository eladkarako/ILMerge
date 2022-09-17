namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class FaultHandlerList
    {
        private int count;
        private FaultHandler[] elements;

        public FaultHandlerList()
        {
            this.elements = new FaultHandler[4];
        }

        public FaultHandlerList(int n)
        {
            this.elements = new FaultHandler[n];
        }

        public void Add(FaultHandler element)
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
                FaultHandler[] handlerArray = new FaultHandler[num4];
                for (int i = 0; i < length; i++)
                {
                    handlerArray[i] = this.elements[i];
                }
                this.elements = handlerArray;
            }
            this.elements[index] = element;
        }

        public FaultHandlerList Clone()
        {
            FaultHandler[] elements = this.elements;
            int count = this.count;
            FaultHandlerList list = new FaultHandlerList(count) {
                count = count
            };
            FaultHandler[] handlerArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                handlerArray2[i] = elements[i];
            }
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public FaultHandler this[int index]
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
            private readonly FaultHandlerList list;
            public Enumerator(FaultHandlerList list)
            {
                this.index = -1;
                this.list = list;
            }

            public FaultHandler Current =>
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

