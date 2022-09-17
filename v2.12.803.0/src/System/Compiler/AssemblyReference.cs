namespace System.Compiler
{
    using System;
    using System.Collections;
    using System.Compiler.Metadata;
    using System.Configuration.Assemblies;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;

    internal class AssemblyReference : Node
    {
        public IdentifierList Aliases;
        protected internal AssemblyNode assembly;
        private System.Reflection.AssemblyName assemblyName;
        private string culture;
        private AssemblyFlags flags;
        private byte[] hashValue;
        private string location;
        private string name;
        private byte[] publicKeyOrToken;
        internal System.Compiler.Metadata.Reader Reader;
        protected string strongName;
        private byte[] token;
        private System.Version version;

        public AssemblyReference() : base(NodeType.AssemblyReference)
        {
        }

        public AssemblyReference(AssemblyNode assembly) : base(NodeType.AssemblyReference)
        {
            this.culture = assembly.Culture;
            this.flags = assembly.Flags & ~AssemblyFlags.PublicKey;
            this.hashValue = assembly.HashValue;
            this.name = assembly.Name;
            this.publicKeyOrToken = assembly.PublicKeyOrToken;
            if ((assembly.PublicKeyOrToken != null) && (assembly.PublicKeyOrToken.Length > 8))
            {
                this.flags |= AssemblyFlags.PublicKey;
            }
            this.location = assembly.Location;
            this.version = assembly.Version;
            this.assembly = assembly;
        }

        public AssemblyReference(string assemblyStrongName) : base(NodeType.AssemblyReference)
        {
            AssemblyFlags none = AssemblyFlags.None;
            if (assemblyStrongName == null)
            {
                assemblyStrongName = "";
            }
            int i = 0;
            int length = assemblyStrongName.Length;
            string str = ParseToken(assemblyStrongName, ref i);
            string version = null;
            string strA = null;
            string str4 = null;
            while (i < length)
            {
                if (assemblyStrongName[i] != ',')
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExceptionStrings.InvalidAssemblyStrongName, new object[] { assemblyStrongName }), "assemblyStrongName");
                }
                i++;
                while ((i < length) && char.IsWhiteSpace(assemblyStrongName[i]))
                {
                    i++;
                }
                switch (assemblyStrongName[i])
                {
                    case 'p':
                    case 'P':
                        if (PlatformHelpers.StringCompareOrdinalIgnoreCase(assemblyStrongName, i, "PublicKeyToken", 0, "PublicKeyToken".Length) == 0)
                        {
                            str4 = ParseAssignment(assemblyStrongName, "PublicKeyToken", ref i);
                        }
                        else
                        {
                            str4 = ParseAssignment(assemblyStrongName, "PublicKey", ref i);
                            none |= AssemblyFlags.PublicKey;
                        }
                        break;

                    case 'r':
                    case 'R':
                        if (PlatformHelpers.StringCompareOrdinalIgnoreCase(ParseAssignment(assemblyStrongName, "Retargetable", ref i), "Yes") == 0)
                        {
                            none |= AssemblyFlags.Retargetable;
                        }
                        break;

                    case 'v':
                    case 'V':
                        version = ParseAssignment(assemblyStrongName, "Version", ref i);
                        break;

                    case 'c':
                    case 'C':
                        if (PlatformHelpers.StringCompareOrdinalIgnoreCase(assemblyStrongName, i, "Culture", 0, "Culture".Length) == 0)
                        {
                            strA = ParseAssignment(assemblyStrongName, "Culture", ref i);
                        }
                        else
                        {
                            ParseAssignment(assemblyStrongName, "ContentType", ref i);
                        }
                        break;
                }
                while ((i < length) && (assemblyStrongName[i] == ']'))
                {
                    i++;
                }
            }
            while ((i < length) && char.IsWhiteSpace(assemblyStrongName[i]))
            {
                i++;
            }
            if (i < length)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExceptionStrings.InvalidAssemblyStrongName, new object[] { assemblyStrongName }), "assemblyStrongName");
            }
            if (PlatformHelpers.StringCompareOrdinalIgnoreCase(strA, "neutral") == 0)
            {
                strA = null;
            }
            if (PlatformHelpers.StringCompareOrdinalIgnoreCase(str4, "null") == 0)
            {
                str4 = null;
            }
            byte[] buffer = null;
            if ((str4 != null) && ((length = str4.Length) > 0))
            {
                if (length > 0x10)
                {
                    ArrayList list = new ArrayList();
                    if ((length % 2) == 1)
                    {
                        list.Add(byte.Parse(str4.Substring(0, 1), NumberStyles.HexNumber, (IFormatProvider) null));
                        length--;
                    }
                    for (i = 0; i < length; i += 2)
                    {
                        byte result = 0;
                        byte.TryParse(str4.Substring(i, 2), NumberStyles.HexNumber, (IFormatProvider) null, out result);
                        list.Add(result);
                    }
                    buffer = (byte[]) list.ToArray(typeof(byte));
                }
                else
                {
                    ulong num4 = ulong.Parse(str4, NumberStyles.HexNumber, null);
                    buffer = new byte[] { (byte) (num4 >> 0x38), (byte) (num4 >> 0x30), (byte) (num4 >> 40), (byte) (num4 >> 0x20), (byte) (num4 >> 0x18), (byte) (num4 >> 0x10), (byte) (num4 >> 8), (byte) num4 };
                }
            }
            this.culture = strA;
            this.name = str;
            this.publicKeyOrToken = buffer;
            this.version = ((version == null) || (version.Length == 0)) ? null : new System.Version(version);
            this.flags = none;
        }

        public AssemblyReference(string assemblyStrongName, SourceContext sctx) : this(assemblyStrongName)
        {
            base.SourceContext = sctx;
        }

        public System.Reflection.AssemblyName GetAssemblyName()
        {
            if (this.assemblyName == null)
            {
                System.Reflection.AssemblyName name = new System.Reflection.AssemblyName {
                    CultureInfo = new CultureInfo((this.Culture == null) ? "" : this.Culture)
                };
                if ((this.PublicKeyOrToken != null) && (this.PublicKeyOrToken.Length > 8))
                {
                    name.Flags = AssemblyNameFlags.PublicKey;
                }
                if ((this.Flags & AssemblyFlags.Retargetable) != AssemblyFlags.None)
                {
                    name.Flags |= AssemblyNameFlags.Retargetable;
                }
                name.HashAlgorithm = System.Configuration.Assemblies.AssemblyHashAlgorithm.SHA1;
                if (this.PublicKeyOrToken != null)
                {
                    if (this.PublicKeyOrToken.Length > 8)
                    {
                        name.SetPublicKey(this.PublicKeyOrToken);
                    }
                    else if (this.PublicKeyOrToken.Length > 0)
                    {
                        name.SetPublicKeyToken(this.PublicKeyOrToken);
                    }
                }
                else
                {
                    name.SetPublicKey(new byte[0]);
                }
                name.Name = this.Name;
                name.Version = this.Version;
                switch ((this.Flags & AssemblyFlags.CompatibilityMask))
                {
                    case AssemblyFlags.NonSideBySideCompatible:
                        name.VersionCompatibility = AssemblyVersionCompatibility.SameDomain;
                        break;

                    case AssemblyFlags.NonSideBySideProcess:
                        name.VersionCompatibility = AssemblyVersionCompatibility.SameProcess;
                        break;

                    case AssemblyFlags.NonSideBySideMachine:
                        name.VersionCompatibility = AssemblyVersionCompatibility.SameMachine;
                        break;
                }
                this.assemblyName = name;
            }
            return this.assemblyName;
        }

        public bool Matches(string name, System.Version version, string culture, byte[] publicKeyToken)
        {
            if ((culture != null) && (culture.Length == 0))
            {
                culture = null;
            }
            if ((this.Culture != null) && (this.Culture.Length == 0))
            {
                this.Culture = null;
            }
            if (((this.Version != version) && (this.Version != null)) && ((version == null) || !this.Version.Equals(version)))
            {
                return false;
            }
            if ((PlatformHelpers.StringCompareOrdinalIgnoreCase(this.Name, name) != 0) || (PlatformHelpers.StringCompareOrdinalIgnoreCase(this.Culture, culture) != 0))
            {
                return false;
            }
            if ((this.Flags & AssemblyFlags.Retargetable) == AssemblyFlags.None)
            {
                byte[] buffer = this.PublicKeyToken;
                if (publicKeyToken == null)
                {
                    return (buffer == null);
                }
                if (buffer == publicKeyToken)
                {
                    return true;
                }
                if (buffer == null)
                {
                    return false;
                }
                int length = publicKeyToken.Length;
                if (length != buffer.Length)
                {
                    return false;
                }
                for (int i = 0; i < length; i++)
                {
                    if (buffer[i] != publicKeyToken[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool MatchesIgnoringVersion(AssemblyReference reference)
        {
            if (reference == null)
            {
                return false;
            }
            return this.Matches(reference.Name, this.Version, reference.Culture, reference.PublicKeyToken);
        }

        private static string ParseAssignment(string assemblyStrongName, string target, ref int i)
        {
            int length = assemblyStrongName.Length;
            if (PlatformHelpers.StringCompareOrdinalIgnoreCase(assemblyStrongName, i, target, 0, target.Length) == 0)
            {
                i += target.Length;
                while ((i < length) && char.IsWhiteSpace(assemblyStrongName[i]))
                {
                    i++;
                }
                if ((i < length) && (assemblyStrongName[i] == '='))
                {
                    i++;
                    if (i < length)
                    {
                        return ParseToken(assemblyStrongName, ref i);
                    }
                }
            }
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExceptionStrings.InvalidAssemblyStrongName, new object[] { assemblyStrongName }), "assemblyStrongName");
        }

        private static string ParseToken(string assemblyStrongName, ref int i)
        {
            int length = assemblyStrongName.Length;
            while ((i < length) && char.IsWhiteSpace(assemblyStrongName[i]))
            {
                i++;
            }
            StringBuilder builder = new StringBuilder(length);
            while (i < length)
            {
                char c = assemblyStrongName[i];
                if (((c == ',') || (c == ']')) || char.IsWhiteSpace(c))
                {
                    break;
                }
                builder.Append(c);
                i++;
            }
            while ((i < length) && char.IsWhiteSpace(assemblyStrongName[i]))
            {
                i++;
            }
            return builder.ToString();
        }

        public virtual AssemblyNode Assembly
        {
            get
            {
                if (this.assembly != null)
                {
                    return this.assembly;
                }
                if (this.Reader != null)
                {
                    return (this.assembly = this.Reader.GetAssemblyFromReference(this));
                }
                return null;
            }
            set
            {
                this.assembly = value;
            }
        }

        public string Culture
        {
            get => 
                this.culture;
            set
            {
                this.culture = value;
            }
        }

        public AssemblyFlags Flags
        {
            get => 
                this.flags;
            set
            {
                this.flags = value;
            }
        }

        public byte[] HashValue
        {
            get => 
                this.hashValue;
            set
            {
                this.hashValue = value;
            }
        }

        public string Location
        {
            get => 
                this.location;
            set
            {
                this.location = value;
            }
        }

        public string Name
        {
            get => 
                this.name;
            set
            {
                this.name = value;
            }
        }

        public byte[] PublicKeyOrToken
        {
            get => 
                this.publicKeyOrToken;
            set
            {
                this.publicKeyOrToken = value;
            }
        }

        public byte[] PublicKeyToken
        {
            get
            {
                if (this.token != null)
                {
                    return this.token;
                }
                if ((this.PublicKeyOrToken == null) || (this.PublicKeyOrToken.Length == 0))
                {
                    return null;
                }
                if (this.PublicKeyOrToken.Length == 8)
                {
                    return (this.token = this.PublicKeyOrToken);
                }
                byte[] buffer2 = new SHA1CryptoServiceProvider().ComputeHash(this.PublicKeyOrToken);
                byte[] buffer3 = new byte[8];
                int index = 0;
                int num2 = buffer2.Length - 1;
                while (index < 8)
                {
                    buffer3[index] = buffer2[num2 - index];
                    index++;
                }
                return (this.token = buffer3);
            }
        }

        public virtual string StrongName
        {
            get
            {
                if (this.strongName == null)
                {
                    this.strongName = AssemblyNode.GetStrongName(this.Name, this.Version, this.Culture, this.PublicKeyOrToken, (this.Flags & AssemblyFlags.Retargetable) != AssemblyFlags.None);
                }
                return this.strongName;
            }
        }

        public System.Version Version
        {
            get => 
                this.version;
            set
            {
                this.version = value;
            }
        }
    }
}

