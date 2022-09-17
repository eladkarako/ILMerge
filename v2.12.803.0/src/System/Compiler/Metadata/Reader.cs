namespace System.Compiler.Metadata
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Compiler;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    internal class Reader : IDisposable
    {
        private int bufferLength;
        private TypeNodeList currentMethodTypeParameters;
        internal TypeNode currentType;
        private TypeNodeList currentTypeParameters;
        private Dictionary<IntPtr, UnmanagedDocument> debugDocuments;
        internal ISymUnmanagedReader debugReader;
        private string directory;
        private bool doNotLockFile;
        private TypeExtensionProvider dummyTEProvider;
        private string fileName;
        internal bool getDebugSymbols;
        private bool getDebugSymbolsFailed;
        private IDictionary localAssemblyCache;
        private System.Compiler.Module module;
        internal NamespaceList namespaceList;
        private TrivialHashtable namespaceTable;
        internal bool preserveShortBranches;
        private long sortedTablesMask;
        internal static readonly IDictionary StaticAssemblyCache = new SynchronizedWeakDictionary();
        internal MetadataReader tables;
        private TrivialHashtable TypeExtensionTable;
        private UnmanagedBuffer unmanagedBuffer;
        private bool useStaticCache;

        internal Reader(IDictionary localAssemblyCache, bool doNotLockFile, bool getDebugInfo, bool useStaticCache, bool preserveShortBranches)
        {
            this.module = new System.Compiler.Module();
            this.TypeExtensionTable = new TrivialHashtable();
            this.dummyTEProvider = new TypeExtensionProvider(Reader.DummyTypeExtensionProvider);
            if (localAssemblyCache == null)
            {
                localAssemblyCache = new Hashtable();
            }
            this.localAssemblyCache = localAssemblyCache;
            this.directory = System.IO.Directory.GetCurrentDirectory();
            this.getDebugSymbols = getDebugInfo;
            this.doNotLockFile = doNotLockFile;
            this.useStaticCache = useStaticCache;
            this.preserveShortBranches = preserveShortBranches;
        }

        internal unsafe Reader(byte[] buffer, IDictionary localAssemblyCache, bool doNotLockFile, bool getDebugInfo, bool useStaticCache, bool preserveShortBranches)
        {
            this.module = new System.Compiler.Module();
            this.TypeExtensionTable = new TrivialHashtable();
            this.dummyTEProvider = new TypeExtensionProvider(Reader.DummyTypeExtensionProvider);
            if (localAssemblyCache == null)
            {
                localAssemblyCache = new Hashtable();
            }
            this.localAssemblyCache = localAssemblyCache;
            this.getDebugSymbols = getDebugInfo;
            this.doNotLockFile = false;
            this.useStaticCache = useStaticCache;
            this.preserveShortBranches = preserveShortBranches;
            int length = this.bufferLength = buffer.Length;
            this.unmanagedBuffer = new UnmanagedBuffer(length);
            byte* pointer = (byte*) this.unmanagedBuffer.Pointer;
            for (int i = 0; i < length; i++)
            {
                pointer++;
                pointer[0] = buffer[i];
            }
        }

        internal Reader(string fileName, IDictionary localAssemblyCache, bool doNotLockFile, bool getDebugInfo, bool useStaticCache, bool preserveShortBranches)
        {
            this.module = new System.Compiler.Module();
            this.TypeExtensionTable = new TrivialHashtable();
            this.dummyTEProvider = new TypeExtensionProvider(Reader.DummyTypeExtensionProvider);
            if (localAssemblyCache == null)
            {
                localAssemblyCache = new Hashtable();
            }
            this.localAssemblyCache = localAssemblyCache;
            fileName = Path.GetFullPath(fileName);
            this.fileName = fileName;
            this.directory = Path.GetDirectoryName(fileName);
            this.getDebugSymbols = getDebugInfo;
            this.doNotLockFile = doNotLockFile;
            this.useStaticCache = useStaticCache;
            this.preserveShortBranches = preserveShortBranches;
        }

        private void AddEventsToType(TypeNode type, EventRow[] eventDefs, EventPtrRow[] eventPtrs, int start, int end)
        {
            MetadataReader tables = this.tables;
            for (int i = start; i < end; i++)
            {
                int eventIndex = i;
                if (eventPtrs.Length > 0)
                {
                    eventIndex = eventPtrs[i].Event;
                }
                EventRow row = eventDefs[eventIndex - 1];
                Event evnt = new Event {
                    Attributes = this.GetCustomAttributesFor((int) ((eventIndex << 5) | 10)),
                    DeclaringType = type,
                    Flags = (EventFlags) row.Flags,
                    HandlerType = this.DecodeAndGetTypeDefOrRefOrSpec(row.EventType),
                    Name = tables.GetIdentifier(row.Name)
                };
                if (((evnt.Flags & EventFlags.ReservedMask) == EventFlags.None) || (evnt.Name.UniqueIdKey != StandardIds._Deleted.UniqueIdKey))
                {
                    this.AddMethodsToEvent(eventIndex, evnt);
                    type.Members.Add(evnt);
                }
            }
        }

        private void AddFieldsToType(TypeNode type, FieldRow[] fieldDefs, FieldPtrRow[] fieldPtrs, int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                int field = i;
                if (fieldPtrs.Length > 0)
                {
                    field = fieldPtrs[i - 1].Field;
                }
                Field fieldFromDef = this.GetFieldFromDef(field, type);
                if (fieldFromDef != null)
                {
                    type.Members.Add(fieldFromDef);
                }
            }
        }

        private void AddMethodsToEvent(int eventIndex, Event evnt)
        {
            int num = eventIndex << 1;
            MetadataReader tables = this.tables;
            MethodRow[] methodTable = tables.MethodTable;
            MethodSemanticsRow[] methodSemanticsTable = tables.MethodSemanticsTable;
            int index = 0;
            int length = methodSemanticsTable.Length;
            int num4 = length - 1;
            bool flag = ((this.sortedTablesMask >> 0x18) % 2L) == 1L;
            if (flag)
            {
                while (index < num4)
                {
                    int num5 = (index + num4) / 2;
                    if (methodSemanticsTable[num5].Association < num)
                    {
                        index = num5 + 1;
                    }
                    else
                    {
                        num4 = num5;
                    }
                }
                while ((index > 0) && (methodSemanticsTable[index - 1].Association == num))
                {
                    index--;
                }
            }
            MethodFlags compilerControlled = MethodFlags.CompilerControlled;
            while (index < length)
            {
                MethodSemanticsRow row = methodSemanticsTable[index];
                Method element = methodTable[row.Method - 1].Method;
                if (element != null)
                {
                    if (row.Association == num)
                    {
                        element.DeclaringMember = evnt;
                        int semantics = row.Semantics;
                        if (semantics == 8)
                        {
                            evnt.HandlerAdder = element;
                            compilerControlled = element.Flags;
                        }
                        else if (semantics == 0x10)
                        {
                            evnt.HandlerRemover = element;
                            compilerControlled = element.Flags;
                        }
                        else if (semantics == 0x20)
                        {
                            evnt.HandlerCaller = element;
                        }
                        else
                        {
                            if (evnt.OtherMethods == null)
                            {
                                evnt.OtherMethods = new MethodList();
                            }
                            evnt.OtherMethods.Add(element);
                        }
                    }
                    else if (flag)
                    {
                        break;
                    }
                }
                index++;
            }
            evnt.HandlerFlags = compilerControlled;
        }

        private void AddMethodsToProperty(int propIndex, Property property)
        {
            int num = (propIndex << 1) | 1;
            MetadataReader tables = this.tables;
            MethodRow[] methodTable = tables.MethodTable;
            MethodSemanticsRow[] methodSemanticsTable = tables.MethodSemanticsTable;
            int index = 0;
            int length = methodSemanticsTable.Length;
            int num4 = length - 1;
            bool flag = ((this.sortedTablesMask >> 0x18) % 2L) == 1L;
            if (flag)
            {
                while (index < num4)
                {
                    int num5 = (index + num4) / 2;
                    if (methodSemanticsTable[num5].Association < num)
                    {
                        index = num5 + 1;
                    }
                    else
                    {
                        num4 = num5;
                    }
                }
                while ((index > 0) && (methodSemanticsTable[index - 1].Association == num))
                {
                    index--;
                }
            }
            while (index < length)
            {
                MethodSemanticsRow row = methodSemanticsTable[index];
                Method element = methodTable[row.Method - 1].Method;
                if (element != null)
                {
                    if (row.Association == num)
                    {
                        element.DeclaringMember = property;
                        switch (row.Semantics)
                        {
                            case 1:
                                property.Setter = element;
                                goto Label_014C;

                            case 2:
                                property.Getter = element;
                                goto Label_014C;
                        }
                        if (property.OtherMethods == null)
                        {
                            property.OtherMethods = new MethodList();
                        }
                        property.OtherMethods.Add(element);
                    }
                    else if (flag)
                    {
                        return;
                    }
                }
            Label_014C:
                index++;
            }
        }

        private void AddMethodsToType(TypeNode type, MethodPtrRow[] methodPtrs, int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                int index = i;
                if (methodPtrs.Length > 0)
                {
                    index = methodPtrs[i - 1].Method;
                }
                Method methodFromDef = this.GetMethodFromDef(index, type);
                if ((methodFromDef != null) && (((methodFromDef.Flags & MethodFlags.RTSpecialName) == MethodFlags.CompilerControlled) || (methodFromDef.Name.UniqueIdKey != StandardIds._Deleted.UniqueIdKey)))
                {
                    type.members.Add(methodFromDef);
                }
            }
        }

        private void AddMoreStuffToParameters(Method method, ParameterList parameters, int start, int end)
        {
            ParamRow[] paramTable = this.tables.ParamTable;
            int num = (parameters == null) ? 0 : parameters.Count;
            for (int i = start; i < end; i++)
            {
                ParamRow row = paramTable[i - 1];
                if ((row.Sequence == 0) && (method != null))
                {
                    method.ReturnAttributes = this.GetCustomAttributesFor((int) ((i << 5) | 4));
                    if ((row.Flags & 0x2000) != 0)
                    {
                        method.ReturnTypeMarshallingInformation = this.GetMarshallingInformation((i << 1) | 1);
                    }
                    this.AddMoreStuffToParameters(null, parameters, start + 1, end);
                    return;
                }
                int sequence = row.Sequence;
                if (((sequence >= 1) && (sequence <= num)) && (parameters != null))
                {
                    Parameter parameter = parameters[sequence - 1];
                    parameter.Attributes = this.GetCustomAttributesFor((int) ((i << 5) | 4));
                    parameter.Flags = (ParameterFlags) row.Flags;
                    if ((parameter.Flags & ParameterFlags.HasDefault) != ParameterFlags.None)
                    {
                        parameter.DefaultValue = this.GetLiteral((i << 2) | 1, parameter.Type);
                    }
                    if ((parameter.Flags & ParameterFlags.HasFieldMarshal) != ParameterFlags.None)
                    {
                        parameter.MarshallingInformation = this.GetMarshallingInformation((i << 1) | 1);
                    }
                    parameter.Name = this.tables.GetIdentifier(row.Name);
                }
            }
        }

        private void AddPropertiesToType(TypeNode type, PropertyRow[] propertyDefs, PropertyPtrRow[] propertyPtrs, int start, int end)
        {
            MetadataReader tables = this.tables;
            for (int i = start; i < end; i++)
            {
                int propIndex = i;
                if (propertyPtrs.Length > 0)
                {
                    propIndex = propertyPtrs[i - 1].Property;
                }
                PropertyRow row = propertyDefs[propIndex - 1];
                Property property = new Property {
                    Attributes = this.GetCustomAttributesFor((int) ((propIndex << 5) | 9)),
                    DeclaringType = type,
                    Flags = (PropertyFlags) row.Flags,
                    Name = tables.GetIdentifier(row.Name)
                };
                if (((property.Flags & PropertyFlags.RTSpecialName) == PropertyFlags.None) || (property.Name.UniqueIdKey != StandardIds._Deleted.UniqueIdKey))
                {
                    this.AddMethodsToProperty(propIndex, property);
                    type.members.Add(property);
                }
            }
        }

        private static bool CanCacheMember(Member member)
        {
            if ((member.DeclaringType != null) && !CanCacheTypeNode(member.DeclaringType))
            {
                return false;
            }
            if (member.NodeType == NodeType.Method)
            {
                return CanCacheMethodHelper((Method) member);
            }
            return true;
        }

        private static bool CanCacheMethodHelper(Method method)
        {
            if (method.IsGeneric)
            {
                if (method.TemplateArguments == null)
                {
                    return false;
                }
                for (int i = 0; i < method.TemplateArguments.Count; i++)
                {
                    if (!CanCacheTypeNode(method.TemplateArguments[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool CanCacheTypeNode(TypeNode type)
        {
            if ((type.IsGeneric || ((type.Template != null) && type.IsNotFullySpecialized)) || (((type.NodeType == NodeType.TypeParameter) || (type.NodeType == NodeType.ClassParameter)) || (type.NodeType == NodeType.InterfaceExpression)))
            {
                return false;
            }
            TypeNodeList structuralElementTypes = type.StructuralElementTypes;
            int num = 0;
            int num2 = (structuralElementTypes == null) ? 0 : structuralElementTypes.Count;
            while (num < num2)
            {
                if (!CanCacheTypeNode(type.StructuralElementTypes[num]))
                {
                    return false;
                }
                num++;
            }
            return true;
        }

        private TypeNode ConstructCorrectTypeNodeSubclass(int i, Identifier namesp, int firstInterfaceIndex, int lastInterfaceIndex, TypeFlags flags, InterfaceList interfaces, int baseTypeCodedIndex, bool isSystemEnum)
        {
            TypeNode node;
            TypeNode.TypeAttributeProvider provideAttributes = new TypeNode.TypeAttributeProvider(this.GetTypeAttributes);
            TypeNode.NestedTypeProvider provideNestedTypes = new TypeNode.NestedTypeProvider(this.GetNestedTypes);
            TypeNode.TypeMemberProvider provideMembers = new TypeNode.TypeMemberProvider(this.GetTypeMembers);
            bool flag = false;
            if ((flags & TypeFlags.ClassSemanticsMask) != TypeFlags.AnsiClass)
            {
                if (flag)
                {
                    node = new TypeParameter(interfaces, provideNestedTypes, provideAttributes, provideMembers, i);
                }
                else
                {
                    node = new Interface(interfaces, provideNestedTypes, provideAttributes, provideMembers, i);
                }
            }
            else if (flag)
            {
                node = new ClassParameter(provideNestedTypes, provideAttributes, provideMembers, i);
            }
            else
            {
                node = null;
                TypeNode typeIfNotGenericInstance = this.GetTypeIfNotGenericInstance(baseTypeCodedIndex);
                if (typeIfNotGenericInstance != null)
                {
                    if (typeIfNotGenericInstance == CoreSystemTypes.MulticastDelegate)
                    {
                        node = new DelegateNode(provideNestedTypes, provideAttributes, provideMembers, i);
                    }
                    else if ((typeIfNotGenericInstance == CoreSystemTypes.Enum) || ((typeIfNotGenericInstance.Namespace.UniqueIdKey == StandardIds.System.UniqueIdKey) && (typeIfNotGenericInstance.Name.UniqueIdKey == StandardIds.Enum.UniqueIdKey)))
                    {
                        node = new EnumNode(provideNestedTypes, provideAttributes, provideMembers, i);
                    }
                    else if ((typeIfNotGenericInstance == CoreSystemTypes.ValueType) && (!isSystemEnum || ((flags & TypeFlags.Sealed) != TypeFlags.AnsiClass)))
                    {
                        node = new Struct(provideNestedTypes, provideAttributes, provideMembers, i);
                    }
                }
                if (node == null)
                {
                    node = new Class(provideNestedTypes, provideAttributes, provideMembers, i);
                }
            }
            node.Flags = flags;
            node.Interfaces = interfaces;
            return node;
        }

        private Array ConstructCustomAttributeLiteralArray(int numElems, TypeNode elemType)
        {
            if (numElems == -1)
            {
                return null;
            }
            if (numElems < 0)
            {
                throw new InvalidMetadataException(ExceptionStrings.UnexpectedTypeInCustomAttribute);
            }
            switch (elemType.typeCode)
            {
                case ElementType.Boolean:
                    return new bool[numElems];

                case ElementType.Char:
                    return new char[numElems];

                case ElementType.Int8:
                    return new sbyte[numElems];

                case ElementType.UInt8:
                    return new byte[numElems];

                case ElementType.Int16:
                    return new short[numElems];

                case ElementType.UInt16:
                    return new ushort[numElems];

                case ElementType.Int32:
                    return new int[numElems];

                case ElementType.UInt32:
                    return new uint[numElems];

                case ElementType.Int64:
                    return new long[numElems];

                case ElementType.UInt64:
                    return new ulong[numElems];

                case ElementType.Single:
                    return new float[numElems];

                case ElementType.Double:
                    return new double[numElems];

                case ElementType.String:
                    return new string[numElems];

                case ElementType.ValueType:
                {
                    TypeNode type = elemType;
                    EnumNode customAttributeEnumNode = GetCustomAttributeEnumNode(ref type);
                    return this.ConstructCustomAttributeLiteralArray(numElems, customAttributeEnumNode.UnderlyingType);
                }
                case ElementType.Class:
                    return new TypeNode[numElems];

                case ElementType.Object:
                    return new object[numElems];

                case ElementType.SzArray:
                    throw new InvalidMetadataException(ExceptionStrings.BadCustomAttributeTypeEncodedToken);
            }
            throw new InvalidMetadataException(ExceptionStrings.UnexpectedTypeInCustomAttribute);
        }

        private TypeNode DecodeAndGetTypeDefOrRefOrSpec(int codedIndex)
        {
            if (codedIndex == 0)
            {
                return null;
            }
            switch ((codedIndex & 3))
            {
                case 0:
                    return this.GetTypeFromDef(codedIndex >> 2);

                case 1:
                    return this.GetTypeFromRef(codedIndex >> 2);

                case 2:
                    return this.GetTypeFromSpec(codedIndex >> 2);
            }
            throw new InvalidMetadataException(ExceptionStrings.BadTypeDefOrRef);
        }

        private TypeNode DecodeAndGetTypeDefOrRefOrSpec(int codedIndex, bool expectStruct)
        {
            if (codedIndex == 0)
            {
                return null;
            }
            switch ((codedIndex & 3))
            {
                case 0:
                    return this.GetTypeFromDef(codedIndex >> 2);

                case 1:
                    return this.GetTypeFromRef(codedIndex >> 2, expectStruct);

                case 2:
                    return this.GetTypeFromSpec(codedIndex >> 2);
            }
            throw new InvalidMetadataException(ExceptionStrings.BadTypeDefOrRef);
        }

        public void Dispose()
        {
            if (this.unmanagedBuffer != null)
            {
                this.unmanagedBuffer.Dispose();
            }
            this.unmanagedBuffer = null;
            if (this.tables != null)
            {
                this.tables.Dispose();
            }
            if (this.debugReader != null)
            {
                Marshal.ReleaseComObject(this.debugReader);
            }
            this.debugReader = null;
            this.debugDocuments = null;
        }

        private static TypeNode DummyTypeExtensionProvider(TypeNode.NestedTypeProvider nprovider, TypeNode.TypeAttributeProvider aprovider, TypeNode.TypeMemberProvider mprovider, TypeNode baseType, object handle) => 
            null;

        private static int FindFirstCommaOutsideBrackets(string serializedName)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = (serializedName == null) ? 0 : serializedName.Length;
            while (num3 < num4)
            {
                char ch = serializedName[num3];
                switch (ch)
                {
                    case '[':
                        num++;
                        break;

                    case ']':
                        if (--num < 0)
                        {
                            return -1;
                        }
                        break;

                    case '<':
                        num2++;
                        break;

                    case '>':
                        if (--num2 < 0)
                        {
                            return -1;
                        }
                        break;

                    default:
                        if (((ch == ',') && (num == 0)) && (num2 == 0))
                        {
                            return num3;
                        }
                        break;
                }
                num3++;
            }
            return -1;
        }

        private static void GetAndCheckSignatureToken(int expectedToken, MemoryCursor sigReader)
        {
            if (sigReader.ReadCompressedInt() != expectedToken)
            {
                throw new InvalidMetadataException(ExceptionStrings.MalformedSignature);
            }
        }

        internal AssemblyNode GetAssemblyFromReference(AssemblyReference assemblyReference)
        {
            lock (System.Compiler.Module.GlobalLock)
            {
                if ((SystemAssemblyLocation.ParsedAssembly != null) && (((assemblyReference.Name == "mscorlib") || (assemblyReference.Name == "basetypes")) || ((assemblyReference.Name == "ioconfig") || (assemblyReference.Name == "singularity.v1"))))
                {
                    return SystemAssemblyLocation.ParsedAssembly;
                }
                if ((CoreSystemTypes.SystemAssembly != null) && (CoreSystemTypes.SystemAssembly.Name == assemblyReference.Name))
                {
                    return CoreSystemTypes.SystemAssembly;
                }
                string strongName = null;
                object obj3 = null;
                if ((assemblyReference.PublicKeyOrToken == null) || (assemblyReference.PublicKeyOrToken.Length == 0))
                {
                    if (assemblyReference.Location != null)
                    {
                        obj3 = this.localAssemblyCache[assemblyReference.Location];
                    }
                    if (obj3 == null)
                    {
                        obj3 = this.localAssemblyCache[assemblyReference.Name];
                        if ((obj3 != null) && (assemblyReference.Location != null))
                        {
                            this.localAssemblyCache[assemblyReference.Location] = obj3;
                        }
                    }
                }
                else
                {
                    strongName = assemblyReference.StrongName;
                    if (this.useStaticCache)
                    {
                        if (assemblyReference.Location != null)
                        {
                            obj3 = StaticAssemblyCache[assemblyReference.Location];
                        }
                        if (obj3 == null)
                        {
                            obj3 = StaticAssemblyCache[strongName];
                        }
                    }
                    if (obj3 == null)
                    {
                        obj3 = this.localAssemblyCache[strongName];
                    }
                }
                if (obj3 == null)
                {
                    AssemblyReference reference = (AssemblyReference) TargetPlatform.AssemblyReferenceFor[Identifier.For(assemblyReference.Name).UniqueIdKey];
                    if (((reference != null) && (assemblyReference.Version != null)) && ((reference.Version >= assemblyReference.Version) && reference.MatchesIgnoringVersion(assemblyReference)))
                    {
                        AssemblyNode assembly = reference.assembly;
                        if (assembly == null)
                        {
                            assembly = AssemblyNode.GetAssembly(reference.Location, this.doNotLockFile, this.getDebugSymbols, this.useStaticCache, this.ReferringAssemblyPostLoad);
                        }
                        if (assembly != null)
                        {
                            if (strongName == null)
                            {
                                strongName = assemblyReference.Name;
                            }
                            lock (StaticAssemblyCache)
                            {
                                if (reference.Location != null)
                                {
                                    StaticAssemblyCache[reference.Location] = assembly;
                                }
                                StaticAssemblyCache[strongName] = assembly;
                            }
                            reference.assembly = assembly;
                            return assembly;
                        }
                    }
                }
                AssemblyNode node3 = obj3 as AssemblyNode;
                if (node3 != null)
                {
                    goto Label_088C;
                }
                if (this.module != null)
                {
                    node3 = this.module.Resolve(assemblyReference);
                    if (node3 != null)
                    {
                        if (strongName == null)
                        {
                            this.localAssemblyCache[node3.Name] = node3;
                            if (node3.Location != null)
                            {
                                this.localAssemblyCache[node3.Location] = node3;
                            }
                            goto Label_088C;
                        }
                        if ((CoreSystemTypes.SystemAssembly != null) && (CoreSystemTypes.SystemAssembly.Name == node3.Name))
                        {
                            return CoreSystemTypes.SystemAssembly;
                        }
                        lock (StaticAssemblyCache)
                        {
                            if (this.useStaticCache)
                            {
                                if (node3.Location != null)
                                {
                                    StaticAssemblyCache[node3.Location] = node3;
                                }
                                StaticAssemblyCache[strongName] = node3;
                            }
                            else
                            {
                                this.localAssemblyCache[strongName] = node3;
                                if (node3.Location != null)
                                {
                                    this.localAssemblyCache[node3.Location] = node3;
                                }
                            }
                            goto Label_088C;
                        }
                    }
                }
                if (this.directory != null)
                {
                    string str2 = Path.Combine(this.directory, assemblyReference.Name + ".dll");
                    if (File.Exists(str2))
                    {
                        node3 = AssemblyNode.GetAssembly(str2, this.localAssemblyCache, this.doNotLockFile, this.getDebugSymbols, this.useStaticCache, this.ReferringAssemblyPostLoad);
                        if ((node3 != null) && ((strongName == null) || assemblyReference.Matches(node3.Name, node3.Version, node3.Culture, node3.PublicKeyToken)))
                        {
                            goto Label_081F;
                        }
                    }
                    str2 = Path.Combine(this.directory, assemblyReference.Name + ".exe");
                    if (File.Exists(str2))
                    {
                        node3 = AssemblyNode.GetAssembly(str2, this.localAssemblyCache, this.doNotLockFile, this.getDebugSymbols, this.useStaticCache, this.ReferringAssemblyPostLoad);
                        if ((node3 != null) && ((strongName == null) || assemblyReference.Matches(node3.Name, node3.Version, node3.Culture, node3.PublicKeyToken)))
                        {
                            goto Label_081F;
                        }
                    }
                    str2 = Path.Combine(this.directory, assemblyReference.Name + ".winmd");
                    if (File.Exists(str2))
                    {
                        node3 = AssemblyNode.GetAssembly(str2, this.localAssemblyCache, this.doNotLockFile, this.getDebugSymbols, this.useStaticCache, this.ReferringAssemblyPostLoad);
                        if ((node3 != null) && ((strongName == null) || assemblyReference.Matches(node3.Name, node3.Version, node3.Culture, node3.PublicKeyToken)))
                        {
                            goto Label_081F;
                        }
                    }
                    str2 = Path.Combine(this.directory, assemblyReference.Name + ".ill");
                    if (File.Exists(str2))
                    {
                        node3 = AssemblyNode.GetAssembly(str2, this.localAssemblyCache, this.doNotLockFile, this.getDebugSymbols, this.useStaticCache, this.ReferringAssemblyPostLoad);
                        if ((node3 != null) && ((strongName == null) || assemblyReference.Matches(node3.Name, node3.Version, node3.Culture, node3.PublicKeyToken)))
                        {
                            goto Label_081F;
                        }
                    }
                }
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyReference.Name + ".dll");
                if (File.Exists(path))
                {
                    node3 = AssemblyNode.GetAssembly(path, this.localAssemblyCache, this.doNotLockFile, this.getDebugSymbols, this.useStaticCache, this.ReferringAssemblyPostLoad);
                    if ((node3 != null) && ((strongName == null) || assemblyReference.Matches(node3.Name, node3.Version, node3.Culture, node3.PublicKeyToken)))
                    {
                        goto Label_081F;
                    }
                }
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyReference.Name + ".exe");
                if (File.Exists(path))
                {
                    node3 = AssemblyNode.GetAssembly(path, this.localAssemblyCache, this.doNotLockFile, this.getDebugSymbols, this.useStaticCache, this.ReferringAssemblyPostLoad);
                    if ((node3 != null) && ((strongName == null) || assemblyReference.Matches(node3.Name, node3.Version, node3.Culture, node3.PublicKeyToken)))
                    {
                        goto Label_081F;
                    }
                }
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyReference.Name + ".winmd");
                if (File.Exists(path))
                {
                    node3 = AssemblyNode.GetAssembly(path, this.localAssemblyCache, this.doNotLockFile, this.getDebugSymbols, this.useStaticCache, this.ReferringAssemblyPostLoad);
                    if ((node3 != null) && ((strongName == null) || assemblyReference.Matches(node3.Name, node3.Version, node3.Culture, node3.PublicKeyToken)))
                    {
                        goto Label_081F;
                    }
                }
                node3 = null;
                string location = null;
                if (strongName == null)
                {
                    goto Label_088C;
                }
                location = GlobalAssemblyCache.GetLocation(assemblyReference);
                if ((location != null) && (location.Length == 0))
                {
                    location = null;
                }
                if (location == null)
                {
                    goto Label_088C;
                }
                node3 = AssemblyNode.GetAssembly(location, this.useStaticCache ? StaticAssemblyCache : this.localAssemblyCache, this.doNotLockFile, this.getDebugSymbols, this.useStaticCache, this.ReferringAssemblyPostLoad);
                if (node3 == null)
                {
                    goto Label_088C;
                }
                lock (StaticAssemblyCache)
                {
                    if (this.useStaticCache)
                    {
                        StaticAssemblyCache[location] = node3;
                        StaticAssemblyCache[strongName] = node3;
                    }
                    else
                    {
                        this.localAssemblyCache[location] = node3;
                        this.localAssemblyCache[strongName] = node3;
                    }
                    goto Label_088C;
                }
            Label_081F:
                if (strongName == null)
                {
                    this.localAssemblyCache[node3.Name] = node3;
                    if (node3.Location != null)
                    {
                        this.localAssemblyCache[node3.Location] = node3;
                    }
                }
                else
                {
                    this.localAssemblyCache[strongName] = node3;
                    if (node3.Location != null)
                    {
                        this.localAssemblyCache[node3.Location] = node3;
                    }
                }
            Label_088C:
                if (node3 != null)
                {
                    node3.InitializeAssemblyReferenceResolution(this.module);
                }
                if (node3 == null)
                {
                    if (this.module != null)
                    {
                        node3 = this.module.ResolveAfterProbingFailed(assemblyReference);
                        if (node3 != null)
                        {
                            goto Label_081F;
                        }
                        HandleError(this.module, string.Format(CultureInfo.CurrentCulture, ExceptionStrings.AssemblyReferenceNotResolved, new object[] { assemblyReference.StrongName }));
                    }
                    node3 = new AssemblyNode {
                        Culture = assemblyReference.Culture,
                        Name = assemblyReference.Name,
                        PublicKeyOrToken = assemblyReference.PublicKeyOrToken,
                        Version = assemblyReference.Version,
                        Location = "unknown:location"
                    };
                    goto Label_081F;
                }
                return node3;
            }
        }

        private AssemblyNode GetCachedAssembly(AssemblyNode assembly)
        {
            if ((assembly.PublicKeyOrToken == null) || (assembly.PublicKeyOrToken.Length == 0))
            {
                AssemblyNode node = null;
                if (assembly.Location != null)
                {
                    node = this.localAssemblyCache[assembly.Location] as AssemblyNode;
                }
                if ((node == null) && (assembly.Name != null))
                {
                    node = this.localAssemblyCache[assembly.Name] as AssemblyNode;
                    if ((node != null) && (assembly.Location != null))
                    {
                        this.localAssemblyCache[assembly.Location] = node;
                    }
                }
                if (node != null)
                {
                    if ((node.reader != this) && (node.reader != null))
                    {
                        if ((this.getDebugSymbols && !node.reader.getDebugSymbols) && !node.reader.getDebugSymbolsFailed)
                        {
                            node.SetupDebugReader(null);
                        }
                        this.Dispose();
                    }
                    return node;
                }
                lock (StaticAssemblyCache)
                {
                    if (assembly.Name != null)
                    {
                        this.localAssemblyCache[assembly.Name] = assembly;
                    }
                    if (this.fileName != null)
                    {
                        this.localAssemblyCache[this.fileName] = assembly;
                    }
                    goto Label_0501;
                }
            }
            string strongName = assembly.StrongName;
            AssemblyNode node2 = null;
            if (this.useStaticCache)
            {
                AssemblyReference reference = new AssemblyReference(assembly);
                AssemblyReference reference2 = (AssemblyReference) TargetPlatform.AssemblyReferenceFor[Identifier.For(reference.Name).UniqueIdKey];
                if (((reference2 != null) && (reference.Version != null)) && ((reference2.Version >= reference.Version) && reference2.MatchesIgnoringVersion(reference)))
                {
                    AssemblyNode node3 = reference2.assembly;
                    if (node3 == null)
                    {
                        if (Path.GetFullPath(reference2.Location) == assembly.Location)
                        {
                            if (reference2.Version != reference.Version)
                            {
                                HandleError(assembly, string.Format(CultureInfo.CurrentCulture, ExceptionStrings.BadTargetPlatformLocation, new object[] { assembly.Name, TargetPlatform.PlatformAssembliesLocation, assembly.Version, reference2.Version }));
                            }
                            lock (StaticAssemblyCache)
                            {
                                StaticAssemblyCache[strongName] = assembly;
                                if (reference2.Location != null)
                                {
                                    StaticAssemblyCache[reference2.Location] = assembly;
                                }
                                reference2.Assembly = assembly;
                            }
                            return null;
                        }
                        node3 = AssemblyNode.GetAssembly(reference2.Location, this.doNotLockFile, this.getDebugSymbols, this.useStaticCache);
                    }
                    if (node3 != null)
                    {
                        lock (StaticAssemblyCache)
                        {
                            if (reference2.Location != null)
                            {
                                StaticAssemblyCache[reference2.Location] = node3;
                            }
                            StaticAssemblyCache[strongName] = node3;
                        }
                        return (reference2.assembly = node3);
                    }
                }
                node2 = StaticAssemblyCache[strongName] as AssemblyNode;
                if (node2 != null)
                {
                    if ((((reference2 == null) && (assembly.FileLastWriteTimeUtc > node2.FileLastWriteTimeUtc)) && ((assembly.Location != null) && (node2.Location != null))) && (assembly.Location == node2.Location))
                    {
                        lock (StaticAssemblyCache)
                        {
                            StaticAssemblyCache[strongName] = assembly;
                        }
                        return null;
                    }
                    if ((node2.reader != this) && (node2.reader != null))
                    {
                        if ((this.getDebugSymbols && !node2.reader.getDebugSymbols) && !node2.reader.getDebugSymbolsFailed)
                        {
                            node2.SetupDebugReader(null);
                        }
                        this.Dispose();
                    }
                    return node2;
                }
                lock (StaticAssemblyCache)
                {
                    StaticAssemblyCache[strongName] = assembly;
                    if (this.fileName != null)
                    {
                        StaticAssemblyCache[this.fileName] = assembly;
                    }
                    goto Label_0501;
                }
            }
            node2 = this.localAssemblyCache[strongName] as AssemblyNode;
            if (node2 != null)
            {
                if (((assembly.FileLastWriteTimeUtc > node2.FileLastWriteTimeUtc) && (assembly.Location != null)) && ((node2.Location != null) && (assembly.Location == node2.Location)))
                {
                    this.localAssemblyCache[strongName] = assembly;
                    return null;
                }
                if ((node2.reader != this) && (node2.reader != null))
                {
                    if ((this.getDebugSymbols && (node2.reader.debugReader == null)) && !node2.reader.getDebugSymbolsFailed)
                    {
                        node2.SetupDebugReader(null);
                    }
                    this.Dispose();
                }
                return node2;
            }
            this.localAssemblyCache[strongName] = assembly;
            if (this.fileName != null)
            {
                this.localAssemblyCache[this.fileName] = assembly;
            }
        Label_0501:
            return null;
        }

        internal FunctionPointer GetCalliSignature(int ssigToken)
        {
            StandAloneSigRow row = this.tables.StandAloneSigTable[(ssigToken & 0xffffff) - 1];
            MemoryCursor blobCursor = this.tables.GetBlobCursor(row.Signature);
            return this.ParseFunctionPointer(blobCursor);
        }

        private void GetClassSizeAndPackingSize(int i, TypeNode result)
        {
            ClassLayoutRow[] classLayoutTable = this.tables.ClassLayoutTable;
            int index = 0;
            int length = classLayoutTable.Length;
            while (index < length)
            {
                ClassLayoutRow row = classLayoutTable[index];
                if (row.Parent == i)
                {
                    result.ClassSize = row.ClassSize;
                    result.PackingSize = row.PackingSize;
                    return;
                }
                index++;
            }
        }

        private Method GetConstructorDefOrRef(int codedIndex, out TypeNodeList varArgTypes)
        {
            varArgTypes = null;
            switch ((codedIndex & 7))
            {
                case 2:
                    return this.GetMethodFromDef(codedIndex >> 3);

                case 3:
                    return (Method) this.GetMemberFromRef(codedIndex >> 3, out varArgTypes);
            }
            throw new InvalidMetadataException(ExceptionStrings.BadCustomAttributeTypeEncodedToken);
        }

        private AttributeNode GetCustomAttribute(int i)
        {
            TypeNodeList list;
            int num;
            CustomAttributeRow row = this.tables.CustomAttributeTable[i];
            Method constructorDefOrRef = this.GetConstructorDefOrRef(row.Constructor, out list);
            if (constructorDefOrRef == null)
            {
                constructorDefOrRef = new Method();
            }
            MemoryCursor blobCursor = this.tables.GetBlobCursor(row.Value, out num);
            return this.GetCustomAttribute(constructorDefOrRef, blobCursor, num);
        }

        private AttributeNode GetCustomAttribute(Method cons, MemoryCursor sigReader, int blobLength)
        {
            AttributeNode node = new AttributeNode {
                Constructor = new MemberBinding(null, cons)
            };
            int n = (cons.Parameters == null) ? 0 : cons.Parameters.Count;
            ExpressionList arguments = node.Expressions = new ExpressionList(n);
            int position = sigReader.Position;
            sigReader.ReadUInt16();
            for (int i = 0; i < n; i++)
            {
                TypeNode node2 = TypeNode.StripModifiers(cons.Parameters[i].Type);
                if (node2 != null)
                {
                    TypeNode type = node2;
                    object customAttributeLiteralValue = null;
                    try
                    {
                        customAttributeLiteralValue = this.GetCustomAttributeLiteralValue(sigReader, ref type);
                    }
                    catch (Exception exception)
                    {
                        if (this.module.MetadataImportErrors == null)
                        {
                            this.module.MetadataImportErrors = new ArrayList();
                        }
                        this.module.MetadataImportErrors.Add(exception);
                    }
                    Literal element = customAttributeLiteralValue as Literal;
                    if (element == null)
                    {
                        element = new Literal(customAttributeLiteralValue, type);
                    }
                    arguments.Add(element);
                }
            }
            if ((sigReader.Position + 1) < (position + blobLength))
            {
                ushort numNamed = sigReader.ReadUInt16();
                this.GetCustomAttributeNamedArguments(arguments, numNamed, sigReader);
            }
            return node;
        }

        private static EnumNode GetCustomAttributeEnumNode(ref TypeNode type)
        {
            EnumNode node = type as EnumNode;
            if ((node == null) || (node.UnderlyingType == null))
            {
                node = new EnumNode {
                    Name = type.Name,
                    Namespace = type.Namespace,
                    DeclaringModule = type.DeclaringModule,
                    UnderlyingType = CoreSystemTypes.Int32
                };
                type = node;
            }
            return node;
        }

        private Array GetCustomAttributeLiteralArray(MemoryCursor sigReader, int numElems, TypeNode elemType)
        {
            Array array = this.ConstructCustomAttributeLiteralArray(numElems, elemType);
            for (int i = 0; i < numElems; i++)
            {
                object customAttributeLiteralValue = this.GetCustomAttributeLiteralValue(sigReader, elemType);
                array.SetValue(customAttributeLiteralValue, i);
            }
            return array;
        }

        private object GetCustomAttributeLiteralValue(MemoryCursor sigReader, TypeNode type)
        {
            TypeNode node = type;
            object customAttributeLiteralValue = this.GetCustomAttributeLiteralValue(sigReader, ref node);
            EnumNode node2 = node as EnumNode;
            if ((node2 != null) && (type == CoreSystemTypes.Object))
            {
                return new Literal(customAttributeLiteralValue, node2);
            }
            if (((type == CoreSystemTypes.Object) && (node != CoreSystemTypes.Object)) && !(customAttributeLiteralValue is Literal))
            {
                customAttributeLiteralValue = new Literal(customAttributeLiteralValue, node);
            }
            return customAttributeLiteralValue;
        }

        private object GetCustomAttributeLiteralValue(MemoryCursor sigReader, ref TypeNode type)
        {
            if (type == null)
            {
                return sigReader.ReadInt32();
            }
            switch (type.typeCode)
            {
                case ElementType.Boolean:
                    return sigReader.ReadBoolean();

                case ElementType.Char:
                    return sigReader.ReadChar();

                case ElementType.Int8:
                    return sigReader.ReadSByte();

                case ElementType.UInt8:
                    return sigReader.ReadByte();

                case ElementType.Int16:
                    return sigReader.ReadInt16();

                case ElementType.UInt16:
                    return sigReader.ReadUInt16();

                case ElementType.Int32:
                    return sigReader.ReadInt32();

                case ElementType.UInt32:
                    return sigReader.ReadUInt32();

                case ElementType.Int64:
                    return sigReader.ReadInt64();

                case ElementType.UInt64:
                    return sigReader.ReadUInt64();

                case ElementType.Single:
                    return sigReader.ReadSingle();

                case ElementType.Double:
                    return sigReader.ReadDouble();

                case ElementType.String:
                    return ReadSerString(sigReader);

                case ElementType.ValueType:
                {
                    EnumNode customAttributeEnumNode = GetCustomAttributeEnumNode(ref type);
                    object customAttributeLiteralValue = this.GetCustomAttributeLiteralValue(sigReader, customAttributeEnumNode.UnderlyingType);
                    if (((this.module.ContainingAssembly != null) && ((this.module.ContainingAssembly.Flags & AssemblyFlags.ContainsForeignTypes) != AssemblyFlags.None)) && (customAttributeEnumNode == SystemTypes.AttributeTargets))
                    {
                        int num = (int) customAttributeLiteralValue;
                        if (num > 0x40)
                        {
                            if (num <= 0x100)
                            {
                                switch (num)
                                {
                                    case 0x80:
                                        return 0x800;

                                    case 0x100:
                                        return 0x80;
                                }
                                return customAttributeLiteralValue;
                            }
                            switch (num)
                            {
                                case 0x200:
                                    return 4;

                                case 0x400:
                                    return 8;

                                case 0x800:
                                    return 0;
                            }
                            return customAttributeLiteralValue;
                        }
                        if (num > 8)
                        {
                            switch (num)
                            {
                                case 0x10:
                                    return 0x400;

                                case 0x20:
                                    return 0;

                                case 0x40:
                                    return 0x40;
                            }
                            return customAttributeLiteralValue;
                        }
                        switch (num)
                        {
                            case -1:
                                return 0x7fff;

                            case 0:
                            case 3:
                                return customAttributeLiteralValue;

                            case 1:
                                return 0x1000;

                            case 2:
                                return 0x10;

                            case 4:
                                return 0x200;

                            case 8:
                                return 0x100;
                        }
                    }
                    return customAttributeLiteralValue;
                }
                case ElementType.Class:
                    return this.GetTypeFromSerializedName(ReadSerString(sigReader));

                case ElementType.Object:
                    type = this.ParseTypeSignature(sigReader);
                    return this.GetCustomAttributeLiteralValue(sigReader, ref type);

                case ElementType.SzArray:
                {
                    int numElems = sigReader.ReadInt32();
                    TypeNode elementType = ((ArrayType) type).ElementType;
                    return this.GetCustomAttributeLiteralArray(sigReader, numElems, elementType);
                }
            }
            throw new InvalidMetadataException(ExceptionStrings.UnexpectedTypeInCustomAttribute);
        }

        private void GetCustomAttributeNamedArguments(ExpressionList arguments, ushort numNamed, MemoryCursor sigReader)
        {
            for (int i = 0; i < numNamed; i++)
            {
                int num2 = sigReader.ReadByte();
                bool flag = sigReader.Byte(0) == 0x51;
                TypeNode type = this.ParseTypeSignature(sigReader);
                Identifier name = sigReader.ReadIdentifierFromSerString();
                object customAttributeLiteralValue = this.GetCustomAttributeLiteralValue(sigReader, ref type);
                Literal literal = customAttributeLiteralValue as Literal;
                if (literal == null)
                {
                    literal = new Literal(customAttributeLiteralValue, type);
                }
                NamedArgument element = new NamedArgument(name, literal) {
                    Type = type,
                    IsCustomAttributeProperty = num2 == 0x54,
                    ValueIsBoxed = flag
                };
                arguments.Add(element);
            }
        }

        private void GetCustomAttributesFor(System.Compiler.Module module)
        {
            try
            {
                if (this.tables.entryPointToken != 0)
                {
                    module.EntryPoint = (Method) this.GetMemberFromToken(this.tables.entryPointToken);
                }
                else
                {
                    module.EntryPoint = System.Compiler.Module.NoSuchMethod;
                }
                if (module.NodeType == NodeType.Module)
                {
                    module.Attributes = this.GetCustomAttributesNonNullFor(0x27);
                }
                else
                {
                    AssemblyNode node = (AssemblyNode) module;
                    node.SecurityAttributes = this.GetSecurityAttributesFor(6);
                    node.Attributes = this.GetCustomAttributesNonNullFor(0x2e);
                    node.ModuleAttributes = this.GetCustomAttributesNonNullFor(0x27);
                }
            }
            catch (Exception exception)
            {
                if (this.module != null)
                {
                    if (this.module.MetadataImportErrors == null)
                    {
                        this.module.MetadataImportErrors = new ArrayList();
                    }
                    this.module.MetadataImportErrors.Add(exception);
                    module.Attributes = new AttributeList(0);
                }
            }
        }

        private AttributeList GetCustomAttributesFor(int parentIndex)
        {
            CustomAttributeRow[] customAttributeTable = this.tables.CustomAttributeTable;
            AttributeList list = null;
            try
            {
                int index = 0;
                int length = customAttributeTable.Length;
                int num3 = length - 1;
                if (length == 0)
                {
                    return null;
                }
                bool flag = ((this.sortedTablesMask >> 12) % 2L) == 1L;
                if (flag)
                {
                    while (index < num3)
                    {
                        int num4 = (index + num3) / 2;
                        if (customAttributeTable[num4].Parent < parentIndex)
                        {
                            index = num4 + 1;
                        }
                        else
                        {
                            num3 = num4;
                        }
                    }
                    while ((index > 0) && (customAttributeTable[index - 1].Parent == parentIndex))
                    {
                        index--;
                    }
                }
                int capacity = 0;
                for (int i = index; i < length; i++)
                {
                    if (customAttributeTable[i].Parent == parentIndex)
                    {
                        capacity++;
                    }
                    else if (flag)
                    {
                        break;
                    }
                }
                if (capacity > 0)
                {
                    list = new AttributeList(capacity);
                }
                while (index < length)
                {
                    if (customAttributeTable[index].Parent == parentIndex)
                    {
                        list.Add(this.GetCustomAttribute(index));
                    }
                    else if (flag)
                    {
                        return list;
                    }
                    index++;
                }
            }
            catch (Exception exception)
            {
                if (this.module == null)
                {
                    return list;
                }
                if (this.module.MetadataImportErrors == null)
                {
                    this.module.MetadataImportErrors = new ArrayList();
                }
                this.module.MetadataImportErrors.Add(exception);
            }
            return list;
        }

        private AttributeList GetCustomAttributesNonNullFor(int parentIndex)
        {
            AttributeList customAttributesFor = this.GetCustomAttributesFor(parentIndex);
            if (customAttributesFor != null)
            {
                return customAttributesFor;
            }
            return new AttributeList(0);
        }

        private TypeNode GetDummyTypeNode(Identifier namesp, Identifier name, System.Compiler.Module declaringModule, TypeNode declaringType, bool expectStruct)
        {
            TypeNode element = null;
            if (this.module != null)
            {
                string str = (declaringModule == null) ? "" : ((declaringModule.Name == null) ? "" : declaringModule.Name.ToString());
                string str2 = (declaringType != null) ? declaringType.FullName : namesp?.Name;
                HandleError(this.module, string.Format(CultureInfo.CurrentCulture, ExceptionStrings.CouldNotResolveTypeReference, new object[] { string.Concat(new object[] { "[", str, "]", str2, ".", name }) }));
            }
            element = expectStruct ? ((TypeNode) new Struct()) : ((TypeNode) new Class());
            if (((name != null) && name.ToString().StartsWith("I")) && ((name.ToString().Length > 1) && char.IsUpper(name.ToString()[1])))
            {
                element = new Interface();
            }
            element.Flags |= TypeFlags.Public;
            element.Name = name;
            element.Namespace = namesp;
            if (declaringType != null)
            {
                element.DeclaringType = declaringType;
                element.DeclaringType.DeclaringModule = declaringType.DeclaringModule;
                declaringType.Members.Add(element);
                return element;
            }
            if (declaringModule == null)
            {
                declaringModule = this.module;
            }
            element.DeclaringModule = declaringModule;
            if (declaringModule.types != null)
            {
                declaringModule.types.Add(element);
            }
            return element;
        }

        internal Field GetFieldFromDef(int i) => 
            this.GetFieldFromDef(i, null);

        internal Field GetFieldFromDef(int i, TypeNode declaringType)
        {
            FieldRow[] fieldTable = this.tables.FieldTable;
            FieldRow row = fieldTable[i - 1];
            if (row.Field != null)
            {
                return row.Field;
            }
            Field field = new Field();
            fieldTable[i - 1].Field = field;
            field.Attributes = this.GetCustomAttributesFor((int) ((i << 5) | 1));
            field.Flags = (FieldFlags) row.Flags;
            field.Name = this.tables.GetIdentifier(row.Name);
            if (((field.Flags & FieldFlags.RTSpecialName) != FieldFlags.CompilerControlled) && (field.Name.UniqueIdKey == StandardIds._Deleted.UniqueIdKey))
            {
                return null;
            }
            this.tables.GetSignatureLength(row.Signature);
            MemoryCursor newCursor = this.tables.GetNewCursor();
            GetAndCheckSignatureToken(6, newCursor);
            field.Type = this.ParseTypeSignature(newCursor);
            RequiredModifier type = field.Type as RequiredModifier;
            if ((type != null) && (type.Modifier == CoreSystemTypes.IsVolatile))
            {
                field.IsVolatile = true;
                field.Type = type.ModifiedType;
            }
            if ((field.Flags & FieldFlags.HasDefault) != FieldFlags.CompilerControlled)
            {
                field.DefaultValue = this.GetLiteral(i << 2, field.Type);
            }
            if ((field.Flags & FieldFlags.HasFieldMarshal) != FieldFlags.CompilerControlled)
            {
                field.MarshallingInformation = this.GetMarshallingInformation(i << 1);
            }
            if ((field.Flags & FieldFlags.HasFieldRVA) != FieldFlags.CompilerControlled)
            {
                field.InitialData = this.GetInitialData(i, field.Type, out field.section);
            }
            if (declaringType == null)
            {
                TypeDefRow[] typeDefTable = this.tables.TypeDefTable;
                int num = i;
                FieldPtrRow[] fieldPtrTable = this.tables.FieldPtrTable;
                int length = fieldPtrTable.Length;
                for (int j = 0; j < length; j++)
                {
                    if (fieldPtrTable[j].Field == i)
                    {
                        num = j + 1;
                        break;
                    }
                }
                length = typeDefTable.Length;
                for (int k = length - 1; k >= 0; k--)
                {
                    TypeDefRow row2 = typeDefTable[k];
                    if (row2.FieldList <= num)
                    {
                        declaringType = this.GetTypeFromDef(k + 1);
                        break;
                    }
                }
            }
            field.DeclaringType = declaringType;
            if ((declaringType != null) && ((declaringType.Flags & TypeFlags.ExplicitLayout) != TypeFlags.AnsiClass))
            {
                FieldLayoutRow[] fieldLayoutTable = this.tables.FieldLayoutTable;
                int num5 = fieldLayoutTable.Length;
                for (int m = num5 - 1; m >= 0; m--)
                {
                    FieldLayoutRow row3 = fieldLayoutTable[m];
                    if (row3.Field == i)
                    {
                        field.Offset = row3.Offset;
                        return field;
                    }
                }
            }
            return field;
        }

        private TypeNode GetForwardedTypeFromName(Identifier Namespace, Identifier name)
        {
            ExportedTypeRow[] exportedTypeTable = this.tables.ExportedTypeTable;
            int index = 0;
            int num2 = (exportedTypeTable == null) ? 0 : exportedTypeTable.Length;
            while (index < num2)
            {
                ExportedTypeRow row = exportedTypeTable[index];
                if ((((row.Flags & 0x200000) != 0) && (this.tables.GetString(row.TypeNamespace) == Namespace.Name)) && (this.tables.GetString(row.TypeName) == name.Name))
                {
                    int num3 = row.Implementation >> 2;
                    AssemblyRefRow row2 = this.tables.AssemblyRefTable[num3 - 1];
                    return row2.AssemblyReference.Assembly.GetType(Namespace, name);
                }
                index++;
            }
            return this.GetReplacedTypeFromName(Namespace, name);
        }

        private TypeNode GetGenericParameter(int index, int parameterListIndex, Member parent)
        {
            GenericParamRow row = this.tables.GenericParamTable[index++];
            string name = this.tables.GetString(row.Name);
            GenericParamConstraintRow[] genericParamConstraintTable = this.tables.GenericParamConstraintTable;
            bool flag = false;
            int num = 0;
            int length = genericParamConstraintTable.Length;
            int num3 = length - 1;
            bool flag2 = ((this.sortedTablesMask >> 0x2c) % 2L) == 1L;
            if (flag2)
            {
                while (num < num3)
                {
                    int num4 = (num + num3) / 2;
                    if (genericParamConstraintTable[num4].Param < index)
                    {
                        num = num4 + 1;
                    }
                    else
                    {
                        num3 = num4;
                    }
                }
                while ((num > 0) && (genericParamConstraintTable[num - 1].Param == index))
                {
                    num--;
                }
            }
            while ((num < length) && !flag)
            {
                if (genericParamConstraintTable[num].Param == index)
                {
                    flag = this.TypeDefOrRefOrSpecIsClass(genericParamConstraintTable[num].Constraint);
                }
                else if (flag2)
                {
                    break;
                }
                num++;
            }
            if (flag)
            {
                ClassParameter parameter = (parent is Method) ? new MethodClassParameter() : new ClassParameter();
                parameter.DeclaringMember = parent;
                parameter.ParameterListIndex = parameterListIndex;
                parameter.Name = Identifier.For(name);
                parameter.DeclaringModule = this.module;
                parameter.TypeParameterFlags = (TypeParameterFlags) row.Flags;
                parameter.ProvideTypeAttributes = new TypeNode.TypeAttributeProvider(this.GetTypeParameterAttributes);
                parameter.ProviderHandle = index;
                return parameter;
            }
            TypeParameter parameter2 = (parent is Method) ? new MethodTypeParameter() : new TypeParameter();
            parameter2.DeclaringMember = parent;
            parameter2.ParameterListIndex = parameterListIndex;
            parameter2.Name = Identifier.For(name);
            parameter2.DeclaringModule = this.module;
            parameter2.TypeParameterFlags = (TypeParameterFlags) row.Flags;
            parameter2.ProvideTypeAttributes = new TypeNode.TypeAttributeProvider(this.GetTypeParameterAttributes);
            parameter2.ProviderHandle = index;
            return parameter2;
        }

        private void GetGenericParameterConstraints(int index, ref TypeNode parameter)
        {
            index++;
            GenericParamConstraintRow[] genericParamConstraintTable = this.tables.GenericParamConstraintTable;
            TypeNodeList list = new TypeNodeList();
            Class class2 = null;
            InterfaceList list2 = new InterfaceList();
            int num = 0;
            int length = genericParamConstraintTable.Length;
            int num3 = length - 1;
            bool flag = ((this.sortedTablesMask >> 0x2c) % 2L) == 1L;
            if (flag)
            {
                while (num < num3)
                {
                    int num4 = (num + num3) / 2;
                    if (genericParamConstraintTable[num4].Param < index)
                    {
                        num = num4 + 1;
                    }
                    else
                    {
                        num3 = num4;
                    }
                }
                while ((num > 0) && (genericParamConstraintTable[num - 1].Param == index))
                {
                    num--;
                }
            }
            while (num < length)
            {
                if (genericParamConstraintTable[num].Param == index)
                {
                    TypeNode element = this.DecodeAndGetTypeDefOrRefOrSpec(genericParamConstraintTable[num].Constraint);
                    Class class3 = element as Class;
                    if (class3 != null)
                    {
                        class2 = class3;
                    }
                    else if (element is Interface)
                    {
                        list2.Add((Interface) element);
                    }
                    list.Add(element);
                }
                else if (flag)
                {
                    break;
                }
                num++;
            }
            ClassParameter parameter2 = parameter as ClassParameter;
            if ((parameter2 == null) && (class2 != null))
            {
                parameter2 = (((ITypeParameter) parameter).DeclaringMember is Method) ? new MethodClassParameter() : new ClassParameter();
                parameter2.Name = parameter.Name;
                parameter2.DeclaringMember = ((ITypeParameter) parameter).DeclaringMember;
                parameter2.ParameterListIndex = ((ITypeParameter) parameter).ParameterListIndex;
                parameter2.DeclaringModule = this.module;
                parameter2.TypeParameterFlags = ((ITypeParameter) parameter).TypeParameterFlags;
                parameter2.ProvideTypeAttributes = new TypeNode.TypeAttributeProvider(this.GetTypeParameterAttributes);
                parameter2.ProviderHandle = index;
                parameter = parameter2;
            }
            if (parameter2 != null)
            {
                parameter2.structuralElementTypes = list;
            }
            else
            {
                ((TypeParameter) parameter).structuralElementTypes = list;
            }
            if ((class2 != null) && (parameter2 != null))
            {
                parameter2.BaseClass = class2;
            }
            parameter.Interfaces = list2;
        }

        private static int GetInheritedTypeParameterCount(TypeNode type)
        {
            if (type == null)
            {
                return 0;
            }
            int num = 0;
            type = type.DeclaringType;
            while (type != null)
            {
                num += (type.templateParameters == null) ? 0 : type.templateParameters.Count;
                type = type.DeclaringType;
            }
            return num;
        }

        private byte[] GetInitialData(int fieldIndex, TypeNode fieldType, out PESection targetSection)
        {
            targetSection = PESection.Text;
            FieldRvaRow[] fieldRvaTable = this.tables.FieldRvaTable;
            bool flag = ((this.sortedTablesMask >> 0x1d) % 2L) == 1L;
            int index = 0;
            int length = fieldRvaTable.Length;
            int num3 = length - 1;
            if (length == 0)
            {
                return null;
            }
            if (!flag)
            {
                while (index < num3)
                {
                    if (fieldRvaTable[index].Field == fieldIndex)
                    {
                        break;
                    }
                    index++;
                }
            }
            else
            {
                while (index < num3)
                {
                    int num4 = (index + num3) / 2;
                    if (fieldRvaTable[num4].Field < fieldIndex)
                    {
                        index = num4 + 1;
                    }
                    else
                    {
                        num3 = num4;
                    }
                }
            }
            FieldRvaRow row = fieldRvaTable[index];
            if (row.Field != fieldIndex)
            {
                return null;
            }
            Field field = this.tables.FieldTable[fieldIndex - 1].Field;
            if (field != null)
            {
                field.Offset = row.RVA;
            }
            fieldType = TypeNode.StripModifiers(fieldType);
            EnumNode node = fieldType as EnumNode;
            if (node != null)
            {
                fieldType = TypeNode.StripModifiers(node.UnderlyingType);
            }
            if (fieldType == null)
            {
                return null;
            }
            int classSize = fieldType.ClassSize;
            if (classSize <= 0)
            {
                switch (fieldType.typeCode)
                {
                    case ElementType.Boolean:
                        classSize = 1;
                        goto Label_0226;

                    case ElementType.Char:
                        classSize = 2;
                        goto Label_0226;

                    case ElementType.Int8:
                        classSize = 1;
                        goto Label_0226;

                    case ElementType.UInt8:
                        classSize = 1;
                        goto Label_0226;

                    case ElementType.Int16:
                        classSize = 2;
                        goto Label_0226;

                    case ElementType.UInt16:
                        classSize = 2;
                        goto Label_0226;

                    case ElementType.Int32:
                        classSize = 4;
                        goto Label_0226;

                    case ElementType.UInt32:
                        classSize = 4;
                        goto Label_0226;

                    case ElementType.Int64:
                        classSize = 8;
                        goto Label_0226;

                    case ElementType.UInt64:
                        classSize = 8;
                        goto Label_0226;

                    case ElementType.Single:
                        classSize = 4;
                        goto Label_0226;

                    case ElementType.Double:
                        classSize = 8;
                        goto Label_0226;
                }
                if ((fieldType is System.Compiler.Pointer) || (fieldType is FunctionPointer))
                {
                    classSize = 4;
                }
                else if (index < (length - 1))
                {
                    classSize = fieldRvaTable[index + 1].RVA - row.RVA;
                }
                else if (targetSection != PESection.Text)
                {
                    classSize = this.tables.GetOffsetToEndOfSection(row.RVA);
                }
            }
        Label_0226:
            if (classSize <= 0)
            {
                return null;
            }
            if (this.tables.NoOffsetFor(row.RVA) || this.tables.NoOffsetFor((row.RVA + classSize) - 1))
            {
                return null;
            }
            MemoryCursor newCursor = this.tables.GetNewCursor(row.RVA, out targetSection);
            byte[] buffer = new byte[classSize];
            for (index = 0; index < classSize; index++)
            {
                buffer[index] = newCursor.ReadByte();
            }
            return buffer;
        }

        internal TypeNodeList GetInstantiatedTypes()
        {
            TypeNodeList list = null;
            TypeDefRow[] typeDefTable = this.tables.TypeDefTable;
            int index = 0;
            int length = typeDefTable.Length;
            while (index < length)
            {
                TypeNode type = typeDefTable[index].Type;
                if (type != null)
                {
                    if (list == null)
                    {
                        list = new TypeNodeList();
                    }
                    list.Add(type);
                }
                index++;
            }
            return list;
        }

        private void GetInterfaceIndices(int i, out int firstInterfaceIndex, out int lastInterfaceIndex)
        {
            firstInterfaceIndex = -1;
            lastInterfaceIndex = -1;
            InterfaceImplRow[] interfaceImplTable = this.tables.InterfaceImplTable;
            int index = 0;
            int length = interfaceImplTable.Length;
            while (index < length)
            {
                if (interfaceImplTable[index].Class == i)
                {
                    if (firstInterfaceIndex == -1)
                    {
                        firstInterfaceIndex = index;
                    }
                    lastInterfaceIndex = index;
                }
                index++;
            }
        }

        private void GetInterfaces(int i, int firstInterfaceIndex, InterfaceList interfaces)
        {
            InterfaceImplRow[] interfaceImplTable = this.tables.InterfaceImplTable;
            int index = firstInterfaceIndex;
            int length = interfaceImplTable.Length;
            while (index < length)
            {
                if (interfaceImplTable[index].Class == i)
                {
                    TypeNode node = this.DecodeAndGetTypeDefOrRefOrSpec(interfaceImplTable[index].Interface);
                    Interface element = node as Interface;
                    if (element == null)
                    {
                        element = new Interface();
                        if (node != null)
                        {
                            element.DeclaringModule = node.DeclaringModule;
                            element.Namespace = node.Namespace;
                            element.Name = node.Name;
                        }
                    }
                    interfaces.Add(element);
                    AttributeList customAttributesFor = this.GetCustomAttributesFor((int) ((index << 5) | 5));
                    if (customAttributesFor != null)
                    {
                        interfaces.AddAttributes(interfaces.Count - 1, customAttributesFor);
                    }
                }
                index++;
            }
        }

        private Literal GetLiteral(int parentCodedIndex, TypeNode type)
        {
            ConstantRow[] constantTable = this.tables.ConstantTable;
            int index = 0;
            int length = constantTable.Length;
            while (index < length)
            {
                if (constantTable[index].Parent == parentCodedIndex)
                {
                    object valueFromBlob = this.tables.GetValueFromBlob(constantTable[index].Type, constantTable[index].Value);
                    TypeCode typeCode = Convert.GetTypeCode(valueFromBlob);
                    TypeNode underlyingType = type;
                    if (type is EnumNode)
                    {
                        underlyingType = ((EnumNode) type).UnderlyingType;
                    }
                    if (underlyingType.TypeCode != typeCode)
                    {
                        type = CoreSystemTypes.Object;
                    }
                    if ((type == CoreSystemTypes.Object) && (valueFromBlob != null))
                    {
                        switch (typeCode)
                        {
                            case TypeCode.Empty:
                            case TypeCode.Object:
                                type = CoreSystemTypes.Type;
                                break;

                            case TypeCode.Boolean:
                                type = CoreSystemTypes.Boolean;
                                break;

                            case TypeCode.Char:
                                type = CoreSystemTypes.Char;
                                break;

                            case TypeCode.SByte:
                                type = CoreSystemTypes.Int8;
                                break;

                            case TypeCode.Byte:
                                type = CoreSystemTypes.UInt8;
                                break;

                            case TypeCode.Int16:
                                type = CoreSystemTypes.Int16;
                                break;

                            case TypeCode.UInt16:
                                type = CoreSystemTypes.UInt16;
                                break;

                            case TypeCode.Int32:
                                type = CoreSystemTypes.Int32;
                                break;

                            case TypeCode.UInt32:
                                type = CoreSystemTypes.UInt32;
                                break;

                            case TypeCode.Int64:
                                type = CoreSystemTypes.Int64;
                                break;

                            case TypeCode.UInt64:
                                type = CoreSystemTypes.UInt64;
                                break;

                            case TypeCode.Single:
                                type = CoreSystemTypes.Single;
                                break;

                            case TypeCode.Double:
                                type = CoreSystemTypes.Double;
                                break;

                            case TypeCode.String:
                                type = CoreSystemTypes.String;
                                break;
                        }
                    }
                    return new Literal(valueFromBlob, type);
                }
                index++;
            }
            throw new InvalidMetadataException(ExceptionStrings.BadConstantParentIndex);
        }

        internal void GetLocals(int localIndex, LocalList locals, Dictionary<int, LocalInfo> localSourceNames)
        {
            if (localIndex != 0)
            {
                StandAloneSigRow row = this.tables.StandAloneSigTable[(localIndex & 0xffffff) - 1];
                this.tables.GetSignatureLength(row.Signature);
                MemoryCursor newCursor = this.tables.GetNewCursor();
                if (newCursor.ReadByte() != 7)
                {
                    throw new InvalidMetadataException(ExceptionStrings.InvalidLocalSignature);
                }
                int num = newCursor.ReadCompressedInt();
                for (int i = 0; i < num; i++)
                {
                    LocalInfo info;
                    bool flag = localSourceNames.TryGetValue(i, out info);
                    string name = info.Name;
                    string str2 = string.IsNullOrEmpty(name) ? ("local" + i) : name;
                    bool pinned = false;
                    TypeNode type = this.ParseTypeSignature(newCursor, ref pinned);
                    Local element = new Local(Identifier.For(str2), type) {
                        Pinned = pinned,
                        HasNoPDBInfo = !flag,
                        Attributes = info.Attributes
                    };
                    locals.Add(element);
                }
            }
        }

        internal void GetLocalSourceNames(ISymUnmanagedScope scope, Dictionary<int, LocalInfo> localSourceNames)
        {
            uint num6;
            uint localCount = scope.GetLocalCount();
            IntPtr[] locals = new IntPtr[localCount];
            scope.GetLocals((uint) locals.Length, out localCount, locals);
            char[] name = new char[100];
            for (int i = 0; i < localCount; i++)
            {
                ISymUnmanagedVariable typedObjectForIUnknown = (ISymUnmanagedVariable) Marshal.GetTypedObjectForIUnknown(locals[i], typeof(ISymUnmanagedVariable));
                if (typedObjectForIUnknown != null)
                {
                    uint num3;
                    typedObjectForIUnknown.GetName(0, out num3, null);
                    if (num3 > name.Length)
                    {
                        name = new char[num3];
                    }
                    typedObjectForIUnknown.GetName((uint) name.Length, out num3, name);
                    int num4 = (int) typedObjectForIUnknown.GetAddressField1();
                    string str = new string(name, 0, ((int) num3) - 1);
                    uint attributes = typedObjectForIUnknown.GetAttributes();
                    localSourceNames[num4] = new LocalInfo(str, attributes);
                    Marshal.ReleaseComObject(typedObjectForIUnknown);
                }
                Marshal.Release(locals[i]);
            }
            IntPtr[] children = new IntPtr[100];
            scope.GetChildren((uint) children.Length, out num6, children);
            for (int j = 0; j < num6; j++)
            {
                ISymUnmanagedScope scope2 = (ISymUnmanagedScope) Marshal.GetTypedObjectForIUnknown(children[j], typeof(ISymUnmanagedScope));
                if (scope2 != null)
                {
                    this.GetLocalSourceNames(scope2, localSourceNames);
                    Marshal.ReleaseComObject(scope2);
                }
                Marshal.Release(children[j]);
            }
        }

        private MarshallingInformation GetMarshallingInformation(int parentCodedIndex)
        {
            FieldMarshalRow[] fieldMarshalTable = this.tables.FieldMarshalTable;
            bool flag = ((this.sortedTablesMask >> 13) % 2L) == 1L;
            int index = 0;
            int length = fieldMarshalTable.Length;
            int num3 = length - 1;
            if (length == 0)
            {
                return null;
            }
            if (!flag)
            {
                while (index < num3)
                {
                    if (fieldMarshalTable[index].Parent == parentCodedIndex)
                    {
                        break;
                    }
                    index++;
                }
            }
            else
            {
                while (index < num3)
                {
                    int num4 = (index + num3) / 2;
                    if (fieldMarshalTable[num4].Parent < parentCodedIndex)
                    {
                        index = num4 + 1;
                    }
                    else
                    {
                        num3 = num4;
                    }
                }
                while ((index > 0) && (fieldMarshalTable[index - 1].Parent == parentCodedIndex))
                {
                    index--;
                }
            }
            FieldMarshalRow row = fieldMarshalTable[index];
            if (row.Parent != parentCodedIndex)
            {
                return null;
            }
            MarshallingInformation information = new MarshallingInformation();
            int blobLength = 0;
            MemoryCursor blobCursor = this.tables.GetBlobCursor(row.NativeType, out blobLength);
            int position = blobCursor.Position;
            information.NativeType = (NativeType) blobCursor.ReadByte();
            if (information.NativeType == NativeType.CustomMarshaler)
            {
                blobCursor.ReadUInt16();
                information.Class = ReadSerString(blobCursor);
                information.Cookie = ReadSerString(blobCursor);
                return information;
            }
            if (blobLength > 1)
            {
                if (information.NativeType == NativeType.LPArray)
                {
                    information.ElementType = (NativeType) blobCursor.ReadByte();
                    information.ParamIndex = -1;
                    int num7 = 2;
                    if (num7 < blobLength)
                    {
                        int num8 = blobCursor.Position;
                        information.ParamIndex = blobCursor.ReadCompressedInt();
                        num7 += blobCursor.Position - num8;
                        if (num7 >= blobLength)
                        {
                            return information;
                        }
                        num8 = blobCursor.Position;
                        information.ElementSize = blobCursor.ReadCompressedInt();
                        num7 += blobCursor.Position - num8;
                        if (num7 < blobLength)
                        {
                            information.NumberOfElements = blobCursor.ReadCompressedInt();
                        }
                    }
                    return information;
                }
                if (information.NativeType == NativeType.SafeArray)
                {
                    information.ElementType = (NativeType) blobCursor.ReadByte();
                    if (blobCursor.Position < ((position + blobLength) - 1))
                    {
                        information.Class = ReadSerString(blobCursor);
                    }
                    return information;
                }
                information.Size = blobCursor.ReadCompressedInt();
                if (information.NativeType != NativeType.ByValArray)
                {
                    return information;
                }
                if (blobCursor.Position < (position + blobLength))
                {
                    information.ElementType = (NativeType) blobCursor.ReadByte();
                    return information;
                }
                information.ElementType = NativeType.NotSpecified;
            }
            return information;
        }

        internal Member GetMemberFromRef(int i, out TypeNodeList varArgTypes) => 
            this.GetMemberFromRef(i, out varArgTypes, 0);

        internal Member GetMemberFromRef(int i, out TypeNodeList varArgTypes, int numGenericArgs)
        {
            MemoryCursor cursor2;
            int num7;
            CallingConventionFlags flags;
            TypeNode node6;
            MemberRefRow row = this.tables.MemberRefTable[i - 1];
            if (row.Member != null)
            {
                varArgTypes = row.VarargTypes;
                return row.Member;
            }
            varArgTypes = null;
            Member element = null;
            int num = row.Class;
            if (num == 0)
            {
                return null;
            }
            TypeNode declaringType = null;
            TypeNodeList currentTypeParameters = this.currentTypeParameters;
            switch ((num & 7))
            {
                case 0:
                    declaringType = this.GetTypeFromDef(num >> 3);
                    break;

                case 1:
                    declaringType = this.GetTypeFromRef(num >> 3);
                    break;

                case 2:
                    declaringType = this.GetTypeGlobalMemberContainerTypeFromModule(num >> 3);
                    break;

                case 3:
                    element = this.GetMethodFromDef(num >> 3);
                    if ((((Method) element).CallingConvention & CallingConventionFlags.VarArg) != CallingConventionFlags.Default)
                    {
                        MemoryCursor blobCursor = this.tables.GetBlobCursor(row.Signature);
                        blobCursor.ReadByte();
                        int num3 = blobCursor.ReadCompressedInt();
                        this.ParseTypeSignature(blobCursor);
                        bool genericParameterEncountered = false;
                        this.ParseParameterTypes(out varArgTypes, blobCursor, num3, ref genericParameterEncountered);
                    }
                    goto Label_071F;

                case 4:
                    declaringType = this.GetTypeFromSpec(num >> 3);
                    break;

                default:
                    throw new InvalidMetadataException("");
            }
            if ((declaringType != null) && declaringType.IsGeneric)
            {
                if (declaringType.Template != null)
                {
                    this.currentTypeParameters = declaringType.ConsolidatedTemplateArguments;
                }
                else
                {
                    this.currentTypeParameters = declaringType.ConsolidatedTemplateParameters;
                }
            }
            Identifier name = this.tables.GetIdentifier(row.Name);
            Method method = null;
        Label_017D:
            cursor2 = this.tables.GetBlobCursor(row.Signature);
            byte num4 = cursor2.ReadByte();
            if (num4 != 6)
            {
                num7 = -2147483648;
                flags = CallingConventionFlags.Default;
                if ((num4 & 0x20) != 0)
                {
                    flags |= CallingConventionFlags.HasThis;
                }
                if ((num4 & 0x40) != 0)
                {
                    flags |= CallingConventionFlags.ExplicitThis;
                }
                switch ((num4 & 7))
                {
                    case 1:
                        flags |= CallingConventionFlags.C;
                        break;

                    case 2:
                        flags |= CallingConventionFlags.StandardCall;
                        break;

                    case 3:
                        flags |= CallingConventionFlags.ThisCall;
                        break;

                    case 4:
                        flags |= CallingConventionFlags.FastCall;
                        break;

                    case 5:
                        flags |= CallingConventionFlags.VarArg;
                        break;
                }
            }
            else
            {
                Class class2;
                TypeNode type = this.ParseTypeSignature(cursor2);
                TypeNode node3 = TypeNode.StripModifiers(type);
                for (TypeNode node4 = declaringType; node4 != null; node4 = class2.BaseClass)
                {
                    MemberList members = node4.Members;
                    int num5 = 0;
                    int count = members.Count;
                    while (num5 < count)
                    {
                        Field field = members[num5] as Field;
                        if (((field != null) && (field.Name.UniqueIdKey == name.UniqueIdKey)) && (TypeNode.StripModifiers(field.Type) == node3))
                        {
                            element = field;
                            goto Label_071F;
                        }
                        num5++;
                    }
                    class2 = node4 as Class;
                    if (class2 == null)
                    {
                        break;
                    }
                }
                if (element != null)
                {
                    goto Label_071F;
                }
                element = new Field(name) {
                    DeclaringType = declaringType
                };
                ((Field) element).Type = type;
                goto Label_06C2;
            }
            if ((num4 & 0x10) != 0)
            {
                num7 = cursor2.ReadCompressedInt();
                flags |= CallingConventionFlags.Generic;
            }
            int paramCount = cursor2.ReadCompressedInt();
            TypeNodeList currentMethodTypeParameters = this.currentMethodTypeParameters;
            TypeNode baseClass = declaringType;
            if (numGenericArgs > 0)
            {
                bool flag2 = method != null;
                while (baseClass != null)
                {
                    MemberList membersNamed = baseClass.GetMembersNamed(name);
                    int num10 = 0;
                    int num11 = membersNamed.Count;
                    while (num10 < num11)
                    {
                        Method method2 = membersNamed[num10] as Method;
                        if (method2 != null)
                        {
                            if (flag2)
                            {
                                if (method2 == method)
                                {
                                    flag2 = false;
                                }
                            }
                            else if (((method2.TemplateParameters != null) && (method2.TemplateParameters.Count == numGenericArgs)) && ((method2.Parameters != null) && (method2.Parameters.Count == paramCount)))
                            {
                                this.currentMethodTypeParameters = method2.TemplateParameters;
                                this.currentTypeParameters = baseClass.ConsolidatedTemplateArguments;
                                method = method2;
                                goto Label_0438;
                            }
                        }
                        num10++;
                    }
                    Class class3 = baseClass as Class;
                    if (class3 == null)
                    {
                        break;
                    }
                    baseClass = class3.BaseClass;
                }
                method = null;
            }
        Label_0438:
            node6 = this.ParseTypeSignature(cursor2);
            if (node6 == null)
            {
                node6 = CoreSystemTypes.Object;
            }
            bool isGeneric = node6.IsGeneric;
            TypeNodeList argumentTypes = this.ParseParameterTypes(out varArgTypes, cursor2, paramCount, ref isGeneric);
            this.currentMethodTypeParameters = currentMethodTypeParameters;
            this.currentTypeParameters = currentTypeParameters;
            baseClass = declaringType;
            while (baseClass != null)
            {
                MemberList constructors = baseClass.GetMembersNamed(name);
                int num12 = 0;
                int num13 = constructors.Count;
                while (num12 < num13)
                {
                    Method method3 = constructors[num12] as Method;
                    if (((((method3 != null) && (method3.ReturnType != null)) && (TypeNode.StripModifiers(method3.ReturnType).IsStructurallyEquivalentTo(TypeNode.StripModifiers(node6)) && method3.ParameterTypesMatchStructurally(argumentTypes))) && (method3.CallingConvention == flags)) && ((num7 == -2147483648) || ((method3.IsGeneric && (method3.TemplateParameters != null)) && (method3.TemplateParameters.Count == num7))))
                    {
                        element = method3;
                        goto Label_071F;
                    }
                    num12++;
                }
                if (name.UniqueIdKey == StandardIds.Ctor.UniqueIdKey)
                {
                    constructors = baseClass.GetConstructors();
                    if (((constructors == null) || (constructors.Count != 1)) || (paramCount != 0))
                    {
                        break;
                    }
                    element = constructors[0];
                    goto Label_071F;
                }
                Class class4 = baseClass as Class;
                if (class4 != null)
                {
                    baseClass = class4.BaseClass;
                }
                else
                {
                    Interface interface2 = baseClass as Interface;
                    if (interface2 != null)
                    {
                        int num14 = 0;
                        int num15 = (interface2 == null) ? 0 : interface2.Interfaces.Count;
                        while (num14 < num15)
                        {
                            element = this.SearchBaseInterface(interface2.Interfaces[num14], name, node6, argumentTypes, num7, flags);
                            if (element != null)
                            {
                                goto Label_071F;
                            }
                            num14++;
                        }
                    }
                    break;
                }
            }
            if (element == null)
            {
                if (method != null)
                {
                    goto Label_017D;
                }
                ParameterList parameters = new ParameterList(paramCount);
                for (int j = 0; j < paramCount; j++)
                {
                    Parameter parameter = new Parameter(Identifier.Empty, argumentTypes[j]);
                    parameters.Add(parameter);
                }
                Method method4 = new Method(declaringType, null, name, parameters, node6, null) {
                    CallingConvention = flags
                };
                if ((flags & CallingConventionFlags.HasThis) == CallingConventionFlags.Default)
                {
                    method4.Flags |= MethodFlags.Static;
                }
                element = method4;
            }
        Label_06C2:
            if (this.module != null)
            {
                HandleError(this.module, string.Format(CultureInfo.CurrentCulture, ExceptionStrings.CouldNotResolveMemberReference, new object[] { declaringType.FullName + "::" + name }));
                if (declaringType != null)
                {
                    declaringType.Members.Add(element);
                }
            }
        Label_071F:
            if (CanCacheMember(element))
            {
                this.tables.MemberRefTable[i - 1].Member = element;
                this.tables.MemberRefTable[i - 1].VarargTypes = varArgTypes;
            }
            this.currentTypeParameters = currentTypeParameters;
            return element;
        }

        internal Member GetMemberFromToken(int tok)
        {
            TypeNodeList list;
            return this.GetMemberFromToken(tok, out list);
        }

        internal Member GetMemberFromToken(int tok, out TypeNodeList varArgTypes)
        {
            varArgTypes = null;
            Member typeFromSpec = null;
            switch (((TableIndices) (tok >> 0x18)))
            {
                case TableIndices.TypeSpec:
                    typeFromSpec = this.GetTypeFromSpec(tok & 0xffffff);
                    break;

                case TableIndices.MethodSpec:
                    typeFromSpec = this.GetMethodFromSpec(tok & 0xffffff);
                    break;

                case TableIndices.TypeRef:
                    typeFromSpec = this.GetTypeFromRef(tok & 0xffffff);
                    break;

                case TableIndices.TypeDef:
                    typeFromSpec = this.GetTypeFromDef(tok & 0xffffff);
                    break;

                case TableIndices.Field:
                    typeFromSpec = this.GetFieldFromDef(tok & 0xffffff);
                    break;

                case TableIndices.Method:
                    typeFromSpec = this.GetMethodFromDef(tok & 0xffffff);
                    break;

                case TableIndices.MemberRef:
                    typeFromSpec = this.GetMemberFromRef(tok & 0xffffff, out varArgTypes);
                    break;

                default:
                    throw new InvalidMetadataException(ExceptionStrings.BadMemberToken);
            }
            if (typeFromSpec == null)
            {
                throw new InvalidMetadataException(ExceptionStrings.BadMemberToken);
            }
            return typeFromSpec;
        }

        private void GetMethodAttributes(Method method, object handle)
        {
            TypeNodeList currentTypeParameters = this.currentTypeParameters;
            TypeNodeList currentMethodTypeParameters = this.currentMethodTypeParameters;
            try
            {
                MetadataReader tables = this.tables;
                int num = (int) handle;
                MethodRow[] methodTable = tables.MethodTable;
                int length = methodTable.Length;
                if ((num < 1) || (num > length))
                {
                    throw new ArgumentOutOfRangeException("handle", ExceptionStrings.InvalidTypeTableIndex);
                }
                MethodRow row = methodTable[num - 1];
                if (method != row.Method)
                {
                    throw new ArgumentOutOfRangeException("handle", ExceptionStrings.InvalidTypeTableIndex);
                }
                method.Attributes = this.GetCustomAttributesNonNullFor(num << 5);
                this.currentTypeParameters = currentTypeParameters;
                this.currentMethodTypeParameters = currentMethodTypeParameters;
                if ((method.Flags & MethodFlags.HasSecurity) != MethodFlags.CompilerControlled)
                {
                    method.SecurityAttributes = this.GetSecurityAttributesFor((num << 2) | 1);
                }
            }
            catch (Exception exception)
            {
                if (this.module != null)
                {
                    if (this.module.MetadataImportErrors == null)
                    {
                        this.module.MetadataImportErrors = new ArrayList();
                    }
                    this.module.MetadataImportErrors.Add(exception);
                }
                method.Attributes = new AttributeList(0);
                this.currentTypeParameters = currentTypeParameters;
                this.currentMethodTypeParameters = currentMethodTypeParameters;
            }
        }

        private void GetMethodBody(Method method, object i, bool asInstructionList)
        {
            if (asInstructionList)
            {
                this.GetMethodInstructions(method, i);
            }
            else
            {
                TypeNodeList currentMethodTypeParameters = this.currentMethodTypeParameters;
                this.currentMethodTypeParameters = method.templateParameters;
                TypeNode currentType = this.currentType;
                this.currentType = method.DeclaringType;
                TypeNodeList currentTypeParameters = this.currentTypeParameters;
                this.currentTypeParameters = this.currentType.TemplateParameters;
                try
                {
                    StatementList list3;
                    MethodRow row = this.tables.MethodTable[((int) i) - 1];
                    if ((row.RVA != 0) && ((row.ImplFlags & 4) == 0))
                    {
                        if (this.getDebugSymbols)
                        {
                            this.GetMethodDebugSymbols(method, (uint) (0x6000000 | ((int) i)));
                        }
                        list3 = this.ParseMethodBody(method, (int) i, row.RVA);
                    }
                    else
                    {
                        list3 = new StatementList(0);
                    }
                    method.Body = new Block(list3);
                    method.Body.HasLocals = true;
                }
                catch (Exception exception)
                {
                    if (this.module != null)
                    {
                        if (this.module.MetadataImportErrors == null)
                        {
                            this.module.MetadataImportErrors = new ArrayList();
                        }
                        this.module.MetadataImportErrors.Add(exception);
                    }
                    method.Body = new Block(new StatementList(0));
                }
                finally
                {
                    this.currentMethodTypeParameters = currentMethodTypeParameters;
                    this.currentType = currentType;
                    this.currentTypeParameters = currentTypeParameters;
                }
            }
        }

        private void GetMethodDebugSymbols(Method method, uint methodToken)
        {
            ISymUnmanagedMethod method2 = null;
            try
            {
                this.debugReader.GetMethod(methodToken, ref method2);
                this.debugDocuments = new Dictionary<IntPtr, UnmanagedDocument>(2);
                method.RecordSequencePoints(method2, this.debugDocuments);
            }
            catch (COMException)
            {
            }
            catch (InvalidCastException)
            {
            }
            catch (InvalidComObjectException)
            {
            }
            finally
            {
                if (method2 != null)
                {
                    Marshal.ReleaseComObject(method2);
                }
            }
        }

        private Method GetMethodDefOrRef(int codedIndex)
        {
            switch ((codedIndex & 1))
            {
                case 0:
                    return this.GetMethodFromDef(codedIndex >> 1);

                case 1:
                    TypeNodeList list;
                    return (Method) this.GetMemberFromRef(codedIndex >> 1, out list);
            }
            throw new InvalidMetadataException(ExceptionStrings.BadCustomAttributeTypeEncodedToken);
        }

        private Method GetMethodDefOrRef(int codedIndex, int numberOfGenericArguments)
        {
            switch ((codedIndex & 1))
            {
                case 0:
                    return this.GetMethodFromDef(codedIndex >> 1);

                case 1:
                    TypeNodeList list;
                    return (Method) this.GetMemberFromRef(codedIndex >> 1, out list, numberOfGenericArguments);
            }
            throw new InvalidMetadataException(ExceptionStrings.BadCustomAttributeTypeEncodedToken);
        }

        internal Method GetMethodFromDef(int index) => 
            this.GetMethodFromDef(index, null);

        internal Method GetMethodFromDef(int index, TypeNode declaringType)
        {
            Method method2;
            TypeNodeList currentMethodTypeParameters = this.currentMethodTypeParameters;
            TypeNodeList currentTypeParameters = this.currentTypeParameters;
            MethodRow[] methodTable = this.tables.MethodTable;
            MethodRow row = methodTable[index - 1];
            if (row.Method != null)
            {
                return row.Method;
            }
            if (declaringType == null)
            {
                int num = index;
                MethodPtrRow[] methodPtrTable = this.tables.MethodPtrTable;
                int length = methodPtrTable.Length;
                int num3 = 0;
                int num4 = length - 1;
                if (((this.sortedTablesMask >> 5) % 2L) == 1L)
                {
                    while (num3 < num4)
                    {
                        int num5 = (num3 + num4) / 2;
                        if (methodPtrTable[num5].Method < index)
                        {
                            num3 = num5 + 1;
                        }
                        else
                        {
                            num4 = num5;
                        }
                    }
                    while ((num3 > 0) && (methodPtrTable[num3 - 1].Method == index))
                    {
                        num3--;
                    }
                }
                while (num3 < length)
                {
                    if (methodPtrTable[num3].Method == index)
                    {
                        num = num3 + 1;
                        break;
                    }
                    num3++;
                }
                TypeDefRow[] typeDefTable = this.tables.TypeDefTable;
                length = typeDefTable.Length;
                num3 = 0;
                num4 = length - 1;
                if (((this.sortedTablesMask >> 2) % 2L) == 1L)
                {
                    while (num3 < num4)
                    {
                        int num6 = (num3 + num4) / 2;
                        if (typeDefTable[num6].MethodList < num)
                        {
                            num3 = num6 + 1;
                        }
                        else
                        {
                            num4 = num6;
                        }
                    }
                    num4 = num3;
                    while ((num4 < (length - 1)) && (typeDefTable[num4 + 1].MethodList == num))
                    {
                        num4++;
                    }
                }
                while (num4 >= 0)
                {
                    if (typeDefTable[num4].MethodList <= num)
                    {
                        declaringType = this.GetTypeFromDef(num4 + 1);
                        break;
                    }
                    num4--;
                }
            }
            Method.MethodBodyProvider provider = new Method.MethodBodyProvider(this.GetMethodBody);
            Identifier identifier = this.tables.GetIdentifier(row.Name);
            if (((row.Flags & 0x800) != 0) && ((row.Flags & 0x800) != 0))
            {
                if (identifier.Name == ".ctor")
                {
                    method2 = methodTable[index - 1].Method = new InstanceInitializer(provider, index, 0x6000000 | index);
                }
                else if (identifier.Name == ".cctor")
                {
                    method2 = methodTable[index - 1].Method = new StaticInitializer(provider, index, 0x6000000 | index);
                }
                else
                {
                    method2 = methodTable[index - 1].Method = new Method(provider, index, 0x6000000 | index);
                }
            }
            else
            {
                method2 = methodTable[index - 1].Method = new Method(provider, index, 0x6000000 | index);
            }
            method2.ProvideMethodAttributes = new Method.MethodAttributeProvider(this.GetMethodAttributes);
            method2.Flags = (MethodFlags) row.Flags;
            method2.ImplFlags = (MethodImplFlags) row.ImplFlags;
            method2.Name = identifier;
            if (declaringType != null)
            {
                if (declaringType.IsGeneric)
                {
                    if (declaringType.Template != null)
                    {
                        this.currentTypeParameters = declaringType.ConsolidatedTemplateArguments;
                    }
                    else
                    {
                        this.currentTypeParameters = declaringType.ConsolidatedTemplateParameters;
                    }
                }
                if (this.module.ProjectTypesContainedInModule && ((declaringType.Flags & TypeFlags.IsForeign) != TypeFlags.AnsiClass))
                {
                    if (method2.IsStatic)
                    {
                        method2.Flags &= ~MethodFlags.NewSlot;
                    }
                    if (declaringType is DelegateNode)
                    {
                        method2.Flags &= ~MethodFlags.MethodAccessMask;
                        method2.Flags |= MethodFlags.Public;
                    }
                    else
                    {
                        method2.ImplFlags |= MethodImplFlags.InternalCall | MethodImplFlags.CodeTypeMask;
                    }
                }
            }
            this.tables.GetSignatureLength(row.Signature);
            MemoryCursor newCursor = this.tables.GetNewCursor();
            method2.CallingConvention = (CallingConventionFlags) newCursor.ReadByte();
            if (method2.IsGeneric = (method2.CallingConvention & CallingConventionFlags.Generic) != CallingConventionFlags.Default)
            {
                int num7 = newCursor.ReadCompressedInt();
                this.currentMethodTypeParameters = new TypeNodeList(num7);
                this.currentMethodTypeParameters = method2.TemplateParameters = this.GetTypeParametersFor((index << 1) | 1, method2);
                this.GetTypeParameterConstraints((index << 1) | 1, method2.TemplateParameters);
            }
            int capacity = newCursor.ReadCompressedInt();
            method2.ReturnType = this.ParseTypeSignature(newCursor);
            ParameterList parameters = method2.Parameters = new ParameterList(capacity);
            if (capacity > 0)
            {
                int num9 = method2.IsStatic ? 0 : 1;
                for (int i = 0; i < capacity; i++)
                {
                    Parameter element = new Parameter {
                        ParameterListIndex = i,
                        ArgumentListIndex = i + num9,
                        Type = this.ParseTypeSignature(newCursor),
                        DeclaringMethod = method2
                    };
                    parameters.Add(element);
                }
                int end = this.tables.ParamTable.Length + 1;
                if (index < methodTable.Length)
                {
                    end = methodTable[index].ParamList;
                }
                this.AddMoreStuffToParameters(method2, parameters, row.ParamList, end);
                for (int j = 0; j < capacity; j++)
                {
                    Parameter parameter2 = parameters[j];
                    if (parameter2.Name == null)
                    {
                        parameter2.Name = Identifier.For("param" + j);
                        parameter2.Flags |= ParameterFlags.ParameterNameMissing;
                    }
                }
            }
            else if (method2.ReturnType != CoreSystemTypes.Void)
            {
                int paramList = row.ParamList;
                ParamPtrRow[] paramPtrTable = this.tables.ParamPtrTable;
                ParamRow[] paramTable = this.tables.ParamTable;
                int num14 = methodTable.Length;
                int num15 = paramTable.Length;
                if (index < num14)
                {
                    num15 = methodTable[index].ParamList - 1;
                }
                if (paramPtrTable.Length > 0)
                {
                    if (((paramTable != null) && (0 < paramList)) && (paramList <= num15))
                    {
                        int param = paramPtrTable[paramList - 1].Param;
                        ParamRow row2 = paramTable[param - 1];
                        if (row2.Sequence == 0)
                        {
                            this.AddMoreStuffToParameters(method2, null, param, param + 1);
                        }
                    }
                }
                else if (((paramTable != null) && (0 < paramList)) && (paramList <= num15))
                {
                    ParamRow row3 = paramTable[paramList - 1];
                    if (row3.Sequence == 0)
                    {
                        this.AddMoreStuffToParameters(method2, null, paramList, paramList + 1);
                    }
                }
            }
            if ((method2.Flags & MethodFlags.PInvokeImpl) != MethodFlags.CompilerControlled)
            {
                ImplMapRow[] implMapTable = this.tables.ImplMapTable;
                int num17 = implMapTable.Length;
                int num18 = 0;
                int num19 = num17 - 1;
                if (((this.sortedTablesMask >> 0x1c) % 2L) == 1L)
                {
                    while (num18 < num19)
                    {
                        int num20 = (num18 + num19) / 2;
                        if ((implMapTable[num20].MemberForwarded >> 1) < index)
                        {
                            num18 = num20 + 1;
                        }
                        else
                        {
                            num19 = num20;
                        }
                    }
                    while ((num18 > 0) && ((implMapTable[num18 - 1].MemberForwarded >> 1) == index))
                    {
                        num18--;
                    }
                }
                while (num18 < num17)
                {
                    ImplMapRow row4 = implMapTable[num18];
                    if ((row4.MemberForwarded >> 1) == index)
                    {
                        method2.PInvokeFlags = (PInvokeFlags) row4.MappingFlags;
                        method2.PInvokeImportName = this.tables.GetString(row4.ImportName);
                        method2.PInvokeModule = this.module.ModuleReferences[row4.ImportScope - 1].Module;
                        break;
                    }
                    num18++;
                }
            }
            method2.DeclaringType = declaringType;
            this.currentMethodTypeParameters = currentMethodTypeParameters;
            this.currentTypeParameters = currentTypeParameters;
            return method2;
        }

        private Method GetMethodFromSpec(int i)
        {
            MethodSpecRow row = this.tables.MethodSpecTable[i - 1];
            if (row.InstantiatedMethod != null)
            {
                return row.InstantiatedMethod;
            }
            MemoryCursor blobCursor = this.tables.GetBlobCursor(row.Instantiation);
            blobCursor.ReadByte();
            TypeNodeList typeArguments = this.ParseTypeList(blobCursor);
            Method methodDefOrRef = this.GetMethodDefOrRef(row.Method, typeArguments.Count);
            if (methodDefOrRef == null)
            {
                return new Method();
            }
            if (methodDefOrRef.TemplateParameters == null)
            {
                return methodDefOrRef;
            }
            return methodDefOrRef.GetTemplateInstance(this.currentType, typeArguments);
        }

        private void GetMethodInstructions(Method method, object i)
        {
            TypeNodeList currentMethodTypeParameters = this.currentMethodTypeParameters;
            this.currentMethodTypeParameters = method.templateParameters;
            TypeNode currentType = this.currentType;
            this.currentType = method.DeclaringType;
            try
            {
                MethodRow row = this.tables.MethodTable[((int) i) - 1];
                if ((row.RVA != 0) && ((row.ImplFlags & 4) == 0))
                {
                    if (this.getDebugSymbols)
                    {
                        this.GetMethodDebugSymbols(method, (uint) (0x6000000 | ((int) i)));
                    }
                    method.Instructions = this.ParseMethodInstructions(method, (int) i, row.RVA);
                }
                else
                {
                    method.Instructions = new InstructionList(0);
                }
            }
            catch (Exception exception)
            {
                if (this.module != null)
                {
                    if (this.module.MetadataImportErrors == null)
                    {
                        this.module.MetadataImportErrors = new ArrayList();
                    }
                    this.module.MetadataImportErrors.Add(exception);
                }
                method.Instructions = new InstructionList(0);
            }
            finally
            {
                this.currentMethodTypeParameters = currentMethodTypeParameters;
                this.currentType = currentType;
            }
        }

        internal void GetNamespaces()
        {
            TypeDefRow[] typeDefTable = this.tables.TypeDefTable;
            int length = typeDefTable.Length;
            TrivialHashtable hashtable2 = this.namespaceTable = new TrivialHashtable(length);
            TrivialHashtable hashtable3 = new TrivialHashtable(0x80);
            NamespaceList list2 = this.namespaceList = new NamespaceList(length);
            for (int i = 0; i < length; i++)
            {
                TypeDefRow row = typeDefTable[i];
                TrivialHashtable hashtable4 = (TrivialHashtable) hashtable2[row.NamespaceKey];
                Namespace element = (Namespace) hashtable3[row.NamespaceKey];
                if (hashtable4 == null)
                {
                    hashtable2[row.NamespaceKey] = hashtable4 = new TrivialHashtable();
                    hashtable3[row.NamespaceKey] = element = new Namespace(row.NamespaceId);
                    list2.Add(element);
                }
                if ((row.Flags & 7) == 0)
                {
                    hashtable4[row.NameKey] = i + 1;
                }
                else if ((row.Flags & 7) == 1)
                {
                    element.isPublic = true;
                    hashtable4[row.NameKey] = i + 1;
                }
            }
        }

        private static System.Compiler.Module GetNestedModule(System.Compiler.Module module, string modName, ref string modLocation)
        {
            if ((module == null) || (modName == null))
            {
                return null;
            }
            System.Compiler.Module nestedModule = module.GetNestedModule(modName);
            if (nestedModule == null)
            {
                if (module.Location != null)
                {
                    modLocation = Path.Combine(Path.GetDirectoryName(module.Location), modName);
                }
                if ((modLocation != null) && File.Exists(modLocation))
                {
                    nestedModule = System.Compiler.Module.GetModule(modLocation);
                    if (nestedModule != null)
                    {
                        nestedModule.ContainingAssembly = module.ContainingAssembly;
                        module.ModuleReferences.Add(new ModuleReference(modName, nestedModule));
                    }
                }
            }
            if (nestedModule == null)
            {
                HandleError(module, string.Format(CultureInfo.CurrentCulture, ExceptionStrings.CouldNotFindReferencedModule, new object[] { modLocation }));
                nestedModule = new System.Compiler.Module {
                    Name = modName,
                    ContainingAssembly = module.ContainingAssembly,
                    Kind = ModuleKindFlags.DynamicallyLinkedLibrary
                };
            }
            return nestedModule;
        }

        private void GetNestedTypes(TypeNode type, object handle)
        {
            type.nestedTypes = null;
            TypeNodeList list = new TypeNodeList();
            TypeNodeList currentTypeParameters = this.currentTypeParameters;
            TypeNode currentType = this.currentType;
            try
            {
                if (type.IsGeneric)
                {
                    if (type.templateParameters == null)
                    {
                        type.templateParameters = new TypeNodeList(0);
                    }
                    this.currentTypeParameters = type.ConsolidatedTemplateParameters;
                }
                this.currentType = type;
                for (TypeNode node2 = type.DeclaringType; (this.currentTypeParameters == null) && (node2 != null); node2 = node2.DeclaringType)
                {
                    if (node2.IsGeneric)
                    {
                        if (node2.templateParameters == null)
                        {
                            node2.templateParameters = new TypeNodeList(0);
                        }
                        this.currentTypeParameters = node2.ConsolidatedTemplateParameters;
                    }
                }
                MetadataReader tables = this.tables;
                int num = (int) handle;
                int length = tables.TypeDefTable.Length;
                if ((num < 1) || (num > length))
                {
                    throw new ArgumentOutOfRangeException("handle", ExceptionStrings.InvalidTypeTableIndex);
                }
                NestedClassRow[] nestedClassTable = tables.NestedClassTable;
                length = nestedClassTable.Length;
                for (int i = 0; i < length; i++)
                {
                    NestedClassRow row = nestedClassTable[i];
                    if (row.EnclosingClass == num)
                    {
                        TypeNode typeFromDef = this.GetTypeFromDef(row.NestedClass);
                        if (typeFromDef == null)
                        {
                            throw new InvalidMetadataException("Invalid nested class row");
                        }
                        if (type.nestedTypes != null)
                        {
                            return;
                        }
                        typeFromDef.DeclaringType = type;
                        if (((typeFromDef.Flags & TypeFlags.RTSpecialName) == TypeFlags.AnsiClass) || (typeFromDef.Name.UniqueIdKey != StandardIds._Deleted.UniqueIdKey))
                        {
                            list.Add(typeFromDef);
                        }
                    }
                }
                type.nestedTypes = list;
            }
            catch (Exception exception)
            {
                if (this.module != null)
                {
                    if (this.module.MetadataImportErrors == null)
                    {
                        this.module.MetadataImportErrors = new ArrayList();
                    }
                    this.module.MetadataImportErrors.Add(exception);
                }
            }
            finally
            {
                this.currentTypeParameters = currentTypeParameters;
                this.currentType = currentType;
            }
        }

        internal static Block GetOrCreateBlock(TrivialHashtable blockMap, int address)
        {
            Block block = (Block) blockMap[address + 1];
            if (block == null)
            {
                blockMap[address + 1] = block = new Block(new StatementList());
                block.ILOffset = address;
                block.ILOffset = address;
            }
            return block;
        }

        private AttributeNode GetPermissionAttribute(MemoryCursor sigReader)
        {
            TypeNodeList list;
            sigReader.ReadInt32();
            int bytesToRead = sigReader.ReadInt32();
            sigReader.ReadUTF8(bytesToRead);
            int codedIndex = sigReader.ReadInt32();
            sigReader.ReadInt32();
            sigReader.ReadInt32();
            int blobLength = sigReader.ReadInt32();
            sigReader.ReadInt32();
            Method constructorDefOrRef = this.GetConstructorDefOrRef(codedIndex, out list);
            if (constructorDefOrRef == null)
            {
                constructorDefOrRef = new Method();
            }
            return this.GetCustomAttribute(constructorDefOrRef, sigReader, blobLength);
        }

        private AttributeNode GetPermissionAttribute2(MemoryCursor sigReader, SecurityAction action)
        {
            int bytesToRead = sigReader.ReadCompressedInt();
            string serializedName = sigReader.ReadUTF8(bytesToRead);
            TypeNode typeFromSerializedName = null;
            try
            {
                typeFromSerializedName = this.GetTypeFromSerializedName(serializedName);
            }
            catch (InvalidMetadataException)
            {
            }
            if (typeFromSerializedName == null)
            {
                HandleError(this.module, string.Format(CultureInfo.CurrentCulture, ExceptionStrings.CouldNotResolveType, new object[] { serializedName }));
                return null;
            }
            InstanceInitializer constructor = typeFromSerializedName.GetConstructor(new TypeNode[] { CoreSystemTypes.SecurityAction });
            if (constructor == null)
            {
                HandleError(this.module, string.Format(CultureInfo.CurrentCulture, ExceptionStrings.SecurityAttributeTypeDoesNotHaveADefaultConstructor, new object[] { serializedName }));
                return null;
            }
            sigReader.ReadCompressedInt();
            int num2 = sigReader.ReadCompressedInt();
            ExpressionList arguments = new ExpressionList(num2 + 1);
            arguments.Add(new Literal(action, CoreSystemTypes.SecurityAction));
            this.GetCustomAttributeNamedArguments(arguments, (ushort) num2, sigReader);
            return new AttributeNode(new MemberBinding(null, constructor), arguments);
        }

        private AttributeList GetPermissionAttributes(int blobIndex, SecurityAction action)
        {
            int num;
            AttributeList list = new AttributeList();
            MemoryCursor blobCursor = this.tables.GetBlobCursor(blobIndex, out num);
            if (num != 0)
            {
                switch (blobCursor.ReadByte())
                {
                    case 0x2a:
                    {
                        blobCursor.ReadInt32();
                        blobCursor.ReadInt32();
                        int num3 = blobCursor.ReadInt32();
                        for (int i = 0; i < num3; i++)
                        {
                            list.Add(this.GetPermissionAttribute(blobCursor));
                        }
                        return list;
                    }
                    case 60:
                        return null;

                    case 0x2e:
                        return this.GetPermissionAttributes2(blobIndex, action);
                }
                HandleError(this.module, ExceptionStrings.BadSecurityPermissionSetBlob);
            }
            return null;
        }

        private AttributeList GetPermissionAttributes2(int blobIndex, SecurityAction action)
        {
            int num;
            AttributeList list = new AttributeList();
            MemoryCursor blobCursor = this.tables.GetBlobCursor(blobIndex, out num);
            if (num == 0)
            {
                return null;
            }
            if (blobCursor.ReadByte() != 0x2e)
            {
                HandleError(this.module, ExceptionStrings.BadSecurityPermissionSetBlob);
                return null;
            }
            int num3 = blobCursor.ReadCompressedInt();
            for (int i = 0; i < num3; i++)
            {
                list.Add(this.GetPermissionAttribute2(blobCursor, action));
            }
            return list;
        }

        private TypeNode GetReplacedTypeFromName(Identifier Namespace, Identifier name)
        {
            if ((this.module.ContainingAssembly != null) && ((this.module.ContainingAssembly.Flags & AssemblyFlags.ContainsForeignTypes) != AssemblyFlags.None))
            {
                if (!SystemTypes.Initialized || (this.module.ContainingAssembly == null))
                {
                    return null;
                }
                int uniqueIdKey = Namespace.UniqueIdKey;
                int num2 = name.UniqueIdKey;
                if (uniqueIdKey == StandardIds.WindowsFoundationMetadata.UniqueIdKey)
                {
                    if (num2 == SystemTypes.AttributeUsageAttribute.Name.UniqueIdKey)
                    {
                        return SystemTypes.AttributeUsageAttribute;
                    }
                    if (num2 == SystemTypes.AttributeTargets.Name.UniqueIdKey)
                    {
                        return SystemTypes.AttributeTargets;
                    }
                }
                else if (uniqueIdKey == StandardIds.WindowsUI.UniqueIdKey)
                {
                    if ((num2 == SystemTypes.Color.Name.UniqueIdKey) && (SystemTypes.Color.DeclaringModule.Location != "unknown:location"))
                    {
                        return SystemTypes.Color;
                    }
                }
                else if (uniqueIdKey == StandardIds.WindowsFoundation.UniqueIdKey)
                {
                    if (num2 == SystemTypes.DateTime.Name.UniqueIdKey)
                    {
                        return SystemTypes.DateTimeOffset;
                    }
                    if ((SystemTypes.EventHandler1 != null) && (num2 == SystemTypes.EventHandler1.Name.UniqueIdKey))
                    {
                        return SystemTypes.EventHandler1;
                    }
                    if (num2 == SystemTypes.EventRegistrationToken.Name.UniqueIdKey)
                    {
                        return SystemTypes.EventRegistrationToken;
                    }
                    if (num2 == StandardIds.HResult.UniqueIdKey)
                    {
                        return SystemTypes.Exception;
                    }
                    if (num2 == StandardIds.IReference1.UniqueIdKey)
                    {
                        return SystemTypes.GenericNullable;
                    }
                    if (num2 == SystemTypes.Point.Name.UniqueIdKey)
                    {
                        return SystemTypes.Point;
                    }
                    if (num2 == SystemTypes.Rect.Name.UniqueIdKey)
                    {
                        return SystemTypes.Rect;
                    }
                    if (num2 == SystemTypes.Size.Name.UniqueIdKey)
                    {
                        return SystemTypes.Size;
                    }
                    if (num2 == SystemTypes.TimeSpan.Name.UniqueIdKey)
                    {
                        return SystemTypes.TimeSpan;
                    }
                    if (num2 == SystemTypes.Uri.Name.UniqueIdKey)
                    {
                        return SystemTypes.Uri;
                    }
                    if (num2 == StandardIds.IClosable.UniqueIdKey)
                    {
                        return SystemTypes.IDisposable;
                    }
                }
                else if (uniqueIdKey == StandardIds.WindowsFoundationCollections.UniqueIdKey)
                {
                    if (num2 == StandardIds.IIterable1.UniqueIdKey)
                    {
                        return SystemTypes.GenericIEnumerable;
                    }
                    if (num2 == StandardIds.IVector1.UniqueIdKey)
                    {
                        return SystemTypes.GenericIList;
                    }
                    if (num2 == StandardIds.IVectorView1.UniqueIdKey)
                    {
                        return SystemTypes.GenericIReadOnlyList;
                    }
                    if (num2 == StandardIds.IMap2.UniqueIdKey)
                    {
                        return SystemTypes.GenericIDictionary;
                    }
                    if (num2 == StandardIds.IMapView2.UniqueIdKey)
                    {
                        return SystemTypes.GenericIReadOnlyDictionary;
                    }
                    if (num2 == StandardIds.IKeyValuePair2.UniqueIdKey)
                    {
                        return SystemTypes.GenericKeyValuePair;
                    }
                }
                else if (uniqueIdKey == StandardIds.WindowsUIXaml.UniqueIdKey)
                {
                    if ((num2 == SystemTypes.CornerRadius.Name.UniqueIdKey) && (SystemTypes.CornerRadius.DeclaringModule.Location != "unknown:location"))
                    {
                        return SystemTypes.CornerRadius;
                    }
                    if ((num2 == SystemTypes.Duration.Name.UniqueIdKey) && (SystemTypes.Duration.DeclaringModule.Location != "unknown:location"))
                    {
                        return SystemTypes.Duration;
                    }
                    if ((SystemTypes.DurationType != null) && (num2 == SystemTypes.DurationType.Name.UniqueIdKey))
                    {
                        return SystemTypes.DurationType;
                    }
                    if ((num2 == SystemTypes.GridLength.Name.UniqueIdKey) && (SystemTypes.GridLength.DeclaringModule.Location != "unknown:location"))
                    {
                        return SystemTypes.GridLength;
                    }
                    if ((SystemTypes.GridUnitType != null) && (num2 == SystemTypes.GridUnitType.Name.UniqueIdKey))
                    {
                        return SystemTypes.GridUnitType;
                    }
                    if ((num2 == SystemTypes.Thickness.Name.UniqueIdKey) && (SystemTypes.Thickness.DeclaringModule.Location != "unknown:location"))
                    {
                        return SystemTypes.Thickness;
                    }
                }
                else if (uniqueIdKey == StandardIds.WindowsUIXamlData.UniqueIdKey)
                {
                    if ((num2 == SystemTypes.INotifyPropertyChanged.Name.UniqueIdKey) && (SystemTypes.INotifyPropertyChanged.DeclaringModule.Location != "unknown:location"))
                    {
                        return SystemTypes.INotifyPropertyChanged;
                    }
                    if ((num2 == SystemTypes.PropertyChangedEventHandler.Name.UniqueIdKey) && (SystemTypes.PropertyChangedEventHandler.DeclaringModule.Location != "unknown:location"))
                    {
                        return SystemTypes.PropertyChangedEventHandler;
                    }
                    if ((num2 == SystemTypes.PropertyChangedEventArgs.Name.UniqueIdKey) && (SystemTypes.PropertyChangedEventArgs.DeclaringModule.Location != "unknown:location"))
                    {
                        return SystemTypes.PropertyChangedEventArgs;
                    }
                }
                else if (uniqueIdKey == StandardIds.WindowsUIXamlInput.UniqueIdKey)
                {
                    if (num2 == StandardIds.ICommand.UniqueIdKey)
                    {
                        return SystemTypes.ICommand;
                    }
                }
                else if (uniqueIdKey == StandardIds.WindowsUIXamlInterop.UniqueIdKey)
                {
                    if (num2 == StandardIds.IBindableIterable.UniqueIdKey)
                    {
                        return SystemTypes.IBindableIterable;
                    }
                    if (num2 == StandardIds.IBindableVector.UniqueIdKey)
                    {
                        return SystemTypes.IBindableVector;
                    }
                    if (num2 == StandardIds.INotifyCollectionChanged.UniqueIdKey)
                    {
                        return SystemTypes.INotifyCollectionChanged;
                    }
                    if (num2 == StandardIds.NotifyCollectionChangedAction.UniqueIdKey)
                    {
                        return SystemTypes.NotifyCollectionChangedAction;
                    }
                    if (num2 == StandardIds.NotifyCollectionChangedEventArgs.UniqueIdKey)
                    {
                        return SystemTypes.NotifyCollectionChangedEventArgs;
                    }
                    if (num2 == StandardIds.NotifyCollectionChangedEventHandler.UniqueIdKey)
                    {
                        return SystemTypes.NotifyCollectionChangedEventHandler;
                    }
                    if (num2 == StandardIds.TypeName.UniqueIdKey)
                    {
                        return SystemTypes.Type;
                    }
                }
                else if (uniqueIdKey == StandardIds.WindowsUIXamlControlsPrimitives.UniqueIdKey)
                {
                    if ((num2 == SystemTypes.GeneratorPosition.Name.UniqueIdKey) && (SystemTypes.GeneratorPosition.DeclaringModule.Location != "unknown:location"))
                    {
                        return SystemTypes.GeneratorPosition;
                    }
                }
                else if (uniqueIdKey == StandardIds.WindowsUIXamlMedia.UniqueIdKey)
                {
                    if ((num2 == SystemTypes.Matrix.Name.UniqueIdKey) && (SystemTypes.Matrix.DeclaringModule.Location != "unknown:location"))
                    {
                        return SystemTypes.Matrix;
                    }
                }
                else if (uniqueIdKey == StandardIds.WindowsUIXamlMediaAnimation.UniqueIdKey)
                {
                    if ((num2 == SystemTypes.KeyTime.Name.UniqueIdKey) && (SystemTypes.KeyTime.DeclaringModule.Location != "unknown:location"))
                    {
                        return SystemTypes.KeyTime;
                    }
                    if ((num2 == SystemTypes.RepeatBehavior.Name.UniqueIdKey) && (SystemTypes.KeyTime.DeclaringModule.Location != "unknown:location"))
                    {
                        return SystemTypes.RepeatBehavior;
                    }
                    if ((SystemTypes.RepeatBehaviorType != null) && (num2 == SystemTypes.RepeatBehaviorType.Name.UniqueIdKey))
                    {
                        return SystemTypes.RepeatBehaviorType;
                    }
                }
                else if (((uniqueIdKey == StandardIds.WindowsUIXamlMediaMedia3D.UniqueIdKey) && (num2 == SystemTypes.Matrix3D.Name.UniqueIdKey)) && (SystemTypes.Matrix3D.DeclaringModule.Location != "unknown:location"))
                {
                    return SystemTypes.Matrix3D;
                }
            }
            return null;
        }

        private void GetResources(System.Compiler.Module module)
        {
            ManifestResourceRow[] manifestResourceTable = this.tables.ManifestResourceTable;
            int length = manifestResourceTable.Length;
            ResourceList list = new ResourceList(length);
            for (int i = 0; i < length; i++)
            {
                ManifestResourceRow row = manifestResourceTable[i];
                Resource element = new Resource {
                    Name = this.tables.GetString(row.Name),
                    IsPublic = (row.Flags & 7) == 1
                };
                int implementation = row.Implementation;
                if (implementation != 0)
                {
                    string str;
                    switch ((implementation & 3))
                    {
                        case 0:
                            str = this.tables.GetString(this.tables.FileTable[(implementation >> 2) - 1].Name);
                            if ((this.tables.FileTable[(implementation >> 2) - 1].Flags & 1) == 0)
                            {
                                break;
                            }
                            element.DefiningModule = new System.Compiler.Module();
                            element.DefiningModule.Directory = module.Directory;
                            element.DefiningModule.Location = Path.Combine(module.Directory, str);
                            element.DefiningModule.Name = str;
                            element.DefiningModule.Kind = ModuleKindFlags.ManifestResourceFile;
                            element.DefiningModule.ContainingAssembly = module.ContainingAssembly;
                            element.DefiningModule.HashValue = this.tables.GetBlob(this.tables.FileTable[(implementation >> 2) - 1].HashValue);
                            goto Label_01D6;

                        case 1:
                            element.DefiningModule = this.tables.AssemblyRefTable[(implementation >> 2) - 1].AssemblyReference.Assembly;
                            goto Label_01D6;

                        default:
                            goto Label_01D6;
                    }
                    string modLocation = str;
                    element.DefiningModule = GetNestedModule(module, str, ref modLocation);
                }
                else
                {
                    element.DefiningModule = module;
                    element.Data = this.tables.GetResourceData(row.Offset);
                }
            Label_01D6:
                list.Add(element);
            }
            module.Resources = list;
            module.Win32Resources = this.tables.ReadWin32Resources();
        }

        private System.Compiler.SecurityAttribute GetSecurityAttribute(int i)
        {
            DeclSecurityRow row = this.tables.DeclSecurityTable[i];
            System.Compiler.SecurityAttribute attribute = new System.Compiler.SecurityAttribute {
                Action = (SecurityAction) row.Action
            };
            if ((this.module.MetadataFormatMajorVersion > 1) || (this.module.MetadataFormatMinorVersion > 0))
            {
                attribute.PermissionAttributes = this.GetPermissionAttributes(row.PermissionSet, attribute.Action);
                if (attribute.PermissionAttributes != null)
                {
                    return attribute;
                }
            }
            attribute.SerializedPermissions = this.tables.GetBlobString(row.PermissionSet);
            return attribute;
        }

        private SecurityAttributeList GetSecurityAttributesFor(int parentIndex)
        {
            DeclSecurityRow[] declSecurityTable = this.tables.DeclSecurityTable;
            SecurityAttributeList list = new SecurityAttributeList();
            try
            {
                int index = 0;
                int length = declSecurityTable.Length;
                int num3 = length - 1;
                if (length == 0)
                {
                    return list;
                }
                bool flag = ((this.sortedTablesMask >> 14) % 2L) == 1L;
                if (flag)
                {
                    while (index < num3)
                    {
                        int num4 = (index + num3) / 2;
                        if (declSecurityTable[num4].Parent < parentIndex)
                        {
                            index = num4 + 1;
                        }
                        else
                        {
                            num3 = num4;
                        }
                    }
                    while ((index > 0) && (declSecurityTable[index - 1].Parent == parentIndex))
                    {
                        index--;
                    }
                }
                while (index < length)
                {
                    if (declSecurityTable[index].Parent == parentIndex)
                    {
                        list.Add(this.GetSecurityAttribute(index));
                    }
                    else if (flag)
                    {
                        return list;
                    }
                    index++;
                }
            }
            catch (Exception exception)
            {
                if (this.module == null)
                {
                    return list;
                }
                if (this.module.MetadataImportErrors == null)
                {
                    this.module.MetadataImportErrors = new ArrayList();
                }
                this.module.MetadataImportErrors.Add(exception);
            }
            return list;
        }

        private void GetTypeAttributes(TypeNode type, object handle)
        {
            TypeNodeList currentTypeParameters = this.currentTypeParameters;
            try
            {
                MetadataReader tables = this.tables;
                int num = (int) handle;
                TypeDefRow[] typeDefTable = tables.TypeDefTable;
                int length = typeDefTable.Length;
                if ((num < 1) || (num > length))
                {
                    throw new ArgumentOutOfRangeException("handle", ExceptionStrings.InvalidTypeTableIndex);
                }
                TypeDefRow row = typeDefTable[num - 1];
                if (type != row.Type)
                {
                    throw new ArgumentOutOfRangeException("handle", ExceptionStrings.InvalidTypeTableIndex);
                }
                type.Attributes = this.GetCustomAttributesNonNullFor((num << 5) | 3);
                this.currentTypeParameters = currentTypeParameters;
                if ((type.Flags & TypeFlags.HasSecurity) != TypeFlags.AnsiClass)
                {
                    type.SecurityAttributes = this.GetSecurityAttributesFor(num << 2);
                }
            }
            catch (Exception exception)
            {
                if (this.module != null)
                {
                    if (this.module.MetadataImportErrors == null)
                    {
                        this.module.MetadataImportErrors = new ArrayList();
                    }
                    this.module.MetadataImportErrors.Add(exception);
                }
                type.Attributes = new AttributeList(0);
                this.currentTypeParameters = currentTypeParameters;
            }
        }

        private TypeNode GetTypeExtensionFromDef(TypeNode.NestedTypeProvider nestedTypeProvider, TypeNode.TypeAttributeProvider attributeProvider, TypeNode.TypeMemberProvider memberProvider, object handle, TypeNode baseType, Interface lastInterface)
        {
            Assembly assembly;
            if (lastInterface.Namespace.UniqueIdKey != StandardIds.CciTypeExtensions.UniqueIdKey)
            {
                return null;
            }
            TypeExtensionProvider dummyTEProvider = (TypeExtensionProvider) this.TypeExtensionTable[lastInterface.Name.UniqueIdKey];
            if (dummyTEProvider != null)
            {
                goto Label_014E;
            }
            string path = lastInterface.DeclaringModule.Location.ToLower(CultureInfo.InvariantCulture);
            if (!path.EndsWith(".runtime.dll"))
            {
                goto Label_014E;
            }
            string assemblyString = Path.GetFileName(path).Replace(".runtime.dll", "");
            try
            {
                assembly = Assembly.Load(assemblyString);
            }
            catch
            {
                HandleError(this.module, string.Format(CultureInfo.CurrentCulture, ExceptionStrings.CannotLoadTypeExtension, new object[] { lastInterface.FullName, assemblyString }));
                goto Label_0129;
            }
            if (assembly != null)
            {
                Type type = assembly.GetType(StandardIds.CciTypeExtensions.Name + "." + lastInterface.Name.Name + "Provider");
                if (type != null)
                {
                    System.Reflection.MethodInfo method = type.GetMethod("For");
                    if (method != null)
                    {
                        dummyTEProvider = (TypeExtensionProvider) Delegate.CreateDelegate(typeof(TypeExtensionProvider), method);
                    }
                }
            }
        Label_0129:
            if (dummyTEProvider == null)
            {
                dummyTEProvider = this.dummyTEProvider;
            }
            this.TypeExtensionTable[lastInterface.Name.UniqueIdKey] = dummyTEProvider;
        Label_014E:
            if (dummyTEProvider == null)
            {
                return null;
            }
            return dummyTEProvider(nestedTypeProvider, attributeProvider, memberProvider, baseType, handle);
        }

        internal TypeNode GetTypeFromDef(int i)
        {
            TypeNode typeFromDefHelper;
            TypeDefRow row = this.tables.TypeDefTable[i - 1];
            if (row.Type != null)
            {
                return row.Type;
            }
            TypeNodeList currentTypeParameters = this.currentTypeParameters;
            TypeNode currentType = this.currentType;
            try
            {
                typeFromDefHelper = this.GetTypeFromDefHelper(i);
            }
            catch (Exception exception)
            {
                if (this.module == null)
                {
                    return new Class();
                }
                if (this.module.MetadataImportErrors == null)
                {
                    this.module.MetadataImportErrors = new ArrayList();
                }
                this.module.MetadataImportErrors.Add(exception);
                typeFromDefHelper = new Class();
            }
            finally
            {
                this.currentTypeParameters = currentTypeParameters;
                this.currentType = currentType;
            }
            return typeFromDefHelper;
        }

        internal TypeNode GetTypeFromDefHelper(int i)
        {
            int num;
            int num2;
            this.tables.TypeDefTable[i - 1].Type = Class.Dummy;
            TypeDefRow row = this.tables.TypeDefTable[i - 1];
            Identifier name = this.tables.GetIdentifier(row.Name);
            Identifier identifier = this.tables.GetIdentifier(row.Namespace);
            if ((identifier.Name.Length > 0) && ((row.Flags & 7) >= 2))
            {
                name = Identifier.For(identifier.Name + "." + name.Name);
                identifier = Identifier.Empty;
            }
            TypeNode replacedTypeFromName = this.GetReplacedTypeFromName(identifier, name);
            if (replacedTypeFromName != null)
            {
                this.tables.TypeDefTable[i - 1].Type = replacedTypeFromName;
                this.currentType = replacedTypeFromName;
                return replacedTypeFromName;
            }
            this.GetInterfaceIndices(i, out num, out num2);
            InterfaceList interfaces = new InterfaceList();
            replacedTypeFromName = this.ConstructCorrectTypeNodeSubclass(i, identifier, num, num2, (TypeFlags) row.Flags, interfaces, row.Extends, (name.UniqueIdKey == StandardIds.Enum.UniqueIdKey) && (identifier.UniqueIdKey == StandardIds.System.UniqueIdKey));
            replacedTypeFromName.DeclaringModule = this.module;
            replacedTypeFromName.Name = name;
            replacedTypeFromName.Namespace = identifier;
            TypeNodeList typeParameters = this.currentTypeParameters = this.GetTypeParametersFor(i << 1, replacedTypeFromName);
            replacedTypeFromName.TemplateParameters = typeParameters;
            replacedTypeFromName.IsGeneric = typeParameters != null;
            this.tables.TypeDefTable[i - 1].Type = replacedTypeFromName;
            this.currentType = replacedTypeFromName;
            this.RemoveTypeParametersBelongingToDeclaringType(i, ref typeParameters, replacedTypeFromName);
            if ((replacedTypeFromName is Class) && (replacedTypeFromName.BaseType == null))
            {
                TypeNode node2 = this.DecodeAndGetTypeDefOrRefOrSpec(row.Extends);
                ((Class) replacedTypeFromName).BaseClass = node2 as Class;
                if (((node2 != null) && !(node2 is Class)) && (this.module != null))
                {
                    HandleError(this.module, ExceptionStrings.InvalidBaseClass);
                }
            }
            if (replacedTypeFromName.IsGeneric)
            {
                this.GetTypeParameterConstraints(i << 1, typeParameters);
                if (replacedTypeFromName.templateParameters != null)
                {
                    int num3 = 0;
                    int num4 = typeParameters.Count - replacedTypeFromName.templateParameters.Count;
                    int count = replacedTypeFromName.templateParameters.Count;
                    while (num3 < count)
                    {
                        replacedTypeFromName.templateParameters[num3] = typeParameters[num3 + num4];
                        num3++;
                    }
                }
            }
            if (num >= 0)
            {
                this.GetInterfaces(i, num, interfaces);
            }
            if ((replacedTypeFromName.Flags & TypeFlags.LayoutMask) != TypeFlags.AnsiClass)
            {
                this.GetClassSizeAndPackingSize(i, replacedTypeFromName);
            }
            return replacedTypeFromName;
        }

        private TypeNode GetTypeFromName(Identifier Namespace, Identifier name)
        {
            try
            {
                if (this.namespaceTable == null)
                {
                    this.GetNamespaces();
                }
                TrivialHashtable hashtable = (TrivialHashtable) this.namespaceTable[Namespace.UniqueIdKey];
                if (hashtable == null)
                {
                    return this.GetForwardedTypeFromName(Namespace, name);
                }
                object obj2 = hashtable[name.UniqueIdKey];
                if (obj2 == null)
                {
                    return this.GetForwardedTypeFromName(Namespace, name);
                }
                return this.GetTypeFromDef((int) obj2);
            }
            catch (Exception exception)
            {
                if (this.module != null)
                {
                    if (this.module.MetadataImportErrors == null)
                    {
                        this.module.MetadataImportErrors = new ArrayList();
                    }
                    this.module.MetadataImportErrors.Add(exception);
                }
                return null;
            }
        }

        internal TypeNode GetTypeFromRef(int i) => 
            this.GetTypeFromRef(i, false);

        internal TypeNode GetTypeFromRef(int i, bool expectStruct)
        {
            TypeRefRow[] typeRefTable = this.tables.TypeRefTable;
            TypeRefRow row = typeRefTable[i - 1];
            TypeNode type = row.Type;
            if (type == null)
            {
                Identifier name = this.tables.GetIdentifier(row.Name);
                Identifier identifier = this.tables.GetIdentifier(row.Namespace);
                int resolutionScope = row.ResolutionScope;
                System.Compiler.Module declaringModule = null;
                TypeNode declaringType = null;
                int num2 = resolutionScope >> 2;
                switch ((resolutionScope & 3))
                {
                    case 0:
                        declaringModule = this.module;
                        type = declaringModule.GetType(identifier, name);
                        break;

                    case 1:
                        declaringModule = this.tables.ModuleRefTable[num2 - 1].Module;
                        if (declaringModule != null)
                        {
                            type = declaringModule.GetType(identifier, name);
                        }
                        break;

                    case 2:
                        if (num2 > 0)
                        {
                            declaringModule = this.tables.AssemblyRefTable[num2 - 1].AssemblyReference.Assembly;
                        }
                        if (declaringModule != null)
                        {
                            type = declaringModule.GetType(identifier, name);
                        }
                        break;

                    case 3:
                        declaringType = this.GetTypeFromRef(num2);
                        declaringModule = declaringType.DeclaringModule;
                        if ((identifier != null) && (identifier.length != 0))
                        {
                            type = (TypeNode) declaringType.GetMembersNamed(Identifier.For(identifier.Name + "." + name.Name))[0];
                            break;
                        }
                        type = (TypeNode) declaringType.GetMembersNamed(name)[0];
                        break;

                    default:
                        declaringModule = this.module;
                        break;
                }
                if (type == null)
                {
                    type = this.GetDummyTypeNode(identifier, name, declaringModule, declaringType, expectStruct);
                }
                typeRefTable[i - 1].Type = type;
                if (!CanCacheTypeNode(type))
                {
                    typeRefTable[i - 1].Type = null;
                }
            }
            return type;
        }

        private TypeNode GetTypeFromSerializedName(string serializedName)
        {
            if (serializedName == null)
            {
                return null;
            }
            string assemblyName = null;
            string typeName = serializedName;
            int length = FindFirstCommaOutsideBrackets(serializedName);
            if (length > 0)
            {
                int num2 = 1;
                while (((length + num2) < serializedName.Length) && (serializedName[length + num2] == ' '))
                {
                    num2++;
                }
                assemblyName = serializedName.Substring(length + num2);
                typeName = serializedName.Substring(0, length);
            }
            return this.GetTypeFromSerializedName(typeName, assemblyName);
        }

        private TypeNode GetTypeFromSerializedName(string typeName, TypeNode nestingType)
        {
            string str;
            int i = 0;
            ParseSimpleTypeName(typeName, out str, ref i);
            TypeNode nestedType = nestingType.GetNestedType(Identifier.For(str));
            if (nestedType == null)
            {
                nestedType = this.GetDummyTypeNode(Identifier.Empty, Identifier.For(str), nestingType.DeclaringModule, nestingType, false);
            }
            if (i >= typeName.Length)
            {
                return nestedType;
            }
            switch (typeName[i])
            {
                case '+':
                    return this.GetTypeFromSerializedName(typeName.Substring(i + 1), nestedType);

                case '&':
                    return nestedType.GetReferenceType();

                case '*':
                    return nestedType.GetPointerType();

                case '[':
                    return this.ParseArrayOrGenericType(typeName.Substring(i + 1, (typeName.Length - 1) - i), nestedType);
            }
            throw new InvalidMetadataException(ExceptionStrings.BadSerializedTypeName);
        }

        private TypeNode GetTypeFromSerializedName(string typeName, string assemblyName)
        {
            string str;
            string str2;
            int num;
            ParseTypeName(typeName, out str, out str2, out num);
            System.Compiler.Module module = null;
            TypeNode nestingType = this.LookupType(str, str2, assemblyName, out module);
            if (nestingType == null)
            {
                if ((num < typeName.Length) && (typeName[num] == '!'))
                {
                    int result = 0;
                    if (PlatformHelpers.TryParseInt32(typeName.Substring(0, num), out result))
                    {
                        nestingType = this.DecodeAndGetTypeDefOrRefOrSpec(result);
                        if (nestingType != null)
                        {
                            return nestingType;
                        }
                    }
                }
                nestingType = this.GetDummyTypeNode(Identifier.For(str), Identifier.For(str2), module, null, false);
            }
            if (num >= typeName.Length)
            {
                return nestingType;
            }
            switch (typeName[num])
            {
                case '+':
                    return this.GetTypeFromSerializedName(typeName.Substring(num + 1), nestingType);

                case '&':
                    return nestingType.GetReferenceType();

                case '*':
                    return nestingType.GetPointerType();

                case '[':
                    return this.ParseArrayOrGenericType(typeName.Substring(num + 1, (typeName.Length - 1) - num), nestingType);
            }
            throw new InvalidMetadataException(ExceptionStrings.BadSerializedTypeName);
        }

        internal TypeNode GetTypeFromSpec(int i)
        {
            TypeSpecRow row = this.tables.TypeSpecTable[i - 1];
            if (row.Type != null)
            {
                return row.Type;
            }
            this.tables.GetSignatureLength(row.Signature);
            bool pinned = false;
            bool isTypeArgument = false;
            TypeNode type = this.ParseTypeSignature(this.tables.GetNewCursor(), ref pinned, ref isTypeArgument);
            if (type == null)
            {
                type = new Class();
            }
            AttributeList customAttributesFor = this.GetCustomAttributesFor((int) ((i << 5) | 13));
            if ((customAttributesFor != null) && (customAttributesFor.Count > 0))
            {
                AttributeList attributes = type.Attributes;
                int num = 0;
                int num2 = (attributes == null) ? 0 : attributes.Count;
                while (num < num2)
                {
                    AttributeNode element = type.Attributes[num];
                    if (element != null)
                    {
                        customAttributesFor.Add(element);
                    }
                    num++;
                }
                type.Attributes = customAttributesFor;
            }
            if (!isTypeArgument && CanCacheTypeNode(type))
            {
                this.tables.TypeSpecTable[i - 1].Type = type;
            }
            return type;
        }

        private TypeNode GetTypeGlobalMemberContainerTypeFromModule(int i)
        {
            ModuleRefRow row = this.tables.ModuleRefTable[i - 1];
            System.Compiler.Module declaringModule = row.Module;
            TypeNode node = null;
            if (((declaringModule != null) && (declaringModule.Types != null)) && (declaringModule.Types.Count > 0))
            {
                node = declaringModule.Types[0];
            }
            if (node == null)
            {
                node = this.GetDummyTypeNode(Identifier.Empty, Identifier.For("<Module>"), declaringModule, null, false);
                if (declaringModule != null)
                {
                    declaringModule.Types = new TypeNodeList(new TypeNode[] { node });
                }
            }
            return node;
        }

        private TypeNode GetTypeIfNotGenericInstance(int codedIndex)
        {
            if (codedIndex != 0)
            {
                switch ((codedIndex & 3))
                {
                    case 0:
                        return this.GetTypeFromDef(codedIndex >> 2);

                    case 1:
                        return this.GetTypeFromRef(codedIndex >> 2, false);
                }
            }
            return null;
        }

        private void GetTypeList(System.Compiler.Module module)
        {
            TypeNodeList list = new TypeNodeList();
            TypeDefRow[] typeDefTable = this.tables.TypeDefTable;
            int num = 0;
            int length = typeDefTable.Length;
            while (num < length)
            {
                TypeNode typeFromDef = this.GetTypeFromDef(num + 1);
                if ((typeFromDef != null) && (typeFromDef.DeclaringType == null))
                {
                    list.Add(typeFromDef);
                }
                num++;
            }
            module.Types = list;
            AssemblyNode node2 = module as AssemblyNode;
            if (node2 != null)
            {
                list = new TypeNodeList();
                ExportedTypeRow[] exportedTypeTable = this.tables.ExportedTypeTable;
                int index = 0;
                int num4 = exportedTypeTable.Length;
                while (index < num4)
                {
                    ExportedTypeRow row = exportedTypeTable[index];
                    Identifier identifier = Identifier.For(this.tables.GetString(row.TypeNamespace));
                    Identifier name = Identifier.For(this.tables.GetString(row.TypeName));
                    TypeNode element = null;
                    switch ((row.Implementation & 3))
                    {
                        case 0:
                        {
                            string modName = this.tables.GetString(this.tables.FileTable[(row.Implementation >> 2) - 1].Name);
                            string modLocation = modName;
                            System.Compiler.Module module2 = GetNestedModule(node2, modName, ref modLocation);
                            if (module2 != null)
                            {
                                element = module2.GetType(identifier, name);
                                if (element == null)
                                {
                                    HandleError(node2, string.Format(CultureInfo.CurrentCulture, ExceptionStrings.CouldNotFindExportedTypeInModule, new object[] { identifier + "." + name, modLocation }));
                                    element = new Class {
                                        Name = name,
                                        Namespace = identifier,
                                        Flags = TypeFlags.Public,
                                        DeclaringModule = module2
                                    };
                                }
                            }
                            break;
                        }
                        case 1:
                        {
                            AssemblyReference assemblyReference = this.tables.AssemblyRefTable[(row.Implementation >> 2) - 1].AssemblyReference;
                            if (assemblyReference == null)
                            {
                                HandleError(node2, ExceptionStrings.BadMetadataInExportTypeTableNoSuchAssemblyReference);
                                assemblyReference = new AssemblyReference("dummy assembly for bad reference");
                            }
                            AssemblyNode assembly = assemblyReference.Assembly;
                            if (assembly == null)
                            {
                                goto Label_0365;
                            }
                            element = assembly.GetType(identifier, name);
                            if (element == null)
                            {
                                HandleError(node2, string.Format(CultureInfo.CurrentCulture, ExceptionStrings.CouldNotFindExportedTypeInAssembly, new object[] { identifier + "." + name, assembly.StrongName }));
                                element = new Class {
                                    Name = name,
                                    Namespace = identifier,
                                    Flags = TypeFlags.Public,
                                    DeclaringModule = assembly
                                };
                            }
                            break;
                        }
                        case 2:
                        {
                            TypeNode node5 = list[(row.Implementation >> 2) - 1];
                            if (node5 == null)
                            {
                                HandleError(node2, ExceptionStrings.BadMetadataInExportTypeTableNoSuchParentType);
                                node5 = new Class {
                                    DeclaringModule = this.module,
                                    Name = Identifier.For("Missing parent type")
                                };
                            }
                            element = node5.GetNestedType(name);
                            if (element == null)
                            {
                                HandleError(node2, string.Format(CultureInfo.CurrentCulture, ExceptionStrings.CouldNotFindExportedNestedTypeInType, new object[] { name, node5.FullName }));
                                element = new Class {
                                    Name = name,
                                    Flags = TypeFlags.NestedPublic,
                                    DeclaringType = node5,
                                    DeclaringModule = node5.DeclaringModule
                                };
                            }
                            break;
                        }
                    }
                    list.Add(element);
                Label_0365:
                    index++;
                }
                node2.ExportedTypes = list;
            }
        }

        private void GetTypeMembers(TypeNode type, object handle)
        {
            TypeNodeList currentTypeParameters = this.currentTypeParameters;
            TypeNode currentType = this.currentType;
            try
            {
                MetadataReader tables = this.tables;
                int index = (int) handle;
                TypeDefRow[] typeDefTable = tables.TypeDefTable;
                FieldRow[] fieldTable = tables.FieldTable;
                FieldPtrRow[] fieldPtrTable = tables.FieldPtrTable;
                MethodRow[] methodTable = tables.MethodTable;
                MethodPtrRow[] methodPtrTable = tables.MethodPtrTable;
                EventMapRow[] eventMapTable = tables.EventMapTable;
                EventRow[] eventTable = tables.EventTable;
                EventPtrRow[] eventPtrTable = tables.EventPtrTable;
                MethodImplRow[] methodImplTable = tables.MethodImplTable;
                PropertyMapRow[] propertyMapTable = tables.PropertyMapTable;
                PropertyPtrRow[] propertyPtrTable = tables.PropertyPtrTable;
                PropertyRow[] propertyTable = this.tables.PropertyTable;
                NestedClassRow[] nestedClassTable = tables.NestedClassTable;
                int length = typeDefTable.Length;
                if ((index < 1) || (index > length))
                {
                    throw new ArgumentOutOfRangeException("handle", ExceptionStrings.InvalidTypeTableIndex);
                }
                TypeDefRow row = typeDefTable[index - 1];
                if (type != row.Type)
                {
                    throw new ArgumentOutOfRangeException("handle", ExceptionStrings.InvalidTypeTableIndex);
                }
                if (type.IsGeneric)
                {
                    if (type.templateParameters == null)
                    {
                        type.templateParameters = new TypeNodeList(0);
                    }
                    this.currentTypeParameters = type.ConsolidatedTemplateParameters;
                }
                this.currentType = type;
                for (TypeNode node2 = type.DeclaringType; (this.currentTypeParameters == null) && (node2 != null); node2 = node2.DeclaringType)
                {
                    if (node2.IsGeneric)
                    {
                        if (node2.templateParameters == null)
                        {
                            node2.templateParameters = new TypeNodeList(0);
                        }
                        this.currentTypeParameters = node2.ConsolidatedTemplateParameters;
                    }
                }
                type.members = new MemberList();
                length = nestedClassTable.Length;
                for (int i = 0; i < length; i++)
                {
                    NestedClassRow row2 = nestedClassTable[i];
                    if (row2.EnclosingClass == index)
                    {
                        TypeNode typeFromDef = this.GetTypeFromDef(row2.NestedClass);
                        if (typeFromDef != null)
                        {
                            typeFromDef.DeclaringType = type;
                            if (((typeFromDef.Flags & TypeFlags.RTSpecialName) == TypeFlags.AnsiClass) || (typeFromDef.Name.UniqueIdKey != StandardIds._Deleted.UniqueIdKey))
                            {
                                type.Members.Add(typeFromDef);
                            }
                        }
                    }
                }
                length = typeDefTable.Length;
                int num4 = fieldTable.Length;
                int fieldList = row.FieldList;
                int end = num4 + 1;
                if (index < length)
                {
                    end = typeDefTable[index].FieldList;
                }
                if (type is EnumNode)
                {
                    this.GetUnderlyingTypeOfEnumNode((EnumNode) type, fieldTable, fieldPtrTable, fieldList, end);
                }
                this.AddFieldsToType(type, fieldTable, fieldPtrTable, fieldList, end);
                num4 = methodTable.Length;
                fieldList = row.MethodList;
                end = num4 + 1;
                if (index < length)
                {
                    end = typeDefTable[index].MethodList;
                }
                this.AddMethodsToType(type, methodPtrTable, fieldList, end);
                length = propertyMapTable.Length;
                num4 = propertyTable.Length;
                for (int j = 0; j < length; j++)
                {
                    PropertyMapRow row3 = propertyMapTable[j];
                    if (row3.Parent == index)
                    {
                        fieldList = row3.PropertyList;
                        end = num4 + 1;
                        if (j < (length - 1))
                        {
                            end = propertyMapTable[j + 1].PropertyList;
                        }
                        this.AddPropertiesToType(type, propertyTable, propertyPtrTable, fieldList, end);
                    }
                }
                length = eventMapTable.Length;
                num4 = eventTable.Length;
                for (int k = 0; k < length; k++)
                {
                    EventMapRow row4 = eventMapTable[k];
                    if (row4.Parent == index)
                    {
                        fieldList = row4.EventList;
                        end = num4 + 1;
                        if (k < (length - 1))
                        {
                            end = eventMapTable[k + 1].EventList;
                        }
                        this.AddEventsToType(type, eventTable, eventPtrTable, fieldList, end);
                    }
                }
                length = methodImplTable.Length;
                for (int m = 0; m < length; m++)
                {
                    MethodImplRow row5 = methodImplTable[m];
                    if (row5.Class == index)
                    {
                        Method methodDefOrRef = this.GetMethodDefOrRef(row5.MethodBody);
                        if (methodDefOrRef != null)
                        {
                            MethodList implementedInterfaceMethods = methodDefOrRef.ImplementedInterfaceMethods;
                            if (implementedInterfaceMethods == null)
                            {
                                implementedInterfaceMethods = methodDefOrRef.ImplementedInterfaceMethods = new MethodList();
                            }
                            TypeNodeList currentMethodTypeParameters = this.currentMethodTypeParameters;
                            this.currentMethodTypeParameters = methodDefOrRef.TemplateParameters;
                            implementedInterfaceMethods.Add(this.GetMethodDefOrRef(row5.MethodDeclaration));
                            this.currentMethodTypeParameters = currentMethodTypeParameters;
                        }
                    }
                }
                this.currentTypeParameters = currentTypeParameters;
            }
            catch (Exception exception)
            {
                if (this.module != null)
                {
                    if (this.module.MetadataImportErrors == null)
                    {
                        this.module.MetadataImportErrors = new ArrayList();
                    }
                    this.module.MetadataImportErrors.Add(exception);
                }
                type.Members = new MemberList(0);
            }
            finally
            {
                this.currentTypeParameters = currentTypeParameters;
                this.currentType = currentType;
            }
        }

        private void GetTypeParameterAttributes(TypeNode type, object handle)
        {
            TypeNodeList currentTypeParameters = this.currentTypeParameters;
            try
            {
                MetadataReader tables = this.tables;
                int num = (int) handle;
                GenericParamRow[] genericParamTable = tables.GenericParamTable;
                int length = genericParamTable.Length;
                if ((num < 1) || (num > length))
                {
                    throw new ArgumentOutOfRangeException("handle", ExceptionStrings.InvalidTypeTableIndex);
                }
                GenericParamRow row1 = genericParamTable[num - 1];
                type.Attributes = this.GetCustomAttributesNonNullFor((num << 5) | 0x13);
                this.currentTypeParameters = currentTypeParameters;
            }
            catch (Exception exception)
            {
                if (this.module != null)
                {
                    if (this.module.MetadataImportErrors == null)
                    {
                        this.module.MetadataImportErrors = new ArrayList();
                    }
                    this.module.MetadataImportErrors.Add(exception);
                }
                type.Attributes = new AttributeList(0);
                this.currentTypeParameters = currentTypeParameters;
            }
        }

        private void GetTypeParameterConstraints(int parentIndex, TypeNodeList parameters)
        {
            if (parameters != null)
            {
                GenericParamRow[] genericParamTable = this.tables.GenericParamTable;
                int index = 0;
                int length = genericParamTable.Length;
                int num3 = length - 1;
                bool flag = ((this.sortedTablesMask >> 0x2a) % 2L) == 1L;
                if (flag)
                {
                    while (index < num3)
                    {
                        int num4 = (index + num3) / 2;
                        if (genericParamTable[num4].Owner < parentIndex)
                        {
                            index = num4 + 1;
                        }
                        else
                        {
                            num3 = num4;
                        }
                    }
                    while ((index > 0) && (genericParamTable[index - 1].Owner == parentIndex))
                    {
                        index--;
                    }
                }
                for (int i = 0; (index < length) && (i < parameters.Count); i++)
                {
                    if (genericParamTable[index].Owner == parentIndex)
                    {
                        TypeNode parameter = parameters[i];
                        this.GetGenericParameterConstraints(index, ref parameter);
                        parameters[i] = parameter;
                    }
                    else if (flag)
                    {
                        return;
                    }
                    index++;
                }
            }
        }

        private TypeNodeList GetTypeParametersFor(int parentIndex, Member parent)
        {
            GenericParamRow[] genericParamTable = this.tables.GenericParamTable;
            TypeNodeList list = new TypeNodeList();
            int index = 0;
            int length = genericParamTable.Length;
            int num3 = length - 1;
            bool flag = ((this.sortedTablesMask >> 0x2a) % 2L) == 1L;
            if (flag)
            {
                while (index < num3)
                {
                    int num4 = (index + num3) / 2;
                    if (genericParamTable[num4].Owner < parentIndex)
                    {
                        index = num4 + 1;
                    }
                    else
                    {
                        num3 = num4;
                    }
                }
                while ((index > 0) && (genericParamTable[index - 1].Owner == parentIndex))
                {
                    index--;
                }
            }
            for (int i = 0; index < length; i++)
            {
                if (genericParamTable[index].Owner == parentIndex)
                {
                    list.Add(this.GetGenericParameter(index, i, parent));
                }
                else if (flag)
                {
                    break;
                }
                index++;
            }
            if (list.Count == 0)
            {
                return null;
            }
            return list;
        }

        private void GetUnderlyingTypeOfEnumNode(EnumNode enumNode, FieldRow[] fieldDefs, FieldPtrRow[] fieldPtrs, int start, int end)
        {
            TypeNode type = null;
            for (int i = start; i < end; i++)
            {
                int field = i;
                if (fieldPtrs.Length > 0)
                {
                    field = fieldPtrs[i - 1].Field;
                }
                FieldRow row = fieldDefs[field - 1];
                if ((row.Field != null) && !row.Field.IsStatic)
                {
                    type = row.Field.Type;
                    break;
                }
                if ((row.Flags & 0x10) == 0)
                {
                    this.tables.GetSignatureLength(row.Signature);
                    MemoryCursor newCursor = this.tables.GetNewCursor();
                    GetAndCheckSignatureToken(6, newCursor);
                    type = this.ParseTypeSignature(newCursor);
                    break;
                }
            }
            enumNode.underlyingType = type;
        }

        private static void HandleError(System.Compiler.Module mod, string errorMessage)
        {
            if ((mod != null) && ((mod.ContainingAssembly == null) || ((mod.ContainingAssembly.Flags & AssemblyFlags.ContainsForeignTypes) == AssemblyFlags.None)))
            {
                if (mod.MetadataImportErrors == null)
                {
                    mod.MetadataImportErrors = new ArrayList();
                }
                mod.MetadataImportErrors.Add(new InvalidMetadataException(errorMessage));
            }
        }

        internal bool IsValidTypeName(Identifier Namespace, Identifier name)
        {
            try
            {
                if (this.namespaceTable == null)
                {
                    this.GetNamespaces();
                }
                TrivialHashtable hashtable = (TrivialHashtable) this.namespaceTable[Namespace.UniqueIdKey];
                return (hashtable?[name.UniqueIdKey] != null);
            }
            catch (Exception exception)
            {
                if (this.module != null)
                {
                    if (this.module.MetadataImportErrors == null)
                    {
                        this.module.MetadataImportErrors = new ArrayList();
                    }
                    this.module.MetadataImportErrors.Add(exception);
                }
                return false;
            }
        }

        private TypeNode LookupType(string nameSpace, string name, string assemblyName, out System.Compiler.Module module)
        {
            Identifier identifier = Identifier.For(nameSpace);
            Identifier identifier2 = Identifier.For(name);
            module = this.module;
            if (assemblyName == null)
            {
                TypeNode type = module.GetType(identifier, identifier2);
                if (type != null)
                {
                    return type;
                }
                module = CoreSystemTypes.SystemAssembly;
                return CoreSystemTypes.SystemAssembly.GetType(identifier, identifier2);
            }
            AssemblyReferenceList assemblyReferences = module.AssemblyReferences;
            int num = 0;
            int num2 = (assemblyReferences == null) ? 0 : assemblyReferences.Count;
            while (num < num2)
            {
                AssemblyReference reference = assemblyReferences[num];
                if (((reference != null) && (reference.StrongName == assemblyName)) && (reference.Assembly != null))
                {
                    module = reference.Assembly;
                    return reference.Assembly.GetType(identifier, identifier2);
                }
                num++;
            }
            AssemblyReference assemblyReference = new AssemblyReference(assemblyName);
            AssemblyNode node2 = this.module as AssemblyNode;
            if ((node2 != null) && ((node2.Flags & AssemblyFlags.Retargetable) != AssemblyFlags.None))
            {
                assemblyReference.Flags |= AssemblyFlags.Retargetable;
            }
            AssemblyNode assemblyFromReference = this.GetAssemblyFromReference(assemblyReference);
            if (assemblyFromReference != null)
            {
                module = assemblyFromReference;
                return assemblyFromReference.GetType(identifier, identifier2);
            }
            return null;
        }

        private TypeNode ParseArrayOrGenericType(string typeName, TypeNode rootType)
        {
            if ((typeName == null) || (rootType == null))
            {
                return rootType;
            }
            if (typeName.Length == 0)
            {
                throw new InvalidMetadataException(ExceptionStrings.BadSerializedTypeName);
            }
            if (typeName[0] == ']')
            {
                if (typeName.Length == 1)
                {
                    return rootType.GetArrayType(1);
                }
                if ((typeName[1] != '[') || (typeName.Length <= 2))
                {
                    throw new InvalidMetadataException(ExceptionStrings.BadSerializedTypeName);
                }
                return this.ParseArrayOrGenericType(typeName.Substring(2), rootType.GetArrayType(1));
            }
            if (typeName[0] == '*')
            {
                if ((typeName.Length > 1) && (typeName[1] == ']'))
                {
                    if (typeName.Length == 2)
                    {
                        return rootType.GetArrayType(1, true);
                    }
                    if ((typeName[2] == '[') && (typeName.Length > 3))
                    {
                        return this.ParseArrayOrGenericType(typeName.Substring(3), rootType.GetArrayType(1, true));
                    }
                }
                throw new InvalidMetadataException(ExceptionStrings.BadSerializedTypeName);
            }
            if (typeName[0] == ',')
            {
                int rank = 1;
                while ((rank < typeName.Length) && (typeName[rank] == ','))
                {
                    rank++;
                }
                if ((rank < typeName.Length) && (typeName[rank] == ']'))
                {
                    if (typeName.Length == (rank + 1))
                    {
                        return rootType.GetArrayType(rank + 1);
                    }
                    if ((typeName[rank + 1] == '[') && (typeName.Length > (rank + 2)))
                    {
                        return this.ParseArrayOrGenericType(typeName.Substring(rank + 2), rootType.GetArrayType(rank));
                    }
                }
                throw new InvalidMetadataException(ExceptionStrings.BadSerializedTypeName);
            }
            int startIndex = 0;
            if (typeName[0] == '[')
            {
                startIndex = 1;
            }
            TypeNodeList consolidatedArguments = new TypeNodeList();
            for (int i = FindFirstCommaOutsideBrackets(typeName); i > 1; i = FindFirstCommaOutsideBrackets(typeName))
            {
                consolidatedArguments.Add(this.GetTypeFromSerializedName(typeName.Substring(startIndex, i - startIndex)));
                typeName = typeName.Substring(i + 1);
                startIndex = (typeName[0] == '[') ? 1 : 0;
            }
            int num4 = startIndex;
            int num5 = 0;
            while (num4 < typeName.Length)
            {
                char ch = typeName[num4];
                if (ch == '[')
                {
                    num5++;
                }
                else if (ch == ']')
                {
                    num5--;
                    if (num5 < 0)
                    {
                        break;
                    }
                }
                num4++;
            }
            consolidatedArguments.Add(this.GetTypeFromSerializedName(typeName.Substring(startIndex, num4 - startIndex)));
            TypeNode genericTemplateInstance = rootType.GetGenericTemplateInstance(this.module, consolidatedArguments);
            if (((num4 + 1) < typeName.Length) && (typeName[num4 + 1] == ']'))
            {
                num4++;
            }
            if ((num4 + 1) < typeName.Length)
            {
                switch (typeName[num4 + 1])
                {
                    case '+':
                        genericTemplateInstance = this.GetTypeFromSerializedName(typeName.Substring(num4 + 2), genericTemplateInstance);
                        break;

                    case '&':
                        genericTemplateInstance = genericTemplateInstance.GetReferenceType();
                        break;

                    case '*':
                        genericTemplateInstance = genericTemplateInstance.GetPointerType();
                        break;

                    case '[':
                        return this.ParseArrayOrGenericType(typeName.Substring(num4 + 2, ((typeName.Length - 1) - num4) - 1), genericTemplateInstance);
                }
            }
            return genericTemplateInstance;
        }

        private FunctionPointer ParseFunctionPointer(MemoryCursor sigReader)
        {
            CallingConventionFlags flags = (CallingConventionFlags) sigReader.ReadByte();
            int capacity = sigReader.ReadCompressedInt();
            TypeNode returnType = this.ParseTypeSignature(sigReader);
            if (returnType == null)
            {
                returnType = CoreSystemTypes.Object;
            }
            TypeNodeList parameterTypes = new TypeNodeList(capacity);
            int num2 = capacity;
            for (int i = 0; i < capacity; i++)
            {
                TypeNode element = this.ParseTypeSignature(sigReader);
                if (element == null)
                {
                    num2 = i--;
                }
                else
                {
                    parameterTypes.Add(element);
                }
            }
            FunctionPointer pointer = FunctionPointer.For(parameterTypes, returnType);
            pointer.CallingConvention = flags;
            pointer.VarArgStart = num2;
            return pointer;
        }

        private StatementList ParseMethodBody(Method method, int methodIndex, int RVA)
        {
            TypeNodeList currentTypeParameters = this.currentTypeParameters;
            if (method.DeclaringType.Template != null)
            {
                this.currentTypeParameters = method.DeclaringType.ConsolidatedTemplateArguments;
            }
            else
            {
                this.currentTypeParameters = method.DeclaringType.ConsolidatedTemplateParameters;
            }
            StatementList list2 = new BodyParser(this, method, methodIndex, RVA).ParseStatements();
            this.currentTypeParameters = currentTypeParameters;
            return list2;
        }

        private InstructionList ParseMethodInstructions(Method method, int methodIndex, int RVA)
        {
            TypeNodeList currentTypeParameters = this.currentTypeParameters;
            if (method.DeclaringType.Template != null)
            {
                this.currentTypeParameters = method.DeclaringType.ConsolidatedTemplateArguments;
            }
            else
            {
                this.currentTypeParameters = method.DeclaringType.ConsolidatedTemplateParameters;
            }
            InstructionList list2 = new InstructionParser(this, method, methodIndex, RVA).ParseInstructions();
            this.currentTypeParameters = currentTypeParameters;
            return list2;
        }

        private TypeNodeList ParseParameterTypes(out TypeNodeList varArgTypes, MemoryCursor sigReader, int paramCount, ref bool genericParameterEncountered)
        {
            varArgTypes = null;
            TypeNodeList list = new TypeNodeList(paramCount);
            for (int i = 0; i < paramCount; i++)
            {
                TypeNode element = this.ParseTypeSignature(sigReader);
                if (element == null)
                {
                    varArgTypes = new TypeNodeList(paramCount - i);
                    i--;
                }
                else if (varArgTypes != null)
                {
                    varArgTypes.Add(element);
                }
                else
                {
                    if (element.IsGeneric)
                    {
                        genericParameterEncountered = true;
                    }
                    list.Add(element);
                }
            }
            return list;
        }

        private static void ParseSimpleTypeName(string source, out string name, ref int i)
        {
            int length = source.Length;
            int startIndex = i;
            while (i < length)
            {
                char ch = source[i];
                if (ch == '\\')
                {
                    i++;
                }
                else
                {
                    if ((((ch == '.') || (ch == '+')) || ((ch == '&') || (ch == '*'))) || ((ch == '[') || (ch == '!')))
                    {
                        break;
                    }
                    if (ch == '<')
                    {
                        int num3 = 1;
                        while ((num3 > 0) && (++i < length))
                        {
                            switch (source[i])
                            {
                                case '\\':
                                {
                                    i++;
                                    continue;
                                }
                                case '<':
                                {
                                    num3++;
                                    continue;
                                }
                                case '>':
                                    num3--;
                                    break;
                            }
                        }
                    }
                }
                i++;
            }
            if (i < length)
            {
                name = source.Substring(startIndex, i - startIndex);
            }
            else
            {
                name = source.Substring(startIndex);
            }
        }

        private TypeNodeList ParseTypeList(MemoryCursor sigReader)
        {
            int capacity = sigReader.ReadCompressedInt();
            TypeNodeList list = new TypeNodeList(capacity);
            for (int i = 0; i < capacity; i++)
            {
                TypeNode element = this.ParseTypeSignature(sigReader);
                if ((element == null) || (element == Struct.Dummy))
                {
                    if ((this.currentType != null) && !CoreSystemTypes.Initialized)
                    {
                        element = this.currentType;
                    }
                    else
                    {
                        element = new TypeParameter {
                            Name = Identifier.For("Bad type parameter in position " + i),
                            DeclaringModule = this.module
                        };
                    }
                }
                list.Add(element);
            }
            return list;
        }

        private static void ParseTypeName(string source, out string nspace, out string name, out int i)
        {
            int num2;
            i = 0;
            int length = source.Length;
            nspace = string.Empty;
            while (true)
            {
                num2 = i;
                ParseSimpleTypeName(source, out name, ref i);
                if ((i >= length) || (source[i] != '.'))
                {
                    break;
                }
                i++;
            }
            if (num2 != 0)
            {
                nspace = source.Substring(0, num2 - 1);
            }
        }

        private TypeNode ParseTypeSignature(MemoryCursor sigReader)
        {
            bool pinned = false;
            return this.ParseTypeSignature(sigReader, ref pinned, ref pinned);
        }

        private TypeNode ParseTypeSignature(MemoryCursor sigReader, ref bool pinned)
        {
            bool isTypeArgument = false;
            return this.ParseTypeSignature(sigReader, ref pinned, ref isTypeArgument);
        }

        private TypeNode ParseTypeSignature(MemoryCursor sigReader, ref bool pinned, ref bool isTypeArgument)
        {
            int num;
            ElementType type = (ElementType) sigReader.ReadCompressedInt();
            if (type == ElementType.Pinned)
            {
                pinned = true;
                type = (ElementType) sigReader.ReadCompressedInt();
            }
            switch (type)
            {
                case ElementType.Type:
                    return CoreSystemTypes.Type;

                case ElementType.BoxedEnum:
                case ElementType.Object:
                    return CoreSystemTypes.Object;

                case ElementType.Enum:
                    return this.GetTypeFromSerializedName(ReadSerString(sigReader));

                case ElementType.Void:
                    return CoreSystemTypes.Void;

                case ElementType.Boolean:
                    return CoreSystemTypes.Boolean;

                case ElementType.Char:
                    return CoreSystemTypes.Char;

                case ElementType.Int8:
                    return CoreSystemTypes.Int8;

                case ElementType.UInt8:
                    return CoreSystemTypes.UInt8;

                case ElementType.Int16:
                    return CoreSystemTypes.Int16;

                case ElementType.UInt16:
                    return CoreSystemTypes.UInt16;

                case ElementType.Int32:
                    return CoreSystemTypes.Int32;

                case ElementType.UInt32:
                    return CoreSystemTypes.UInt32;

                case ElementType.Int64:
                    return CoreSystemTypes.Int64;

                case ElementType.UInt64:
                    return CoreSystemTypes.UInt64;

                case ElementType.Single:
                    return CoreSystemTypes.Single;

                case ElementType.Double:
                    return CoreSystemTypes.Double;

                case ElementType.String:
                    return CoreSystemTypes.String;

                case ElementType.Pointer:
                    return this.ParseTypeSignature(sigReader, ref pinned)?.GetPointerType();

                case ElementType.Reference:
                    return this.ParseTypeSignature(sigReader, ref pinned)?.GetReferenceType();

                case ElementType.ValueType:
                    return this.DecodeAndGetTypeDefOrRefOrSpec(sigReader.ReadCompressedInt(), true);

                case ElementType.Class:
                    return this.DecodeAndGetTypeDefOrRefOrSpec(sigReader.ReadCompressedInt());

                case ElementType.TypeParameter:
                {
                    TypeNode node4 = null;
                    num = sigReader.ReadCompressedInt();
                    if ((this.currentTypeParameters != null) && (this.currentTypeParameters.Count > num))
                    {
                        node4 = this.currentTypeParameters[num];
                    }
                    if (node4 == null)
                    {
                        HandleError(this.module, string.Format(CultureInfo.CurrentCulture, ExceptionStrings.BadTypeParameterInPositionForType, new object[] { num, (this.currentType == null) ? "" : this.currentType.FullName }));
                        node4 = new TypeParameter {
                            Name = Identifier.For("Bad type parameter in position " + num),
                            DeclaringModule = this.module
                        };
                    }
                    isTypeArgument = true;
                    return node4;
                }
                case ElementType.Array:
                {
                    TypeNode node = this.ParseTypeSignature(sigReader, ref pinned);
                    if (node == null)
                    {
                        node = CoreSystemTypes.Object;
                    }
                    if (node == null)
                    {
                        return null;
                    }
                    int rank = sigReader.ReadCompressedInt();
                    int numSizes = sigReader.ReadCompressedInt();
                    int[] sizes = new int[numSizes];
                    for (int i = 0; i < numSizes; i++)
                    {
                        sizes[i] = sigReader.ReadCompressedInt();
                    }
                    int numLoBounds = sigReader.ReadCompressedInt();
                    int[] loBounds = new int[numLoBounds];
                    for (int j = 0; j < numLoBounds; j++)
                    {
                        loBounds[j] = sigReader.ReadCompressedInt();
                    }
                    return node.GetArrayType(rank, numSizes, numLoBounds, sizes, loBounds);
                }
                case ElementType.GenericTypeInstance:
                {
                    TypeNodeList currentTypeParameters = this.currentTypeParameters;
                    TypeNode node6 = this.ParseTypeSignature(sigReader, ref pinned);
                    this.currentTypeParameters = currentTypeParameters;
                    if (node6 != null)
                    {
                        if (node6.ConsolidatedTemplateParameters == null)
                        {
                            node6.IsGeneric = true;
                            node6.templateParameters = new TypeNodeList();
                            int num2 = 0;
                            int num3 = sigReader.Byte(0);
                            while (num2 < num3)
                            {
                                node6.templateParameters.Add(new TypeParameter());
                                num2++;
                            }
                        }
                        if (CoreSystemTypes.Initialized)
                        {
                            if ((this.currentTypeParameters == null) || (this.currentTypeParameters.Count == 0))
                            {
                                this.currentTypeParameters = node6.ConsolidatedTemplateParameters;
                            }
                            TypeNodeList consolidatedArguments = this.ParseTypeList(sigReader);
                            if (this.module == null)
                            {
                                return null;
                            }
                            TypeNode genericTemplateInstance = node6.GetGenericTemplateInstance(this.module, consolidatedArguments);
                            this.currentTypeParameters = currentTypeParameters;
                            return genericTemplateInstance;
                        }
                        InterfaceExpression expression = new InterfaceExpression(null) {
                            Template = node6,
                            Namespace = node6.Namespace,
                            Name = node6.Name,
                            TemplateArguments = this.ParseTypeList(sigReader)
                        };
                        this.currentTypeParameters = currentTypeParameters;
                        return expression;
                    }
                    return null;
                }
                case ElementType.DynamicallyTypedReference:
                    return CoreSystemTypes.DynamicallyTypedReference;

                case ElementType.IntPtr:
                    return CoreSystemTypes.IntPtr;

                case ElementType.UIntPtr:
                    return CoreSystemTypes.UIntPtr;

                case ElementType.FunctionPointer:
                    return this.ParseFunctionPointer(sigReader);

                case ElementType.SzArray:
                    return this.ParseTypeSignature(sigReader, ref pinned)?.GetArrayType(1);

                case ElementType.MethodParameter:
                {
                    TypeNode node5 = null;
                    num = sigReader.ReadCompressedInt();
                    if ((this.currentMethodTypeParameters != null) && (this.currentMethodTypeParameters.Count > num))
                    {
                        node5 = this.currentMethodTypeParameters[num];
                    }
                    if (node5 == null)
                    {
                        HandleError(this.module, string.Format(CultureInfo.CurrentCulture, ExceptionStrings.BadMethodTypeParameterInPosition, new object[] { num }));
                        node5 = new MethodTypeParameter {
                            Name = Identifier.For("Bad method type parameter in position " + num)
                        };
                    }
                    isTypeArgument = true;
                    return node5;
                }
                case ElementType.RequiredModifier:
                case ElementType.OptionalModifier:
                {
                    TypeNode modifier = this.DecodeAndGetTypeDefOrRefOrSpec(sigReader.ReadCompressedInt());
                    if (modifier == null)
                    {
                        modifier = CoreSystemTypes.Object;
                    }
                    TypeNode modified = this.ParseTypeSignature(sigReader, ref pinned);
                    if (modified == null)
                    {
                        modified = CoreSystemTypes.Object;
                    }
                    if ((modified == null) || (modified == null))
                    {
                        return null;
                    }
                    if (type == ElementType.RequiredModifier)
                    {
                        return RequiredModifier.For(modifier, modified);
                    }
                    return OptionalModifier.For(modifier, modified);
                }
                case ElementType.Sentinel:
                    return null;
            }
            throw new InvalidMetadataException(ExceptionStrings.MalformedSignature);
        }

        private AssemblyNode ReadAssembly() => 
            this.ReadAssembly(null);

        private AssemblyNode ReadAssembly(AssemblyNode.PostAssemblyLoadProcessor postLoadEvent)
        {
            try
            {
                AssemblyNode module = new AssemblyNode(new System.Compiler.Module.TypeNodeProvider(this.GetTypeFromName), new System.Compiler.Module.TypeNodeListProvider(this.GetTypeList), new System.Compiler.Module.CustomAttributeProvider(this.GetCustomAttributesFor), new System.Compiler.Module.ResourceProvider(this.GetResources), this.directory) {
                    reader = this
                };
                this.ReadModuleProperties(module);
                this.ReadAssemblyProperties(module);
                this.module = module;
                this.ReadAssemblyReferences(module);
                this.ReadModuleReferences(module);
                AssemblyNode cachedAssembly = this.GetCachedAssembly(module);
                if (cachedAssembly != null)
                {
                    return cachedAssembly;
                }
                if (this.getDebugSymbols)
                {
                    module.SetupDebugReader(null);
                }
                if (postLoadEvent != null)
                {
                    module.AfterAssemblyLoad += postLoadEvent;
                    postLoadEvent(module);
                }
                return module;
            }
            catch (Exception exception)
            {
                if (this.module == null)
                {
                    return null;
                }
                if (this.module.MetadataImportErrors == null)
                {
                    this.module.MetadataImportErrors = new ArrayList();
                }
                this.module.MetadataImportErrors.Add(exception);
                return (this.module as AssemblyNode);
            }
        }

        private void ReadAssemblyProperties(AssemblyNode assembly)
        {
            AssemblyRow row = this.tables.AssemblyTable[0];
            assembly.HashAlgorithm = (AssemblyHashAlgorithm) row.HashAlgId;
            assembly.Version = new Version(row.MajorVersion, row.MinorVersion, row.BuildNumber, row.RevisionNumber);
            assembly.Flags = (AssemblyFlags) row.Flags;
            assembly.PublicKeyOrToken = this.tables.GetBlob(row.PublicKey);
            assembly.ModuleName = assembly.Name;
            assembly.Name = this.tables.GetString(row.Name);
            assembly.Culture = this.tables.GetString(row.Culture);
            if (this.fileName != null)
            {
                assembly.FileLastWriteTimeUtc = File.GetLastWriteTimeUtc(this.fileName);
            }
            assembly.ContainingAssembly = assembly;
        }

        private void ReadAssemblyReferences(System.Compiler.Module module)
        {
            AssemblyRefRow[] assemblyRefTable = this.tables.AssemblyRefTable;
            int length = assemblyRefTable.Length;
            AssemblyReferenceList list2 = module.AssemblyReferences = new AssemblyReferenceList(length);
            for (int i = 0; i < length; i++)
            {
                AssemblyRefRow row = assemblyRefTable[i];
                AssemblyReference element = new AssemblyReference {
                    Version = new Version(row.MajorVersion, row.MinorVersion, row.BuildNumber, row.RevisionNumber),
                    Flags = (AssemblyFlags) row.Flags,
                    PublicKeyOrToken = this.tables.GetBlob(row.PublicKeyOrToken),
                    Name = this.tables.GetString(row.Name),
                    Culture = this.tables.GetString(row.Culture)
                };
                if ((element.Culture != null) && (element.Culture.Length == 0))
                {
                    element.Culture = null;
                }
                element.HashValue = this.tables.GetBlob(row.HashValue);
                element.Reader = this;
                assemblyRefTable[i].AssemblyReference = element;
                list2.Add(element);
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32", SetLastError=true)]
        private static extern unsafe bool ReadFile(IntPtr FileHandle, byte* Buffer, int NumberOfBytesToRead, int* NumberOfBytesRead, IntPtr Overlapped);
        private unsafe void ReadFileIntoUnmanagedBuffer(FileStream inputStream)
        {
            long num = inputStream.Seek(0L, System.IO.SeekOrigin.End);
            if (num > 0x7fffffffL)
            {
                throw new FileLoadException();
            }
            inputStream.Seek(0L, System.IO.SeekOrigin.Begin);
            int length = (int) num;
            this.bufferLength = length;
            this.unmanagedBuffer = new UnmanagedBuffer(length);
            byte* pointer = (byte*) this.unmanagedBuffer.Pointer;
            if (!ReadFile(inputStream.SafeFileHandle.DangerousGetHandle(), pointer, length, &length, IntPtr.Zero))
            {
                throw new FileLoadException();
            }
        }

        internal System.Compiler.Module ReadModule() => 
            this.ReadModule(null);

        internal System.Compiler.Module ReadModule(AssemblyNode.PostAssemblyLoadProcessor postLoadEvent)
        {
            try
            {
                if (this.fileName != null)
                {
                    AssemblyNode node;
                    if (!File.Exists(this.fileName))
                    {
                        return null;
                    }
                    if (this.useStaticCache)
                    {
                        node = StaticAssemblyCache[this.fileName] as AssemblyNode;
                        if ((node != null) && (node.FileLastWriteTimeUtc == File.GetLastWriteTimeUtc(this.fileName)))
                        {
                            this.Dispose();
                            return node;
                        }
                    }
                    node = this.localAssemblyCache[this.fileName] as AssemblyNode;
                    if ((node != null) && (node.FileLastWriteTimeUtc == File.GetLastWriteTimeUtc(this.fileName)))
                    {
                        this.Dispose();
                        return node;
                    }
                }
                this.SetupReader();
                if (this.tables.AssemblyTable.Length > 0)
                {
                    return this.ReadAssembly(postLoadEvent);
                }
                System.Compiler.Module module = this.module = new System.Compiler.Module(new System.Compiler.Module.TypeNodeProvider(this.GetTypeFromName), new System.Compiler.Module.TypeNodeListProvider(this.GetTypeList), new System.Compiler.Module.CustomAttributeProvider(this.GetCustomAttributesFor), new System.Compiler.Module.ResourceProvider(this.GetResources));
                module.reader = this;
                this.ReadModuleProperties(module);
                this.module = module;
                this.ReadAssemblyReferences(module);
                this.ReadModuleReferences(module);
                if (this.getDebugSymbols)
                {
                    this.SetupDebugReader(this.fileName, null);
                }
                return module;
            }
            catch (Exception exception)
            {
                if (this.module == null)
                {
                    return null;
                }
                if (this.module.MetadataImportErrors == null)
                {
                    this.module.MetadataImportErrors = new ArrayList();
                }
                this.module.MetadataImportErrors.Add(exception);
                return this.module;
            }
        }

        private void ReadModuleProperties(System.Compiler.Module module)
        {
            ModuleRow[] moduleTable = this.tables.ModuleTable;
            if (moduleTable.Length != 1)
            {
                throw new InvalidMetadataException(ExceptionStrings.InvalidModuleTable);
            }
            ModuleRow row = moduleTable[0];
            module.reader = this;
            module.DllCharacteristics = this.tables.dllCharacteristics;
            module.FileAlignment = this.tables.fileAlignment;
            module.HashValue = this.tables.HashValue;
            module.Kind = this.tables.moduleKind;
            module.Location = this.fileName;
            module.TargetRuntimeVersion = this.tables.targetRuntimeVersion;
            module.LinkerMajorVersion = this.tables.linkerMajorVersion;
            module.LinkerMinorVersion = this.tables.linkerMinorVersion;
            module.MetadataFormatMajorVersion = this.tables.metadataFormatMajorVersion;
            module.MetadataFormatMinorVersion = this.tables.metadataFormatMinorVersion;
            module.Name = this.tables.GetString(row.Name);
            module.Mvid = this.tables.GetGuid(row.Mvid);
            module.PEKind = this.tables.peKind;
            module.TrackDebugData = this.tables.TrackDebugData;
        }

        private void ReadModuleReferences(System.Compiler.Module module)
        {
            FileRow[] fileTable = this.tables.FileTable;
            ModuleRefRow[] moduleRefTable = this.tables.ModuleRefTable;
            int length = moduleRefTable.Length;
            ModuleReferenceList list2 = module.ModuleReferences = new ModuleReferenceList(length);
            for (int i = 0; i < length; i++)
            {
                System.Compiler.Module module2;
                int name = moduleRefTable[i].Name;
                string str = this.tables.GetString(name);
                string location = BetterPath.Combine(BetterPath.GetDirectoryName(this.module.Location), str);
                int index = 0;
                int num5 = (fileTable == null) ? 0 : fileTable.Length;
                while (index < num5)
                {
                    if (fileTable[index].Name == name)
                    {
                        if ((fileTable[index].Flags & 1) == 0)
                        {
                            module2 = System.Compiler.Module.GetModule(location, this.doNotLockFile, this.getDebugSymbols, false);
                        }
                        else
                        {
                            module2 = null;
                        }
                        if (module2 == null)
                        {
                            module2 = new System.Compiler.Module {
                                Name = str,
                                Location = location,
                                Kind = ModuleKindFlags.UnmanagedDynamicallyLinkedLibrary
                            };
                        }
                        module2.HashValue = this.tables.GetBlob(fileTable[index].HashValue);
                        module2.ContainingAssembly = module.ContainingAssembly;
                        moduleRefTable[i].Module = module2;
                        list2.Add(new ModuleReference(str, module2));
                        continue;
                    }
                    index++;
                }
                module2 = new System.Compiler.Module {
                    Name = str,
                    Kind = ModuleKindFlags.UnmanagedDynamicallyLinkedLibrary
                };
                if (File.Exists(location))
                {
                    module2.Location = location;
                }
                module2.ContainingAssembly = module.ContainingAssembly;
                moduleRefTable[i].Module = module2;
                list2.Add(new ModuleReference(str, module2));
            }
        }

        private static string ReadSerString(MemoryCursor sigReader)
        {
            int bytesToRead = sigReader.ReadCompressedInt();
            if (bytesToRead < 0)
            {
                return null;
            }
            return sigReader.ReadUTF8(bytesToRead);
        }

        private void RemoveTypeParametersBelongingToDeclaringType(int i, ref TypeNodeList typeParameters, TypeNode type)
        {
            NestedClassRow[] nestedClassTable = this.tables.NestedClassTable;
            int index = 0;
            int length = nestedClassTable.Length;
            while (index < length)
            {
                NestedClassRow row = nestedClassTable[index];
                if (row.NestedClass == i)
                {
                    type.DeclaringType = this.GetTypeFromDef(row.EnclosingClass);
                    if (((type.DeclaringType != null) && type.DeclaringType.IsGeneric) && (type.templateParameters != null))
                    {
                        int inheritedTypeParameterCount = GetInheritedTypeParameterCount(type);
                        int count = type.templateParameters.Count;
                        if (inheritedTypeParameterCount >= count)
                        {
                            type.templateParameters = null;
                        }
                        else
                        {
                            TypeNodeList list = new TypeNodeList(count - inheritedTypeParameterCount);
                            for (int j = inheritedTypeParameterCount; j < count; j++)
                            {
                                list.Add(type.templateParameters[j]);
                            }
                            type.templateParameters = list;
                        }
                        this.currentTypeParameters = typeParameters = type.ConsolidatedTemplateParameters;
                        return;
                    }
                    break;
                }
                index++;
            }
        }

        private Method SearchBaseInterface(Interface iface, Identifier memberName, TypeNode returnType, TypeNodeList paramTypes, int typeParamCount, CallingConventionFlags callingConvention)
        {
            MemberList membersNamed = iface.GetMembersNamed(memberName);
            int num = 0;
            int count = membersNamed.Count;
            while (num < count)
            {
                Method method = membersNamed[num] as Method;
                if (((((method != null) && (method.ReturnType != null)) && (TypeNode.StripModifiers(method.ReturnType).IsStructurallyEquivalentTo(TypeNode.StripModifiers(returnType)) && method.ParameterTypesMatchStructurally(paramTypes))) && (method.CallingConvention == callingConvention)) && ((typeParamCount == -2147483648) || ((method.IsGeneric && (method.TemplateParameters != null)) && (method.TemplateParameters.Count == typeParamCount))))
                {
                    return method;
                }
                num++;
            }
            int num3 = 0;
            int num4 = (iface == null) ? 0 : iface.Interfaces.Count;
            while (num3 < num4)
            {
                Method method2 = this.SearchBaseInterface(iface.Interfaces[num3], memberName, returnType, paramTypes, typeParamCount, callingConvention);
                if (method2 != null)
                {
                    return method2;
                }
                num3++;
            }
            return null;
        }

        internal void SetupDebugReader(string filename, string pdbSearchPath)
        {
            if (filename != null)
            {
                CorSymBinder o = null;
                CorSymBinder2 binder2 = null;
                this.getDebugSymbolsFailed = false;
                object importer = null;
                try
                {
                    int num = 0;
                    try
                    {
                        binder2 = new CorSymBinder2();
                        ISymUnmanagedBinder2 binder3 = (ISymUnmanagedBinder2) binder2;
                        importer = new Ir2md(new System.Compiler.Module());
                        num = binder3.GetReaderForFile(importer, filename, pdbSearchPath, out this.debugReader);
                    }
                    catch (COMException exception)
                    {
                        if (exception.ErrorCode != -2147221231)
                        {
                            throw;
                        }
                        o = new CorSymBinder();
                        num = ((ISymUnmanagedBinder) o).GetReaderForFile(importer, filename, null, out this.debugReader);
                    }
                    switch (((uint) num))
                    {
                        case 0:
                            return;

                        case 0x806d0005:
                        case 0x806d0014:
                            if (File.Exists(Path.ChangeExtension(filename, ".pdb")))
                            {
                                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExceptionStrings.PdbAssociatedWithFileIsOutOfDate, new object[] { filename }));
                            }
                            return;
                    }
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExceptionStrings.GetReaderForFileReturnedUnexpectedHResult, new object[] { num.ToString("X") }));
                }
                catch (Exception exception2)
                {
                    this.getDebugSymbols = false;
                    this.getDebugSymbolsFailed = true;
                    if (this.module.MetadataImportErrors == null)
                    {
                        this.module.MetadataImportErrors = new ArrayList();
                    }
                    this.module.MetadataImportErrors.Add(exception2);
                }
                finally
                {
                    if (o != null)
                    {
                        Marshal.ReleaseComObject(o);
                    }
                    if (binder2 != null)
                    {
                        Marshal.ReleaseComObject(binder2);
                    }
                }
            }
        }

        private unsafe void SetupReader()
        {
            if (this.doNotLockFile)
            {
                using (FileStream stream = new FileStream(this.fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    this.ReadFileIntoUnmanagedBuffer(stream);
                }
            }
            if (this.unmanagedBuffer == null)
            {
                this.tables = new MetadataReader(this.fileName);
            }
            else
            {
                this.tables = new MetadataReader((byte*) this.unmanagedBuffer.Pointer, this.bufferLength);
            }
            this.sortedTablesMask = this.tables.tablesHeader.maskSorted;
        }

        private bool TypeDefIsClass(int i)
        {
            if (i == 0)
            {
                return false;
            }
            TypeDefRow row = this.tables.TypeDefTable[i - 1];
            if (row.Type != null)
            {
                return (row.Type is Class);
            }
            if ((row.Flags & 0x20) != 0)
            {
                return false;
            }
            return this.TypeDefOrRefOrSpecIsClassButNotValueTypeBaseClass(row.Extends);
        }

        private bool TypeDefIsClassButNotValueTypeBaseClass(int i)
        {
            if (i == 0)
            {
                return false;
            }
            TypeDefRow row = this.tables.TypeDefTable[i - 1];
            if (row.Type != null)
            {
                return (((row.Type != CoreSystemTypes.ValueType) && (row.Type != CoreSystemTypes.Enum)) && (row.Type is Class));
            }
            if ((row.Flags & 0x20) != 0)
            {
                return false;
            }
            return this.TypeDefOrRefOrSpecIsClassButNotValueTypeBaseClass(row.Extends);
        }

        private bool TypeDefOrRefOrSpecIsClass(int codedIndex)
        {
            if (codedIndex == 0)
            {
                return false;
            }
            switch ((codedIndex & 3))
            {
                case 0:
                    return this.TypeDefIsClass(codedIndex >> 2);

                case 1:
                    return (this.GetTypeFromRef(codedIndex >> 2) is Class);

                case 2:
                    return this.TypeSpecIsClass(codedIndex >> 2);
            }
            throw new InvalidMetadataException(ExceptionStrings.BadTypeDefOrRef);
        }

        private bool TypeDefOrRefOrSpecIsClassButNotValueTypeBaseClass(int codedIndex)
        {
            if (codedIndex == 0)
            {
                return false;
            }
            switch ((codedIndex & 3))
            {
                case 0:
                    return this.TypeDefIsClassButNotValueTypeBaseClass(codedIndex >> 2);

                case 1:
                {
                    TypeNode typeFromRef = this.GetTypeFromRef(codedIndex >> 2);
                    if ((typeFromRef == CoreSystemTypes.ValueType) || (typeFromRef == CoreSystemTypes.Enum))
                    {
                        return false;
                    }
                    return (typeFromRef is Class);
                }
                case 2:
                    return this.TypeSpecIsClass(codedIndex >> 2);
            }
            throw new InvalidMetadataException(ExceptionStrings.BadTypeDefOrRef);
        }

        private bool TypeSignatureIsClass(MemoryCursor sigReader)
        {
            switch (((ElementType) sigReader.ReadCompressedInt()))
            {
                case ElementType.Pointer:
                case ElementType.Reference:
                case ElementType.Pinned:
                    return this.TypeSignatureIsClass(sigReader);

                case ElementType.Class:
                    return true;

                case ElementType.TypeParameter:
                {
                    int num = sigReader.ReadCompressedInt();
                    if ((this.currentTypeParameters == null) || (this.currentTypeParameters.Count <= num))
                    {
                        return false;
                    }
                    TypeNode node = this.currentTypeParameters[num];
                    if (node == null)
                    {
                        return false;
                    }
                    return (node is Class);
                }
                case ElementType.GenericTypeInstance:
                    return this.TypeSignatureIsClass(sigReader);

                case ElementType.MethodParameter:
                {
                    int num2 = sigReader.ReadCompressedInt();
                    if ((this.currentMethodTypeParameters == null) || (this.currentMethodTypeParameters.Count <= num2))
                    {
                        return false;
                    }
                    TypeNode node2 = this.currentMethodTypeParameters[num2];
                    if (node2 == null)
                    {
                        return false;
                    }
                    return (node2 is Class);
                }
                case ElementType.RequiredModifier:
                case ElementType.OptionalModifier:
                    sigReader.ReadCompressedInt();
                    return this.TypeSignatureIsClass(sigReader);
            }
            return false;
        }

        private bool TypeSpecIsClass(int i)
        {
            TypeSpecRow row = this.tables.TypeSpecTable[i - 1];
            if (row.Type != null)
            {
                return (row.Type is Class);
            }
            this.tables.GetSignatureLength(row.Signature);
            return this.TypeSignatureIsClass(this.tables.GetNewCursor());
        }

        private AssemblyNode.PostAssemblyLoadProcessor ReferringAssemblyPostLoad
        {
            get
            {
                AssemblyNode module = this.module as AssemblyNode;
                return module?.GetAfterAssemblyLoad();
            }
        }

        private delegate TypeNode TypeExtensionProvider(TypeNode.NestedTypeProvider nprovider, TypeNode.TypeAttributeProvider aprovider, TypeNode.TypeMemberProvider mprovider, TypeNode baseType, object handle);
    }
}

