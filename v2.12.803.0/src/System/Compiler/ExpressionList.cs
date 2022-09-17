namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class ExpressionList
    {
        private int count;
        private Expression[] elements;

        public ExpressionList()
        {
            this.elements = new Expression[8];
        }

        public ExpressionList(int n)
        {
            this.elements = new Expression[n];
        }

        public ExpressionList(params Expression[] elements)
        {
            if (elements == null)
            {
                elements = new Expression[0];
            }
            this.elements = elements;
            this.count = elements.Length;
        }

        public void Add(Expression element)
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
                Expression[] expressionArray = new Expression[num4];
                for (int i = 0; i < length; i++)
                {
                    expressionArray[i] = this.elements[i];
                }
                this.elements = expressionArray;
            }
            this.elements[index] = element;
        }

        public ExpressionList Clone()
        {
            Expression[] elements = this.elements;
            int count = this.count;
            ExpressionList list = new ExpressionList(count) {
                count = count
            };
            Expression[] expressionArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                expressionArray2[i] = elements[i];
            }
            return list;
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

        public Expression this[int index]
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
            private readonly ExpressionList list;
            public Enumerator(ExpressionList list)
            {
                this.index = -1;
                this.list = list;
            }

            public Expression Current =>
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

