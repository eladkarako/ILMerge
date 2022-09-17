namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class StatementList
    {
        private int count;
        private Statement[] elements;

        public StatementList()
        {
            this.elements = new Statement[4];
        }

        public StatementList(int capacity)
        {
            this.elements = new Statement[capacity];
        }

        public StatementList(params Statement[] elements)
        {
            if (elements == null)
            {
                elements = new Statement[0];
            }
            this.elements = elements;
            this.count = elements.Length;
        }

        public void Add(Statement statement)
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
                Statement[] statementArray = new Statement[num4];
                for (int i = 0; i < length; i++)
                {
                    statementArray[i] = this.elements[i];
                }
                this.elements = statementArray;
            }
            this.elements[index] = statement;
        }

        public StatementList Clone()
        {
            Statement[] elements = this.elements;
            int count = this.count;
            StatementList list = new StatementList(count) {
                count = count
            };
            Statement[] statementArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                statementArray2[i] = elements[i];
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

        public Statement this[int index]
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
            private readonly StatementList list;
            public Enumerator(StatementList list)
            {
                this.index = -1;
                this.list = list;
            }

            public Statement Current =>
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

