namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class ExceptionHandlerList
    {
        private int count;
        private ExceptionHandler[] elements;

        public ExceptionHandlerList()
        {
            this.elements = new ExceptionHandler[4];
        }

        public ExceptionHandlerList(int n)
        {
            this.elements = new ExceptionHandler[4];
            this.elements = new ExceptionHandler[n];
        }

        public void Add(ExceptionHandler element)
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
                ExceptionHandler[] handlerArray = new ExceptionHandler[num4];
                for (int i = 0; i < length; i++)
                {
                    handlerArray[i] = this.elements[i];
                }
                this.elements = handlerArray;
            }
            this.elements[index] = element;
        }

        public ExceptionHandlerList Clone()
        {
            ExceptionHandler[] elements = this.elements;
            int count = this.count;
            ExceptionHandlerList list = new ExceptionHandlerList(count) {
                count = count
            };
            ExceptionHandler[] handlerArray2 = list.elements;
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

        public ExceptionHandler this[int index]
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
            private readonly ExceptionHandlerList list;
            public Enumerator(ExceptionHandlerList list)
            {
                this.index = -1;
                this.list = list;
            }

            public ExceptionHandler Current =>
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

