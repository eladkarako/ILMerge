namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class ParameterList
    {
        private int count;
        private Parameter[] elements;
        public static readonly ParameterList Empty = new ParameterList(0);

        public ParameterList()
        {
            this.elements = new Parameter[8];
        }

        public ParameterList(int capacity)
        {
            this.elements = new Parameter[capacity];
        }

        public ParameterList(params Parameter[] elements)
        {
            if (elements == null)
            {
                elements = new Parameter[0];
            }
            this.elements = elements;
            this.count = elements.Length;
        }

        public void Add(Parameter element)
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
                Parameter[] parameterArray = new Parameter[num4];
                for (int i = 0; i < length; i++)
                {
                    parameterArray[i] = this.elements[i];
                }
                this.elements = parameterArray;
            }
            this.elements[index] = element;
        }

        public ParameterList Clone()
        {
            Parameter[] elements = this.elements;
            int count = this.count;
            ParameterList list = new ParameterList(count) {
                count = count
            };
            Parameter[] parameterArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                parameterArray2[i] = elements[i];
            }
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < this.count; i++)
            {
                if (i > 0)
                {
                    str = str + ",";
                }
                Parameter parameter = this.elements[i];
                if (parameter != null)
                {
                    str = str + parameter.ToString();
                }
            }
            return str;
        }

        public int Count
        {
            get => 
                this.count;
            set
            {
                this.count = value;
            }
        }

        public Parameter this[int index]
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
            private readonly ParameterList list;
            public Enumerator(ParameterList list)
            {
                this.index = -1;
                this.list = list;
            }

            public Parameter Current =>
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

