namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class ResourceList
    {
        private int count;
        private Resource[] elements;

        public ResourceList()
        {
            this.elements = new Resource[4];
        }

        public ResourceList(int capacity)
        {
            this.elements = new Resource[capacity];
        }

        public void Add(Resource element)
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
                Resource[] resourceArray = new Resource[num4];
                for (int i = 0; i < length; i++)
                {
                    resourceArray[i] = this.elements[i];
                }
                this.elements = resourceArray;
            }
            this.elements[index] = element;
        }

        public ResourceList Clone()
        {
            Resource[] elements = this.elements;
            int count = this.count;
            ResourceList list = new ResourceList(count) {
                count = count
            };
            Resource[] resourceArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                resourceArray2[i] = elements[i];
            }
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public Resource this[int index]
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
            private readonly ResourceList list;
            public Enumerator(ResourceList list)
            {
                this.index = -1;
                this.list = list;
            }

            public Resource Current =>
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

