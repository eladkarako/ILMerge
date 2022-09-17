namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class MemberList
    {
        private int count;
        private Member[] elements;

        public MemberList()
        {
            this.elements = new Member[0x10];
        }

        public MemberList(int capacity)
        {
            this.elements = new Member[capacity];
        }

        public MemberList(params Member[] elements)
        {
            if (elements == null)
            {
                elements = new Member[0];
            }
            this.elements = elements;
            this.count = elements.Length;
        }

        public void Add(Member element)
        {
            int length = this.elements.Length;
            int index = this.count++;
            if (index == length)
            {
                int num4 = length * 2;
                if (num4 < 0x10)
                {
                    num4 = 0x10;
                }
                Member[] memberArray = new Member[num4];
                for (int i = 0; i < length; i++)
                {
                    memberArray[i] = this.elements[i];
                }
                this.elements = memberArray;
            }
            this.elements[index] = element;
        }

        public void AddList(MemberList memberList)
        {
            if ((memberList != null) && (memberList.Count != 0))
            {
                int length = this.elements.Length;
                int num2 = this.count + memberList.count;
                if (num2 > length)
                {
                    int num3 = num2;
                    if (num3 < 0x10)
                    {
                        num3 = 0x10;
                    }
                    Member[] memberArray = new Member[num3];
                    for (int j = 0; j < length; j++)
                    {
                        memberArray[j] = this.elements[j];
                    }
                    this.elements = memberArray;
                }
                int count = this.count;
                for (int i = 0; count < num2; i++)
                {
                    this.elements[count] = memberList.elements[i];
                    count++;
                }
                this.count = num2;
            }
        }

        public MemberList Clone()
        {
            Member[] elements = this.elements;
            int count = this.count;
            MemberList list = new MemberList(count) {
                count = count
            };
            Member[] memberArray2 = list.elements;
            for (int i = 0; i < count; i++)
            {
                memberArray2[i] = elements[i];
            }
            return list;
        }

        public bool Contains(Member element)
        {
            int count = this.count;
            for (int i = 0; i < count; i++)
            {
                if (this.elements[i] == element)
                {
                    return true;
                }
            }
            return false;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public void Remove(Member member)
        {
            int count = this.count;
            for (int i = 0; i < count; i++)
            {
                if (this.elements[i] == member)
                {
                    this.elements[i] = null;
                    return;
                }
            }
        }

        public void RemoveAt(int index)
        {
            if ((index < this.count) && (index >= 0))
            {
                int count = this.count;
                for (int i = index + 1; i < count; i++)
                {
                    this.elements[i - 1] = this.elements[i];
                }
                this.count--;
            }
        }

        public Member[] ToArray()
        {
            Member[] destinationArray = new Member[this.count];
            Array.Copy(this.elements, destinationArray, this.count);
            return destinationArray;
        }

        public int Count =>
            this.count;

        public Member this[int index]
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
            private readonly MemberList list;
            public Enumerator(MemberList list)
            {
                this.index = -1;
                this.list = list;
            }

            public Member Current =>
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

