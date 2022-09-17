namespace System.Compiler
{
    using System;
    using System.Collections;
    using System.Compiler.Metadata;
    using System.Configuration.Assemblies;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using System.Security.Policy;
    using System.Text;
    using System.Threading;

    internal class AssemblyNode : System.Compiler.Module
    {
        private System.Reflection.AssemblyName assemblyName;
        private CachedRuntimeAssembly cachedRuntimeAssembly;
        private static Hashtable CompiledAssemblies;
        protected AssemblyNode contractAssembly;
        private string culture;
        internal static readonly AssemblyNode Dummy = new AssemblyNode();
        protected TypeNodeList exportedTypes;
        private DateTime fileLastWriteTimeUtc;
        private AssemblyFlags flags;
        protected TrivialHashtable friends;
        public byte[] KeyBlob;
        public string KeyContainerName;
        protected AttributeList moduleAttributes;
        private string moduleName;
        private byte[] publicKeyOrToken;
        protected string strongName;
        protected byte[] token;
        private System.Version version;

        public event PostAssemblyLoadProcessor AfterAssemblyLoad;

        public AssemblyNode()
        {
            base.NodeType = NodeType.Assembly;
            base.ContainingAssembly = this;
        }

        public AssemblyNode(System.Compiler.Module.TypeNodeProvider provider, System.Compiler.Module.TypeNodeListProvider listProvider, System.Compiler.Module.CustomAttributeProvider provideCustomAttributes, System.Compiler.Module.ResourceProvider provideResources, string directory) : base(provider, listProvider, provideCustomAttributes, provideResources)
        {
            base.Directory = directory;
            base.NodeType = NodeType.Assembly;
            base.ContainingAssembly = this;
        }

        private void AddCachedAssembly(Assembly runtimeAssembly)
        {
            if (CompiledAssemblies == null)
            {
                CompiledAssemblies = Hashtable.Synchronized(new Hashtable());
            }
            CompiledAssemblies[runtimeAssembly] = new WeakReference(this);
        }

        public override void Dispose()
        {
            if (this.cachedRuntimeAssembly != null)
            {
                this.cachedRuntimeAssembly.Dispose();
            }
            this.cachedRuntimeAssembly = null;
            lock (Reader.StaticAssemblyCache)
            {
                foreach (object obj2 in new ArrayList(Reader.StaticAssemblyCache.Keys))
                {
                    if (Reader.StaticAssemblyCache[obj2] == this)
                    {
                        Reader.StaticAssemblyCache.Remove(obj2);
                    }
                }
                if (TargetPlatform.AssemblyReferenceForInitialized)
                {
                    AssemblyReference reference = (AssemblyReference) TargetPlatform.AssemblyReferenceFor[Identifier.For(base.Name).UniqueIdKey];
                    if ((reference != null) && (reference.Assembly == this))
                    {
                        reference.Assembly = null;
                    }
                }
            }
            base.Dispose();
        }

        public PostAssemblyLoadProcessor GetAfterAssemblyLoad() => 
            this.AfterAssemblyLoad;

        public static AssemblyNode GetAssembly(byte[] buffer) => 
            GetAssembly(buffer, null, false, false, true, false);

        public static AssemblyNode GetAssembly(AssemblyReference assemblyReference) => 
            GetAssembly(assemblyReference, null, false, false, true, false);

        public static AssemblyNode GetAssembly(Assembly runtimeAssembly) => 
            GetAssembly(runtimeAssembly, null, false, true, false);

        public static AssemblyNode GetAssembly(string location) => 
            GetAssembly(location, null, false, false, true, false);

        public static AssemblyNode GetAssembly(byte[] buffer, IDictionary cache) => 
            GetAssembly(buffer, cache, false, false, false, false);

        public static AssemblyNode GetAssembly(AssemblyReference assemblyReference, IDictionary cache) => 
            GetAssembly(assemblyReference, cache, false, false, false, false);

        public static AssemblyNode GetAssembly(Assembly runtimeAssembly, IDictionary cache) => 
            GetAssembly(runtimeAssembly, cache, false, false, false);

        public static AssemblyNode GetAssembly(string location, IDictionary cache) => 
            GetAssembly(location, cache, false, false, false, false);

        public static AssemblyNode GetAssembly(AssemblyReference assemblyReference, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache) => 
            GetAssembly(assemblyReference, null, doNotLockFile, getDebugInfo, useGlobalCache, false);

        public static AssemblyNode GetAssembly(Assembly runtimeAssembly, IDictionary cache, bool getDebugInfo, bool useGlobalCache) => 
            GetAssembly(runtimeAssembly, cache, getDebugInfo, useGlobalCache, false);

        public static AssemblyNode GetAssembly(string location, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache) => 
            GetAssembly(location, null, doNotLockFile, getDebugInfo, useGlobalCache, false);

        public static AssemblyNode GetAssembly(byte[] buffer, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache) => 
            GetAssembly(buffer, cache, doNotLockFile, getDebugInfo, useGlobalCache, false);

        public static AssemblyNode GetAssembly(AssemblyReference assemblyReference, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache) => 
            GetAssembly(assemblyReference, cache, doNotLockFile, getDebugInfo, useGlobalCache, false);

        public static AssemblyNode GetAssembly(Assembly runtimeAssembly, IDictionary cache, bool getDebugInfo, bool useGlobalCache, bool preserveShortBranches)
        {
            if (runtimeAssembly != null)
            {
                AssemblyNode systemAssembly = CoreSystemTypes.SystemAssembly;
                if (runtimeAssembly.GetName().Name == "mscorlib")
                {
                    return CoreSystemTypes.SystemAssembly;
                }
                if (CompiledAssemblies != null)
                {
                    WeakReference reference = (WeakReference) CompiledAssemblies[runtimeAssembly];
                    if (reference != null)
                    {
                        AssemblyNode target = (AssemblyNode) reference.Target;
                        if (target == null)
                        {
                            CompiledAssemblies.Remove(runtimeAssembly);
                        }
                        return target;
                    }
                }
                if ((runtimeAssembly.Location != null) && (runtimeAssembly.Location.Length > 0))
                {
                    return GetAssembly(runtimeAssembly.Location, cache, false, getDebugInfo, useGlobalCache, preserveShortBranches);
                }
            }
            return null;
        }

        public static AssemblyNode GetAssembly(string location, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache, PostAssemblyLoadProcessor postLoadEvent) => 
            GetAssembly(location, null, doNotLockFile, getDebugInfo, useGlobalCache, false, postLoadEvent);

        public static AssemblyNode GetAssembly(string location, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache) => 
            GetAssembly(location, cache, doNotLockFile, getDebugInfo, useGlobalCache, false);

        public static AssemblyNode GetAssembly(byte[] buffer, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache, bool preserveShortBranches)
        {
            if (buffer == null)
            {
                return null;
            }
            AssemblyNode systemAssembly = CoreSystemTypes.SystemAssembly;
            return (new Reader(buffer, cache, doNotLockFile, getDebugInfo, useGlobalCache, preserveShortBranches).ReadModule() as AssemblyNode);
        }

        public static AssemblyNode GetAssembly(AssemblyReference assemblyReference, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache, bool preserveShortBranches)
        {
            if (assemblyReference == null)
            {
                return null;
            }
            AssemblyNode systemAssembly = CoreSystemTypes.SystemAssembly;
            Reader reader = new Reader(cache, doNotLockFile, getDebugInfo, useGlobalCache, preserveShortBranches);
            return (assemblyReference.Assembly = reader.GetAssemblyFromReference(assemblyReference));
        }

        public static AssemblyNode GetAssembly(string location, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache, bool preserveShortBranches) => 
            GetAssembly(location, cache, doNotLockFile, getDebugInfo, useGlobalCache, preserveShortBranches, null);

        public static AssemblyNode GetAssembly(string location, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache, PostAssemblyLoadProcessor postLoadEvent) => 
            GetAssembly(location, cache, doNotLockFile, getDebugInfo, useGlobalCache, false, postLoadEvent);

        public static AssemblyNode GetAssembly(string location, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache, bool preserveShortBranches, PostAssemblyLoadProcessor postLoadEvent)
        {
            if (location == null)
            {
                return null;
            }
            AssemblyNode systemAssembly = CoreSystemTypes.SystemAssembly;
            return (new Reader(location, cache, doNotLockFile, getDebugInfo, useGlobalCache, preserveShortBranches).ReadModule(postLoadEvent) as AssemblyNode);
        }

        public System.Reflection.AssemblyName GetAssemblyName()
        {
            if (this.assemblyName == null)
            {
                System.Reflection.AssemblyName name = new System.Reflection.AssemblyName();
                if ((base.Location != null) && (base.Location != "unknown:location"))
                {
                    StringBuilder builder = new StringBuilder("file:///");
                    builder.Append(Path.GetFullPath(base.Location));
                    builder.Replace('\\', '/');
                    name.CodeBase = builder.ToString();
                }
                name.CultureInfo = new CultureInfo(this.Culture);
                if ((this.PublicKeyOrToken != null) && (this.PublicKeyOrToken.Length > 8))
                {
                    name.Flags = AssemblyNameFlags.PublicKey;
                }
                if ((this.Flags & AssemblyFlags.Retargetable) != AssemblyFlags.None)
                {
                    name.Flags |= AssemblyNameFlags.Retargetable;
                }
                name.HashAlgorithm = (System.Configuration.Assemblies.AssemblyHashAlgorithm) base.HashAlgorithm;
                if ((this.PublicKeyOrToken != null) && (this.PublicKeyOrToken.Length > 0))
                {
                    name.SetPublicKey(this.PublicKeyOrToken);
                }
                else
                {
                    name.SetPublicKey(new byte[0]);
                }
                name.Name = base.Name;
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

        [Obsolete("Please use GetAttribute(TypeNode attributeType)")]
        public virtual AttributeNode GetAttributeByName(TypeNode attributeType)
        {
            if (attributeType != null)
            {
                AttributeList attributes = this.Attributes;
                int num = 0;
                int num2 = (attributes == null) ? 0 : attributes.Count;
                while (num < num2)
                {
                    AttributeNode node = attributes[num];
                    if (node != null)
                    {
                        MemberBinding constructor = node.Constructor as MemberBinding;
                        if (((constructor != null) && (constructor.BoundMember != null)) && ((constructor.BoundMember.DeclaringType != null) && (constructor.BoundMember.DeclaringType.FullName == attributeType.FullName)))
                        {
                            return node;
                        }
                    }
                    num++;
                }
            }
            return null;
        }

        public AssemblyReferenceList GetFriendAssemblies()
        {
            if (SystemTypes.InternalsVisibleToAttribute == null)
            {
                return null;
            }
            AttributeList attributes = this.Attributes;
            if (attributes == null)
            {
                return null;
            }
            int count = attributes.Count;
            if (count == 0)
            {
                return null;
            }
            AssemblyReferenceList list2 = new AssemblyReferenceList(count);
            for (int i = 0; i < count; i++)
            {
                AttributeNode node = attributes[i];
                if (node == null)
                {
                    continue;
                }
                MemberBinding constructor = node.Constructor as MemberBinding;
                if (constructor != null)
                {
                    if ((constructor.BoundMember != null) && (constructor.BoundMember.DeclaringType == SystemTypes.InternalsVisibleToAttribute))
                    {
                        goto Label_00BF;
                    }
                    continue;
                }
                Literal literal = node.Constructor as Literal;
                if ((literal == null) || ((literal.Value as TypeNode) != SystemTypes.InternalsVisibleToAttribute))
                {
                    continue;
                }
            Label_00BF:
                if ((node.Expressions != null) && (node.Expressions.Count >= 1))
                {
                    Literal literal2 = node.Expressions[0] as Literal;
                    if (literal2 != null)
                    {
                        string assemblyStrongName = literal2.Value as string;
                        if (assemblyStrongName != null)
                        {
                            list2.Add(new AssemblyReference(assemblyStrongName));
                        }
                    }
                }
            }
            return list2;
        }

        private static string GetKeyString(byte[] publicKey)
        {
            StringBuilder builder;
            if (publicKey == null)
            {
                return null;
            }
            int length = publicKey.Length;
            if (length > 8)
            {
                publicKey = new SHA1CryptoServiceProvider().ComputeHash(publicKey);
                byte[] buffer = new byte[8];
                int index = 0;
                int num3 = publicKey.Length - 1;
                while (index < 8)
                {
                    buffer[index] = publicKey[num3 - index];
                    index++;
                }
                publicKey = buffer;
                length = 8;
            }
            if (length == 0)
            {
                builder = new StringBuilder(", PublicKeyToken=null");
            }
            else
            {
                builder = new StringBuilder(", PublicKeyToken=", (length * 2) + 0x11);
            }
            for (int i = 0; i < length; i++)
            {
                builder.Append(publicKey[i].ToString("x2"));
            }
            return builder.ToString();
        }

        public virtual AttributeNode GetModuleAttribute(TypeNode attributeType)
        {
            if (attributeType != null)
            {
                AttributeList moduleAttributes = this.ModuleAttributes;
                int num = 0;
                int num2 = (moduleAttributes == null) ? 0 : moduleAttributes.Count;
                while (num < num2)
                {
                    AttributeNode node = moduleAttributes[num];
                    if (node != null)
                    {
                        MemberBinding constructor = node.Constructor as MemberBinding;
                        if (constructor != null)
                        {
                            if ((constructor.BoundMember != null) && !(constructor.BoundMember.DeclaringType != attributeType))
                            {
                                return node;
                            }
                        }
                        else
                        {
                            Literal literal = node.Constructor as Literal;
                            if ((literal != null) && !((literal.Value as TypeNode) != attributeType))
                            {
                                return node;
                            }
                        }
                    }
                    num++;
                }
            }
            return null;
        }

        public Assembly GetRuntimeAssembly() => 
            this.GetRuntimeAssembly(null, null);

        public Assembly GetRuntimeAssembly(AppDomain targetAppDomain) => 
            this.GetRuntimeAssembly(null, targetAppDomain);

        public Assembly GetRuntimeAssembly(Evidence evidence) => 
            this.GetRuntimeAssembly(evidence, null);

        public Assembly GetRuntimeAssembly(Evidence evidence, AppDomain targetAppDomain)
        {
            Assembly runtimeAssembly = (this.cachedRuntimeAssembly == null) ? null : this.cachedRuntimeAssembly.Value;
            if (((runtimeAssembly == null) || (evidence != null)) || (targetAppDomain != null))
            {
                lock (this)
                {
                    if (((this.cachedRuntimeAssembly != null) && (evidence == null)) && (targetAppDomain == null))
                    {
                        return this.cachedRuntimeAssembly.Value;
                    }
                    if (targetAppDomain == null)
                    {
                        targetAppDomain = AppDomain.CurrentDomain;
                    }
                    if (base.Location != null)
                    {
                        string strongName = this.StrongName;
                        Assembly[] assemblies = targetAppDomain.GetAssemblies();
                        if (assemblies != null)
                        {
                            int index = 0;
                            int length = assemblies.Length;
                            while (index < length)
                            {
                                Assembly assembly3 = assemblies[index];
                                if ((assembly3 != null) && (assembly3.FullName == strongName))
                                {
                                    runtimeAssembly = assembly3;
                                    break;
                                }
                                index++;
                            }
                        }
                        if (runtimeAssembly == null)
                        {
                            if (evidence != null)
                            {
                                runtimeAssembly = targetAppDomain.Load(this.GetAssemblyName(), evidence);
                            }
                            else
                            {
                                runtimeAssembly = targetAppDomain.Load(this.GetAssemblyName());
                            }
                        }
                    }
                    else
                    {
                        byte[] executable = null;
                        byte[] debugSymbols = null;
                        if ((this.Flags & (AssemblyFlags.EnableJITcompileTracking | AssemblyFlags.DisableJITcompileOptimizer)) != AssemblyFlags.None)
                        {
                            this.WriteModule(out executable, out debugSymbols);
                            if (evidence != null)
                            {
                                runtimeAssembly = targetAppDomain.Load(executable, debugSymbols, evidence);
                            }
                            else
                            {
                                runtimeAssembly = targetAppDomain.Load(executable, debugSymbols);
                            }
                        }
                        else
                        {
                            this.WriteModule(out executable);
                            if (evidence != null)
                            {
                                runtimeAssembly = targetAppDomain.Load(executable, null, evidence);
                            }
                            else
                            {
                                runtimeAssembly = targetAppDomain.Load(executable);
                            }
                        }
                    }
                    if (((runtimeAssembly != null) && (evidence == null)) && (targetAppDomain == AppDomain.CurrentDomain))
                    {
                        this.AddCachedAssembly(runtimeAssembly);
                        this.cachedRuntimeAssembly = new CachedRuntimeAssembly(runtimeAssembly);
                    }
                }
            }
            return runtimeAssembly;
        }

        internal static string GetStrongName(string name, System.Version version, string culture, byte[] publicKey, bool retargetable)
        {
            if (version == null)
            {
                version = new System.Version();
            }
            StringBuilder builder = new StringBuilder();
            builder.Append(name);
            builder.Append(", Version=");
            builder.Append(version.ToString());
            builder.Append(", Culture=");
            builder.Append(((culture == null) || (culture.Length == 0)) ? "neutral" : culture);
            builder.Append(GetKeyString(publicKey));
            if (retargetable)
            {
                builder.Append(", Retargetable=Yes");
            }
            return builder.ToString();
        }

        public virtual bool MayAccessInternalTypesOf(AssemblyNode assembly)
        {
            if (this == assembly)
            {
                return true;
            }
            if ((assembly != null) && (SystemTypes.InternalsVisibleToAttribute != null))
            {
                if (this.friends == null)
                {
                    this.friends = new TrivialHashtable();
                }
                object obj2 = this.friends[assembly.UniqueKey];
                if (obj2 == string.Empty)
                {
                    return false;
                }
                if (obj2 == this)
                {
                    return true;
                }
                AttributeList attributes = assembly.Attributes;
                int num = 0;
                int num2 = (attributes == null) ? 0 : attributes.Count;
                while (num < num2)
                {
                    AttributeNode node = attributes[num];
                    if (node == null)
                    {
                        goto Label_01EB;
                    }
                    MemberBinding constructor = node.Constructor as MemberBinding;
                    if (constructor != null)
                    {
                        if ((constructor.BoundMember != null) && (constructor.BoundMember.DeclaringType == SystemTypes.InternalsVisibleToAttribute))
                        {
                            goto Label_0101;
                        }
                        goto Label_01EB;
                    }
                    Literal literal = node.Constructor as Literal;
                    if ((literal == null) || ((literal.Value as TypeNode) != SystemTypes.InternalsVisibleToAttribute))
                    {
                        goto Label_01EB;
                    }
                Label_0101:
                    if ((node.Expressions != null) && (node.Expressions.Count >= 1))
                    {
                        Literal literal2 = node.Expressions[0] as Literal;
                        if (literal2 != null)
                        {
                            string assemblyStrongName = literal2.Value as string;
                            if (assemblyStrongName != null)
                            {
                                try
                                {
                                    AssemblyReference reference = new AssemblyReference(assemblyStrongName);
                                    byte[] publicKeyToken = reference.PublicKeyToken;
                                    if ((publicKeyToken != null) && (this.PublicKeyOrToken != null))
                                    {
                                        publicKeyToken = this.PublicKeyToken;
                                    }
                                    if (!reference.Matches(base.Name, reference.Version, reference.Culture, publicKeyToken))
                                    {
                                        goto Label_01EB;
                                    }
                                }
                                catch (ArgumentException exception)
                                {
                                    if (base.MetadataImportErrors == null)
                                    {
                                        base.MetadataImportErrors = new ArrayList();
                                    }
                                    base.MetadataImportErrors.Add(exception.Message);
                                    goto Label_01EB;
                                }
                                this.friends[assembly.UniqueKey] = this;
                                return true;
                            }
                        }
                    }
                Label_01EB:
                    num++;
                }
                this.friends[assembly.UniqueKey] = string.Empty;
            }
            return false;
        }

        public void SetupDebugReader(string pdbSearchPath)
        {
            if (base.reader != null)
            {
                base.reader.SetupDebugReader(base.Location, pdbSearchPath);
            }
        }

        public override string ToString() => 
            base.Name;

        public virtual AssemblyNode ContractAssembly
        {
            get => 
                this.contractAssembly;
            set
            {
                if (this.contractAssembly == null)
                {
                    this.contractAssembly = value;
                    if (value != null)
                    {
                        AssemblyReferenceList list = new AssemblyReferenceList();
                        AssemblyReferenceList assemblyReferences = value.AssemblyReferences;
                        int num = 0;
                        int num2 = (assemblyReferences == null) ? 0 : assemblyReferences.Count;
                        while (num < num2)
                        {
                            AssemblyReference reference = assemblyReferences[num];
                            if ((reference != null) && (reference.Assembly != this))
                            {
                                int num3 = 0;
                                int num4 = (base.AssemblyReferences == null) ? 0 : base.AssemblyReferences.Count;
                                while (num3 < num4)
                                {
                                    if (((reference.Assembly.Name != null) && (base.AssemblyReferences[num3].Name != null)) && reference.Assembly.Name.Equals(base.AssemblyReferences[num3].Name))
                                    {
                                        break;
                                    }
                                    num3++;
                                }
                                if (num3 == num4)
                                {
                                    list.Add(assemblyReferences[num]);
                                }
                            }
                            num++;
                        }
                        if (base.AssemblyReferences == null)
                        {
                            base.AssemblyReferences = new AssemblyReferenceList();
                        }
                        int num5 = 0;
                        int count = list.Count;
                        while (num5 < count)
                        {
                            base.AssemblyReferences.Add(list[num5]);
                            num5++;
                        }
                        TypeNodeList instantiatedTypes = null;
                        if (base.reader != null)
                        {
                            instantiatedTypes = base.reader.GetInstantiatedTypes();
                        }
                        if (instantiatedTypes != null)
                        {
                            int num7 = 0;
                            int num8 = instantiatedTypes.Count;
                            while (num7 < num8)
                            {
                                TypeNode node = instantiatedTypes[num7];
                                if (node != null)
                                {
                                    MemberList members = node.members;
                                }
                                num7++;
                            }
                        }
                    }
                }
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

        public virtual TypeNodeList ExportedTypes
        {
            get
            {
                if (this.exportedTypes == null)
                {
                    if (base.provideTypeNodeList != null)
                    {
                        if (this.Types != null)
                        {
                        }
                    }
                    else
                    {
                        this.exportedTypes = new TypeNodeList();
                    }
                }
                return this.exportedTypes;
            }
            set
            {
                this.exportedTypes = value;
            }
        }

        public DateTime FileLastWriteTimeUtc
        {
            get => 
                this.fileLastWriteTimeUtc;
            set
            {
                this.fileLastWriteTimeUtc = value;
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

        public bool GetDebugSymbols
        {
            get
            {
                if (base.reader == null)
                {
                    return false;
                }
                return base.reader.getDebugSymbols;
            }
            set
            {
                if (base.reader != null)
                {
                    base.reader.getDebugSymbols = value;
                }
            }
        }

        public virtual AttributeList ModuleAttributes
        {
            get
            {
                if (this.moduleAttributes != null)
                {
                    return this.moduleAttributes;
                }
                if (base.provideCustomAttributes != null)
                {
                    lock (System.Compiler.Module.GlobalLock)
                    {
                        if (this.moduleAttributes == null)
                        {
                            base.provideCustomAttributes(this);
                        }
                        goto Label_0057;
                    }
                }
                this.moduleAttributes = new AttributeList();
            Label_0057:
                return this.moduleAttributes;
            }
            set
            {
                this.moduleAttributes = value;
            }
        }

        public string ModuleName
        {
            get => 
                this.moduleName;
            set
            {
                this.moduleName = value;
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

        public virtual byte[] PublicKeyToken
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
                    this.strongName = GetStrongName(base.Name, this.Version, this.Culture, this.PublicKeyOrToken, (this.Flags & AssemblyFlags.Retargetable) != AssemblyFlags.None);
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

        private sealed class CachedRuntimeAssembly : IDisposable
        {
            internal Assembly Value;

            internal CachedRuntimeAssembly(Assembly assembly)
            {
                this.Value = assembly;
            }

            public void Dispose()
            {
                if ((this.Value != null) && (AssemblyNode.CompiledAssemblies != null))
                {
                    AssemblyNode.CompiledAssemblies.Remove(this.Value);
                }
                this.Value = null;
                GC.SuppressFinalize(this);
            }

            ~CachedRuntimeAssembly()
            {
                this.Dispose();
            }
        }

        public delegate void PostAssemblyLoadProcessor(AssemblyNode loadedAssembly);
    }
}

