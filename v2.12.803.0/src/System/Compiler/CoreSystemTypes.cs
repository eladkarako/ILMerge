namespace System.Compiler
{
    using System;
    using System.Collections;
    using System.Compiler.Metadata;
    using System.Reflection;

    internal sealed class CoreSystemTypes
    {
        public static Struct ArgIterator;
        public static Class Array;
        public static Class Attribute;
        public static Struct Boolean;
        public static Struct Char;
        public static Struct DateTime;
        public static Class DBNull;
        public static Struct Decimal;
        public static Class Delegate;
        public static Struct Double;
        public static Struct DynamicallyTypedReference;
        public static Class Enum;
        public static Class Exception;
        internal static bool Initialized;
        public static Struct Int16;
        public static Struct Int32;
        public static Struct Int64;
        public static Struct Int8;
        public static Struct IntPtr;
        public static Class IsVolatile;
        public static Class MulticastDelegate;
        public static Class Object;
        public static Struct RuntimeArgumentHandle;
        public static Struct RuntimeFieldHandle;
        public static Struct RuntimeMethodHandle;
        public static Struct RuntimeTypeHandle;
        public static EnumNode SecurityAction;
        public static Struct Single;
        public static Class String;
        public static AssemblyNode SystemAssembly;
        public static Class Type;
        public static Struct UInt16;
        public static Struct UInt32;
        public static Struct UInt64;
        public static Struct UInt8;
        public static Struct UIntPtr;
        public static Class ValueType;
        public static Struct Void;

        static CoreSystemTypes()
        {
            Initialize(TargetPlatform.DoNotLockFiles, TargetPlatform.GetDebugInfo);
        }

        private CoreSystemTypes()
        {
        }

        public static void Clear()
        {
            lock (System.Compiler.Module.GlobalLock)
            {
                if (Reader.StaticAssemblyCache != null)
                {
                    foreach (AssemblyNode node in new ArrayList(Reader.StaticAssemblyCache.Values))
                    {
                        if (node != null)
                        {
                            node.Dispose();
                        }
                    }
                    Reader.StaticAssemblyCache.Clear();
                }
                if ((SystemAssembly != null) && (SystemAssembly != AssemblyNode.Dummy))
                {
                    SystemAssembly.Dispose();
                    SystemAssembly = null;
                }
                ClearStatics();
                Initialized = false;
                TargetPlatform.AssemblyReferenceFor = new TrivialHashtable(0);
            }
        }

        private static void ClearStatics()
        {
            Object = null;
            String = null;
            ValueType = null;
            Enum = null;
            MulticastDelegate = null;
            Array = null;
            Type = null;
            Delegate = null;
            Exception = null;
            Attribute = null;
            Boolean = null;
            Char = null;
            Int8 = null;
            UInt8 = null;
            Int16 = null;
            UInt16 = null;
            Int32 = null;
            UInt32 = null;
            Int64 = null;
            UInt64 = null;
            Single = null;
            Double = null;
            IntPtr = null;
            UIntPtr = null;
            DynamicallyTypedReference = null;
            DBNull = null;
            DateTime = null;
            Decimal = null;
            RuntimeArgumentHandle = null;
            ArgIterator = null;
            RuntimeFieldHandle = null;
            RuntimeMethodHandle = null;
            RuntimeTypeHandle = null;
            IsVolatile = null;
            Void = null;
            SecurityAction = null;
        }

        internal static TypeNode GetDummyTypeNode(AssemblyNode declaringAssembly, string nspace, string name, ElementType typeCode)
        {
            TypeNode node = null;
            switch (typeCode)
            {
                case ElementType.String:
                case ElementType.Class:
                case ElementType.Object:
                    if (((name.Length > 1) && (name[0] == 'I')) && char.IsUpper(name[1]))
                    {
                        node = new Interface();
                    }
                    else if ((name == "MulticastDelegate") || (name == "Delegate"))
                    {
                        node = new Class();
                    }
                    else if (((name.EndsWith("Callback") || name.EndsWith("Handler")) || (name.EndsWith("Delegate") || (name == "ThreadStart"))) || ((name == "FrameGuardGetter") || (name == "GuardThreadStart")))
                    {
                        node = new DelegateNode();
                    }
                    else
                    {
                        node = new Class();
                    }
                    break;

                default:
                    if (name == "CciMemberKind")
                    {
                        node = new EnumNode();
                    }
                    else
                    {
                        node = new Struct();
                    }
                    break;
            }
            node.Name = Identifier.For(name);
            node.Namespace = Identifier.For(nspace);
            node.DeclaringModule = declaringAssembly;
            return node;
        }

        private static AssemblyNode GetSystemAssembly(bool doNotLockFile, bool getDebugInfo)
        {
            AssemblyNode parsedAssembly = SystemAssemblyLocation.ParsedAssembly;
            if (parsedAssembly != null)
            {
                parsedAssembly.TargetRuntimeVersion = TargetPlatform.TargetRuntimeVersion;
                parsedAssembly.MetadataFormatMajorVersion = 1;
                parsedAssembly.MetadataFormatMinorVersion = 1;
                parsedAssembly.LinkerMajorVersion = 8;
                parsedAssembly.LinkerMinorVersion = 0;
                return parsedAssembly;
            }
            if (string.IsNullOrEmpty(SystemAssemblyLocation.Location))
            {
                SystemAssemblyLocation.Location = typeof(object).Assembly.Location;
            }
            parsedAssembly = (AssemblyNode) new Reader(SystemAssemblyLocation.Location, SystemAssemblyLocation.SystemAssemblyCache, doNotLockFile, getDebugInfo, true, false).ReadModule();
            if (((parsedAssembly == null) && (TargetPlatform.TargetVersion != null)) && (TargetPlatform.TargetVersion == typeof(object).Assembly.GetName().Version))
            {
                SystemAssemblyLocation.Location = typeof(object).Assembly.Location;
                parsedAssembly = (AssemblyNode) new Reader(SystemAssemblyLocation.Location, SystemAssemblyLocation.SystemAssemblyCache, doNotLockFile, getDebugInfo, true, false).ReadModule();
            }
            if (parsedAssembly == null)
            {
                parsedAssembly = new AssemblyNode();
                System.Reflection.AssemblyName name = typeof(object).Assembly.GetName();
                parsedAssembly.Name = name.Name;
                parsedAssembly.Version = TargetPlatform.TargetVersion;
                parsedAssembly.PublicKeyOrToken = name.GetPublicKeyToken();
            }
            TargetPlatform.TargetVersion = parsedAssembly.Version;
            TargetPlatform.TargetRuntimeVersion = parsedAssembly.TargetRuntimeVersion;
            return parsedAssembly;
        }

        private static TypeNode GetTypeNodeFor(string nspace, string name, ElementType typeCode)
        {
            TypeNode type = null;
            if (SystemAssembly != null)
            {
                type = SystemAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
            }
            if (type == null)
            {
                type = GetDummyTypeNode(SystemAssembly, nspace, name, typeCode);
            }
            type.typeCode = typeCode;
            return type;
        }

        public static void Initialize(bool doNotLockFile, bool getDebugInfo)
        {
            if (Initialized)
            {
                Clear();
            }
            if (SystemAssembly == null)
            {
                SystemAssembly = GetSystemAssembly(doNotLockFile, getDebugInfo);
            }
            if (SystemAssembly == null)
            {
                throw new InvalidOperationException(ExceptionStrings.InternalCompilerError);
            }
            if (TargetPlatform.TargetVersion == null)
            {
                TargetPlatform.TargetVersion = SystemAssembly.Version;
                if (TargetPlatform.TargetVersion == null)
                {
                    TargetPlatform.TargetVersion = typeof(object).Assembly.GetName().Version;
                }
            }
            if ((TargetPlatform.TargetVersion != null) && (((TargetPlatform.TargetVersion.Major > 1) || (TargetPlatform.TargetVersion.Minor > 1)) || ((TargetPlatform.TargetVersion.Minor == 1) && (TargetPlatform.TargetVersion.Build == 0x270f))))
            {
                if (SystemAssembly.IsValidTypeName(StandardIds.System, Identifier.For("Nullable`1")))
                {
                    TargetPlatform.GenericTypeNamesMangleChar = '`';
                }
                else if (SystemAssembly.IsValidTypeName(StandardIds.System, Identifier.For("Nullable!1")))
                {
                    TargetPlatform.GenericTypeNamesMangleChar = '!';
                }
                else if ((TargetPlatform.TargetVersion.Major == 1) && (TargetPlatform.TargetVersion.Minor == 2))
                {
                    TargetPlatform.GenericTypeNamesMangleChar = '\0';
                }
            }
            Object = (Class) GetTypeNodeFor("System", "Object", ElementType.Object);
            ValueType = (Class) GetTypeNodeFor("System", "ValueType", ElementType.Class);
            Char = (Struct) GetTypeNodeFor("System", "Char", ElementType.Char);
            String = (Class) GetTypeNodeFor("System", "String", ElementType.String);
            Enum = (Class) GetTypeNodeFor("System", "Enum", ElementType.Class);
            MulticastDelegate = (Class) GetTypeNodeFor("System", "MulticastDelegate", ElementType.Class);
            Array = (Class) GetTypeNodeFor("System", "Array", ElementType.Class);
            Type = (Class) GetTypeNodeFor("System", "Type", ElementType.Class);
            Boolean = (Struct) GetTypeNodeFor("System", "Boolean", ElementType.Boolean);
            Int8 = (Struct) GetTypeNodeFor("System", "SByte", ElementType.Int8);
            UInt8 = (Struct) GetTypeNodeFor("System", "Byte", ElementType.UInt8);
            Int16 = (Struct) GetTypeNodeFor("System", "Int16", ElementType.Int16);
            UInt16 = (Struct) GetTypeNodeFor("System", "UInt16", ElementType.UInt16);
            Int32 = (Struct) GetTypeNodeFor("System", "Int32", ElementType.Int32);
            UInt32 = (Struct) GetTypeNodeFor("System", "UInt32", ElementType.UInt32);
            Int64 = (Struct) GetTypeNodeFor("System", "Int64", ElementType.Int64);
            UInt64 = (Struct) GetTypeNodeFor("System", "UInt64", ElementType.UInt64);
            Single = (Struct) GetTypeNodeFor("System", "Single", ElementType.Single);
            Double = (Struct) GetTypeNodeFor("System", "Double", ElementType.Double);
            IntPtr = (Struct) GetTypeNodeFor("System", "IntPtr", ElementType.IntPtr);
            UIntPtr = (Struct) GetTypeNodeFor("System", "UIntPtr", ElementType.UIntPtr);
            DynamicallyTypedReference = (Struct) GetTypeNodeFor("System", "TypedReference", ElementType.DynamicallyTypedReference);
            Delegate = (Class) GetTypeNodeFor("System", "Delegate", ElementType.Class);
            Exception = (Class) GetTypeNodeFor("System", "Exception", ElementType.Class);
            Attribute = (Class) GetTypeNodeFor("System", "Attribute", ElementType.Class);
            DBNull = (Class) GetTypeNodeFor("System", "DBNull", ElementType.Class);
            DateTime = (Struct) GetTypeNodeFor("System", "DateTime", ElementType.ValueType);
            Decimal = (Struct) GetTypeNodeFor("System", "Decimal", ElementType.ValueType);
            ArgIterator = (Struct) GetTypeNodeFor("System", "ArgIterator", ElementType.ValueType);
            IsVolatile = (Class) GetTypeNodeFor("System.Runtime.CompilerServices", "IsVolatile", ElementType.Class);
            Void = (Struct) GetTypeNodeFor("System", "Void", ElementType.Void);
            RuntimeFieldHandle = (Struct) GetTypeNodeFor("System", "RuntimeFieldHandle", ElementType.ValueType);
            RuntimeMethodHandle = (Struct) GetTypeNodeFor("System", "RuntimeMethodHandle", ElementType.ValueType);
            RuntimeTypeHandle = (Struct) GetTypeNodeFor("System", "RuntimeTypeHandle", ElementType.ValueType);
            RuntimeArgumentHandle = (Struct) GetTypeNodeFor("System", "RuntimeArgumentHandle", ElementType.ValueType);
            SecurityAction = GetTypeNodeFor("System.Security.Permissions", "SecurityAction", ElementType.ValueType) as EnumNode;
            Initialized = true;
            InstantiateGenericInterfaces();
            Literal.Initialize();
            TrivialHashtable assemblyReferenceFor = TargetPlatform.AssemblyReferenceFor;
        }

        private static void InstantiateGenericInterfaces()
        {
            if (((TargetPlatform.TargetVersion == null) || (TargetPlatform.TargetVersion.Major >= 2)) || (TargetPlatform.TargetVersion.Minor >= 2))
            {
                InstantiateGenericInterfaces(String);
                InstantiateGenericInterfaces(Boolean);
                InstantiateGenericInterfaces(Char);
                InstantiateGenericInterfaces(Int8);
                InstantiateGenericInterfaces(UInt8);
                InstantiateGenericInterfaces(Int16);
                InstantiateGenericInterfaces(UInt16);
                InstantiateGenericInterfaces(Int32);
                InstantiateGenericInterfaces(UInt32);
                InstantiateGenericInterfaces(Int64);
                InstantiateGenericInterfaces(UInt64);
                InstantiateGenericInterfaces(Single);
                InstantiateGenericInterfaces(Double);
                InstantiateGenericInterfaces(DBNull);
                InstantiateGenericInterfaces(DateTime);
                InstantiateGenericInterfaces(Decimal);
            }
        }

        private static void InstantiateGenericInterfaces(TypeNode type)
        {
            if (type != null)
            {
                InterfaceList interfaces = type.Interfaces;
                int num = 0;
                int num2 = (interfaces == null) ? 0 : interfaces.Count;
                while (num < num2)
                {
                    InterfaceExpression expression = interfaces[num] as InterfaceExpression;
                    if ((expression != null) && (expression.Template != null))
                    {
                        TypeNodeList templateArguments = expression.TemplateArguments;
                        int num3 = 0;
                        int count = templateArguments.Count;
                        while (num3 < count)
                        {
                            InterfaceExpression expression2 = templateArguments[num3] as InterfaceExpression;
                            if (expression2 != null)
                            {
                                templateArguments[num3] = expression2.Template.GetGenericTemplateInstance(type.DeclaringModule, expression2.ConsolidatedTemplateArguments);
                            }
                            num3++;
                        }
                        interfaces[num] = (Interface) expression.Template.GetGenericTemplateInstance(type.DeclaringModule, expression.ConsolidatedTemplateArguments);
                    }
                    num++;
                }
            }
        }

        internal static bool IsInitialized =>
            Initialized;
    }
}

