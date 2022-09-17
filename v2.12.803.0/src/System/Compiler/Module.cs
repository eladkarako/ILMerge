namespace System.Compiler
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Compiler.Metadata;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Xml;

    internal class Module : Node, IDisposable
    {
        private AssemblyReferenceList assemblyReferences;
        protected AttributeList attributes;
        private AssemblyNode containingAssembly;
        public MethodBodySpecializerFactory CreateMethodBodySpecializer;
        private string directory;
        private ushort dllCharacteristics;
        protected XmlDocument documentation;
        public Node DocumentationNode;
        protected System.Compiler.Method entryPoint;
        internal int FileAlignment;
        internal static readonly object GlobalLock = new object();
        private AssemblyHashAlgorithm hashAlgorithm;
        private byte[] hashValue;
        public bool IsNormalized;
        private ModuleKindFlags kind;
        private int linkerMajorVersion;
        private int linkerMinorVersion;
        private string location;
        protected TrivialHashtable memberDocumentationCache;
        private int metadataFormatMajorVersion;
        private int metadataFormatMinorVersion;
        private ArrayList metadataImportErrors;
        protected ModuleReferenceList moduleReferences;
        private Guid mvid;
        private string name;
        protected NamespaceList namespaceList;
        protected TrivialHashtable namespaceTable;
        protected internal static readonly System.Compiler.Method NoSuchMethod = new System.Compiler.Method();
        private PEKindFlags peKind;
        private bool? projectModule;
        protected CustomAttributeProvider provideCustomAttributes;
        protected ResourceProvider provideResources;
        protected TypeNodeProvider provideTypeNode;
        protected TypeNodeListProvider provideTypeNodeList;
        internal Reader reader;
        protected TrivialHashtable referencedModulesAndAssemblies;
        protected ResourceList resources;
        protected int savedTypesLength;
        protected SecurityAttributeList securityAttributes;
        public bool StripOptionalModifiersFromLocals;
        private TrivialHashtableUsingWeakReferences structurallyEquivalentType;
        private string targetRuntimeVersion;
        private bool trackDebugData;
        protected internal TypeNodeList types;
        public bool UsePublicKeyTokensForAssemblyReferences;
        protected TrivialHashtable validNamespaces;
        protected Win32ResourceList win32Resources;

        public event AssemblyReferenceResolver AssemblyReferenceResolution;

        public event AssemblyReferenceResolver AssemblyReferenceResolutionAfterProbingFailed;

        public event DocumentationResolver DocumentationResolution;

        public Module() : base(NodeType.Module)
        {
            this.namespaceTable = new TrivialHashtable();
            this.UsePublicKeyTokensForAssemblyReferences = true;
            this.FileAlignment = 0x200;
            this.StripOptionalModifiersFromLocals = true;
            this.hashAlgorithm = AssemblyHashAlgorithm.SHA1;
            this.linkerMajorVersion = 6;
            this.projectModule = null;
            this.peKind = PEKindFlags.ILonly;
            this.IsNormalized = false;
        }

        public Module(TypeNodeProvider provider, TypeNodeListProvider listProvider, CustomAttributeProvider provideCustomAttributes, ResourceProvider provideResources) : base(NodeType.Module)
        {
            this.namespaceTable = new TrivialHashtable();
            this.UsePublicKeyTokensForAssemblyReferences = true;
            this.FileAlignment = 0x200;
            this.StripOptionalModifiersFromLocals = true;
            this.hashAlgorithm = AssemblyHashAlgorithm.SHA1;
            this.linkerMajorVersion = 6;
            this.projectModule = null;
            this.peKind = PEKindFlags.ILonly;
            this.provideCustomAttributes = provideCustomAttributes;
            this.provideResources = provideResources;
            this.provideTypeNode = provider;
            this.provideTypeNodeList = listProvider;
            this.IsNormalized = true;
        }

        public virtual void AddWin32Icon(Stream win32IconStream)
        {
            Writer.AddWin32Icon(this, win32IconStream);
        }

        public virtual void AddWin32Icon(string win32IconFilePath)
        {
            if (win32IconFilePath != null)
            {
                Writer.AddWin32Icon(this, win32IconFilePath);
            }
        }

        public virtual void AddWin32ResourceFile(Stream win32ResourceStream)
        {
            if (win32ResourceStream != null)
            {
                Writer.AddWin32ResourceFileToModule(this, win32ResourceStream);
            }
        }

        public virtual void AddWin32ResourceFile(string win32ResourceFilePath)
        {
            if (win32ResourceFilePath != null)
            {
                Writer.AddWin32ResourceFileToModule(this, win32ResourceFilePath);
            }
        }

        public void AddWin32VersionInfo(CompilerOptions options)
        {
            if (options != null)
            {
                Writer.AddWin32VersionInfo(this, options);
            }
        }

        public virtual bool ContainsModule(Module module)
        {
            if (((module != null) && (this.ModuleReferences != null)) && (this.ModuleReferences.Count != 0))
            {
                int count = this.ModuleReferences.Count;
                for (int i = 0; i < count; i++)
                {
                    ModuleReference reference = this.ModuleReferences[i];
                    if ((reference != null) && (reference.Module == module))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual void Dispose()
        {
            if (this.reader != null)
            {
                this.reader.Dispose();
            }
            this.reader = null;
            ModuleReferenceList moduleReferences = this.moduleReferences;
            int num = 0;
            int num2 = (moduleReferences == null) ? 0 : moduleReferences.Count;
            while (num < num2)
            {
                ModuleReference reference = moduleReferences[num];
                if ((reference == null) || (reference.Module != null))
                {
                    reference.Module.Dispose();
                }
                num++;
            }
            this.moduleReferences = null;
        }

        public virtual AttributeNode GetAttribute(TypeNode attributeType)
        {
            AttributeList attributes = this.GetAttributes(attributeType, 1);
            if ((attributes != null) && (attributes.Count > 0))
            {
                return attributes[0];
            }
            return null;
        }

        public virtual AttributeList GetAttributes(TypeNode attributeType) => 
            this.GetAttributes(attributeType, 0x7fffffff);

        public virtual AttributeList GetAttributes(TypeNode attributeType, int maxCount)
        {
            AttributeList list = new AttributeList();
            if (attributeType != null)
            {
                AttributeList attributes = this.Attributes;
                int num = 0;
                int num2 = 0;
                int num3 = (attributes == null) ? 0 : attributes.Count;
                while ((num < num3) && (num2 < maxCount))
                {
                    AttributeNode element = attributes[num];
                    if (element != null)
                    {
                        MemberBinding constructor = element.Constructor as MemberBinding;
                        if (constructor != null)
                        {
                            if ((constructor.BoundMember != null) && !(constructor.BoundMember.DeclaringType != attributeType))
                            {
                                list.Add(element);
                                num2++;
                            }
                        }
                        else
                        {
                            Literal literal = element.Constructor as Literal;
                            if ((literal != null) && !((literal.Value as TypeNode) != attributeType))
                            {
                                list.Add(element);
                                num2++;
                            }
                        }
                    }
                    num++;
                }
            }
            return list;
        }

        public TrivialHashtable GetMemberDocumentationCache()
        {
            TrivialHashtable memberDocumentationCache = this.memberDocumentationCache;
            if (memberDocumentationCache != null)
            {
                return memberDocumentationCache;
            }
            lock (this)
            {
                if (this.memberDocumentationCache != null)
                {
                    return this.memberDocumentationCache;
                }
                XmlDocument documentation = this.Documentation;
                if (((documentation == null) && (this.ContainingAssembly != null)) && (this.ContainingAssembly != this))
                {
                    return (this.memberDocumentationCache = this.ContainingAssembly.memberDocumentationCache);
                }
                memberDocumentationCache = this.memberDocumentationCache = new TrivialHashtable();
                if (documentation != null)
                {
                    XmlNode documentElement = documentation.DocumentElement;
                    if (documentElement == null)
                    {
                        return memberDocumentationCache;
                    }
                    XmlNode node2 = null;
                    if (documentElement.HasChildNodes)
                    {
                        foreach (XmlNode node3 in documentElement.ChildNodes)
                        {
                            if (node3.Name == "members")
                            {
                                node2 = node3;
                                break;
                            }
                        }
                    }
                    if ((node2 != null) && node2.HasChildNodes)
                    {
                        foreach (XmlNode node4 in node2.ChildNodes)
                        {
                            if (node4.Name == "member")
                            {
                                XmlNode namedItem = node4.Attributes.GetNamedItem("name");
                                if (namedItem != null)
                                {
                                    memberDocumentationCache[Identifier.For(namedItem.Value).UniqueIdKey] = node4;
                                }
                            }
                        }
                    }
                }
                return memberDocumentationCache;
            }
        }

        public MethodBodySpecializer GetMethodBodySpecializer(TypeNodeList pars, TypeNodeList args)
        {
            if (this.CreateMethodBodySpecializer != null)
            {
                return this.CreateMethodBodySpecializer(this, pars, args);
            }
            return new MethodBodySpecializer(this, pars, args);
        }

        public static Module GetModule(byte[] buffer) => 
            GetModule(buffer, null, false, false, true, false);

        public static Module GetModule(string location) => 
            GetModule(location, null, false, false, true, false);

        public static Module GetModule(byte[] buffer, IDictionary cache) => 
            GetModule(buffer, null, false, false, false, false);

        public static Module GetModule(string location, IDictionary cache) => 
            GetModule(location, cache, false, false, false, false);

        public static Module GetModule(string location, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache) => 
            GetModule(location, null, doNotLockFile, getDebugInfo, useGlobalCache, false);

        public static Module GetModule(byte[] buffer, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache) => 
            GetModule(buffer, cache, doNotLockFile, getDebugInfo, useGlobalCache, false);

        public static Module GetModule(string location, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache) => 
            GetModule(location, cache, doNotLockFile, getDebugInfo, useGlobalCache, false);

        public static Module GetModule(byte[] buffer, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache, bool preserveShortBranches)
        {
            if (buffer == null)
            {
                return null;
            }
            return new Reader(buffer, cache, doNotLockFile, getDebugInfo, useGlobalCache, false).ReadModule();
        }

        public static Module GetModule(string location, IDictionary cache, bool doNotLockFile, bool getDebugInfo, bool useGlobalCache, bool preserveShortBranches)
        {
            if (location == null)
            {
                return null;
            }
            return new Reader(location, cache, doNotLockFile, getDebugInfo, useGlobalCache, preserveShortBranches).ReadModule();
        }

        public NamespaceList GetNamespaceList()
        {
            if (this.reader != null)
            {
                return this.GetNamespaceListFromReader();
            }
            TypeNodeList types = this.Types;
            int num = (types == null) ? 0 : types.Count;
            if ((this.namespaceList == null) || (num > this.savedTypesLength))
            {
                lock (this)
                {
                    if (((this.namespaceList != null) && (this.types != null)) && (this.types.Count == this.savedTypesLength))
                    {
                        return this.namespaceList;
                    }
                    NamespaceList list4 = this.namespaceList = new NamespaceList();
                    TrivialHashtable hashtable2 = this.validNamespaces = new TrivialHashtable();
                    for (int i = 0; i < num; i++)
                    {
                        TypeNode element = this.types[i];
                        if (element != null)
                        {
                            if (element.Namespace == null)
                            {
                                element.Namespace = Identifier.Empty;
                            }
                            System.Compiler.Namespace namespace2 = hashtable2[element.Namespace.UniqueIdKey] as System.Compiler.Namespace;
                            if (namespace2 != null)
                            {
                                if (element.IsPublic)
                                {
                                    namespace2.isPublic = true;
                                }
                                namespace2.Types.Add(element);
                            }
                            else
                            {
                                namespace2 = new System.Compiler.Namespace(element.Namespace) {
                                    isPublic = element.IsPublic,
                                    Types = new TypeNodeList()
                                };
                                namespace2.Types.Add(element);
                                hashtable2[element.Namespace.UniqueIdKey] = namespace2;
                                list4.Add(namespace2);
                            }
                        }
                    }
                }
            }
            return this.namespaceList;
        }

        private NamespaceList GetNamespaceListFromReader()
        {
            if (this.namespaceList == null)
            {
                lock (GlobalLock)
                {
                    this.reader.GetNamespaces();
                    NamespaceList list2 = this.namespaceList = this.reader.namespaceList;
                    TrivialHashtable hashtable2 = this.validNamespaces = new TrivialHashtable();
                    int num = 0;
                    int num2 = (list2 == null) ? 0 : list2.Count;
                    while (num < num2)
                    {
                        System.Compiler.Namespace namespace2 = list2[num];
                        if ((namespace2 != null) && (namespace2.Name != null))
                        {
                            namespace2.ProvideTypes = new System.Compiler.Namespace.TypeProvider(this.GetTypesForNamespace);
                            hashtable2[namespace2.Name.UniqueIdKey] = namespace2;
                        }
                        num++;
                    }
                }
            }
            return this.namespaceList;
        }

        public Module GetNestedModule(string moduleName)
        {
            TypeNodeList types = this.Types;
            ModuleReferenceList moduleReferences = this.ModuleReferences;
            int num = 0;
            int num2 = (moduleReferences == null) ? 0 : moduleReferences.Count;
            while (num < num2)
            {
                ModuleReference reference = moduleReferences[num];
                if ((reference != null) && (reference.Name == moduleName))
                {
                    return reference.Module;
                }
                num++;
            }
            return null;
        }

        public virtual TypeNode GetStructurallyEquivalentType(Identifier ns, Identifier id) => 
            this.GetStructurallyEquivalentType(ns, id, id, true);

        public virtual TypeNode GetStructurallyEquivalentType(Identifier ns, Identifier id, Identifier uniqueMangledName, bool lookInReferencedAssemblies)
        {
            if (uniqueMangledName == null)
            {
                uniqueMangledName = id;
            }
            TypeNode type = (TypeNode) this.StructurallyEquivalentType[uniqueMangledName.UniqueIdKey];
            if (type == Class.DoesNotExist)
            {
                return null;
            }
            if (type != null)
            {
                return type;
            }
            lock (GlobalLock)
            {
                type = this.GetType(ns, id);
                if (type != null)
                {
                    this.StructurallyEquivalentType[uniqueMangledName.UniqueIdKey] = type;
                    return type;
                }
                if (lookInReferencedAssemblies)
                {
                    AssemblyReferenceList assemblyReferences = this.AssemblyReferences;
                    int num = 0;
                    int num2 = (assemblyReferences == null) ? 0 : assemblyReferences.Count;
                    while (num < num2)
                    {
                        AssemblyReference reference = assemblyReferences[num];
                        if (reference != null)
                        {
                            AssemblyNode assembly = reference.Assembly;
                            if (assembly != null)
                            {
                                type = assembly.GetType(ns, id);
                                if (type != null)
                                {
                                    this.StructurallyEquivalentType[uniqueMangledName.UniqueIdKey] = type;
                                    return type;
                                }
                            }
                        }
                        num++;
                    }
                }
                this.StructurallyEquivalentType[uniqueMangledName.UniqueIdKey] = Class.DoesNotExist;
                return null;
            }
        }

        public virtual TypeNode GetType(Identifier @namespace, Identifier name)
        {
            if ((@namespace == null) || (name == null))
            {
                return null;
            }
            TypeNode node = null;
            if (this.namespaceTable == null)
            {
                this.namespaceTable = new TrivialHashtable();
            }
            TrivialHashtable hashtable = (TrivialHashtable) this.namespaceTable[@namespace.UniqueIdKey];
            if (hashtable != null)
            {
                node = (TypeNode) hashtable[name.UniqueIdKey];
                if (node == Class.DoesNotExist)
                {
                    return null;
                }
                if (node != null)
                {
                    return node;
                }
            }
            else
            {
                lock (GlobalLock)
                {
                    hashtable = (TrivialHashtable) this.namespaceTable[@namespace.UniqueIdKey];
                    if (hashtable == null)
                    {
                        this.namespaceTable[@namespace.UniqueIdKey] = hashtable = new TrivialHashtable(0x20);
                    }
                }
            }
            if (this.provideTypeNode != null)
            {
                lock (GlobalLock)
                {
                    node = (TypeNode) hashtable[name.UniqueIdKey];
                    if (node != Class.DoesNotExist)
                    {
                        if (node != null)
                        {
                            return node;
                        }
                        node = this.provideTypeNode(@namespace, name);
                        if (node != null)
                        {
                            hashtable[name.UniqueIdKey] = node;
                            return node;
                        }
                        hashtable[name.UniqueIdKey] = Class.DoesNotExist;
                    }
                    return null;
                }
            }
            if ((this.types == null) || (this.types.Count <= this.savedTypesLength))
            {
                return null;
            }
            int num2 = this.savedTypesLength = this.types.Count;
            for (int i = 0; i < num2; i++)
            {
                TypeNode node3 = this.types[i];
                if (node3 != null)
                {
                    if (node3.Namespace == null)
                    {
                        node3.Namespace = Identifier.Empty;
                    }
                    hashtable = (TrivialHashtable) this.namespaceTable[node3.Namespace.UniqueIdKey];
                    if (hashtable == null)
                    {
                        this.namespaceTable[node3.Namespace.UniqueIdKey] = hashtable = new TrivialHashtable();
                    }
                    hashtable[node3.Name.UniqueIdKey] = node3;
                }
            }
            return this.GetType(@namespace, name);
        }

        public virtual TypeNode GetType(Identifier @namespace, Identifier name, bool lookInReferencedAssemblies) => 
            this.GetType(@namespace, name, lookInReferencedAssemblies, lookInReferencedAssemblies ? new TrivialHashtable() : null);

        protected virtual TypeNode GetType(Identifier @namespace, Identifier name, bool lookInReferencedAssemblies, TrivialHashtable assembliesAlreadyVisited)
        {
            if (assembliesAlreadyVisited != null)
            {
                if (assembliesAlreadyVisited[this.UniqueKey] != null)
                {
                    return null;
                }
                assembliesAlreadyVisited[this.UniqueKey] = this;
            }
            TypeNode type = this.GetType(@namespace, name);
            if ((type != null) || !lookInReferencedAssemblies)
            {
                return type;
            }
            AssemblyReferenceList assemblyReferences = this.AssemblyReferences;
            int num = 0;
            int num2 = (assemblyReferences == null) ? 0 : assemblyReferences.Count;
            while (num < num2)
            {
                AssemblyReference reference = assemblyReferences[num];
                if (reference != null)
                {
                    AssemblyNode assembly = reference.Assembly;
                    if (assembly != null)
                    {
                        type = assembly.GetType(@namespace, name, true, assembliesAlreadyVisited);
                        if (type != null)
                        {
                            return type;
                        }
                    }
                }
                num++;
            }
            return null;
        }

        private void GetTypesForNamespace(System.Compiler.Namespace nspace, object handle)
        {
            if ((nspace != null) && (nspace.Name != null))
            {
                lock (GlobalLock)
                {
                    int uniqueIdKey = nspace.Name.UniqueIdKey;
                    TypeNodeList types = this.Types;
                    TypeNodeList list3 = nspace.Types = new TypeNodeList();
                    int num2 = 0;
                    int num3 = (types == null) ? 0 : types.Count;
                    while (num2 < num3)
                    {
                        TypeNode element = types[num2];
                        if (((element != null) && (element.Namespace != null)) && (element.Namespace.UniqueIdKey == uniqueIdKey))
                        {
                            list3.Add(element);
                        }
                        num2++;
                    }
                }
            }
        }

        public virtual bool HasReferenceTo(Module module)
        {
            if (module != null)
            {
                AssemblyNode node = module as AssemblyNode;
                if (node != null)
                {
                    AssemblyReferenceList assemblyReferences = this.AssemblyReferences;
                    int num = 0;
                    int num2 = (assemblyReferences == null) ? 0 : assemblyReferences.Count;
                    while (num < num2)
                    {
                        AssemblyReference reference = assemblyReferences[num];
                        if ((reference != null) && reference.Matches(node.Name, node.Version, node.Culture, node.PublicKeyToken))
                        {
                            return true;
                        }
                        num++;
                    }
                }
                if (this.ContainingAssembly == module.ContainingAssembly)
                {
                    ModuleReferenceList moduleReferences = this.ModuleReferences;
                    int num3 = 0;
                    int num4 = (moduleReferences == null) ? 0 : moduleReferences.Count;
                    while (num3 < num4)
                    {
                        ModuleReference reference2 = moduleReferences[num3];
                        if (((reference2 != null) && (reference2.Name != null)) && (PlatformHelpers.StringCompareOrdinalIgnoreCase(reference2.Name, module.Name) == 0))
                        {
                            return true;
                        }
                        num3++;
                    }
                }
            }
            return false;
        }

        internal void InitializeAssemblyReferenceResolution(Module referringModule)
        {
            if ((this.AssemblyReferenceResolution == null) && (referringModule != null))
            {
                this.AssemblyReferenceResolution = referringModule.AssemblyReferenceResolution;
                this.AssemblyReferenceResolutionAfterProbingFailed = referringModule.AssemblyReferenceResolutionAfterProbingFailed;
            }
        }

        public bool IsValidNamespace(Identifier nsName)
        {
            if (nsName == null)
            {
                return false;
            }
            this.GetNamespaceList();
            return (this.validNamespaces[nsName.UniqueIdKey] != null);
        }

        public bool IsValidTypeName(Identifier nsName, Identifier typeName)
        {
            if ((nsName == null) || (typeName == null))
            {
                return false;
            }
            if (!this.IsValidNamespace(nsName))
            {
                return false;
            }
            if (this.reader != null)
            {
                return this.reader.IsValidTypeName(nsName, typeName);
            }
            return (this.GetType(nsName, typeName) != null);
        }

        public virtual XmlDocument ProbeForXmlDocumentation(string dir, string subDir, string fileName)
        {
            try
            {
                if ((dir == null) || (fileName == null))
                {
                    return null;
                }
                if (subDir != null)
                {
                    dir = Path.Combine(dir, subDir);
                }
                string path = Path.Combine(dir, fileName);
                if (File.Exists(path))
                {
                    XmlDocument document2 = new XmlDocument();
                    using (TextReader reader = File.OpenText(path))
                    {
                        document2.Load(reader);
                        return document2;
                    }
                }
            }
            catch (Exception exception)
            {
                if (this.MetadataImportErrors == null)
                {
                    this.MetadataImportErrors = new ArrayList();
                }
                this.MetadataImportErrors.Add(exception);
            }
            return null;
        }

        public virtual AssemblyNode Resolve(AssemblyReference assemblyReference) => 
            this.AssemblyReferenceResolution?.Invoke(assemblyReference, this);

        public virtual AssemblyNode ResolveAfterProbingFailed(AssemblyReference assemblyReference) => 
            this.AssemblyReferenceResolutionAfterProbingFailed?.Invoke(assemblyReference, this);

        public virtual TypeNode TryGetTemplateInstance(Identifier uniqueMangledName)
        {
            TypeNode node = (TypeNode) this.StructurallyEquivalentType[uniqueMangledName.UniqueIdKey];
            if (node == Class.DoesNotExist)
            {
                return null;
            }
            return node;
        }

        public virtual void WriteDocumentation(TextWriter doc)
        {
            if (this.documentation != null)
            {
                XmlTextWriter xwriter = new XmlTextWriter(doc) {
                    Formatting = Formatting.Indented,
                    Indentation = 2
                };
                xwriter.WriteProcessingInstruction("xml", "version=\"1.0\"");
                xwriter.WriteStartElement("doc");
                AssemblyNode node = this as AssemblyNode;
                if (node != null)
                {
                    xwriter.WriteStartElement("assembly");
                    xwriter.WriteElementString("name", node.Name);
                    xwriter.WriteEndElement();
                }
                xwriter.WriteStartElement("members");
                TypeNodeList types = this.Types;
                int num = 1;
                int num2 = (types == null) ? 0 : types.Count;
                while (num < num2)
                {
                    TypeNode node2 = types[num];
                    if (node2 != null)
                    {
                        node2.WriteDocumentation(xwriter);
                    }
                    num++;
                }
                xwriter.WriteEndElement();
                xwriter.WriteEndElement();
                xwriter.Close();
            }
        }

        public virtual void WriteModule(out byte[] executable)
        {
            Writer.WritePE(out executable, this);
        }

        public virtual void WriteModule(Stream executable, Stream debugSymbols)
        {
            Writer.WritePE(executable, debugSymbols, this);
        }

        public virtual void WriteModule(string location, bool writeDebugSymbols)
        {
            this.Location = location;
            Writer.WritePE(location, writeDebugSymbols, this);
        }

        public virtual void WriteModule(out byte[] executable, out byte[] debugSymbols)
        {
            Writer.WritePE(out executable, out debugSymbols, this);
        }

        public virtual void WriteModule(string location, CompilerParameters options)
        {
            this.Location = location;
            Writer.WritePE(options, this);
        }

        public AssemblyReferenceList AssemblyReferences
        {
            get => 
                this.assemblyReferences;
            set
            {
                this.assemblyReferences = value;
            }
        }

        public virtual AttributeList Attributes
        {
            get
            {
                if (this.attributes != null)
                {
                    return this.attributes;
                }
                if (this.provideCustomAttributes != null)
                {
                    lock (GlobalLock)
                    {
                        if (this.attributes == null)
                        {
                            this.provideCustomAttributes(this);
                        }
                        goto Label_0057;
                    }
                }
                this.attributes = new AttributeList();
            Label_0057:
                return this.attributes;
            }
            set
            {
                this.attributes = value;
            }
        }

        public AssemblyNode ContainingAssembly
        {
            get => 
                this.containingAssembly;
            set
            {
                this.containingAssembly = value;
            }
        }

        public string Directory
        {
            get => 
                this.directory;
            set
            {
                this.directory = value;
            }
        }

        public ushort DllCharacteristics
        {
            get => 
                this.dllCharacteristics;
            set
            {
                this.dllCharacteristics = value;
            }
        }

        public virtual XmlDocument Documentation
        {
            get
            {
                XmlDocument documentation = this.documentation;
                if (documentation != null)
                {
                    return documentation;
                }
                if (this.DocumentationResolution != null)
                {
                    documentation = this.documentation = this.DocumentationResolution(this);
                }
                if (documentation != null)
                {
                    return documentation;
                }
                XmlDocument document3 = null;
                if ((this.Directory != null) && (this.Name != null))
                {
                    string fileName = this.Name + ".xml";
                    for (CultureInfo info = CultureInfo.CurrentUICulture; (info != null) && (info != CultureInfo.InvariantCulture); info = info.Parent)
                    {
                        document3 = this.ProbeForXmlDocumentation(this.Directory, info.Name, fileName);
                        if (document3 != null)
                        {
                            break;
                        }
                    }
                    if (document3 == null)
                    {
                        document3 = this.ProbeForXmlDocumentation(this.Directory, null, fileName);
                    }
                }
                if (document3 == null)
                {
                    document3 = new XmlDocument();
                }
                return (this.documentation = document3);
            }
            set
            {
                this.documentation = value;
            }
        }

        public virtual System.Compiler.Method EntryPoint
        {
            get
            {
                if (this.entryPoint == null)
                {
                    if (this.provideCustomAttributes != null)
                    {
                        if (this.Attributes != null)
                        {
                        }
                    }
                    else
                    {
                        this.entryPoint = NoSuchMethod;
                    }
                }
                if (this.entryPoint == NoSuchMethod)
                {
                    return null;
                }
                return this.entryPoint;
            }
            set
            {
                this.entryPoint = value;
            }
        }

        public AssemblyHashAlgorithm HashAlgorithm
        {
            get => 
                this.hashAlgorithm;
            set
            {
                this.hashAlgorithm = value;
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

        public ModuleKindFlags Kind
        {
            get => 
                this.kind;
            set
            {
                this.kind = value;
            }
        }

        public int LinkerMajorVersion
        {
            get => 
                this.linkerMajorVersion;
            set
            {
                this.linkerMajorVersion = value;
            }
        }

        public int LinkerMinorVersion
        {
            get => 
                this.linkerMinorVersion;
            set
            {
                this.linkerMinorVersion = value;
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

        public int MetadataFormatMajorVersion
        {
            get => 
                this.metadataFormatMajorVersion;
            set
            {
                this.metadataFormatMajorVersion = value;
            }
        }

        public int MetadataFormatMinorVersion
        {
            get => 
                this.metadataFormatMinorVersion;
            set
            {
                this.metadataFormatMinorVersion = value;
            }
        }

        public ArrayList MetadataImportErrors
        {
            get => 
                this.metadataImportErrors;
            set
            {
                this.metadataImportErrors = value;
            }
        }

        public ModuleReferenceList ModuleReferences
        {
            get
            {
                if (this.Types == null)
                {
                    return this.moduleReferences;
                }
                return this.moduleReferences;
            }
            set
            {
                this.moduleReferences = value;
            }
        }

        public Guid Mvid
        {
            get => 
                this.mvid;
            set
            {
                this.mvid = value;
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

        public PEKindFlags PEKind
        {
            get => 
                this.peKind;
            set
            {
                this.peKind = value;
            }
        }

        public bool ProjectTypesContainedInModule
        {
            get
            {
                if (!this.projectModule.HasValue)
                {
                    if (this.Location == null)
                    {
                    }
                    this.projectModule = new bool?(Path.GetExtension("").Equals(".winmd", StringComparison.OrdinalIgnoreCase));
                }
                return this.projectModule.Value;
            }
            set
            {
                this.projectModule = new bool?(value);
            }
        }

        public virtual ResourceList Resources
        {
            get
            {
                if (this.resources != null)
                {
                    return this.resources;
                }
                if (this.provideResources != null)
                {
                    lock (GlobalLock)
                    {
                        if (this.resources == null)
                        {
                            this.provideResources(this);
                        }
                        goto Label_0057;
                    }
                }
                this.resources = new ResourceList();
            Label_0057:
                return this.resources;
            }
            set
            {
                this.resources = value;
            }
        }

        public virtual SecurityAttributeList SecurityAttributes
        {
            get
            {
                if (this.securityAttributes == null)
                {
                    if (this.provideCustomAttributes != null)
                    {
                        if (this.Attributes != null)
                        {
                        }
                    }
                    else
                    {
                        this.securityAttributes = new SecurityAttributeList();
                    }
                }
                return this.securityAttributes;
            }
            set
            {
                this.securityAttributes = value;
            }
        }

        internal TrivialHashtableUsingWeakReferences StructurallyEquivalentType
        {
            get
            {
                if (this.structurallyEquivalentType == null)
                {
                    this.structurallyEquivalentType = new TrivialHashtableUsingWeakReferences();
                }
                return this.structurallyEquivalentType;
            }
        }

        public string TargetRuntimeVersion
        {
            get => 
                this.targetRuntimeVersion;
            set
            {
                this.targetRuntimeVersion = value;
            }
        }

        public bool TrackDebugData
        {
            get => 
                this.trackDebugData;
            set
            {
                this.trackDebugData = value;
            }
        }

        public virtual TypeNodeList Types
        {
            get
            {
                if (this.types != null)
                {
                    return this.types;
                }
                if (this.provideTypeNodeList != null)
                {
                    lock (GlobalLock)
                    {
                        if (this.types == null)
                        {
                            this.provideTypeNodeList(this);
                        }
                        goto Label_0057;
                    }
                }
                this.types = new TypeNodeList();
            Label_0057:
                return this.types;
            }
            set
            {
                this.types = value;
            }
        }

        public virtual Win32ResourceList Win32Resources
        {
            get
            {
                if (this.win32Resources == null)
                {
                    if (this.provideResources != null)
                    {
                        if (this.Resources != null)
                        {
                        }
                    }
                    else
                    {
                        this.win32Resources = new Win32ResourceList();
                    }
                }
                return this.win32Resources;
            }
            set
            {
                this.win32Resources = value;
            }
        }

        public delegate AssemblyNode AssemblyReferenceResolver(AssemblyReference assemblyReference, Module referencingModule);

        public delegate void CustomAttributeProvider(Module module);

        public delegate XmlDocument DocumentationResolver(Module referencingModule);

        public delegate MethodBodySpecializer MethodBodySpecializerFactory(Module m, TypeNodeList pars, TypeNodeList args);

        public delegate void ResourceProvider(Module module);

        public delegate void TypeNodeListProvider(Module module);

        public delegate TypeNode TypeNodeProvider(Identifier @namespace, Identifier name);
    }
}

