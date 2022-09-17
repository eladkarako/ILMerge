namespace System.Compiler
{
    using System;
    using System.Compiler.Metadata;
    using System.IO;
    using System.Reflection;

    internal sealed class SystemTypes
    {
        public static Class __HandleProtector;
        public static Class Activator;
        public static Class AllowPartiallyTrustedCallersAttribute;
        public static Class AppDomain;
        public static Class ApplicationException;
        public static Class ArgumentException;
        public static Class ArgumentNullException;
        public static Class ArgumentOutOfRangeException;
        public static Class ArrayList;
        public static Class Assembly;
        public static Class AssemblyCompanyAttribute;
        public static Class AssemblyConfigurationAttribute;
        public static Class AssemblyCopyrightAttribute;
        public static Class AssemblyCultureAttribute;
        public static Class AssemblyDelaySignAttribute;
        public static Class AssemblyDescriptionAttribute;
        public static Class AssemblyFileVersionAttribute;
        public static Class AssemblyFlagsAttribute;
        public static Class AssemblyInformationalVersionAttribute;
        public static Class AssemblyKeyFileAttribute;
        public static Class AssemblyKeyNameAttribute;
        public static Class AssemblyProductAttribute;
        public static Class AssemblyTitleAttribute;
        public static Class AssemblyTrademarkAttribute;
        public static Class AssemblyVersionAttribute;
        public static DelegateNode AsyncCallback;
        public static EnumNode AttributeTargets;
        public static Class AttributeUsageAttribute;
        public static Class ClassInterfaceAttribute;
        public static Class CLSCompliantAttribute;
        public static Class CodeAccessPermission;
        public static Class CollectionBase;
        public static AssemblyNode CollectionsAssembly;
        public static Struct Color;
        public static Class ComImportAttribute;
        public static Class ComRegisterFunctionAttribute;
        public static Class ComSourceInterfacesAttribute;
        public static Class ComUnregisterFunctionAttribute;
        public static Class ComVisibleAttribute;
        public static Class ConditionalAttribute;
        public static Struct CornerRadius;
        public static Class CultureInfo;
        public static Struct DateTime;
        public static Struct DateTimeOffset;
        public static Class DBNull;
        public static Class DebuggableAttribute;
        public static Class DebuggerHiddenAttribute;
        public static Class DebuggerStepThroughAttribute;
        public static EnumNode DebuggingModes;
        public static Class DefaultMemberAttribute;
        public static AssemblyNode DiagnosticsDebugAssembly;
        public static AssemblyNode DiagnosticsToolsAssembly;
        public static Class DictionaryBase;
        public static Struct DictionaryEntry;
        public static Class DllImportAttribute;
        public static Class DuplicateWaitObjectException;
        public static Struct Duration;
        public static EnumNode DurationType;
        public static Class Environment;
        public static Class EventArgs;
        public static DelegateNode EventHandler1;
        public static Struct EventRegistrationToken;
        public static Class ExecutionEngineException;
        public static Class FieldOffsetAttribute;
        public static Class FlagsAttribute;
        public static Class GC;
        public static Struct GeneratorPosition;
        public static Struct GenericArraySegment;
        public static Class GenericArrayToIEnumerableAdapter;
        public static Class GenericDictionary;
        public static Interface GenericICollection;
        public static Interface GenericIComparable;
        public static Interface GenericIComparer;
        public static Interface GenericIDictionary;
        public static Interface GenericIEnumerable;
        public static Interface GenericIEnumerator;
        public static Interface GenericIList;
        public static Interface GenericIReadOnlyDictionary;
        public static Interface GenericIReadOnlyList;
        public static Struct GenericKeyValuePair;
        public static Class GenericList;
        public static Struct GenericNullable;
        public static Class GenericQueue;
        public static Class GenericSortedDictionary;
        public static Class GenericStack;
        public static AssemblyNode GlobalizationAssembly;
        public static Struct GridLength;
        public static EnumNode GridUnitType;
        public static Struct Guid;
        public static Class GuidAttribute;
        public static Struct HandleRef;
        public static Class Hashtable;
        public static Interface IASyncResult;
        public static Interface IBindableIterable;
        public static Interface IBindableVector;
        public static Interface ICloneable;
        public static Interface ICollection;
        public static Interface ICommand;
        public static Interface IComparable;
        public static Interface IComparer;
        public static Interface IDictionary;
        public static Interface IDisposable;
        public static Interface IEnumerable;
        public static Interface IEnumerator;
        public static Interface IFormatProvider;
        public static Interface IHashCodeProvider;
        public static Interface IList;
        public static Interface IMembershipCondition;
        public static Class ImportedFromTypeLibAttribute;
        public static Class InAttribute;
        public static Class IndexerNameAttribute;
        public static Class IndexOutOfRangeException;
        internal static bool Initialized;
        public static Interface INotifyCollectionChanged;
        public static Interface INotifyPropertyChanged;
        public static Class InterfaceTypeAttribute;
        public static Class InternalsVisibleToAttribute;
        public static AssemblyNode InteropAssembly;
        public static Class InvalidCastException;
        public static Class InvalidOperationException;
        public static AssemblyNode IOAssembly;
        public static Interface IPermission;
        public static Interface ISerializable;
        public static Interface IStackWalk;
        public static Struct KeyTime;
        public static Class Marshal;
        public static Class MarshalByRefObject;
        public static Struct Matrix;
        public static Struct Matrix3D;
        public static Class MemberInfo;
        public static Class MethodImplAttribute;
        public static Class Monitor;
        public static Struct NativeOverlapped;
        public static Class NonSerializedAttribute;
        public static EnumNode NotifyCollectionChangedAction;
        public static Class NotifyCollectionChangedEventArgs;
        public static DelegateNode NotifyCollectionChangedEventHandler;
        public static Class NotSupportedException;
        public static Class NullReferenceException;
        public static Class ObsoleteAttribute;
        public static Class OptionalAttribute;
        public static Class OutAttribute;
        public static Class OutOfMemoryException;
        public static Class ParamArrayAttribute;
        public static Class ParameterInfo;
        public static Struct Point;
        public static Class PropertyChangedEventArgs;
        public static DelegateNode PropertyChangedEventHandler;
        public static Class Queue;
        public static Class ReadOnlyCollectionBase;
        public static Struct Rect;
        public static AssemblyNode ReflectionAssembly;
        public static Struct RepeatBehavior;
        public static EnumNode RepeatBehaviorType;
        public static Class ResourceManager;
        public static AssemblyNode ResourceManagerAssembly;
        public static Class ResourceSet;
        public static Class RuntimeCompatibilityAttribute;
        public static Class SatelliteContractVersionAttribute;
        public static EnumNode SecurityAction;
        public static Class SecurityAttribute;
        public static Class SecurityCriticalAttribute;
        public static Class SecurityTransparentAttribute;
        public static Class SecurityTreatAsSafeAttribute;
        public static Class SerializableAttribute;
        public static Class SerializationInfo;
        public static Struct Size;
        public static Class Stack;
        public static Class StackOverflowException;
        public static Class STAThreadAttribute;
        public static Class Stream;
        public static Struct StreamingContext;
        public static Class StringBuilder;
        public static Class StringComparer;
        public static EnumNode StringComparison;
        public static Class StructLayoutAttribute;
        public static Class SuppressMessageAttribute;
        public static Class SuppressUnmanagedCodeSecurityAttribute;
        public static AssemblyNode SystemDllAssembly;
        public static Class SystemException;
        public static AssemblyNode SystemRuntimeExtensionsAssembly;
        public static AssemblyNode SystemRuntimeSerializationAssembly;
        public static AssemblyNode SystemRuntimeWindowsRuntimeAssembly;
        public static AssemblyNode SystemRuntimeWindowsRuntimeInteropAssembly;
        public static AssemblyNode SystemRuntimeWindowsRuntimeUIXamlAssembly;
        public static Struct Thickness;
        public static Class Thread;
        public static AssemblyNode ThreadingAssembly;
        public static Struct TimeSpan;
        public static Class Uri;
        public static Class WindowsImpersonationContext;

        static SystemTypes()
        {
            Initialize(TargetPlatform.DoNotLockFiles, TargetPlatform.GetDebugInfo);
        }

        private SystemTypes()
        {
        }

        public static void Clear()
        {
            lock (System.Compiler.Module.GlobalLock)
            {
                CoreSystemTypes.Clear();
                ClearStatics();
                Initialized = false;
            }
        }

        private static void ClearStatics()
        {
            AttributeUsageAttribute = null;
            ConditionalAttribute = null;
            DefaultMemberAttribute = null;
            InternalsVisibleToAttribute = null;
            ObsoleteAttribute = null;
            GenericICollection = null;
            GenericIEnumerable = null;
            GenericIList = null;
            ICloneable = null;
            ICollection = null;
            IEnumerable = null;
            IList = null;
            AllowPartiallyTrustedCallersAttribute = null;
            AssemblyCompanyAttribute = null;
            AssemblyConfigurationAttribute = null;
            AssemblyCopyrightAttribute = null;
            AssemblyCultureAttribute = null;
            AssemblyDelaySignAttribute = null;
            AssemblyDescriptionAttribute = null;
            AssemblyFileVersionAttribute = null;
            AssemblyFlagsAttribute = null;
            AssemblyInformationalVersionAttribute = null;
            AssemblyKeyFileAttribute = null;
            AssemblyKeyNameAttribute = null;
            AssemblyProductAttribute = null;
            AssemblyTitleAttribute = null;
            AssemblyTrademarkAttribute = null;
            AssemblyVersionAttribute = null;
            ClassInterfaceAttribute = null;
            CLSCompliantAttribute = null;
            ComImportAttribute = null;
            ComRegisterFunctionAttribute = null;
            ComSourceInterfacesAttribute = null;
            ComUnregisterFunctionAttribute = null;
            ComVisibleAttribute = null;
            DebuggableAttribute = null;
            DebuggerHiddenAttribute = null;
            DebuggerStepThroughAttribute = null;
            DebuggingModes = null;
            DllImportAttribute = null;
            FieldOffsetAttribute = null;
            FlagsAttribute = null;
            GuidAttribute = null;
            ImportedFromTypeLibAttribute = null;
            InAttribute = null;
            IndexerNameAttribute = null;
            InterfaceTypeAttribute = null;
            MethodImplAttribute = null;
            NonSerializedAttribute = null;
            OptionalAttribute = null;
            OutAttribute = null;
            ParamArrayAttribute = null;
            RuntimeCompatibilityAttribute = null;
            SatelliteContractVersionAttribute = null;
            SerializableAttribute = null;
            SecurityAttribute = null;
            SecurityCriticalAttribute = null;
            SecurityTransparentAttribute = null;
            SecurityTreatAsSafeAttribute = null;
            STAThreadAttribute = null;
            StructLayoutAttribute = null;
            SuppressMessageAttribute = null;
            SuppressUnmanagedCodeSecurityAttribute = null;
            SecurityAction = null;
            DBNull = null;
            DateTime = null;
            DateTimeOffset = null;
            TimeSpan = null;
            Activator = null;
            AppDomain = null;
            ApplicationException = null;
            ArgumentException = null;
            ArgumentNullException = null;
            ArgumentOutOfRangeException = null;
            ArrayList = null;
            AsyncCallback = null;
            Assembly = null;
            AttributeTargets = null;
            CodeAccessPermission = null;
            CollectionBase = null;
            Color = null;
            CornerRadius = null;
            CultureInfo = null;
            DictionaryBase = null;
            DictionaryEntry = null;
            DuplicateWaitObjectException = null;
            Duration = null;
            DurationType = null;
            Environment = null;
            EventArgs = null;
            EventRegistrationToken = null;
            ExecutionEngineException = null;
            GeneratorPosition = null;
            GenericArraySegment = null;
            GenericArrayToIEnumerableAdapter = null;
            GenericDictionary = null;
            GenericIComparable = null;
            GenericIComparer = null;
            GenericIDictionary = null;
            GenericIEnumerator = null;
            GenericKeyValuePair = null;
            GenericList = null;
            GenericNullable = null;
            GenericQueue = null;
            GenericSortedDictionary = null;
            GenericStack = null;
            GC = null;
            GridLength = null;
            GridUnitType = null;
            Guid = null;
            __HandleProtector = null;
            HandleRef = null;
            Hashtable = null;
            IASyncResult = null;
            IComparable = null;
            IDictionary = null;
            IComparer = null;
            IDisposable = null;
            IEnumerator = null;
            IFormatProvider = null;
            IHashCodeProvider = null;
            IMembershipCondition = null;
            IndexOutOfRangeException = null;
            InvalidCastException = null;
            InvalidOperationException = null;
            IPermission = null;
            ISerializable = null;
            IStackWalk = null;
            KeyTime = null;
            Marshal = null;
            MarshalByRefObject = null;
            Matrix = null;
            Matrix3D = null;
            MemberInfo = null;
            NativeOverlapped = null;
            Monitor = null;
            NotSupportedException = null;
            NullReferenceException = null;
            OutOfMemoryException = null;
            ParameterInfo = null;
            Point = null;
            Queue = null;
            ReadOnlyCollectionBase = null;
            Rect = null;
            RepeatBehavior = null;
            RepeatBehaviorType = null;
            ResourceManager = null;
            ResourceSet = null;
            SerializationInfo = null;
            Size = null;
            Stack = null;
            StackOverflowException = null;
            Stream = null;
            StreamingContext = null;
            StringBuilder = null;
            StringComparer = null;
            StringComparison = null;
            SystemException = null;
            Thickness = null;
            Thread = null;
            Uri = null;
            WindowsImpersonationContext = null;
        }

        private static AssemblyNode GetCollectionsAssembly(bool doNotLockFile, bool getDebugInfo)
        {
            if (SystemAssembly.Name == "mscorlib")
            {
                return SystemAssembly;
            }
            Identifier identifier = Identifier.For("System.Collections");
            AssemblyReference assemblyReference = (AssemblyReference) TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey];
            if (assemblyReference == null)
            {
                assemblyReference = new AssemblyReference {
                    Name = "System.Collections",
                    PublicKeyOrToken = new byte[] { 
                        0xb0,
                        0x3f,
                        0x5f,
                        0x7f,
                        0x11,
                        0xd5,
                        10,
                        0x3a
                    },
                    Version = TargetPlatform.TargetVersion
                };
                TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey] = assemblyReference;
            }
            if (string.IsNullOrEmpty(SystemRuntimeCollectionsAssemblyLocation.Location))
            {
                if (assemblyReference.Location == null)
                {
                    SystemRuntimeCollectionsAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Collections.dll");
                }
                else
                {
                    SystemRuntimeCollectionsAssemblyLocation.Location = assemblyReference.Location;
                }
            }
            if (assemblyReference.assembly == null)
            {
                assemblyReference.Location = SystemRuntimeCollectionsAssemblyLocation.Location;
            }
            return (assemblyReference.assembly = AssemblyNode.GetAssembly(assemblyReference));
        }

        private static TypeNode GetCollectionsGenericRuntimeTypeNodeFor(string nspace, string name, int numParams, ElementType typeCode)
        {
            if (TargetPlatform.GenericTypeNamesMangleChar != '\0')
            {
                name = name + TargetPlatform.GenericTypeNamesMangleChar + numParams;
            }
            return GetCollectionsTypeNodeFor(nspace, name, typeCode);
        }

        private static TypeNode GetCollectionsTypeNodeFor(string nspace, string name, ElementType typeCode)
        {
            TypeNode type = null;
            if (CollectionsAssembly != null)
            {
                type = CollectionsAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
            }
            if (type == null)
            {
                type = CoreSystemTypes.GetDummyTypeNode(CollectionsAssembly, nspace, name, typeCode);
            }
            type.typeCode = typeCode;
            return type;
        }

        private static AssemblyNode GetDiagnosticsDebugAssembly(bool doNotLockFile, bool getDebugInfo)
        {
            if (SystemAssembly.Name == "mscorlib")
            {
                return SystemAssembly;
            }
            Identifier identifier = Identifier.For("System.Diagnostics.Debug");
            AssemblyReference assemblyReference = (AssemblyReference) TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey];
            if (assemblyReference == null)
            {
                assemblyReference = new AssemblyReference {
                    Name = "System.Diagnostics.Debug",
                    PublicKeyOrToken = new byte[] { 
                        0xb0,
                        0x3f,
                        0x5f,
                        0x7f,
                        0x11,
                        0xd5,
                        10,
                        0x3a
                    },
                    Version = TargetPlatform.TargetVersion
                };
                TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey] = assemblyReference;
            }
            if (string.IsNullOrEmpty(SystemDiagnosticsDebugAssemblyLocation.Location))
            {
                if (assemblyReference.Location == null)
                {
                    SystemDiagnosticsDebugAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Diagnostics.Debug.dll");
                }
                else
                {
                    SystemDiagnosticsDebugAssemblyLocation.Location = assemblyReference.Location;
                }
            }
            if (assemblyReference.assembly == null)
            {
                assemblyReference.Location = SystemDiagnosticsDebugAssemblyLocation.Location;
            }
            return (assemblyReference.assembly = AssemblyNode.GetAssembly(assemblyReference));
        }

        private static TypeNode GetDiagnosticsDebugTypeNodeFor(string nspace, string name, ElementType typeCode)
        {
            TypeNode type = null;
            if (DiagnosticsDebugAssembly != null)
            {
                type = DiagnosticsDebugAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
            }
            if (type == null)
            {
                type = CoreSystemTypes.GetDummyTypeNode(DiagnosticsDebugAssembly, nspace, name, typeCode);
            }
            type.typeCode = typeCode;
            return type;
        }

        private static AssemblyNode GetDiagnosticsToolsAssembly(bool doNotLockFile, bool getDebugInfo)
        {
            if (SystemAssembly.Name == "mscorlib")
            {
                return SystemAssembly;
            }
            Identifier identifier = Identifier.For("System.Diagnostics.Tools");
            AssemblyReference assemblyReference = (AssemblyReference) TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey];
            if (assemblyReference == null)
            {
                assemblyReference = new AssemblyReference {
                    Name = "System.Diagnostics.Debug",
                    PublicKeyOrToken = new byte[] { 
                        0xb0,
                        0x3f,
                        0x5f,
                        0x7f,
                        0x11,
                        0xd5,
                        10,
                        0x3a
                    },
                    Version = TargetPlatform.TargetVersion
                };
                TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey] = assemblyReference;
            }
            if (string.IsNullOrEmpty(SystemDiagnosticsToolsAssemblyLocation.Location))
            {
                if (assemblyReference.Location == null)
                {
                    SystemDiagnosticsToolsAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Diagnostics.Tools.dll");
                }
                else
                {
                    SystemDiagnosticsToolsAssemblyLocation.Location = assemblyReference.Location;
                }
            }
            if (assemblyReference.assembly == null)
            {
                assemblyReference.Location = SystemDiagnosticsToolsAssemblyLocation.Location;
            }
            return (assemblyReference.assembly = AssemblyNode.GetAssembly(assemblyReference));
        }

        private static TypeNode GetDiagnosticsToolsTypeNodeFor(string nspace, string name, ElementType typeCode)
        {
            TypeNode type = null;
            if (DiagnosticsToolsAssembly != null)
            {
                type = DiagnosticsToolsAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
            }
            if (type == null)
            {
                type = CoreSystemTypes.GetDummyTypeNode(DiagnosticsToolsAssembly, nspace, name, typeCode);
            }
            type.typeCode = typeCode;
            return type;
        }

        private static TypeNode GetGenericRuntimeTypeNodeFor(string nspace, string name, int numParams, ElementType typeCode)
        {
            if (TargetPlatform.GenericTypeNamesMangleChar != '\0')
            {
                name = name + TargetPlatform.GenericTypeNamesMangleChar + numParams;
            }
            return GetTypeNodeFor(nspace, name, typeCode);
        }

        private static TypeNode GetGenericWindowsRuntimeTypeNodeFor(string nspace, string name, int numParams, ElementType typeCode)
        {
            if (TargetPlatform.GenericTypeNamesMangleChar != '\0')
            {
                name = name + TargetPlatform.GenericTypeNamesMangleChar + numParams;
            }
            return GetWindowsRuntimeTypeNodeFor(nspace, name, typeCode);
        }

        private static TypeNode GetGenericWindowsRuntimeUIXamlTypeNodeFor(string nspace, string name, int numParams, ElementType typeCode)
        {
            if (TargetPlatform.GenericTypeNamesMangleChar != '\0')
            {
                name = name + TargetPlatform.GenericTypeNamesMangleChar + numParams;
            }
            return GetWindowsRuntimeUIXamlTypeNodeFor(nspace, name, typeCode);
        }

        private static AssemblyNode GetGlobalizationAssembly(bool doNotLockFile, bool getDebugInfo)
        {
            if (SystemAssembly.Name == "mscorlib")
            {
                return SystemAssembly;
            }
            Identifier identifier = Identifier.For("System.Globalization");
            AssemblyReference assemblyReference = (AssemblyReference) TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey];
            if (assemblyReference == null)
            {
                assemblyReference = new AssemblyReference {
                    Name = "System.Globalization",
                    PublicKeyOrToken = new byte[] { 
                        0xb0,
                        0x3f,
                        0x5f,
                        0x7f,
                        0x11,
                        0xd5,
                        10,
                        0x3a
                    },
                    Version = TargetPlatform.TargetVersion
                };
                TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey] = assemblyReference;
            }
            if (string.IsNullOrEmpty(SystemGlobalizationAssemblyLocation.Location))
            {
                if (assemblyReference.Location == null)
                {
                    SystemGlobalizationAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Globalization.dll");
                }
                else
                {
                    SystemGlobalizationAssemblyLocation.Location = assemblyReference.Location;
                }
            }
            if (assemblyReference.assembly == null)
            {
                assemblyReference.Location = SystemGlobalizationAssemblyLocation.Location;
            }
            return (assemblyReference.assembly = AssemblyNode.GetAssembly(assemblyReference));
        }

        private static TypeNode GetGlobalizationTypeNodeFor(string nspace, string name, ElementType typeCode)
        {
            TypeNode type = null;
            if (GlobalizationAssembly != null)
            {
                type = GlobalizationAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
            }
            if (type == null)
            {
                type = CoreSystemTypes.GetDummyTypeNode(GlobalizationAssembly, nspace, name, typeCode);
            }
            type.typeCode = typeCode;
            return type;
        }

        private static AssemblyNode GetInteropAssembly(bool doNotLockFile, bool getDebugInfo)
        {
            if (SystemAssembly.Name == "mscorlib")
            {
                return SystemAssembly;
            }
            Identifier identifier = Identifier.For("System.Runtime.InteropServices");
            AssemblyReference assemblyReference = (AssemblyReference) TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey];
            if (assemblyReference == null)
            {
                assemblyReference = new AssemblyReference {
                    Name = "System.Runtime.InteropServices",
                    PublicKeyOrToken = new byte[] { 
                        0xb0,
                        0x3f,
                        0x5f,
                        0x7f,
                        0x11,
                        0xd5,
                        10,
                        0x3a
                    },
                    Version = TargetPlatform.TargetVersion
                };
                TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey] = assemblyReference;
            }
            if (string.IsNullOrEmpty(SystemRuntimeInteropServicesAssemblyLocation.Location))
            {
                if (assemblyReference.Location == null)
                {
                    SystemRuntimeInteropServicesAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Runtime.InteropServices.dll");
                }
                else
                {
                    SystemRuntimeInteropServicesAssemblyLocation.Location = assemblyReference.Location;
                }
            }
            if (assemblyReference.assembly == null)
            {
                assemblyReference.Location = SystemRuntimeInteropServicesAssemblyLocation.Location;
            }
            return (assemblyReference.assembly = AssemblyNode.GetAssembly(assemblyReference));
        }

        private static TypeNode GetInteropTypeNodeFor(string nspace, string name, ElementType typeCode)
        {
            TypeNode type = null;
            if (SystemAssembly != null)
            {
                type = InteropAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
            }
            if (type == null)
            {
                type = CoreSystemTypes.GetDummyTypeNode(InteropAssembly, nspace, name, typeCode);
            }
            type.typeCode = typeCode;
            return type;
        }

        private static AssemblyNode GetIOAssembly(bool doNotLockFile, bool getDebugInfo)
        {
            if (SystemAssembly.Name == "mscorlib")
            {
                return SystemAssembly;
            }
            Identifier identifier = Identifier.For("System.IO");
            AssemblyReference assemblyReference = (AssemblyReference) TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey];
            if (assemblyReference == null)
            {
                assemblyReference = new AssemblyReference {
                    Name = "System.IO",
                    PublicKeyOrToken = new byte[] { 
                        0xb0,
                        0x3f,
                        0x5f,
                        0x7f,
                        0x11,
                        0xd5,
                        10,
                        0x3a
                    },
                    Version = TargetPlatform.TargetVersion
                };
                TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey] = assemblyReference;
            }
            if (string.IsNullOrEmpty(SystemRuntimeIOServicesAssemblyLocation.Location))
            {
                if (assemblyReference.Location == null)
                {
                    SystemRuntimeIOServicesAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.IO.dll");
                }
                else
                {
                    SystemRuntimeIOServicesAssemblyLocation.Location = assemblyReference.Location;
                }
            }
            if (assemblyReference.assembly == null)
            {
                assemblyReference.Location = SystemRuntimeIOServicesAssemblyLocation.Location;
            }
            return (assemblyReference.assembly = AssemblyNode.GetAssembly(assemblyReference));
        }

        private static TypeNode GetIOTypeNodeFor(string nspace, string name, ElementType typeCode)
        {
            TypeNode type = null;
            if (IOAssembly != null)
            {
                type = IOAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
            }
            if (type == null)
            {
                type = CoreSystemTypes.GetDummyTypeNode(IOAssembly, nspace, name, typeCode);
            }
            type.typeCode = typeCode;
            return type;
        }

        private static AssemblyNode GetReflectionAssembly(bool doNotLockFile, bool getDebugInfo)
        {
            if (SystemAssembly.Name == "mscorlib")
            {
                return SystemAssembly;
            }
            Identifier identifier = Identifier.For("System.Reflection");
            AssemblyReference assemblyReference = (AssemblyReference) TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey];
            if (assemblyReference == null)
            {
                assemblyReference = new AssemblyReference {
                    Name = "System.Reflection",
                    PublicKeyOrToken = new byte[] { 
                        0xb0,
                        0x3f,
                        0x5f,
                        0x7f,
                        0x11,
                        0xd5,
                        10,
                        0x3a
                    },
                    Version = TargetPlatform.TargetVersion
                };
                TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey] = assemblyReference;
            }
            if (string.IsNullOrEmpty(SystemReflectionAssemblyLocation.Location))
            {
                if (assemblyReference.Location == null)
                {
                    SystemReflectionAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Reflection.dll");
                }
                else
                {
                    SystemReflectionAssemblyLocation.Location = assemblyReference.Location;
                }
            }
            if (assemblyReference.assembly == null)
            {
                assemblyReference.Location = SystemReflectionAssemblyLocation.Location;
            }
            return (assemblyReference.assembly = AssemblyNode.GetAssembly(assemblyReference));
        }

        private static TypeNode GetReflectionTypeNodeFor(string nspace, string name, ElementType typeCode)
        {
            TypeNode type = null;
            if (ReflectionAssembly != null)
            {
                type = ReflectionAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
            }
            if (type == null)
            {
                type = CoreSystemTypes.GetDummyTypeNode(ReflectionAssembly, nspace, name, typeCode);
            }
            type.typeCode = typeCode;
            return type;
        }

        private static AssemblyNode GetResourceManagerAssembly(bool doNotLockFile, bool getDebugInfo)
        {
            if (SystemAssembly.Name == "mscorlib")
            {
                return SystemAssembly;
            }
            Identifier identifier = Identifier.For("System.Resources.ResourceManager");
            AssemblyReference assemblyReference = (AssemblyReference) TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey];
            if (assemblyReference == null)
            {
                assemblyReference = new AssemblyReference {
                    Name = "System.Resources.ResourceManager",
                    PublicKeyOrToken = new byte[] { 
                        0xb0,
                        0x3f,
                        0x5f,
                        0x7f,
                        0x11,
                        0xd5,
                        10,
                        0x3a
                    },
                    Version = TargetPlatform.TargetVersion
                };
                TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey] = assemblyReference;
            }
            if (string.IsNullOrEmpty(SystemResourceManagerAssemblyLocation.Location))
            {
                if (assemblyReference.Location == null)
                {
                    SystemResourceManagerAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Resources.ResourceManager.dll");
                }
                else
                {
                    SystemResourceManagerAssemblyLocation.Location = assemblyReference.Location;
                }
            }
            if (assemblyReference.assembly == null)
            {
                assemblyReference.Location = SystemResourceManagerAssemblyLocation.Location;
            }
            return (assemblyReference.assembly = AssemblyNode.GetAssembly(assemblyReference));
        }

        private static TypeNode GetResourceManagerTypeNodeFor(string nspace, string name, ElementType typeCode)
        {
            TypeNode type = null;
            if (ReflectionAssembly != null)
            {
                type = ReflectionAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
            }
            if (type == null)
            {
                type = CoreSystemTypes.GetDummyTypeNode(ReflectionAssembly, nspace, name, typeCode);
            }
            type.typeCode = typeCode;
            return type;
        }

        private static AssemblyNode GetRuntimeExtensionsAssembly(bool doNotLockFile, bool getDebugInfo)
        {
            if (SystemAssembly.Name == "mscorlib")
            {
                return SystemAssembly;
            }
            Identifier identifier = Identifier.For("System.Runtime.Extensions");
            AssemblyReference assemblyReference = (AssemblyReference) TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey];
            if (assemblyReference == null)
            {
                assemblyReference = new AssemblyReference {
                    Name = "System.Runtime.Extensions",
                    PublicKeyOrToken = new byte[] { 
                        0xb0,
                        0x3f,
                        0x5f,
                        0x7f,
                        0x11,
                        0xd5,
                        10,
                        0x3a
                    },
                    Version = TargetPlatform.TargetVersion
                };
                TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey] = assemblyReference;
            }
            if (string.IsNullOrEmpty(SystemRuntimeExtensionsAssemblyLocation.Location))
            {
                if (assemblyReference.Location == null)
                {
                    SystemRuntimeExtensionsAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Runtime.Extensions.dll");
                }
                else
                {
                    SystemRuntimeExtensionsAssemblyLocation.Location = assemblyReference.Location;
                }
            }
            if (assemblyReference.assembly == null)
            {
                assemblyReference.Location = SystemRuntimeExtensionsAssemblyLocation.Location;
            }
            return (assemblyReference.assembly = AssemblyNode.GetAssembly(assemblyReference));
        }

        private static TypeNode GetRuntimeExtensionsTypeNodeFor(string nspace, string name, ElementType typeCode)
        {
            TypeNode type = null;
            if (SystemRuntimeExtensionsAssembly != null)
            {
                type = SystemRuntimeExtensionsAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
            }
            if (type == null)
            {
                type = CoreSystemTypes.GetDummyTypeNode(SystemRuntimeExtensionsAssembly, nspace, name, typeCode);
            }
            type.typeCode = typeCode;
            return type;
        }

        private static AssemblyNode GetRuntimeSerializationAssembly(bool doNotLockFile, bool getDebugInfo)
        {
            if (SystemAssembly.Name == "mscorlib")
            {
                return SystemAssembly;
            }
            Identifier identifier = Identifier.For("System.Serialization.DataContract");
            AssemblyReference assemblyReference = (AssemblyReference) TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey];
            if (assemblyReference == null)
            {
                assemblyReference = new AssemblyReference {
                    Name = "System.Serialization.DataContract",
                    PublicKeyOrToken = new byte[] { 
                        0xb0,
                        0x3f,
                        0x5f,
                        0x7f,
                        0x11,
                        0xd5,
                        10,
                        0x3a
                    },
                    Version = TargetPlatform.TargetVersion
                };
                TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey] = assemblyReference;
            }
            if (string.IsNullOrEmpty(SystemRuntimeSerializationAssemblyLocation.Location))
            {
                if (assemblyReference.Location == null)
                {
                    SystemRuntimeSerializationAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Serialization.DataContract.dll");
                }
                else
                {
                    SystemRuntimeSerializationAssemblyLocation.Location = assemblyReference.Location;
                }
            }
            if (assemblyReference.assembly == null)
            {
                assemblyReference.Location = SystemRuntimeSerializationAssemblyLocation.Location;
            }
            return (assemblyReference.assembly = AssemblyNode.GetAssembly(assemblyReference));
        }

        private static TypeNode GetRuntimeSerializationTypeNodeFor(string nspace, string name, ElementType typeCode)
        {
            TypeNode type = null;
            if (SystemRuntimeSerializationAssembly != null)
            {
                type = SystemRuntimeSerializationAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
            }
            if (type == null)
            {
                type = CoreSystemTypes.GetDummyTypeNode(SystemRuntimeSerializationAssembly, nspace, name, typeCode);
            }
            type.typeCode = typeCode;
            return type;
        }

        private static AssemblyNode GetSystemDllAssembly(bool doNotLockFile, bool getDebugInfo)
        {
            System.Reflection.AssemblyName name = typeof(System.Uri).Assembly.GetName();
            Identifier identifier = Identifier.For(name.Name);
            AssemblyReference assemblyReference = (AssemblyReference) TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey];
            if (assemblyReference == null)
            {
                assemblyReference = new AssemblyReference {
                    Name = name.Name,
                    PublicKeyOrToken = name.GetPublicKeyToken(),
                    Version = TargetPlatform.TargetVersion
                };
                TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey] = assemblyReference;
            }
            if (string.IsNullOrEmpty(SystemDllAssemblyLocation.Location))
            {
                if (assemblyReference.Location == null)
                {
                    SystemDllAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.dll");
                }
                else
                {
                    SystemDllAssemblyLocation.Location = assemblyReference.Location;
                }
            }
            if (assemblyReference.assembly == null)
            {
                assemblyReference.Location = SystemDllAssemblyLocation.Location;
            }
            return (assemblyReference.assembly = AssemblyNode.GetAssembly(assemblyReference));
        }

        private static AssemblyNode GetSystemRuntimeWindowsRuntimeAssembly(bool doNotLockFile, bool getDebugInfo)
        {
            Identifier identifier = Identifier.For("System.Runtime.WindowsRuntime");
            AssemblyReference assemblyReference = (AssemblyReference) TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey];
            if (assemblyReference == null)
            {
                assemblyReference = new AssemblyReference {
                    Name = "System.Runtime.WindowsRuntime",
                    PublicKeyOrToken = new byte[] { 
                        0xb7,
                        0x7a,
                        0x5c,
                        0x56,
                        0x19,
                        0x34,
                        0xe0,
                        0x89
                    },
                    Version = TargetPlatform.TargetVersion
                };
                TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey] = assemblyReference;
            }
            if (string.IsNullOrEmpty(SystemRuntimeWindowsRuntimeAssemblyLocation.Location))
            {
                if (assemblyReference.Location == null)
                {
                    SystemRuntimeWindowsRuntimeAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Runtime.WindowsRuntime.dll");
                }
                else
                {
                    SystemRuntimeWindowsRuntimeAssemblyLocation.Location = assemblyReference.Location;
                }
            }
            if (assemblyReference.assembly == null)
            {
                assemblyReference.Location = SystemRuntimeWindowsRuntimeAssemblyLocation.Location;
            }
            return (assemblyReference.assembly = AssemblyNode.GetAssembly(assemblyReference));
        }

        private static AssemblyNode GetSystemRuntimeWindowsRuntimeUIXamlAssembly(bool doNotLockFile, bool getDebugInfo)
        {
            Identifier identifier = Identifier.For("System.Runtime.WindowsRuntime.UI.Xaml");
            AssemblyReference assemblyReference = (AssemblyReference) TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey];
            if (assemblyReference == null)
            {
                assemblyReference = new AssemblyReference {
                    Name = "System.Runtime.WindowsRuntime.UI.Xaml",
                    PublicKeyOrToken = new byte[] { 
                        0xb7,
                        0x7a,
                        0x5c,
                        0x56,
                        0x19,
                        0x34,
                        0xe0,
                        0x89
                    },
                    Version = TargetPlatform.TargetVersion
                };
                TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey] = assemblyReference;
            }
            if (string.IsNullOrEmpty(SystemRuntimeWindowsRuntimeUIXamlAssemblyLocation.Location))
            {
                if (assemblyReference.Location == null)
                {
                    SystemRuntimeWindowsRuntimeUIXamlAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Runtime.WindowsRuntime.UI.Xaml.dll");
                }
                else
                {
                    SystemRuntimeWindowsRuntimeUIXamlAssemblyLocation.Location = assemblyReference.Location;
                }
            }
            if (assemblyReference.assembly == null)
            {
                assemblyReference.Location = SystemRuntimeWindowsRuntimeUIXamlAssemblyLocation.Location;
            }
            return (assemblyReference.assembly = AssemblyNode.GetAssembly(assemblyReference));
        }

        private static TypeNode GetSystemTypeNodeFor(string nspace, string name, ElementType typeCode)
        {
            TypeNode type = null;
            if (SystemDllAssembly != null)
            {
                type = SystemDllAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
            }
            if (type == null)
            {
                type = CoreSystemTypes.GetDummyTypeNode(SystemDllAssembly, nspace, name, typeCode);
            }
            type.typeCode = typeCode;
            return type;
        }

        private static AssemblyNode GetThreadingAssembly(bool doNotLockFile, bool getDebugInfo)
        {
            if (SystemAssembly.Name == "mscorlib")
            {
                return SystemAssembly;
            }
            Identifier identifier = Identifier.For("System.Threading");
            AssemblyReference assemblyReference = (AssemblyReference) TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey];
            if (assemblyReference == null)
            {
                assemblyReference = new AssemblyReference {
                    Name = "System.Threading",
                    PublicKeyOrToken = new byte[] { 
                        0xb0,
                        0x3f,
                        0x5f,
                        0x7f,
                        0x11,
                        0xd5,
                        10,
                        0x3a
                    },
                    Version = TargetPlatform.TargetVersion
                };
                TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey] = assemblyReference;
            }
            if (string.IsNullOrEmpty(SystemThreadingAssemblyLocation.Location))
            {
                if (assemblyReference.Location == null)
                {
                    SystemThreadingAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Threading.dll");
                }
                else
                {
                    SystemThreadingAssemblyLocation.Location = assemblyReference.Location;
                }
            }
            if (assemblyReference.assembly == null)
            {
                assemblyReference.Location = SystemThreadingAssemblyLocation.Location;
            }
            return (assemblyReference.assembly = AssemblyNode.GetAssembly(assemblyReference));
        }

        private static TypeNode GetThreadingTypeNodeFor(string nspace, string name, ElementType typeCode)
        {
            TypeNode type = null;
            if (ThreadingAssembly != null)
            {
                type = ThreadingAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
            }
            if (type == null)
            {
                type = CoreSystemTypes.GetDummyTypeNode(ThreadingAssembly, nspace, name, typeCode);
            }
            type.typeCode = typeCode;
            return type;
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
                type = CoreSystemTypes.GetDummyTypeNode(SystemAssembly, nspace, name, typeCode);
            }
            type.typeCode = typeCode;
            return type;
        }

        private static AssemblyNode GetWindowsRuntimeInteropAssembly(bool doNotLockFile, bool getDebugInfo)
        {
            if (SystemAssembly.Name == "mscorlib")
            {
                return SystemAssembly;
            }
            Identifier identifier = Identifier.For("System.Runtime.InteropServices.WindowsRuntime");
            AssemblyReference assemblyReference = (AssemblyReference) TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey];
            if (assemblyReference == null)
            {
                assemblyReference = new AssemblyReference {
                    Name = "System.Runtime.InteropServices.WindowsRuntime",
                    PublicKeyOrToken = new byte[] { 
                        0xb0,
                        0x3f,
                        0x5f,
                        0x7f,
                        0x11,
                        0xd5,
                        10,
                        0x3a
                    },
                    Version = TargetPlatform.TargetVersion
                };
                TargetPlatform.AssemblyReferenceFor[identifier.UniqueIdKey] = assemblyReference;
            }
            if (string.IsNullOrEmpty(SystemRuntimeWindowsRuntimeInteropServicesAssemblyLocation.Location))
            {
                if (assemblyReference.Location == null)
                {
                    SystemRuntimeWindowsRuntimeInteropServicesAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Runtime.InteropServices.WindowsRuntime.dll");
                }
                else
                {
                    SystemRuntimeWindowsRuntimeInteropServicesAssemblyLocation.Location = assemblyReference.Location;
                }
            }
            if (assemblyReference.assembly == null)
            {
                assemblyReference.Location = SystemRuntimeWindowsRuntimeInteropServicesAssemblyLocation.Location;
            }
            return (assemblyReference.assembly = AssemblyNode.GetAssembly(assemblyReference));
        }

        private static TypeNode GetWindowsRuntimeInteropTypeNodeFor(string nspace, string name, ElementType typeCode)
        {
            TypeNode type = null;
            if (SystemRuntimeWindowsRuntimeInteropAssembly != null)
            {
                type = SystemRuntimeWindowsRuntimeInteropAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
            }
            if (type == null)
            {
                type = CoreSystemTypes.GetDummyTypeNode(SystemRuntimeWindowsRuntimeInteropAssembly, nspace, name, typeCode);
            }
            type.typeCode = typeCode;
            return type;
        }

        private static TypeNode GetWindowsRuntimeTypeNodeFor(string nspace, string name, ElementType typeCode)
        {
            TypeNode type = null;
            if (SystemRuntimeWindowsRuntimeAssembly != null)
            {
                type = SystemRuntimeWindowsRuntimeAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
            }
            if (type == null)
            {
                type = CoreSystemTypes.GetDummyTypeNode(SystemRuntimeWindowsRuntimeAssembly, nspace, name, typeCode);
            }
            type.typeCode = typeCode;
            return type;
        }

        private static TypeNode GetWindowsRuntimeUIXamlTypeNodeFor(string nspace, string name, ElementType typeCode)
        {
            TypeNode type = null;
            if (SystemRuntimeWindowsRuntimeUIXamlAssembly != null)
            {
                type = SystemRuntimeWindowsRuntimeUIXamlAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
            }
            if (type == null)
            {
                type = CoreSystemTypes.GetDummyTypeNode(SystemRuntimeWindowsRuntimeUIXamlAssembly, nspace, name, typeCode);
            }
            type.typeCode = typeCode;
            return type;
        }

        public static void Initialize(bool doNotLockFile, bool getDebugInfo)
        {
            if (!TargetPlatform.BusyWithClear)
            {
                if (Initialized)
                {
                    Clear();
                    CoreSystemTypes.Initialize(doNotLockFile, getDebugInfo);
                }
                else if (!CoreSystemTypes.Initialized)
                {
                    CoreSystemTypes.Initialize(doNotLockFile, getDebugInfo);
                }
                if (TargetPlatform.TargetVersion == null)
                {
                    TargetPlatform.TargetVersion = SystemAssembly.Version;
                    if (TargetPlatform.TargetVersion == null)
                    {
                        TargetPlatform.TargetVersion = typeof(object).Assembly.GetName().Version;
                    }
                }
                CollectionsAssembly = GetCollectionsAssembly(doNotLockFile, getDebugInfo);
                DiagnosticsDebugAssembly = GetDiagnosticsDebugAssembly(doNotLockFile, getDebugInfo);
                DiagnosticsToolsAssembly = GetDiagnosticsToolsAssembly(doNotLockFile, getDebugInfo);
                GlobalizationAssembly = GetGlobalizationAssembly(doNotLockFile, getDebugInfo);
                InteropAssembly = GetInteropAssembly(doNotLockFile, getDebugInfo);
                IOAssembly = GetIOAssembly(doNotLockFile, getDebugInfo);
                ReflectionAssembly = GetReflectionAssembly(doNotLockFile, getDebugInfo);
                ResourceManagerAssembly = GetResourceManagerAssembly(doNotLockFile, getDebugInfo);
                SystemDllAssembly = GetSystemDllAssembly(doNotLockFile, getDebugInfo);
                SystemRuntimeExtensionsAssembly = GetRuntimeExtensionsAssembly(doNotLockFile, getDebugInfo);
                SystemRuntimeSerializationAssembly = GetRuntimeSerializationAssembly(doNotLockFile, getDebugInfo);
                SystemRuntimeWindowsRuntimeInteropAssembly = GetWindowsRuntimeInteropAssembly(doNotLockFile, getDebugInfo);
                SystemRuntimeWindowsRuntimeAssembly = GetSystemRuntimeWindowsRuntimeAssembly(doNotLockFile, getDebugInfo);
                SystemRuntimeWindowsRuntimeUIXamlAssembly = GetSystemRuntimeWindowsRuntimeUIXamlAssembly(doNotLockFile, getDebugInfo);
                ThreadingAssembly = GetThreadingAssembly(doNotLockFile, getDebugInfo);
                AttributeUsageAttribute = (Class) GetTypeNodeFor("System", "AttributeUsageAttribute", ElementType.Class);
                ConditionalAttribute = (Class) GetTypeNodeFor("System.Diagnostics", "ConditionalAttribute", ElementType.Class);
                DefaultMemberAttribute = (Class) GetTypeNodeFor("System.Reflection", "DefaultMemberAttribute", ElementType.Class);
                InternalsVisibleToAttribute = (Class) GetTypeNodeFor("System.Runtime.CompilerServices", "InternalsVisibleToAttribute", ElementType.Class);
                ObsoleteAttribute = (Class) GetTypeNodeFor("System", "ObsoleteAttribute", ElementType.Class);
                GenericICollection = (Interface) GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "ICollection", 1, ElementType.Class);
                GenericIEnumerable = (Interface) GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "IEnumerable", 1, ElementType.Class);
                GenericIList = (Interface) GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "IList", 1, ElementType.Class);
                ICloneable = (Interface) GetTypeNodeFor("System", "ICloneable", ElementType.Class);
                ICollection = (Interface) GetTypeNodeFor("System.Collections", "ICollection", ElementType.Class);
                IEnumerable = (Interface) GetTypeNodeFor("System.Collections", "IEnumerable", ElementType.Class);
                IList = (Interface) GetTypeNodeFor("System.Collections", "IList", ElementType.Class);
                AllowPartiallyTrustedCallersAttribute = (Class) GetTypeNodeFor("System.Security", "AllowPartiallyTrustedCallersAttribute", ElementType.Class);
                AssemblyCompanyAttribute = (Class) GetTypeNodeFor("System.Reflection", "AssemblyCompanyAttribute", ElementType.Class);
                AssemblyConfigurationAttribute = (Class) GetTypeNodeFor("System.Reflection", "AssemblyConfigurationAttribute", ElementType.Class);
                AssemblyCopyrightAttribute = (Class) GetTypeNodeFor("System.Reflection", "AssemblyCopyrightAttribute", ElementType.Class);
                AssemblyCultureAttribute = (Class) GetTypeNodeFor("System.Reflection", "AssemblyCultureAttribute", ElementType.Class);
                AssemblyDelaySignAttribute = (Class) GetTypeNodeFor("System.Reflection", "AssemblyDelaySignAttribute", ElementType.Class);
                AssemblyDescriptionAttribute = (Class) GetTypeNodeFor("System.Reflection", "AssemblyDescriptionAttribute", ElementType.Class);
                AssemblyFileVersionAttribute = (Class) GetTypeNodeFor("System.Reflection", "AssemblyFileVersionAttribute", ElementType.Class);
                AssemblyFlagsAttribute = (Class) GetTypeNodeFor("System.Reflection", "AssemblyFlagsAttribute", ElementType.Class);
                AssemblyInformationalVersionAttribute = (Class) GetTypeNodeFor("System.Reflection", "AssemblyInformationalVersionAttribute", ElementType.Class);
                AssemblyKeyFileAttribute = (Class) GetTypeNodeFor("System.Reflection", "AssemblyKeyFileAttribute", ElementType.Class);
                AssemblyKeyNameAttribute = (Class) GetTypeNodeFor("System.Reflection", "AssemblyKeyNameAttribute", ElementType.Class);
                AssemblyProductAttribute = (Class) GetTypeNodeFor("System.Reflection", "AssemblyProductAttribute", ElementType.Class);
                AssemblyTitleAttribute = (Class) GetTypeNodeFor("System.Reflection", "AssemblyTitleAttribute", ElementType.Class);
                AssemblyTrademarkAttribute = (Class) GetTypeNodeFor("System.Reflection", "AssemblyTrademarkAttribute", ElementType.Class);
                AssemblyVersionAttribute = (Class) GetTypeNodeFor("System.Reflection", "AssemblyVersionAttribute", ElementType.Class);
                AttributeTargets = GetTypeNodeFor("System", "AttributeTargets", ElementType.ValueType) as EnumNode;
                Color = (Struct) GetWindowsRuntimeTypeNodeFor("Windows.UI", "Color", ElementType.ValueType);
                CornerRadius = (Struct) GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml", "CornerRadius", ElementType.ValueType);
                ClassInterfaceAttribute = (Class) GetInteropTypeNodeFor("System.Runtime.InteropServices", "ClassInterfaceAttribute", ElementType.Class);
                CLSCompliantAttribute = (Class) GetTypeNodeFor("System", "CLSCompliantAttribute", ElementType.Class);
                ComImportAttribute = (Class) GetInteropTypeNodeFor("System.Runtime.InteropServices", "ComImportAttribute", ElementType.Class);
                ComRegisterFunctionAttribute = (Class) GetInteropTypeNodeFor("System.Runtime.InteropServices", "ComRegisterFunctionAttribute", ElementType.Class);
                ComSourceInterfacesAttribute = (Class) GetInteropTypeNodeFor("System.Runtime.InteropServices", "ComSourceInterfacesAttribute", ElementType.Class);
                ComUnregisterFunctionAttribute = (Class) GetInteropTypeNodeFor("System.Runtime.InteropServices", "ComUnregisterFunctionAttribute", ElementType.Class);
                ComVisibleAttribute = (Class) GetTypeNodeFor("System.Runtime.InteropServices", "ComVisibleAttribute", ElementType.Class);
                DebuggableAttribute = (Class) GetTypeNodeFor("System.Diagnostics", "DebuggableAttribute", ElementType.Class);
                DebuggerHiddenAttribute = (Class) GetDiagnosticsDebugTypeNodeFor("System.Diagnostics", "DebuggerHiddenAttribute", ElementType.Class);
                DebuggerStepThroughAttribute = (Class) GetDiagnosticsDebugTypeNodeFor("System.Diagnostics", "DebuggerStepThroughAttribute", ElementType.Class);
                DebuggingModes = (DebuggableAttribute == null) ? null : (DebuggableAttribute.GetNestedType(Identifier.For("DebuggingModes")) as EnumNode);
                DllImportAttribute = (Class) GetInteropTypeNodeFor("System.Runtime.InteropServices", "DllImportAttribute", ElementType.Class);
                Duration = (Struct) GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml", "Duration", ElementType.ValueType);
                DurationType = GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml", "DurationType", ElementType.ValueType) as EnumNode;
                FieldOffsetAttribute = (Class) GetTypeNodeFor("System.Runtime.InteropServices", "FieldOffsetAttribute", ElementType.Class);
                FlagsAttribute = (Class) GetTypeNodeFor("System", "FlagsAttribute", ElementType.Class);
                GeneratorPosition = (Struct) GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml.Controls.Primitives", "GeneratorPosition", ElementType.ValueType);
                Guid = (Struct) GetTypeNodeFor("System", "Guid", ElementType.ValueType);
                GuidAttribute = (Class) GetInteropTypeNodeFor("System.Runtime.InteropServices", "GuidAttribute", ElementType.Class);
                GridLength = (Struct) GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml", "GridLength", ElementType.ValueType);
                GridUnitType = GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml", "GridUnitType", ElementType.ValueType) as EnumNode;
                ImportedFromTypeLibAttribute = (Class) GetInteropTypeNodeFor("System.Runtime.InteropServices", "ImportedFromTypeLibAttribute", ElementType.Class);
                InAttribute = (Class) GetInteropTypeNodeFor("System.Runtime.InteropServices", "InAttribute", ElementType.Class);
                IndexerNameAttribute = (Class) GetTypeNodeFor("System.Runtime.CompilerServices", "IndexerNameAttribute", ElementType.Class);
                InterfaceTypeAttribute = (Class) GetInteropTypeNodeFor("System.Runtime.InteropServices", "InterfaceTypeAttribute", ElementType.Class);
                KeyTime = (Struct) GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml.Media.Animation", "KeyTime", ElementType.ValueType);
                RepeatBehavior = (Struct) GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml.Media.Animation", "RepeatBehavior", ElementType.ValueType);
                RepeatBehaviorType = GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml.Media.Animation", "RepeatBehaviorType", ElementType.ValueType) as EnumNode;
                MethodImplAttribute = (Class) GetTypeNodeFor("System.Runtime.CompilerServices", "MethodImplAttribute", ElementType.Class);
                Matrix = (Struct) GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml.Media", "Matrix", ElementType.ValueType);
                Matrix3D = (Struct) GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml.Media.Media3D", "Matrix3D", ElementType.ValueType);
                NonSerializedAttribute = (Class) GetTypeNodeFor("System", "NonSerializedAttribute", ElementType.Class);
                OptionalAttribute = (Class) GetInteropTypeNodeFor("System.Runtime.InteropServices", "OptionalAttribute", ElementType.Class);
                OutAttribute = (Class) GetTypeNodeFor("System.Runtime.InteropServices", "OutAttribute", ElementType.Class);
                ParamArrayAttribute = (Class) GetTypeNodeFor("System", "ParamArrayAttribute", ElementType.Class);
                Point = (Struct) GetWindowsRuntimeTypeNodeFor("Windows.Foundation", "Point", ElementType.ValueType);
                Rect = (Struct) GetWindowsRuntimeTypeNodeFor("Windows.Foundation", "Rect", ElementType.ValueType);
                RuntimeCompatibilityAttribute = (Class) GetTypeNodeFor("System.Runtime.CompilerServices", "RuntimeCompatibilityAttribute", ElementType.Class);
                SatelliteContractVersionAttribute = (Class) GetResourceManagerTypeNodeFor("System.Resources", "SatelliteContractVersionAttribute", ElementType.Class);
                SerializableAttribute = (Class) GetTypeNodeFor("System", "SerializableAttribute", ElementType.Class);
                SecurityAttribute = (Class) GetTypeNodeFor("System.Security.Permissions", "SecurityAttribute", ElementType.Class);
                SecurityCriticalAttribute = (Class) GetTypeNodeFor("System.Security", "SecurityCriticalAttribute", ElementType.Class);
                SecurityTransparentAttribute = (Class) GetTypeNodeFor("System.Security", "SecurityTransparentAttribute", ElementType.Class);
                SecurityTreatAsSafeAttribute = (Class) GetTypeNodeFor("System.Security", "SecurityTreatAsSafeAttribute", ElementType.Class);
                Size = (Struct) GetWindowsRuntimeTypeNodeFor("Windows.Foundation", "Size", ElementType.ValueType);
                STAThreadAttribute = (Class) GetTypeNodeFor("System", "STAThreadAttribute", ElementType.Class);
                StructLayoutAttribute = (Class) GetTypeNodeFor("System.Runtime.InteropServices", "StructLayoutAttribute", ElementType.Class);
                SuppressMessageAttribute = (Class) GetDiagnosticsToolsTypeNodeFor("System.Diagnostics.CodeAnalysis", "SuppressMessageAttribute", ElementType.Class);
                SuppressUnmanagedCodeSecurityAttribute = (Class) GetTypeNodeFor("System.Security", "SuppressUnmanagedCodeSecurityAttribute", ElementType.Class);
                SecurityAction = GetTypeNodeFor("System.Security.Permissions", "SecurityAction", ElementType.ValueType) as EnumNode;
                DBNull = (Class) GetTypeNodeFor("System", "DBNull", ElementType.Class);
                DateTime = (Struct) GetTypeNodeFor("System", "DateTime", ElementType.ValueType);
                DateTimeOffset = (Struct) GetTypeNodeFor("System", "DateTimeOffset", ElementType.ValueType);
                TimeSpan = (Struct) GetTypeNodeFor("System", "TimeSpan", ElementType.ValueType);
                Activator = (Class) GetTypeNodeFor("System", "Activator", ElementType.Class);
                AppDomain = (Class) GetTypeNodeFor("System", "AppDomain", ElementType.Class);
                ApplicationException = (Class) GetTypeNodeFor("System", "ApplicationException", ElementType.Class);
                ArgumentException = (Class) GetTypeNodeFor("System", "ArgumentException", ElementType.Class);
                ArgumentNullException = (Class) GetTypeNodeFor("System", "ArgumentNullException", ElementType.Class);
                ArgumentOutOfRangeException = (Class) GetTypeNodeFor("System", "ArgumentOutOfRangeException", ElementType.Class);
                ArrayList = (Class) GetTypeNodeFor("System.Collections", "ArrayList", ElementType.Class);
                AsyncCallback = (DelegateNode) GetTypeNodeFor("System", "AsyncCallback", ElementType.Class);
                Assembly = (Class) GetReflectionTypeNodeFor("System.Reflection", "Assembly", ElementType.Class);
                CodeAccessPermission = (Class) GetTypeNodeFor("System.Security", "CodeAccessPermission", ElementType.Class);
                CollectionBase = (Class) GetTypeNodeFor("System.Collections", "CollectionBase", ElementType.Class);
                CultureInfo = (Class) GetGlobalizationTypeNodeFor("System.Globalization", "CultureInfo", ElementType.Class);
                DictionaryBase = (Class) GetTypeNodeFor("System.Collections", "DictionaryBase", ElementType.Class);
                DictionaryEntry = (Struct) GetTypeNodeFor("System.Collections", "DictionaryEntry", ElementType.ValueType);
                DuplicateWaitObjectException = (Class) GetTypeNodeFor("System", "DuplicateWaitObjectException", ElementType.Class);
                Environment = (Class) GetRuntimeExtensionsTypeNodeFor("System", "Environment", ElementType.Class);
                EventArgs = (Class) GetTypeNodeFor("System", "EventArgs", ElementType.Class);
                EventHandler1 = GetGenericRuntimeTypeNodeFor("System", "EventHandler", 1, ElementType.Class) as DelegateNode;
                EventRegistrationToken = (Struct) GetWindowsRuntimeInteropTypeNodeFor("System.Runtime.InteropServices.WindowsRuntime", "EventRegistrationToken", ElementType.ValueType);
                ExecutionEngineException = (Class) GetTypeNodeFor("System", "ExecutionEngineException", ElementType.Class);
                GenericArraySegment = (Struct) GetGenericRuntimeTypeNodeFor("System", "ArraySegment", 1, ElementType.ValueType);
                GenericDictionary = (Class) GetCollectionsGenericRuntimeTypeNodeFor("System.Collections.Generic", "Dictionary", 2, ElementType.Class);
                GenericIComparable = (Interface) GetCollectionsGenericRuntimeTypeNodeFor("System.Collections.Generic", "IComparable", 1, ElementType.Class);
                GenericIComparer = (Interface) GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "IComparer", 1, ElementType.Class);
                GenericIDictionary = (Interface) GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "IDictionary", 2, ElementType.Class);
                GenericIEnumerator = (Interface) GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "IEnumerator", 1, ElementType.Class);
                GenericIReadOnlyDictionary = (Interface) GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "IReadOnlyDictionary", 2, ElementType.Class);
                GenericIReadOnlyList = (Interface) GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "IReadOnlyList", 1, ElementType.Class);
                GenericKeyValuePair = (Struct) GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "KeyValuePair", 2, ElementType.ValueType);
                GenericList = (Class) GetCollectionsGenericRuntimeTypeNodeFor("System.Collections.Generic", "List", 1, ElementType.Class);
                GenericNullable = (Struct) GetGenericRuntimeTypeNodeFor("System", "Nullable", 1, ElementType.ValueType);
                GenericQueue = (Class) GetCollectionsGenericRuntimeTypeNodeFor("System.Collections.Generic", "Queue", 1, ElementType.Class);
                GenericSortedDictionary = (Class) GetCollectionsGenericRuntimeTypeNodeFor("System.Collections.Generic", "SortedDictionary", 2, ElementType.Class);
                GenericStack = (Class) GetCollectionsGenericRuntimeTypeNodeFor("System.Collections.Generic", "Stack", 1, ElementType.Class);
                GC = (Class) GetRuntimeExtensionsTypeNodeFor("System", "GC", ElementType.Class);
                __HandleProtector = (Class) GetTypeNodeFor("System.Threading", "__HandleProtector", ElementType.Class);
                HandleRef = (Struct) GetInteropTypeNodeFor("System.Runtime.InteropServices", "HandleRef", ElementType.ValueType);
                Hashtable = (Class) GetTypeNodeFor("System.Collections", "Hashtable", ElementType.Class);
                IASyncResult = (Interface) GetTypeNodeFor("System", "IAsyncResult", ElementType.Class);
                ICommand = (Interface) GetTypeNodeFor("System.Windows.Input", "ICommand", ElementType.Class);
                IComparable = (Interface) GetTypeNodeFor("System", "IComparable", ElementType.Class);
                IComparer = (Interface) GetTypeNodeFor("System.Collections", "IComparer", ElementType.Class);
                IDictionary = (Interface) GetTypeNodeFor("System.Collections", "IDictionary", ElementType.Class);
                IDisposable = (Interface) GetTypeNodeFor("System", "IDisposable", ElementType.Class);
                IEnumerator = (Interface) GetTypeNodeFor("System.Collections", "IEnumerator", ElementType.Class);
                IFormatProvider = (Interface) GetTypeNodeFor("System", "IFormatProvider", ElementType.Class);
                IHashCodeProvider = (Interface) GetTypeNodeFor("System.Collections", "IHashCodeProvider", ElementType.Class);
                IMembershipCondition = (Interface) GetTypeNodeFor("System.Security.Policy", "IMembershipCondition", ElementType.Class);
                IndexOutOfRangeException = (Class) GetTypeNodeFor("System", "IndexOutOfRangeException", ElementType.Class);
                IBindableIterable = (Interface) GetTypeNodeFor("System.Collections", "IBindableIterable", ElementType.Class);
                IBindableVector = (Interface) GetTypeNodeFor("System.Collections", "IBindableVector", ElementType.Class);
                INotifyCollectionChanged = (Interface) GetSystemTypeNodeFor("System.Collections.Specialized", "INotifyCollectionChanged", ElementType.Class);
                INotifyPropertyChanged = (Interface) GetSystemTypeNodeFor("System.ComponentModel", "INotifyPropertyChanged", ElementType.Class);
                InvalidCastException = (Class) GetTypeNodeFor("System", "InvalidCastException", ElementType.Class);
                InvalidOperationException = (Class) GetTypeNodeFor("System", "InvalidOperationException", ElementType.Class);
                IPermission = (Interface) GetTypeNodeFor("System.Security", "IPermission", ElementType.Class);
                ISerializable = (Interface) GetTypeNodeFor("System.Runtime.Serialization", "ISerializable", ElementType.Class);
                IStackWalk = (Interface) GetTypeNodeFor("System.Security", "IStackWalk", ElementType.Class);
                Marshal = (Class) GetInteropTypeNodeFor("System.Runtime.InteropServices", "Marshal", ElementType.Class);
                MarshalByRefObject = (Class) GetTypeNodeFor("System", "MarshalByRefObject", ElementType.Class);
                MemberInfo = (Class) GetReflectionTypeNodeFor("System.Reflection", "MemberInfo", ElementType.Class);
                Monitor = (Class) GetThreadingTypeNodeFor("System.Threading", "Monitor", ElementType.Class);
                NativeOverlapped = (Struct) GetThreadingTypeNodeFor("System.Threading", "NativeOverlapped", ElementType.ValueType);
                NotifyCollectionChangedAction = GetSystemTypeNodeFor("System.Collections.Specialized", "NotifyCollectionChangedAction", ElementType.ValueType) as EnumNode;
                NotifyCollectionChangedEventArgs = (Class) GetSystemTypeNodeFor("System.Collections.Specialized", "NotifyCollectionChangedEventArgs", ElementType.Class);
                NotifyCollectionChangedEventHandler = (DelegateNode) GetSystemTypeNodeFor("System.Collections.Specialized", "NotifyCollectionChangedEventHandler", ElementType.Class);
                NotSupportedException = (Class) GetTypeNodeFor("System", "NotSupportedException", ElementType.Class);
                NullReferenceException = (Class) GetTypeNodeFor("System", "NullReferenceException", ElementType.Class);
                OutOfMemoryException = (Class) GetTypeNodeFor("System", "OutOfMemoryException", ElementType.Class);
                ParameterInfo = (Class) GetReflectionTypeNodeFor("System.Reflection", "ParameterInfo", ElementType.Class);
                PropertyChangedEventArgs = (Class) GetSystemTypeNodeFor("System.ComponentModel", "PropertyChangedEventArgs", ElementType.Class);
                PropertyChangedEventHandler = (DelegateNode) GetSystemTypeNodeFor("System.ComponentModel", "PropertyChangedEventHandler", ElementType.Class);
                Queue = (Class) GetCollectionsTypeNodeFor("System.Collections", "Queue", ElementType.Class);
                ReadOnlyCollectionBase = (Class) GetTypeNodeFor("System.Collections", "ReadOnlyCollectionBase", ElementType.Class);
                ResourceManager = (Class) GetResourceManagerTypeNodeFor("System.Resources", "ResourceManager", ElementType.Class);
                ResourceSet = (Class) GetTypeNodeFor("System.Resources", "ResourceSet", ElementType.Class);
                SerializationInfo = (Class) GetTypeNodeFor("System.Runtime.Serialization", "SerializationInfo", ElementType.Class);
                Stack = (Class) GetTypeNodeFor("System.Collections", "Stack", ElementType.Class);
                StackOverflowException = (Class) GetTypeNodeFor("System", "StackOverflowException", ElementType.Class);
                Stream = (Class) GetIOTypeNodeFor("System.IO", "Stream", ElementType.Class);
                StreamingContext = (Struct) GetRuntimeSerializationTypeNodeFor("System.Runtime.Serialization", "StreamingContext", ElementType.ValueType);
                StringBuilder = (Class) GetTypeNodeFor("System.Text", "StringBuilder", ElementType.Class);
                StringComparer = (Class) GetRuntimeExtensionsTypeNodeFor("System", "StringComparer", ElementType.Class);
                StringComparison = GetTypeNodeFor("System", "StringComparison", ElementType.ValueType) as EnumNode;
                SystemException = (Class) GetTypeNodeFor("System", "SystemException", ElementType.Class);
                Thread = (Class) GetThreadingTypeNodeFor("System.Threading", "Thread", ElementType.Class);
                Thickness = (Struct) GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml", "Thickness", ElementType.ValueType);
                Uri = (Class) GetSystemTypeNodeFor("System", "Uri", ElementType.Class);
                WindowsImpersonationContext = (Class) GetTypeNodeFor("System.Security.Principal", "WindowsImpersonationContext", ElementType.Class);
                Initialized = true;
                TrivialHashtable assemblyReferenceFor = TargetPlatform.AssemblyReferenceFor;
            }
        }

        public static Struct ArgIterator =>
            CoreSystemTypes.ArgIterator;

        public static Class Array =>
            CoreSystemTypes.Array;

        public static Class Attribute =>
            CoreSystemTypes.Attribute;

        public static Struct Boolean =>
            CoreSystemTypes.Boolean;

        public static Struct Char =>
            CoreSystemTypes.Char;

        public static Struct Decimal =>
            CoreSystemTypes.Decimal;

        public static Class Delegate =>
            CoreSystemTypes.Delegate;

        public static Struct Double =>
            CoreSystemTypes.Double;

        public static Struct DynamicallyTypedReference =>
            CoreSystemTypes.DynamicallyTypedReference;

        public static Class Enum =>
            CoreSystemTypes.Enum;

        public static Class Exception =>
            CoreSystemTypes.Exception;

        public static Struct Int16 =>
            CoreSystemTypes.Int16;

        public static Struct Int32 =>
            CoreSystemTypes.Int32;

        public static Struct Int64 =>
            CoreSystemTypes.Int64;

        public static Struct Int8 =>
            CoreSystemTypes.Int8;

        public static Struct IntPtr =>
            CoreSystemTypes.IntPtr;

        public static Class IsVolatile =>
            CoreSystemTypes.IsVolatile;

        public static Class MulticastDelegate =>
            CoreSystemTypes.MulticastDelegate;

        public static Class Object =>
            CoreSystemTypes.Object;

        public static Struct RuntimeArgumentHandle =>
            CoreSystemTypes.RuntimeArgumentHandle;

        public static Struct RuntimeFieldHandle =>
            CoreSystemTypes.RuntimeTypeHandle;

        public static Struct RuntimeMethodHandle =>
            CoreSystemTypes.RuntimeTypeHandle;

        public static Struct RuntimeTypeHandle =>
            CoreSystemTypes.RuntimeTypeHandle;

        public static Struct Single =>
            CoreSystemTypes.Single;

        public static Class String =>
            CoreSystemTypes.String;

        public static AssemblyNode SystemAssembly
        {
            get => 
                CoreSystemTypes.SystemAssembly;
            set
            {
                CoreSystemTypes.SystemAssembly = value;
            }
        }

        public static Class Type =>
            CoreSystemTypes.Type;

        public static Struct UInt16 =>
            CoreSystemTypes.UInt16;

        public static Struct UInt32 =>
            CoreSystemTypes.UInt32;

        public static Struct UInt64 =>
            CoreSystemTypes.UInt64;

        public static Struct UInt8 =>
            CoreSystemTypes.UInt8;

        public static Struct UIntPtr =>
            CoreSystemTypes.UIntPtr;

        public static Class ValueType =>
            CoreSystemTypes.ValueType;

        public static Struct Void =>
            CoreSystemTypes.Void;
    }
}

