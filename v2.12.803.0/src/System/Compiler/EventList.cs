namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class EventList
    {
        private int count;
        private Event[] elements;

        public EventList()
        {
            this.elements = new Event[8];
        }

        public EventList(int n)
        {
            this.elements = new Event[n];
        }

        public void Add(Event element)
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
                Event[] eventArray = new Event[num4];
                for (int i = 0; i < length; i++)
                {
                    eventArray[i] = this.elements[i];
                }
                this.elements = eventArray;
            }
            this.elements[index] = element;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int Count =>
            this.count;

        public Event this[int index]
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
            private readonly EventList list;
            public Enumerator(EventList list)
            {
                this.index = -1;
                this.list = list;
            }

            public Event Current =>
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

