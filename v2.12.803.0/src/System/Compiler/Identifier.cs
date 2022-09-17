namespace System.Compiler
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class Identifier : Expression
    {
        private static int count;
        public static readonly Identifier Empty = new Identifier("");
        private int hashCode;
        private static CanonicalIdentifier[] HashTable = new CanonicalIdentifier[0x4000];
        internal int length;
        private static readonly object Lock = new object();
        private string name;
        private int offset;
        public Identifier Prefix;
        private int uniqueIdKey;

        public Identifier(string name) : base(NodeType.Identifier)
        {
            if (name == null)
            {
                name = "";
            }
            this.name = name;
            int num2 = this.length = name.Length;
            ulong num3 = 0L;
            for (int i = 0; i < num2; i++)
            {
                char ch = name[i];
                num3 = (num3 * ((ulong) 0x11L)) + ch;
            }
            this.hashCode = ((int) num3) & 0x7fffffff;
        }

        public Identifier(string name, SourceContext sctx) : this(name)
        {
            base.SourceContext = sctx;
        }

        private unsafe Identifier(byte* pointer, int offset) : base(NodeType.Identifier)
        {
            int num;
            ulong num2;
            this.offset = offset;
            if (ComputeHash(pointer, offset, out num, out num2))
            {
                this.hashCode = ((int) num2) & 0x7fffffff;
                this.length = num;
                this.name = new string((sbyte*) pointer, offset, num, Encoding.ASCII);
            }
            else
            {
                num2 = 0L;
                string str2 = this.name = new string((sbyte*) pointer, offset, num, Encoding.UTF8);
                int num3 = 0;
                int num5 = this.length = str2.Length;
                while (num3 < num5)
                {
                    char ch = str2[num3];
                    num2 = (num2 * ((ulong) 0x11L)) + ch;
                    num3++;
                }
                this.hashCode = ((int) num2) & 0x7fffffff;
            }
        }

        private unsafe Identifier(byte* pointer, uint length) : base(NodeType.Identifier)
        {
            this.length = (int) length;
            ulong num = 0L;
            for (uint i = 0; i < length; i++)
            {
                byte num3 = pointer[(int) ((byte*) i)];
                if ((num3 & 0x80) != 0)
                {
                    string str2 = this.name = new string((sbyte*) pointer, 0, this.length, Encoding.UTF8);
                    int num4 = 0;
                    int num6 = this.length = str2.Length;
                    while (num4 < num6)
                    {
                        char ch = str2[num4];
                        num = (num * ((ulong) 0x11L)) + ch;
                        num4++;
                    }
                    this.hashCode = ((int) num) & 0x7fffffff;
                    return;
                }
                num = (num * ((ulong) 0x11L)) + num3;
            }
            this.hashCode = ((int) num) & 0x7fffffff;
            this.name = new string((sbyte*) pointer, 0, this.length, Encoding.ASCII);
        }

        private static unsafe bool ComputeHash(byte* pointer, int offset, out int length, out ulong hcode)
        {
            length = 0;
            hcode = 0L;
            bool flag = true;
            int index = offset;
            while (true)
            {
                byte num2 = pointer[index];
                if (num2 == 0)
                {
                    return flag;
                }
                if ((num2 & 0x80) != 0)
                {
                    flag = false;
                }
                hcode = (ulong) ((hcode * 0x11L) + num2);
                length++;
                index++;
            }
        }

        public static Identifier For(string name) => 
            new Identifier(name);

        internal static unsafe Identifier For(byte* pointer, int offset)
        {
            int num;
            ulong num2;
            Identifier identifier;
            if (ComputeHash(pointer, offset, out num, out num2))
            {
                identifier = TryHashLookup(pointer, offset, num, ((int) num2) & 0x7fffffff);
                if (identifier != null)
                {
                    return identifier;
                }
            }
            identifier = new Identifier(pointer, offset);
            int uniqueIdKey = identifier.UniqueIdKey;
            return identifier;
        }

        internal static unsafe Identifier For(byte* pointer, uint length) => 
            new Identifier(pointer, length);

        private int GetUniqueIdKey()
        {
            lock (Lock)
            {
                int hashCode = this.hashCode;
                CanonicalIdentifier[] hashTable = HashTable;
                int length = hashTable.Length;
                int index = hashCode % length;
                for (CanonicalIdentifier identifier = hashTable[index]; identifier.Name != null; identifier = hashTable[index])
                {
                    if (this.HasSameNameAs(identifier))
                    {
                        return identifier.UniqueIdKey;
                    }
                    index = (index + 1) % length;
                }
                int uniqueIdKey = count + 1;
                count = uniqueIdKey;
                string name = this.Name;
                hashTable[index] = new CanonicalIdentifier(this, uniqueIdKey, hashCode);
                if (uniqueIdKey > (length / 2))
                {
                    Rehash();
                }
                return uniqueIdKey;
            }
        }

        private bool HasSameNameAs(CanonicalIdentifier id)
        {
            int length = this.length;
            int num2 = id.Name.Length;
            if (length != num2)
            {
                return false;
            }
            string name = this.name;
            string str2 = id.Name;
            return (name == str2);
        }

        private static unsafe bool HasSameNameAs(string name, byte* ptr, int offset, int slen)
        {
            if (slen != name.Length)
            {
                return false;
            }
            for (int i = 0; i < slen; i++)
            {
                if (((short) name[i]) != ptr[offset++])
                {
                    return false;
                }
            }
            return true;
        }

        private static void Rehash()
        {
            CanonicalIdentifier[] hashTable = HashTable;
            int length = hashTable.Length;
            int num2 = length * 2;
            CanonicalIdentifier[] identifierArray2 = new CanonicalIdentifier[num2];
            for (int i = 0; i < length; i++)
            {
                CanonicalIdentifier identifier = hashTable[i];
                if (identifier.Name != null)
                {
                    int index = identifier.HashCode % num2;
                    for (CanonicalIdentifier identifier2 = identifierArray2[index]; identifier2.Name != null; identifier2 = identifierArray2[index])
                    {
                        index = (index + 1) % num2;
                    }
                    identifierArray2[index] = identifier;
                }
            }
            HashTable = identifierArray2;
        }

        public override string ToString()
        {
            if (this.Prefix != null)
            {
                return (this.Prefix.Name + ":" + this.Name);
            }
            if (this.Name == null)
            {
                return "";
            }
            return this.Name;
        }

        private static unsafe Identifier TryHashLookup(byte* ptr, int offset, int slen, int hcode)
        {
            lock (Lock)
            {
                CanonicalIdentifier[] hashTable = HashTable;
                int length = hashTable.Length;
                int index = hcode % length;
                for (CanonicalIdentifier identifier = hashTable[index]; identifier.Name != null; identifier = hashTable[index])
                {
                    if (HasSameNameAs(identifier.Name, ptr, offset, slen))
                    {
                        return identifier.id;
                    }
                    index = (index + 1) % length;
                }
                return null;
            }
        }

        public string Name =>
            this.name;

        public int UniqueIdKey
        {
            get
            {
                int uniqueIdKey = this.uniqueIdKey;
                if (uniqueIdKey != 0)
                {
                    return uniqueIdKey;
                }
                return (this.uniqueIdKey = this.GetUniqueIdKey());
            }
        }

        [Obsolete("Use Identifier.UniqueIdKey instead")]
        public int UniqueKey
        {
            get
            {
                int uniqueIdKey = this.uniqueIdKey;
                if (uniqueIdKey != 0)
                {
                    return uniqueIdKey;
                }
                return (this.uniqueIdKey = this.GetUniqueIdKey());
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CanonicalIdentifier
        {
            internal Identifier id;
            internal int UniqueIdKey;
            internal int HashCode;
            internal string Name
            {
                get
                {
                    if (this.id != null)
                    {
                        return this.id.Name;
                    }
                    return null;
                }
            }
            internal CanonicalIdentifier(Identifier name, int uniqueIdKey, int hashCode)
            {
                this.id = name;
                this.UniqueIdKey = uniqueIdKey;
                this.HashCode = hashCode;
            }
        }
    }
}

