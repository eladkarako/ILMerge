namespace System.Compiler
{
    using System;
    using System.Collections;
    using System.Compiler.Metadata;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Text;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class Ir2md : IMetaDataEmit, IMetaDataImport
    {
        private AssemblyNode assembly;
        private AssemblyReferenceList assemblyRefEntries = new AssemblyReferenceList();
        private TrivialHashtable assemblyRefIndex = new TrivialHashtable();
        private System.Compiler.BinaryWriter blobHeap = new System.Compiler.BinaryWriter(new System.Compiler.MemoryStream(), Encoding.Unicode);
        private Hashtable blobHeapIndex = new Hashtable(new ByteArrayKeyComparer());
        private Hashtable blobHeapStringIndex = new Hashtable();
        private TypeNodeList classLayoutEntries = new TypeNodeList();
        private NodeList constantTableEntries = new NodeList();
        private int customAttributeCount;
        private TrivialHashtable documentMap = new TrivialHashtable();
        private EventList eventEntries = new EventList();
        private TrivialHashtable eventIndex = new TrivialHashtable();
        private EventList eventMapEntries = new EventList();
        private TrivialHashtable eventMapIndex = new TrivialHashtable();
        private TrivialHashtable exceptionBlock = new TrivialHashtable();
        private FieldList fieldEntries = new FieldList();
        private TrivialHashtable fieldIndex = new TrivialHashtable();
        private FieldList fieldLayoutEntries = new FieldList();
        private FieldList fieldRvaEntries = new FieldList();
        private ModuleList fileTableEntries = new ModuleList();
        private Hashtable fileTableIndex = new Hashtable();
        private TypeNodeList genericParamConstraintEntries = new TypeNodeList();
        private MemberList genericParamEntries = new MemberList();
        private TypeNodeList genericParameters = new TypeNodeList();
        private Hashtable genericParamIndex = new Hashtable();
        private ArrayList guidEntries = new ArrayList();
        private Hashtable guidIndex = new Hashtable();
        private static Guid IID_IClassFactory = new Guid("00000001-0000-0000-C000-000000000046");
        private static Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");
        private MethodList implMapEntries = new MethodList();
        private TypeNodeList interfaceEntries = new TypeNodeList();
        private NodeList marshalEntries = new NodeList();
        private MemberList memberRefEntries = new MemberList();
        private TrivialHashtable<int> memberRefIndex = new TrivialHashtable<int>();
        private System.Compiler.BinaryWriter methodBodiesHeap = new System.Compiler.BinaryWriter(new System.Compiler.MemoryStream());
        private TrivialHashtable methodBodiesHeapIndex = new TrivialHashtable();
        private System.Compiler.BinaryWriter methodBodyHeap;
        private MethodList methodEntries = new MethodList();
        private MethodList methodImplEntries = new MethodList();
        private TrivialHashtable<int> methodIndex = new TrivialHashtable<int>();
        private MethodInfo methodInfo;
        private MemberList methodSemanticsEntries = new MemberList();
        private MethodList methodSpecEntries = new MethodList();
        private Hashtable methodSpecIndex = new Hashtable();
        private Module module;
        private ModuleReferenceList moduleRefEntries = new ModuleReferenceList();
        private Hashtable moduleRefIndex = new Hashtable();
        private TypeNodeList nestedClassEntries = new TypeNodeList();
        private NodeList nodesWithCustomAttributes = new NodeList();
        private NodeList nodesWithSecurityAttributes = new NodeList();
        private ParameterList paramEntries = new ParameterList();
        private TrivialHashtable<int> paramIndex = new TrivialHashtable<int>();
        private PropertyList propertyEntries = new PropertyList();
        private TrivialHashtable propertyIndex = new TrivialHashtable();
        private PropertyList propertyMapEntries = new PropertyList();
        private TrivialHashtable propertyMapIndex = new TrivialHashtable();
        private byte[] PublicKey;
        private System.Compiler.BinaryWriter resourceDataHeap = new System.Compiler.BinaryWriter(new System.Compiler.MemoryStream());
        private System.Compiler.BinaryWriter sdataHeap = new System.Compiler.BinaryWriter(new System.Compiler.MemoryStream());
        private int securityAttributeCount;
        private int SignatureKeyLength;
        private int stackHeight;
        private int stackHeightExitTotal;
        private int stackHeightMax;
        private ArrayList standAloneSignatureEntries = new ArrayList();
        private System.Compiler.BinaryWriter stringHeap = new System.Compiler.BinaryWriter(new System.Compiler.MemoryStream());
        private Hashtable stringHeapIndex = new Hashtable();
        private TrivialHashtable structuralTypeSpecIndexFor = new TrivialHashtable();
        private ISymUnmanagedWriter symWriter;
        private System.Compiler.BinaryWriter tlsHeap = new System.Compiler.BinaryWriter(new System.Compiler.MemoryStream());
        private TypeNodeList typeDefEntries = new TypeNodeList();
        private TrivialHashtable typeDefIndex = new TrivialHashtable();
        private TrivialHashtable typeParameterNumber = new TrivialHashtable();
        private TypeNodeList typeRefEntries = new TypeNodeList();
        private TrivialHashtable typeRefIndex = new TrivialHashtable();
        private TypeNodeList typeSpecEntries = new TypeNodeList();
        private TrivialHashtable typeSpecIndex = new TrivialHashtable();
        private TrivialHashtable unspecializedFieldFor = new TrivialHashtable();
        private TrivialHashtable unspecializedMethodFor = new TrivialHashtable();
        private bool UseGenerics;
        private System.Compiler.BinaryWriter userStringHeap = new System.Compiler.BinaryWriter(new System.Compiler.MemoryStream(), Encoding.Unicode);
        private Hashtable userStringHeapIndex = new Hashtable();
        private MetadataWriter writer;

        internal Ir2md(Module module)
        {
            this.assembly = module as AssemblyNode;
            this.module = module;
            this.blobHeap.Write((byte) 0);
            this.stringHeap.Write((byte) 0);
            this.userStringHeap.Write((byte) 0);
            if (this.assembly != null)
            {
                this.PublicKey = this.assembly.PublicKeyOrToken;
                this.SignatureKeyLength = 0;
                for (int i = 0; i < this.assembly.Attributes.Count; i++)
                {
                    AttributeNode node = this.assembly.Attributes[i];
                    if ((node != null) && (node.Type.ToString() == "System.Reflection.AssemblySignatureKeyAttribute"))
                    {
                        string str = node.GetPositionalArgument(0).ToString();
                        this.SignatureKeyLength = str.Length / 2;
                    }
                }
            }
        }

        private void AppendAssemblyQualifierIfNecessary(StringBuilder sb, TypeNode type, out bool isAssemQualified)
        {
            isAssemQualified = false;
            if (type != null)
            {
                AssemblyNode declaringModule = type.DeclaringModule as AssemblyNode;
                if ((declaringModule != null) && (declaringModule != this.module))
                {
                    sb.Append(", ");
                    sb.Append(declaringModule.StrongName);
                    isAssemQualified = true;
                }
            }
        }

        private void AppendSerializedTypeName(StringBuilder sb, TypeNode type, ref bool isAssemQualified)
        {
            if (type != null)
            {
                string serializedTypeName = this.GetSerializedTypeName(type, ref isAssemQualified);
                if (isAssemQualified)
                {
                    sb.Append('[');
                }
                sb.Append(serializedTypeName);
                if (isAssemQualified)
                {
                    sb.Append(']');
                }
            }
        }

        private static bool AttributesContains(AttributeList al, TypeNode a)
        {
            if (al != null)
            {
                int num = 0;
                int count = al.Count;
                while (num < count)
                {
                    if ((al[num] != null) && (al[num].Type == a))
                    {
                        return true;
                    }
                    num++;
                }
            }
            return false;
        }

        private static object CrossCompileActivate(string server, Guid guid)
        {
            object obj2 = null;
            int hModule = LoadLibrary(server);
            if (hModule != 0)
            {
                IClassFactory factory;
                GetClassObjectDelegate procAddress = GetProcAddress(hModule, "DllGetClassObject");
                if (((procAddress != null) && (procAddress(ref guid, ref IID_IClassFactory, out factory) == 0)) && (factory != null))
                {
                    object ppunk = null;
                    if (factory.CreateInstance(null, ref IID_IUnknown, out ppunk) == 0)
                    {
                        obj2 = ppunk;
                    }
                }
            }
            return obj2;
        }

        private unsafe void DefineLocalVariables(int startAddress, LocalList locals)
        {
            MethodInfo methodInfo = this.methodInfo;
            int num = 0;
            int count = locals.Count;
            while (num < count)
            {
                Local loc = locals[num];
                string name = loc.Name.ToString();
                fixed (byte* numRef = methodInfo.localVarSignature.BaseStream.Buffer)
                {
                    IntPtr signature = (IntPtr) (numRef + methodInfo.signatureOffsets[num]);
                    uint cSig = (uint) methodInfo.signatureLengths[num];
                    uint attributes = loc.Attributes;
                    if (!loc.HasNoPDBInfo)
                    {
                        this.symWriter.DefineLocalVariable(name, attributes, cSig, signature, 1, (uint) this.GetLocalVarIndex(loc), 0, 0, 0);
                    }
                }
                num++;
            }
            int position = this.methodBodyHeap.BaseStream.Position;
            if (position > startAddress)
            {
                this.symWriter.CloseScope((uint) (position - 1));
            }
            else
            {
                this.symWriter.CloseScope((uint) startAddress);
            }
        }

        private void DefineSequencePoint(Node node)
        {
            if (((this.symWriter != null) && (node != null)) && ((node.SourceContext.Document != null) && !node.SourceContext.Document.Hidden))
            {
                if (this.methodInfo.statementNodes.Count > 0)
                {
                    Node node2 = this.methodInfo.statementNodes[this.methodInfo.statementNodes.Count - 1];
                    if (((node2 != null) && (node2.SourceContext.Document == node.SourceContext.Document)) && ((node2.SourceContext.StartPos == node.SourceContext.StartPos) && (node2.SourceContext.EndPos == node.SourceContext.EndPos)))
                    {
                        return;
                    }
                }
                this.methodInfo.statementNodes.Add(node);
                this.methodInfo.statementOffsets.Add(this.methodBodyHeap.BaseStream.Position);
            }
        }

        private void DefineSequencePoints(NodeList statementNodes, Int32List statementOffsets, int start, int count, ISymUnmanagedDocumentWriter doc)
        {
            if (count != 0)
            {
                uint[] offsets = new uint[count];
                uint[] lines = new uint[count];
                uint[] columns = new uint[count];
                uint[] endLines = new uint[count];
                uint[] endColumns = new uint[count];
                for (int i = 0; i < count; i++)
                {
                    Node node = statementNodes[i + start];
                    offsets[i] = ((i + start) == 0) ? 0 : ((uint) statementOffsets[i + start]);
                    lines[i] = (uint) node.SourceContext.StartLine;
                    columns[i] = (uint) node.SourceContext.StartColumn;
                    endLines[i] = (uint) node.SourceContext.EndLine;
                    endColumns[i] = (uint) node.SourceContext.EndColumn;
                }
                this.symWriter.DefineSequencePoints(doc, (uint) count, offsets, lines, columns, endLines, endColumns);
            }
        }

        private int GetAssemblyRefIndex(AssemblyNode assembly)
        {
            if (assembly.Location == "unknown:location")
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExceptionStrings.UnresolvedAssemblyReferenceNotAllowed, new object[] { assembly.Name }));
            }
            object obj2 = this.assemblyRefIndex[assembly.UniqueKey];
            if (obj2 == null)
            {
                obj2 = this.assemblyRefEntries.Count + 1;
                AssemblyReference element = new AssemblyReference(assembly);
                if (this.module.UsePublicKeyTokensForAssemblyReferences)
                {
                    element.PublicKeyOrToken = element.PublicKeyToken;
                    element.HashValue = null;
                    element.Flags &= ~AssemblyFlags.PublicKey;
                }
                this.assemblyRefEntries.Add(element);
                this.assemblyRefIndex[assembly.UniqueKey] = obj2;
            }
            return (int) obj2;
        }

        private int GetBlobIndex(byte[] blob)
        {
            object obj2 = this.blobHeapIndex[blob];
            if (obj2 != null)
            {
                return (int) obj2;
            }
            int position = this.blobHeap.BaseStream.Position;
            int length = blob.Length;
            WriteCompressedInt(this.blobHeap, length);
            this.blobHeap.BaseStream.Write(blob, 0, length);
            this.blobHeapIndex[blob] = position;
            return position;
        }

        private int GetBlobIndex(AttributeList securityAttributes)
        {
            System.Compiler.MemoryStream output = new System.Compiler.MemoryStream();
            System.Compiler.BinaryWriter target = new System.Compiler.BinaryWriter(output);
            target.Write((byte) 0x2e);
            WriteCompressedInt(target, securityAttributes.Count);
            AttributeList.Enumerator enumerator = securityAttributes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AttributeNode current = enumerator.Current;
                this.WriteSecurityAttribute(target, current);
            }
            return this.GetBlobIndex(output.ToArray());
        }

        private int GetBlobIndex(Field field)
        {
            if (((field != null) && (field.DeclaringType != null)) && ((field.DeclaringType.Template != null) && field.DeclaringType.Template.IsGeneric))
            {
                field = this.GetUnspecializedField(field);
            }
            System.Compiler.MemoryStream output = new System.Compiler.MemoryStream();
            System.Compiler.BinaryWriter target = new System.Compiler.BinaryWriter(output);
            target.Write((byte) 6);
            TypeNode type = field.Type;
            if (type == null)
            {
                type = SystemTypes.Object;
            }
            this.WriteTypeSignature(target, type, true);
            return this.GetBlobIndex(output.ToArray());
        }

        private int GetBlobIndex(FunctionPointer fp)
        {
            System.Compiler.MemoryStream output = new System.Compiler.MemoryStream();
            System.Compiler.BinaryWriter target = new System.Compiler.BinaryWriter(output);
            this.WriteMethodSignature(target, fp);
            return this.GetBlobIndex(output.ToArray());
        }

        private int GetBlobIndex(Literal literal)
        {
            int position = this.blobHeap.BaseStream.Position;
            TypeNode type = literal.Type;
            EnumNode node2 = type as EnumNode;
            if (node2 != null)
            {
                type = node2.UnderlyingType;
            }
            IConvertible convertible = literal.Value as IConvertible;
            if (convertible == null)
            {
                convertible = "";
            }
            switch (type.typeCode)
            {
                case ElementType.Boolean:
                    this.blobHeap.Write((byte) 1);
                    this.blobHeap.Write(convertible.ToBoolean(null));
                    return position;

                case ElementType.Char:
                    this.blobHeap.Write((byte) 2);
                    this.blobHeap.Write(convertible.ToChar(null));
                    return position;

                case ElementType.Int8:
                    this.blobHeap.Write((byte) 1);
                    this.blobHeap.Write(convertible.ToSByte(null));
                    return position;

                case ElementType.UInt8:
                    this.blobHeap.Write((byte) 1);
                    this.blobHeap.Write(convertible.ToByte(null));
                    return position;

                case ElementType.Int16:
                    this.blobHeap.Write((byte) 2);
                    this.blobHeap.Write(convertible.ToInt16(null));
                    return position;

                case ElementType.UInt16:
                    this.blobHeap.Write((byte) 2);
                    this.blobHeap.Write(convertible.ToUInt16(null));
                    return position;

                case ElementType.Int32:
                    this.blobHeap.Write((byte) 4);
                    this.blobHeap.Write(convertible.ToInt32(null));
                    return position;

                case ElementType.UInt32:
                    this.blobHeap.Write((byte) 4);
                    this.blobHeap.Write(convertible.ToUInt32(null));
                    return position;

                case ElementType.Int64:
                    this.blobHeap.Write((byte) 8);
                    this.blobHeap.Write(convertible.ToInt64(null));
                    return position;

                case ElementType.UInt64:
                    this.blobHeap.Write((byte) 8);
                    this.blobHeap.Write(convertible.ToUInt64(null));
                    return position;

                case ElementType.Single:
                    this.blobHeap.Write((byte) 4);
                    this.blobHeap.Write(convertible.ToSingle(null));
                    return position;

                case ElementType.Double:
                    this.blobHeap.Write((byte) 8);
                    this.blobHeap.Write(convertible.ToDouble(null));
                    return position;

                case ElementType.String:
                    this.blobHeap.Write((string) literal.Value, false);
                    return position;

                case ElementType.Reference:
                case ElementType.Class:
                case ElementType.Array:
                case ElementType.Object:
                case ElementType.SzArray:
                    this.blobHeap.Write((byte) 4);
                    this.blobHeap.Write(0);
                    return position;
            }
            return 0;
        }

        private int GetBlobIndex(MarshallingInformation marshallingInformation)
        {
            System.Compiler.MemoryStream output = new System.Compiler.MemoryStream();
            System.Compiler.BinaryWriter target = new System.Compiler.BinaryWriter(output);
            target.Write((byte) marshallingInformation.NativeType);
            switch (marshallingInformation.NativeType)
            {
                case NativeType.Interface:
                    if (marshallingInformation.Size > 0)
                    {
                        WriteCompressedInt(target, marshallingInformation.Size);
                    }
                    break;

                case NativeType.SafeArray:
                    target.Write((byte) marshallingInformation.ElementType);
                    if ((marshallingInformation.Class != null) && (marshallingInformation.Class.Length > 0))
                    {
                        target.Write(marshallingInformation.Class, false);
                    }
                    break;

                case NativeType.ByValArray:
                    WriteCompressedInt(target, marshallingInformation.Size);
                    if (marshallingInformation.ElementType != NativeType.NotSpecified)
                    {
                        target.Write((byte) marshallingInformation.ElementType);
                    }
                    break;

                case NativeType.ByValTStr:
                    WriteCompressedInt(target, marshallingInformation.Size);
                    break;

                case NativeType.LPArray:
                    target.Write((byte) marshallingInformation.ElementType);
                    if ((marshallingInformation.ParamIndex >= 0) || (marshallingInformation.ElementSize > 0))
                    {
                        if (marshallingInformation.ParamIndex < 0)
                        {
                            marshallingInformation.ParamIndex = 0;
                        }
                        WriteCompressedInt(target, marshallingInformation.ParamIndex);
                    }
                    if (marshallingInformation.ElementSize > 0)
                    {
                        WriteCompressedInt(target, marshallingInformation.ElementSize);
                        if (marshallingInformation.NumberOfElements > 0)
                        {
                            WriteCompressedInt(target, marshallingInformation.NumberOfElements);
                        }
                    }
                    break;

                case NativeType.CustomMarshaler:
                    target.Write((short) 0);
                    target.Write(marshallingInformation.Class);
                    target.Write(marshallingInformation.Cookie);
                    break;
            }
            return this.GetBlobIndex(output.ToArray());
        }

        private int GetBlobIndex(Property prop)
        {
            System.Compiler.MemoryStream output = new System.Compiler.MemoryStream();
            System.Compiler.BinaryWriter target = new System.Compiler.BinaryWriter(output);
            this.WritePropertySignature(target, prop);
            return this.GetBlobIndex(output.ToArray());
        }

        private int GetBlobIndex(TypeNode type)
        {
            System.Compiler.MemoryStream output = new System.Compiler.MemoryStream();
            System.Compiler.BinaryWriter target = new System.Compiler.BinaryWriter(output);
            this.WriteTypeSignature(target, type, true);
            return this.GetBlobIndex(output.ToArray());
        }

        private int GetBlobIndex(string str)
        {
            object obj2 = this.blobHeapStringIndex[str];
            if (obj2 != null)
            {
                return (int) obj2;
            }
            int position = this.blobHeap.BaseStream.Position;
            this.blobHeap.Write(str);
            this.blobHeapStringIndex[str] = position;
            return position;
        }

        private int GetBlobIndex(ExpressionList expressions, ParameterList parameters)
        {
            System.Compiler.MemoryStream output = new System.Compiler.MemoryStream();
            System.Compiler.BinaryWriter signature = new System.Compiler.BinaryWriter(output);
            this.WriteCustomAttributeSignature(expressions, parameters, false, signature);
            byte[] buffer = output.ToArray();
            int length = buffer.Length;
            int position = this.blobHeap.BaseStream.Position;
            WriteCompressedInt(this.blobHeap, length);
            this.blobHeap.BaseStream.Write(buffer, 0, length);
            return position;
        }

        private int GetBlobIndex(Method method, bool methodSpecSignature)
        {
            System.Compiler.MemoryStream output = new System.Compiler.MemoryStream();
            System.Compiler.BinaryWriter target = new System.Compiler.BinaryWriter(output);
            if (methodSpecSignature)
            {
                this.WriteMethodSpecSignature(target, method);
            }
            else
            {
                this.WriteMethodSignature(target, method);
            }
            return this.GetBlobIndex(output.ToArray());
        }

        private int GetCustomAttributeParentCodedIndex(Node node)
        {
            switch (node.NodeType)
            {
                case NodeType.Assembly:
                    return 0x2e;

                case NodeType.Class:
                case NodeType.DelegateNode:
                case NodeType.EnumNode:
                case NodeType.Interface:
                case NodeType.Struct:
                case NodeType.TupleType:
                case NodeType.TypeAlias:
                case NodeType.TypeIntersection:
                case NodeType.TypeUnion:
                    break;

                case NodeType.ClassParameter:
                case NodeType.TypeParameter:
                    if (!this.UseGenerics)
                    {
                        break;
                    }
                    return ((this.GetGenericParamIndex((TypeNode) node) << 5) | 0x13);

                case NodeType.Event:
                    return ((this.GetEventIndex((Event) node) << 5) | 10);

                case NodeType.Field:
                    return ((this.GetFieldIndex((Field) node) << 5) | 1);

                case NodeType.InstanceInitializer:
                case NodeType.Method:
                case NodeType.StaticInitializer:
                    return (this.GetMethodIndex((Method) node) << 5);

                case NodeType.Module:
                    return 0x27;

                case NodeType.Parameter:
                    return ((this.GetParamIndex((Parameter) node) << 5) | 4);

                case NodeType.Property:
                    return ((this.GetPropertyIndex((Property) node) << 5) | 9);

                default:
                    return 0;
            }
            TypeNode type = (TypeNode) node;
            if (!this.IsStructural(type) || (type.IsGeneric && (((type.Template == null) || (type.ConsolidatedTemplateArguments == null)) || (type.ConsolidatedTemplateArguments.Count <= 0))))
            {
                return ((this.GetTypeDefIndex(type) << 5) | 3);
            }
            return ((this.GetTypeSpecIndex(type) << 5) | 13);
        }

        private ISymUnmanagedDocumentWriter GetDocumentWriter(Document doc)
        {
            int uniqueIdKey = Identifier.For(doc.Name).UniqueIdKey;
            object obj2 = this.documentMap[uniqueIdKey];
            if (obj2 == null)
            {
                obj2 = this.symWriter.DefineDocument(doc.Name, ref doc.Language, ref doc.LanguageVendor, ref doc.DocumentType);
                this.documentMap[uniqueIdKey] = obj2;
            }
            return (ISymUnmanagedDocumentWriter) obj2;
        }

        private int GetEventIndex(Event e) => 
            ((int) this.eventIndex[e.UniqueKey]);

        private int GetFieldIndex(Field f)
        {
            object obj2 = this.fieldIndex[f.UniqueKey];
            if (obj2 == null)
            {
                if (this.fieldEntries == null)
                {
                    return 1;
                }
                obj2 = this.fieldEntries.Count + 1;
                this.fieldEntries.Add(f);
                this.fieldIndex[f.UniqueKey] = obj2;
                if ((f.DefaultValue != null) && !(f.DefaultValue.Value is Parameter))
                {
                    this.constantTableEntries.Add(f);
                }
                if ((!f.IsStatic && (f.DeclaringType != null)) && ((f.DeclaringType.Flags & TypeFlags.ExplicitLayout) != TypeFlags.AnsiClass))
                {
                    this.fieldLayoutEntries.Add(f);
                }
                if ((f.Flags & FieldFlags.HasFieldRVA) != FieldFlags.CompilerControlled)
                {
                    this.fieldRvaEntries.Add(f);
                }
                if (f.MarshallingInformation != null)
                {
                    this.marshalEntries.Add(f);
                }
            }
            return (int) obj2;
        }

        private int GetFieldToken(Field f)
        {
            if ((f.DeclaringType != null) && ((f.DeclaringType.DeclaringModule != this.module) || this.IsStructural(f.DeclaringType)))
            {
                return (0xa000000 | this.GetMemberRefIndex(f));
            }
            return (0x4000000 | this.GetFieldIndex(f));
        }

        private int GetFileTableIndex(Module module)
        {
            object obj2 = this.fileTableIndex[module];
            if (obj2 == null)
            {
                obj2 = this.fileTableEntries.Count + 1;
                this.fileTableEntries.Add(module);
                this.fileTableIndex[module] = obj2;
            }
            return (int) obj2;
        }

        private int GetGenericParamIndex(TypeNode gp) => 
            ((int) this.genericParamIndex[gp.UniqueKey]);

        private int GetGuidIndex(Guid guid)
        {
            object obj2 = this.guidIndex[guid];
            if (obj2 == null)
            {
                obj2 = this.guidEntries.Count + 1;
                this.guidEntries.Add(guid);
                this.guidIndex[guid] = obj2;
            }
            return (int) obj2;
        }

        internal int GetLocalVarIndex(Local loc)
        {
            int num;
            LocalBinding binding = loc as LocalBinding;
            if (binding != null)
            {
                loc = binding.BoundLocal;
            }
            if (this.StripOptionalModifiersFromLocals)
            {
                loc.Type = TypeNode.StripModifiers(loc.Type);
            }
            MethodInfo methodInfo = this.methodInfo;
            if (methodInfo.localVarSignature == null)
            {
                methodInfo.localVarSignature = new System.Compiler.BinaryWriter(new System.Compiler.MemoryStream());
                methodInfo.localVarSignature.Write((short) 0);
                methodInfo.localVarIndex = new TrivialHashtable<int>();
                methodInfo.localVarSigTok = 0x11000000 | this.GetStandAloneSignatureIndex(methodInfo.localVarSignature);
            }
            if (!methodInfo.localVarIndex.TryGetValue(loc.UniqueKey, out num))
            {
                methodInfo.localVarIndex[loc.UniqueKey] = num = methodInfo.localVarIndex.Count;
                int num2 = 0;
                if (((this.symWriter != null) && (loc.Name != null)) && (loc.Name.UniqueIdKey != Identifier.Empty.UniqueIdKey))
                {
                    methodInfo.debugLocals.Add(loc);
                    methodInfo.signatureOffsets.Add(num2 = methodInfo.localVarSignature.BaseStream.Position);
                    if (loc.Pinned)
                    {
                        methodInfo.localVarSignature.Write((byte) 0x45);
                    }
                    this.WriteTypeSignature(methodInfo.localVarSignature, loc.Type, true);
                    methodInfo.signatureLengths.Add(methodInfo.localVarSignature.BaseStream.Position - num2);
                    return num;
                }
                if (loc.Pinned)
                {
                    methodInfo.localVarSignature.Write((byte) 0x45);
                }
                this.WriteTypeSignature(methodInfo.localVarSignature, loc.Type, true);
            }
            return num;
        }

        private int GetMemberRefIndex(Member m)
        {
            int num;
            if (!this.memberRefIndex.TryGetValue(m.UniqueKey, out num))
            {
                num = this.memberRefEntries.Count + 1;
                this.memberRefEntries.Add(m);
                this.memberRefIndex[m.UniqueKey] = num;
                TypeNode declaringType = m.DeclaringType;
                this.VisitReferencedType(declaringType);
            }
            return num;
        }

        private int GetMemberRefParentEncoded(TypeNode type)
        {
            if (type == null)
            {
                return 0;
            }
            if (this.IsStructural(type))
            {
                return ((this.GetTypeSpecIndex(type) << 3) | 4);
            }
            if (type.DeclaringModule != this.module)
            {
                if (type.DeclaringModule != null)
                {
                    return ((this.GetTypeRefIndex(type) << 3) | 1);
                }
                if ((type.typeCode != ElementType.Class) && (type.typeCode != ElementType.ValueType))
                {
                    return 0;
                }
            }
            return (this.GetTypeDefIndex(type) << 3);
        }

        private int GetMemberRefToken(Method m, ExpressionList arguments)
        {
            int capacity = (arguments == null) ? 0 : arguments.Count;
            TypeNodeList parameterTypes = new TypeNodeList(capacity);
            int count = m.Parameters.Count;
            for (int i = 0; i < count; i++)
            {
                parameterTypes.Add(m.Parameters[i].Type);
            }
            for (int j = count; j < capacity; j++)
            {
                parameterTypes.Add(arguments[j].Type);
            }
            VarargMethodCallSignature signature = new VarargMethodCallSignature(m, parameterTypes) {
                VarArgStart = count,
                CallingConvention = m.CallingConvention
            };
            return (0xa000000 | this.GetMemberRefIndex(signature));
        }

        private int GetMethodBodiesHeapIndex(Method m) => 
            ((int) this.methodBodiesHeapIndex[m.UniqueKey]);

        private int GetMethodDefOrRefEncoded(Method m)
        {
            if ((m.DeclaringType.DeclaringModule == this.module) && !this.IsStructural(m.DeclaringType))
            {
                return (this.GetMethodIndex(m) << 1);
            }
            return ((this.GetMemberRefIndex(m) << 1) | 1);
        }

        private int GetMethodDefToken(Method m)
        {
            if (m.DeclaringType.DeclaringModule == this.module)
            {
                return (0x6000000 | this.GetMethodIndex(m));
            }
            return (0xa000000 | this.GetMemberRefIndex(m));
        }

        private int GetMethodIndex(Method m)
        {
            int num;
            if (!this.methodIndex.TryGetValue(m.UniqueKey, out num))
            {
                if (this.methodEntries == null)
                {
                    return 1;
                }
                num = this.methodEntries.Count + 1;
                this.methodEntries.Add(m);
                this.methodIndex[m.UniqueKey] = num;
                if ((m.ReturnTypeMarshallingInformation != null) || ((m.ReturnAttributes != null) && (m.ReturnAttributes.Count > 0)))
                {
                    Parameter element = new Parameter {
                        ParameterListIndex = -1,
                        Attributes = m.ReturnAttributes
                    };
                    if (m.ReturnTypeMarshallingInformation != null)
                    {
                        element.MarshallingInformation = m.ReturnTypeMarshallingInformation;
                        element.Flags = ParameterFlags.HasFieldMarshal;
                        this.marshalEntries.Add(element);
                    }
                    this.paramEntries.Add(element);
                    this.paramIndex[m.UniqueKey] = this.paramEntries.Count;
                    this.paramIndex[element.UniqueKey] = this.paramEntries.Count;
                    this.VisitAttributeList(element.Attributes, element);
                }
                int num2 = m.IsStatic ? 0 : 1;
                if (m.Parameters != null)
                {
                    int num3 = 0;
                    int count = m.Parameters.Count;
                    while (num3 < count)
                    {
                        Parameter parameter2 = m.Parameters[num3];
                        if ((parameter2 != null) && (parameter2 != null))
                        {
                            if (parameter2.DeclaringMethod == null)
                            {
                                parameter2.DeclaringMethod = m;
                            }
                            parameter2.ParameterListIndex = num3;
                            parameter2.ArgumentListIndex = num3 + num2;
                            int num5 = this.paramEntries.Count + 1;
                            this.paramEntries.Add(parameter2);
                            this.paramIndex[parameter2.UniqueKey] = num5;
                            if (parameter2.DefaultValue != null)
                            {
                                this.constantTableEntries.Add(parameter2);
                            }
                            if (parameter2.MarshallingInformation != null)
                            {
                                this.marshalEntries.Add(parameter2);
                            }
                        }
                        num3++;
                    }
                }
                if (m.IsGeneric)
                {
                    this.VisitGenericParameterList(m, m.TemplateParameters);
                }
            }
            return num;
        }

        private int GetMethodSpecIndex(Method m)
        {
            int uniqueKey = m.UniqueKey;
            int blobIndex = this.GetBlobIndex(m, true);
            if (m.Template != null)
            {
                uniqueKey = (m.Template.UniqueKey << 8) + blobIndex;
            }
            object obj2 = this.methodSpecIndex[m.UniqueKey];
            if (obj2 == null)
            {
                obj2 = this.methodSpecIndex[uniqueKey];
                if (obj2 is int)
                {
                    Method method = this.methodSpecEntries[((int) obj2) - 1];
                    if (((method != null) && (method.Template == m.Template)) && (blobIndex == this.GetBlobIndex(method, true)))
                    {
                        return (int) obj2;
                    }
                }
                obj2 = this.methodSpecEntries.Count + 1;
                this.methodSpecEntries.Add(m);
                this.methodSpecIndex[m.UniqueKey] = obj2;
                this.methodSpecIndex[uniqueKey] = obj2;
                this.GetMemberRefIndex(m.Template);
                Method template = m.Template;
                if (template != null)
                {
                    while (template.Template != null)
                    {
                        template = template.Template;
                    }
                    TypeNodeList templateParameters = template.TemplateParameters;
                    if (templateParameters != null)
                    {
                        int num3 = 0;
                        int count = templateParameters.Count;
                        while (num3 < count)
                        {
                            TypeNode node = templateParameters[num3];
                            if (node != null)
                            {
                                this.typeParameterNumber[node.UniqueKey] = -(num3 + 1);
                            }
                            num3++;
                        }
                    }
                }
            }
            return (int) obj2;
        }

        private int GetMethodToken(Method m)
        {
            if ((this.UseGenerics && (m.Template != null)) && m.Template.IsGeneric)
            {
                return (0x2b000000 | this.GetMethodSpecIndex(m));
            }
            if ((m.DeclaringType.DeclaringModule == this.module) && !this.IsStructural(m.DeclaringType))
            {
                return (0x6000000 | this.GetMethodIndex(m));
            }
            return (0xa000000 | this.GetMemberRefIndex(m));
        }

        private int GetModuleRefIndex(Module module)
        {
            if (module.Location == "unknown:location")
            {
                throw new InvalidOperationException(ExceptionStrings.UnresolvedModuleReferenceNotAllowed);
            }
            object obj2 = this.moduleRefIndex[module.Name];
            if (obj2 == null)
            {
                obj2 = this.moduleRefEntries.Count + 1;
                this.moduleRefEntries.Add(new ModuleReference(module.Name, module));
                this.moduleRefIndex[module.Name] = obj2;
                if ((module.HashValue != null) && (module.HashValue.Length > 0))
                {
                    this.GetFileTableIndex(module);
                }
            }
            return (int) obj2;
        }

        private int GetOffset(Block target, int addressOfNextInstruction)
        {
            if (target != null)
            {
                int position = this.methodBodyHeap.BaseStream.Position;
                object obj2 = this.methodInfo.fixupIndex[target.UniqueKey];
                if (obj2 is int)
                {
                    return (((int) obj2) - addressOfNextInstruction);
                }
                Fixup fixup = new Fixup {
                    addressOfNextInstruction = addressOfNextInstruction,
                    fixupLocation = position,
                    shortOffset = false,
                    nextFixUp = (Fixup) obj2
                };
                this.methodInfo.fixupIndex[target.UniqueKey] = fixup;
            }
            return 0;
        }

        private int GetOffset(Block target, ref bool shortOffset)
        {
            if (target != null)
            {
                int num = this.methodBodyHeap.BaseStream.Position + 1;
                object obj2 = this.methodInfo.fixupIndex[target.UniqueKey];
                if (obj2 is int)
                {
                    int num2 = (int) obj2;
                    int num3 = num2 - (num + 1);
                    if ((-128 > num3) || (num3 > 0x7f))
                    {
                        num3 = num2 - (num + 4);
                        shortOffset = false;
                        return num3;
                    }
                    shortOffset = true;
                    return num3;
                }
                Fixup fixup = new Fixup();
                fixup.fixupLocation = fixup.addressOfNextInstruction = num;
                if (shortOffset)
                {
                    fixup.addressOfNextInstruction++;
                }
                else
                {
                    fixup.addressOfNextInstruction += 4;
                }
                fixup.shortOffset = shortOffset;
                fixup.nextFixUp = (Fixup) obj2;
                this.methodInfo.fixupIndex[target.UniqueKey] = fixup;
            }
            return 0;
        }

        private int GetParamIndex(Parameter p)
        {
            if (p == null)
            {
                return 0;
            }
            ParameterBinding binding = p as ParameterBinding;
            if (binding != null)
            {
                p = binding.BoundParameter;
            }
            return this.paramIndex[p.UniqueKey];
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Ansi)]
        private static extern GetClassObjectDelegate GetProcAddress(int hModule, string lpProcName);
        private static string GetProperFullTypeName(TypeNode type)
        {
            if (type.DeclaringType == null)
            {
                return type.FullName;
            }
            return type.Name.Name;
        }

        private int GetPropertyIndex(Property p) => 
            ((int) this.propertyIndex[p.UniqueKey]);

        private int GetResourceDataIndex(byte[] data)
        {
            int position = this.resourceDataHeap.BaseStream.Position;
            this.resourceDataHeap.Write(data.Length);
            this.resourceDataHeap.Write(data);
            return position;
        }

        private int GetSecurityAttributeParentCodedIndex(Node node)
        {
            switch (node.NodeType)
            {
                case NodeType.Assembly:
                    return 6;

                case NodeType.Class:
                case NodeType.DelegateNode:
                case NodeType.EnumNode:
                case NodeType.Interface:
                case NodeType.Struct:
                    return (this.GetTypeDefIndex((TypeNode) node) << 2);

                case NodeType.InstanceInitializer:
                case NodeType.Method:
                case NodeType.StaticInitializer:
                    return ((this.GetMethodIndex((Method) node) << 2) | 1);
            }
            return 0;
        }

        private string GetSerializedTypeName(TypeNode type)
        {
            bool isAssemblyQualified = true;
            return this.GetSerializedTypeName(type, ref isAssemblyQualified);
        }

        private string GetSerializedTypeName(TypeNode type, ref bool isAssemblyQualified)
        {
            if (type == null)
            {
                return null;
            }
            this.VisitReferencedType(type);
            StringBuilder sb = new StringBuilder();
            TypeModifier modifier = type as TypeModifier;
            if (modifier != null)
            {
                sb.Append(this.GetTypeDefOrRefOrSpecEncoded(type));
                sb.Append('!');
                return sb.ToString();
            }
            ArrayType type2 = type as ArrayType;
            if (type2 != null)
            {
                type = type2.ElementType;
                bool isAssemQualified = false;
                this.AppendSerializedTypeName(sb, type2.ElementType, ref isAssemQualified);
                if (type2.IsSzArray())
                {
                    sb.Append("[]");
                }
                else
                {
                    sb.Append('[');
                    if (type2.Rank == 1)
                    {
                        sb.Append('*');
                    }
                    for (int i = 1; i < type2.Rank; i++)
                    {
                        sb.Append(',');
                    }
                    sb.Append(']');
                }
            }
            else
            {
                Pointer pointer = type as Pointer;
                if (pointer != null)
                {
                    type = pointer.ElementType;
                    bool flag2 = false;
                    this.AppendSerializedTypeName(sb, pointer.ElementType, ref flag2);
                    sb.Append('*');
                }
                else
                {
                    Reference reference = type as Reference;
                    if (reference != null)
                    {
                        type = reference.ElementType;
                        bool flag3 = false;
                        this.AppendSerializedTypeName(sb, reference.ElementType, ref flag3);
                        sb.Append('&');
                    }
                    else if (type.Template == null)
                    {
                        sb.Append(type.FullName);
                    }
                    else
                    {
                        sb.Append(type.Template.FullName);
                        sb.Append('[');
                        int num2 = 0;
                        int num3 = (type.ConsolidatedTemplateArguments == null) ? 0 : type.ConsolidatedTemplateArguments.Count;
                        while (num2 < num3)
                        {
                            bool flag4 = true;
                            this.AppendSerializedTypeName(sb, type.ConsolidatedTemplateArguments[num2], ref flag4);
                            if (num2 < (num3 - 1))
                            {
                                sb.Append(',');
                            }
                            num2++;
                        }
                        sb.Append(']');
                    }
                }
            }
            if (isAssemblyQualified)
            {
                this.AppendAssemblyQualifierIfNecessary(sb, type, out isAssemblyQualified);
            }
            return sb.ToString();
        }

        private int GetStandAloneSignatureIndex(System.Compiler.BinaryWriter signatureWriter)
        {
            this.standAloneSignatureEntries.Add(signatureWriter);
            return this.standAloneSignatureEntries.Count;
        }

        private int GetStaticDataIndex(byte[] data, PESection targetSection)
        {
            int position = 0;
            switch (targetSection)
            {
                case PESection.Text:
                    position = this.methodBodiesHeap.BaseStream.Position;
                    this.methodBodiesHeap.Write(data);
                    return position;

                case PESection.SData:
                    position = this.sdataHeap.BaseStream.Position;
                    this.sdataHeap.Write(data);
                    return position;

                case PESection.TLS:
                    position = this.tlsHeap.BaseStream.Position;
                    this.tlsHeap.Write(data);
                    return position;
            }
            return position;
        }

        private int GetStringIndex(string str)
        {
            if ((str == null) || (str.Length == 0))
            {
                return 0;
            }
            object position = this.stringHeapIndex[str];
            if (position == null)
            {
                position = this.stringHeap.BaseStream.Position;
                this.stringHeap.Write(str, true);
                this.stringHeapIndex[str] = position;
            }
            return (int) position;
        }

        private int GetTypeDefIndex(TypeNode type)
        {
            object obj2 = this.typeDefIndex[type.UniqueKey];
            if (obj2 == null)
            {
                if (this.typeDefEntries == null)
                {
                    return 0;
                }
                obj2 = this.typeDefEntries.Count + 1;
                this.typeDefEntries.Add(type);
                this.typeDefIndex[type.UniqueKey] = obj2;
                if (type.IsGeneric && (type.Template == null))
                {
                    this.VisitGenericParameterList(type, type.ConsolidatedTemplateParameters);
                }
            }
            return (int) obj2;
        }

        private int GetTypeDefOrRefOrSpecEncoded(TypeNode type)
        {
            if (type == null)
            {
                return 0;
            }
            if (!this.UseGenerics)
            {
                ClassParameter parameter = type as ClassParameter;
                if (parameter != null)
                {
                    return this.GetTypeDefOrRefOrSpecEncoded(parameter.BaseClass);
                }
            }
            if (this.IsStructural(type))
            {
                return ((this.GetTypeSpecIndex(type) << 2) | 2);
            }
            if (type.DeclaringModule == this.module)
            {
                return (this.GetTypeDefIndex(type) << 2);
            }
            return ((this.GetTypeRefIndex(type) << 2) | 1);
        }

        private int GetTypeDefToken(TypeNode type)
        {
            if (this.IsStructural(type) && (!type.IsGeneric || (((type.Template != null) && (type.ConsolidatedTemplateArguments != null)) && (type.ConsolidatedTemplateArguments.Count > 0))))
            {
                return (0x1b000000 | this.GetTypeSpecIndex(type));
            }
            if (type.DeclaringModule != this.module)
            {
                if (type.DeclaringModule != null)
                {
                    return (0x1000000 | this.GetTypeRefIndex(type));
                }
                if ((type.typeCode != ElementType.ValueType) && (type.typeCode != ElementType.Class))
                {
                    return 0;
                }
                type.DeclaringModule = this.module;
            }
            return (0x2000000 | this.GetTypeDefIndex(type));
        }

        private int GetTypeRefIndex(TypeNode type)
        {
            object obj2 = this.typeRefIndex[type.UniqueKey];
            if (obj2 == null)
            {
                obj2 = this.typeRefEntries.Count + 1;
                this.typeRefEntries.Add(type);
                this.typeRefIndex[type.UniqueKey] = obj2;
                Module declaringModule = type.DeclaringModule;
                AssemblyNode assembly = declaringModule as AssemblyNode;
                if (assembly != null)
                {
                    this.GetAssemblyRefIndex(assembly);
                }
                else
                {
                    this.GetModuleRefIndex(declaringModule);
                }
                if (type.DeclaringType != null)
                {
                    this.GetTypeRefIndex(type.DeclaringType);
                }
            }
            return (int) obj2;
        }

        private int GetTypeSpecIndex(TypeNode type)
        {
            int uniqueKey = type.UniqueKey;
            int blobIndex = 0;
            if (type.Template != null)
            {
                blobIndex = this.GetBlobIndex(type);
                uniqueKey = ((type.Template.UniqueKey << 8) & 0x7fffffff) + blobIndex;
            }
            object obj2 = this.typeSpecIndex[type.UniqueKey];
            if (obj2 == null)
            {
                if (type.Template != null)
                {
                    obj2 = this.structuralTypeSpecIndexFor[uniqueKey];
                    if (obj2 is int)
                    {
                        TypeNode node = this.typeSpecEntries[((int) obj2) - 1];
                        if (((node != null) && (node.Template == type.Template)) && (blobIndex == this.GetBlobIndex(node)))
                        {
                            return (int) obj2;
                        }
                    }
                }
                obj2 = this.typeSpecEntries.Count + 1;
                this.typeSpecEntries.Add(type);
                this.typeSpecIndex[type.UniqueKey] = obj2;
                if (type.Template != null)
                {
                    this.structuralTypeSpecIndexFor[uniqueKey] = obj2;
                }
                if (type.Template != null)
                {
                    if (type.Template.DeclaringModule != this.module)
                    {
                        this.GetTypeRefIndex(type.Template);
                    }
                    TypeNodeList consolidatedTemplateArguments = type.ConsolidatedTemplateArguments;
                    int num3 = 0;
                    int num4 = (consolidatedTemplateArguments == null) ? 0 : consolidatedTemplateArguments.Count;
                    while (num3 < num4)
                    {
                        this.VisitReferencedType(consolidatedTemplateArguments[num3]);
                        num3++;
                    }
                }
                else
                {
                    TypeNodeList structuralElementTypes = type.StructuralElementTypes;
                    int num5 = 0;
                    int num6 = (structuralElementTypes == null) ? 0 : structuralElementTypes.Count;
                    while (num5 < num6)
                    {
                        this.VisitReferencedType(structuralElementTypes[num5]);
                        num5++;
                    }
                }
            }
            return (int) obj2;
        }

        private int GetTypeToken(TypeNode type)
        {
            if (this.IsStructural(type) && (!type.IsGeneric || ((type.ConsolidatedTemplateArguments != null) && (type.ConsolidatedTemplateArguments.Count > 0))))
            {
                return (0x1b000000 | this.GetTypeSpecIndex(type));
            }
            if (type.IsGeneric)
            {
                TypeNode templateInstance = type.GetTemplateInstance(type, type.TemplateParameters);
                return this.GetTypeToken(templateInstance);
            }
            if (type.DeclaringModule != this.module)
            {
                if (type.DeclaringModule != null)
                {
                    return (0x1000000 | this.GetTypeRefIndex(type));
                }
                if ((type.typeCode != ElementType.ValueType) && (type.typeCode != ElementType.Class))
                {
                    return 0;
                }
                type.DeclaringModule = this.module;
            }
            return (0x2000000 | this.GetTypeDefIndex(type));
        }

        private Field GetUnspecializedField(Field field)
        {
            if (((field != null) && (field.DeclaringType != null)) && field.DeclaringType.IsGeneric)
            {
                Field field2 = (Field) this.unspecializedFieldFor[field.UniqueKey];
                if (field2 != null)
                {
                    return field2;
                }
                TypeNode declaringType = field.DeclaringType;
                if (declaringType != null)
                {
                    while (declaringType.Template != null)
                    {
                        declaringType = declaringType.Template;
                    }
                    MemberList members = field.DeclaringType.Members;
                    MemberList list2 = declaringType.Members;
                    int num = 0;
                    int count = members.Count;
                    while (num < count)
                    {
                        if (members[num] == field)
                        {
                            field2 = (Field) list2[num];
                            if (field2 == null)
                            {
                                field2 = field;
                            }
                            this.unspecializedFieldFor[field.UniqueKey] = field2;
                            this.VisitReferencedType(field2.DeclaringType);
                            return field2;
                        }
                        num++;
                    }
                    return field;
                }
            }
            return field;
        }

        private Method GetUnspecializedMethod(Method method)
        {
            Method template = (Method) this.unspecializedMethodFor[method.UniqueKey];
            if (template != null)
            {
                return template;
            }
            TypeNode declaringType = method.DeclaringType;
            if (declaringType != null)
            {
                while (declaringType.Template != null)
                {
                    declaringType = declaringType.Template;
                }
                MemberList members = method.DeclaringType.Members;
                MemberList list2 = declaringType.Members;
                int num = 0;
                int count = members.Count;
                while (num < count)
                {
                    if (members[num] == method)
                    {
                        template = list2[num] as Method;
                        if (template == null)
                        {
                            break;
                        }
                        goto Label_00CD;
                    }
                    num++;
                }
            }
            else
            {
                return method;
            }
            template = method;
            while (template.Template != null)
            {
                template = template.Template;
            }
            if (template.DeclaringType.Template != null)
            {
                return method;
            }
        Label_00CD:
            this.unspecializedMethodFor[method.UniqueKey] = template;
            declaringType = template.DeclaringType;
            while (declaringType.Template != null)
            {
                declaringType = declaringType.Template;
            }
            this.VisitReferencedType(declaringType);
            int num3 = 0;
            int num4 = (template.TemplateParameters == null) ? 0 : template.TemplateParameters.Count;
            while (num3 < num4)
            {
                TypeNode node2 = template.TemplateParameters[num3];
                if (node2 != null)
                {
                    this.typeParameterNumber[node2.UniqueKey] = -(num3 + 1);
                }
                num3++;
            }
            return template;
        }

        private int GetUserStringIndex(string str)
        {
            object position = this.userStringHeapIndex[str];
            if (position == null)
            {
                position = this.userStringHeap.BaseStream.Position;
                WriteCompressedInt(this.userStringHeap, (str.Length * 2) + 1);
                this.userStringHeap.Write(str.ToCharArray());
                this.userStringHeapIndex[str] = position;
                ulong num = 0L;
                foreach (char ch in str)
                {
                    if (ch >= '\x007f')
                    {
                        num += (ulong) 1L;
                    }
                    else
                    {
                        switch (ch)
                        {
                            case '\x0001':
                            case '\x0002':
                            case '\x0003':
                            case '\x0004':
                            case '\x0005':
                            case '\x0006':
                            case '\a':
                            case '\b':
                            case '\x000e':
                            case '\x000f':
                            case '\x0010':
                            case '\x0011':
                            case '\x0012':
                            case '\x0013':
                            case '\x0014':
                            case '\x0015':
                            case '\x0016':
                            case '\x0017':
                            case '\x0018':
                            case '\x0019':
                            case '\x001a':
                            case '\x001b':
                            case '\x001c':
                            case '\x001d':
                            case '\x001e':
                            case '\x001f':
                            case '\'':
                            case '-':
                                num += (ulong) 1L;
                                break;
                        }
                    }
                }
                if (num > 0L)
                {
                    num = 1L;
                }
                this.userStringHeap.Write((byte) num);
            }
            return (int) position;
        }

        internal void IncrementStackHeight()
        {
            this.stackHeight++;
            if (this.stackHeight > this.stackHeightMax)
            {
                this.stackHeightMax = this.stackHeight;
            }
        }

        private bool IsStructural(TypeNode type)
        {
            if (type != null)
            {
                if (this.UseGenerics && (type.IsGeneric || ((type.Template != null) && type.Template.IsGeneric)))
                {
                    return true;
                }
                switch (type.NodeType)
                {
                    case NodeType.OptionalModifier:
                    case NodeType.Pointer:
                    case NodeType.Reference:
                    case NodeType.RequiredModifier:
                    case NodeType.ArrayType:
                        return true;

                    case NodeType.TypeParameter:
                    case NodeType.ClassParameter:
                        return this.UseGenerics;
                }
            }
            return false;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Ansi)]
        private static extern int LoadLibrary(string lpFileName);
        private void PopulateAssemblyRefTable()
        {
            AssemblyReferenceList list2 = this.module.AssemblyReferences = this.assemblyRefEntries;
            if (list2 != null)
            {
                int count = list2.Count;
                AssemblyRefRow[] rowArray2 = this.writer.assemblyRefTable = new AssemblyRefRow[count];
                for (int i = 0; i < count; i++)
                {
                    AssemblyReference reference = list2[i];
                    if (reference.Version != null)
                    {
                        rowArray2[i].MajorVersion = reference.Version.Major;
                        rowArray2[i].MinorVersion = reference.Version.Minor;
                        rowArray2[i].RevisionNumber = reference.Version.Revision;
                        rowArray2[i].BuildNumber = reference.Version.Build;
                        rowArray2[i].Flags = (int) reference.Flags;
                    }
                    if ((reference.PublicKeyOrToken != null) && (0 < reference.PublicKeyOrToken.Length))
                    {
                        rowArray2[i].PublicKeyOrToken = this.GetBlobIndex(reference.PublicKeyOrToken);
                    }
                    if (reference.Name != null)
                    {
                        rowArray2[i].Name = this.GetStringIndex(reference.Name);
                    }
                    if ((reference.Culture != null) && (reference.Culture.Length > 0))
                    {
                        rowArray2[i].Culture = this.GetStringIndex(reference.Culture);
                    }
                    if (reference.HashValue != null)
                    {
                        rowArray2[i].HashValue = this.GetBlobIndex(reference.HashValue);
                    }
                }
            }
        }

        private void PopulateAssemblyTable()
        {
            AssemblyNode assembly = this.assembly;
            AssemblyRow[] rowArray2 = this.writer.assemblyTable = new AssemblyRow[1];
            rowArray2[0].HashAlgId = 0x8004;
            rowArray2[0].Flags = (int) assembly.Flags;
            if (assembly.Version == null)
            {
                assembly.Version = new Version(1, 0, 0, 0);
            }
            rowArray2[0].MajorVersion = assembly.Version.Major;
            rowArray2[0].MinorVersion = assembly.Version.Minor;
            rowArray2[0].RevisionNumber = assembly.Version.Revision;
            rowArray2[0].BuildNumber = assembly.Version.Build;
            if ((assembly.PublicKeyOrToken != null) && (0 < assembly.PublicKeyOrToken.Length))
            {
                rowArray2[0].PublicKey = this.GetBlobIndex(assembly.PublicKeyOrToken);
            }
            if (assembly.Name != null)
            {
                rowArray2[0].Name = this.GetStringIndex(assembly.Name);
            }
            if ((assembly.Culture != null) && (assembly.Culture.Length > 0))
            {
                rowArray2[0].Culture = this.GetStringIndex(assembly.Culture);
            }
            this.writer.assemblyTable = rowArray2;
        }

        private void PopulateClassLayoutTable()
        {
            int count = this.classLayoutEntries.Count;
            if (count != 0)
            {
                ClassLayoutRow[] rowArray2 = this.writer.classLayoutTable = new ClassLayoutRow[count];
                for (int i = 0; i < count; i++)
                {
                    TypeNode type = this.classLayoutEntries[i];
                    rowArray2[i].ClassSize = type.ClassSize;
                    rowArray2[i].PackingSize = type.PackingSize;
                    rowArray2[i].Parent = this.GetTypeDefIndex(type);
                }
            }
        }

        private void PopulateConstantTable()
        {
            int count = this.constantTableEntries.Count;
            if (count != 0)
            {
                ConstantRow[] cr = this.writer.constantTable = new ConstantRow[count];
                for (int i = 0; i < count; i++)
                {
                    Parameter p = this.constantTableEntries[i] as Parameter;
                    if (p != null)
                    {
                        cr[i].Parent = (this.GetParamIndex(p) << 2) | 1;
                        this.SetConstantTableEntryValueAndTypeCode(cr, i, (Literal) p.DefaultValue);
                    }
                    else
                    {
                        Field f = (Field) this.constantTableEntries[i];
                        cr[i].Parent = this.GetFieldIndex(f) << 2;
                        this.SetConstantTableEntryValueAndTypeCode(cr, i, f.DefaultValue);
                    }
                    ConstantRow row = cr[i];
                    int parent = row.Parent;
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (cr[j].Parent > parent)
                        {
                            cr[j + 1] = cr[j];
                            if (j != 0)
                            {
                                continue;
                            }
                            cr[0] = row;
                        }
                        else if (j < (i - 1))
                        {
                            cr[j + 1] = row;
                        }
                        break;
                    }
                }
            }
        }

        private void PopulateCustomAttributeTable()
        {
            if (this.customAttributeCount != 0)
            {
                CustomAttributeRow[] rowArray2 = this.writer.customAttributeTable = new CustomAttributeRow[this.customAttributeCount];
                int index = 0;
                int num2 = 0;
                int count = this.nodesWithCustomAttributes.Count;
                while (num2 < count)
                {
                    TypeNode node2;
                    AttributeList attributes = null;
                    Node node = this.nodesWithCustomAttributes[num2];
                    int num4 = 0;
                    switch (node.NodeType)
                    {
                        case NodeType.Assembly:
                        case NodeType.Module:
                            num4 = 0x20 | ((node.NodeType == NodeType.Module) ? 7 : 14);
                            attributes = ((Module) node).Attributes;
                            goto Label_0285;

                        case NodeType.Class:
                        case NodeType.DelegateNode:
                        case NodeType.EnumNode:
                        case NodeType.Interface:
                        case NodeType.Struct:
                        case NodeType.TupleType:
                        case NodeType.TypeAlias:
                        case NodeType.TypeIntersection:
                        case NodeType.TypeUnion:
                            break;

                        case NodeType.ClassParameter:
                        case NodeType.TypeParameter:
                            if (!this.UseGenerics)
                            {
                                break;
                            }
                            node2 = (TypeNode) node;
                            num4 = (this.GetGenericParamIndex(node2) << 5) | 0x13;
                            attributes = node2.Attributes;
                            goto Label_0285;

                        case NodeType.Event:
                        {
                            Event e = (Event) node;
                            num4 = (this.GetEventIndex(e) << 5) | 10;
                            attributes = e.Attributes;
                            goto Label_0285;
                        }
                        case NodeType.Field:
                        {
                            Field f = (Field) node;
                            num4 = (this.GetFieldIndex(f) << 5) | 1;
                            attributes = f.Attributes;
                            goto Label_0285;
                        }
                        case NodeType.InstanceInitializer:
                        case NodeType.Method:
                        case NodeType.StaticInitializer:
                        {
                            Method m = (Method) node;
                            num4 = this.GetMethodIndex(m) << 5;
                            attributes = m.Attributes;
                            goto Label_0285;
                        }
                        case NodeType.Parameter:
                        {
                            Parameter p = (Parameter) node;
                            num4 = (this.GetParamIndex(p) << 5) | 4;
                            attributes = p.Attributes;
                            goto Label_0285;
                        }
                        case NodeType.Property:
                        {
                            Property property = (Property) node;
                            num4 = (this.GetPropertyIndex(property) << 5) | 9;
                            attributes = property.Attributes;
                            goto Label_0285;
                        }
                        default:
                            goto Label_0285;
                    }
                    node2 = (TypeNode) node;
                    if (this.IsStructural(node2) && (!node2.IsGeneric || (((node2.Template != null) && (node2.ConsolidatedTemplateArguments != null)) && (node2.ConsolidatedTemplateArguments.Count > 0))))
                    {
                        num4 = (this.GetTypeSpecIndex(node2) << 5) | 13;
                    }
                    else
                    {
                        num4 = (this.GetTypeDefIndex(node2) << 5) | 3;
                    }
                    attributes = node2.Attributes;
                Label_0285:
                    if (attributes != null)
                    {
                        bool useGenerics = this.UseGenerics;
                        int num5 = 0;
                        int num6 = attributes.Count;
                        while (num5 < num6)
                        {
                            AttributeNode node3 = attributes[num5];
                            if (node3 != null)
                            {
                                rowArray2[index].Parent = num4;
                                Method boundMember = (Method) ((MemberBinding) node3.Constructor).BoundMember;
                                if ((boundMember.DeclaringType.DeclaringModule == this.module) && !this.IsStructural(boundMember.DeclaringType))
                                {
                                    rowArray2[index].Constructor = (this.GetMethodIndex(boundMember) << 3) | 2;
                                }
                                else
                                {
                                    rowArray2[index].Constructor = (this.GetMemberRefIndex(boundMember) << 3) | 3;
                                }
                                rowArray2[index].Value = this.GetBlobIndex(node3.Expressions, boundMember.Parameters);
                                index++;
                            }
                            num5++;
                        }
                    }
                    num2++;
                }
            }
        }

        private void PopulateDeclSecurityTable()
        {
            if (this.securityAttributeCount != 0)
            {
                DeclSecurityRow[] rowArray2 = this.writer.declSecurityTable = new DeclSecurityRow[this.securityAttributeCount];
                int index = 0;
                int num2 = 0;
                int count = this.nodesWithSecurityAttributes.Count;
                while (num2 < count)
                {
                    SecurityAttributeList securityAttributes = null;
                    Node node = this.nodesWithSecurityAttributes[num2];
                    int num4 = 0;
                    switch (node.NodeType)
                    {
                        case NodeType.Assembly:
                            num4 = 6;
                            securityAttributes = ((AssemblyNode) node).SecurityAttributes;
                            break;

                        case NodeType.Class:
                        case NodeType.DelegateNode:
                        case NodeType.EnumNode:
                        case NodeType.Interface:
                        case NodeType.Struct:
                        {
                            TypeNode type = (TypeNode) node;
                            num4 = this.GetTypeDefIndex(type) << 2;
                            securityAttributes = type.SecurityAttributes;
                            break;
                        }
                        case NodeType.InstanceInitializer:
                        case NodeType.Method:
                        case NodeType.StaticInitializer:
                        {
                            Method m = (Method) node;
                            num4 = (this.GetMethodIndex(m) << 2) | 1;
                            securityAttributes = m.SecurityAttributes;
                            break;
                        }
                    }
                    if (securityAttributes != null)
                    {
                        int num5 = 0;
                        int num6 = securityAttributes.Count;
                        while (num5 < num6)
                        {
                            SecurityAttribute attribute = securityAttributes[num5];
                            if (attribute != null)
                            {
                                this.VisitReferencedType(CoreSystemTypes.SecurityAction);
                                rowArray2[index].Action = (int) attribute.Action;
                                rowArray2[index].Parent = num4;
                                if ((CoreSystemTypes.SystemAssembly.MetadataFormatMajorVersion == 1) && (CoreSystemTypes.SystemAssembly.MetadataFormatMinorVersion < 1))
                                {
                                    rowArray2[index].PermissionSet = this.GetBlobIndex(attribute.SerializedPermissions);
                                }
                                else if (attribute.PermissionAttributes != null)
                                {
                                    rowArray2[index].PermissionSet = this.GetBlobIndex(attribute.PermissionAttributes);
                                }
                                else
                                {
                                    rowArray2[index].PermissionSet = this.GetBlobIndex(attribute.SerializedPermissions);
                                }
                                index++;
                            }
                            num5++;
                        }
                    }
                    num2++;
                }
            }
        }

        private void PopulateEventMapTable()
        {
            int count = this.eventMapEntries.Count;
            if (count != 0)
            {
                EventMapRow[] rowArray2 = this.writer.eventMapTable = new EventMapRow[count];
                for (int i = 0; i < count; i++)
                {
                    Event e = this.eventMapEntries[i];
                    rowArray2[i].Parent = this.GetTypeDefIndex(e.DeclaringType);
                    rowArray2[i].EventList = this.GetEventIndex(e);
                }
            }
        }

        private void PopulateEventTable()
        {
            int count = this.eventEntries.Count;
            if (count != 0)
            {
                EventRow[] rowArray2 = this.writer.eventTable = new EventRow[count];
                for (int i = 0; i < count; i++)
                {
                    Event event2 = this.eventEntries[i];
                    if ((event2 != null) && (event2.Name != null))
                    {
                        rowArray2[i].Flags = (int) event2.Flags;
                        rowArray2[i].Name = this.GetStringIndex(event2.Name.ToString());
                        rowArray2[i].EventType = this.GetTypeDefOrRefOrSpecEncoded(event2.HandlerType);
                    }
                }
            }
        }

        private void PopulateExportedTypeTable()
        {
            if (this.assembly != null)
            {
                TypeNodeList exportedTypes = this.assembly.ExportedTypes;
                int num = (exportedTypes == null) ? 0 : exportedTypes.Count;
                if (num != 0)
                {
                    ExportedTypeRow[] rowArray2 = this.writer.exportedTypeTable = new ExportedTypeRow[num];
                    for (int i = 0; i < num; i++)
                    {
                        TypeNode node = exportedTypes[i];
                        if (((node != null) && (node.Namespace != null)) && (node.Name != null))
                        {
                            rowArray2[i].TypeDefId = 0;
                            rowArray2[i].TypeNamespace = this.GetStringIndex(node.Namespace.ToString());
                            rowArray2[i].TypeName = this.GetStringIndex(node.Name.ToString());
                            rowArray2[i].Flags = ((int) node.Flags) & 7;
                            if (node.DeclaringType != null)
                            {
                                for (int j = 0; j < i; j++)
                                {
                                    if (exportedTypes[j] == node.DeclaringType)
                                    {
                                        rowArray2[i].Implementation = ((j + 1) << 2) | 2;
                                        break;
                                    }
                                }
                            }
                            else if ((node.DeclaringModule != this.module) && (node.DeclaringModule is AssemblyNode))
                            {
                                rowArray2[i].Implementation = (this.GetAssemblyRefIndex((AssemblyNode) node.DeclaringModule) << 2) | 1;
                                rowArray2[i].Flags = 0x200000;
                            }
                            else
                            {
                                rowArray2[i].Implementation = this.GetFileTableIndex(node.DeclaringModule) << 2;
                            }
                        }
                    }
                }
            }
        }

        private void PopulateFieldLayoutTable()
        {
            int count = this.fieldLayoutEntries.Count;
            if (count != 0)
            {
                FieldLayoutRow[] rowArray2 = this.writer.fieldLayoutTable = new FieldLayoutRow[count];
                for (int i = 0; i < count; i++)
                {
                    Field f = this.fieldLayoutEntries[i];
                    rowArray2[i].Field = this.GetFieldIndex(f);
                    rowArray2[i].Offset = f.Offset;
                }
            }
        }

        private void PopulateFieldRVATable()
        {
            int count = this.fieldRvaEntries.Count;
            if (count != 0)
            {
                FieldRvaRow[] rowArray2 = this.writer.fieldRvaTable = new FieldRvaRow[count];
                for (int i = 0; i < count; i++)
                {
                    Field f = this.fieldRvaEntries[i];
                    rowArray2[i].Field = this.GetFieldIndex(f);
                    if (f.InitialData != null)
                    {
                        rowArray2[i].RVA = this.GetStaticDataIndex(f.InitialData, f.Section);
                    }
                    else
                    {
                        rowArray2[i].RVA = f.Offset;
                    }
                    rowArray2[i].TargetSection = f.Section;
                }
            }
        }

        private void PopulateFieldTable()
        {
            int count = this.fieldEntries.Count;
            if (count != 0)
            {
                FieldRow[] rowArray2 = this.writer.fieldTable = new FieldRow[count];
                for (int i = 0; i < count; i++)
                {
                    Field field = this.fieldEntries[i];
                    rowArray2[i].Flags = (int) field.Flags;
                    rowArray2[i].Name = this.GetStringIndex(field.Name.Name);
                    rowArray2[i].Signature = this.GetBlobIndex(field);
                }
            }
        }

        private void PopulateFileTable()
        {
            int count = this.fileTableEntries.Count;
            if (count != 0)
            {
                bool flag = false;
                FileRow[] rowArray2 = this.writer.fileTable = new FileRow[count];
                for (int i = 0; i < count; i++)
                {
                    Module module = this.fileTableEntries[i];
                    switch (module.Kind)
                    {
                        case ModuleKindFlags.ConsoleApplication:
                        case ModuleKindFlags.WindowsApplication:
                        case ModuleKindFlags.DynamicallyLinkedLibrary:
                            rowArray2[i].Flags = 0;
                            break;

                        case ModuleKindFlags.ManifestResourceFile:
                            flag = true;
                            rowArray2[i].Flags = 1;
                            break;

                        case ModuleKindFlags.UnmanagedDynamicallyLinkedLibrary:
                            rowArray2[i].Flags = 1;
                            break;
                    }
                    if (module.HashValue != null)
                    {
                        rowArray2[i].HashValue = this.GetBlobIndex(module.HashValue);
                    }
                    else
                    {
                        rowArray2[i].HashValue = 0;
                    }
                    rowArray2[i].Name = this.GetStringIndex(module.Name);
                    if (flag)
                    {
                        try
                        {
                            FileStream stream = File.OpenRead(module.Location);
                            long length = stream.Length;
                            byte[] buffer = new byte[length];
                            stream.Read(buffer, 0, (int) length);
                            byte[] blob = new SHA1CryptoServiceProvider().ComputeHash(buffer);
                            rowArray2[i].HashValue = this.GetBlobIndex(blob);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        private void PopulateGenericParamConstraintTable()
        {
            int count = this.genericParamConstraintEntries.Count;
            if (count != 0)
            {
                GenericParamConstraintRow[] rowArray2 = this.writer.genericParamConstraintTable = new GenericParamConstraintRow[count];
                TypeNode node = null;
                int genericParamIndex = 0;
                int num3 = 0;
                int num4 = 0;
                for (int i = 0; i < count; i++)
                {
                    TypeNode baseType;
                    TypeNode gp = this.genericParamConstraintEntries[i];
                    if (gp != node)
                    {
                        genericParamIndex = this.GetGenericParamIndex(gp);
                        num3 = 0;
                        num4 = 0;
                    }
                    rowArray2[i].Param = genericParamIndex;
                    if (((num3 == 0) && (gp.BaseType != null)) && (gp.BaseType != CoreSystemTypes.Object))
                    {
                        baseType = gp.BaseType;
                        num4 = 1;
                    }
                    else
                    {
                        baseType = gp.Interfaces[num3 - num4];
                    }
                    rowArray2[i].Constraint = this.GetTypeDefOrRefOrSpecEncoded(baseType);
                    node = gp;
                    num3++;
                    GenericParamConstraintRow row = rowArray2[i];
                    int param = row.Param;
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (rowArray2[j].Param > param)
                        {
                            rowArray2[j + 1] = rowArray2[j];
                            if (j != 0)
                            {
                                continue;
                            }
                            rowArray2[0] = row;
                        }
                        else if (j < (i - 1))
                        {
                            rowArray2[j + 1] = row;
                        }
                        break;
                    }
                }
            }
        }

        private void PopulateGenericParamTable()
        {
            int count = this.genericParamEntries.Count;
            if (count != 0)
            {
                GenericParamRow[] rowArray2 = this.writer.genericParamTable = new GenericParamRow[count];
                Member member = null;
                int num2 = 0;
                for (int i = 0; i < count; i++)
                {
                    Member member2 = this.genericParamEntries[i];
                    TypeNode node = this.genericParameters[i];
                    if ((node != null) && (node.Name != null))
                    {
                        Method m = member2 as Method;
                        TypeNode type = member2 as TypeNode;
                        if (member2 != member)
                        {
                            num2 = 0;
                        }
                        rowArray2[i].GenericParameter = node;
                        rowArray2[i].Number = num2++;
                        if (type != null)
                        {
                            rowArray2[i].Name = this.GetStringIndex(node.Name.ToString());
                            rowArray2[i].Owner = this.GetTypeDefIndex(type) << 1;
                        }
                        else
                        {
                            rowArray2[i].Name = this.GetStringIndex(node.Name.ToString());
                            rowArray2[i].Owner = (this.GetMethodIndex(m) << 1) | 1;
                        }
                        ITypeParameter parameter = node as ITypeParameter;
                        if (parameter != null)
                        {
                            rowArray2[i].Flags = (int) parameter.TypeParameterFlags;
                        }
                        else
                        {
                            rowArray2[i].Flags = 0;
                        }
                        member = member2;
                        GenericParamRow row = rowArray2[i];
                        int owner = row.Owner;
                        for (int n = i - 1; n >= 0; n--)
                        {
                            if (rowArray2[n].Owner > owner)
                            {
                                rowArray2[n + 1] = rowArray2[n];
                                if (n != 0)
                                {
                                    continue;
                                }
                                rowArray2[0] = row;
                            }
                            else if (n < (i - 1))
                            {
                                rowArray2[n + 1] = row;
                            }
                            break;
                        }
                    }
                }
                for (int j = 0; j < count; j++)
                {
                    Member genericParameter = rowArray2[j].GenericParameter;
                    if (genericParameter != null)
                    {
                        this.genericParamIndex[genericParameter.UniqueKey] = j + 1;
                    }
                }
                for (int k = 0; k < count; k++)
                {
                    Member member4 = rowArray2[k].GenericParameter;
                    if (member4 != null)
                    {
                        this.VisitAttributeList(member4.Attributes, member4);
                    }
                }
            }
        }

        private void PopulateGuidTable()
        {
            int count = this.guidEntries.Count;
            Guid[] guidArray2 = this.writer.GuidHeap = new Guid[count];
            for (int i = 0; i < count; i++)
            {
                guidArray2[i] = (Guid) this.guidEntries[i];
            }
        }

        private void PopulateImplMapTable()
        {
            int count = this.implMapEntries.Count;
            if (count != 0)
            {
                ImplMapRow[] rowArray2 = this.writer.implMapTable = new ImplMapRow[count];
                for (int i = 0; i < count; i++)
                {
                    Method m = this.implMapEntries[i];
                    rowArray2[i].ImportName = this.GetStringIndex(m.PInvokeImportName);
                    rowArray2[i].ImportScope = this.GetModuleRefIndex(m.PInvokeModule);
                    rowArray2[i].MappingFlags = (int) m.PInvokeFlags;
                    rowArray2[i].MemberForwarded = (this.GetMethodIndex(m) << 1) | 1;
                }
            }
        }

        private void PopulateInterfaceImplTable()
        {
            int count = this.interfaceEntries.Count;
            if (count != 0)
            {
                InterfaceImplRow[] rowArray2 = this.writer.interfaceImplTable = new InterfaceImplRow[count];
                TypeNode node = null;
                int index = 0;
                int num3 = 0;
                while (index < count)
                {
                    TypeNode type = this.interfaceEntries[index];
                    if (type == node)
                    {
                        num3++;
                    }
                    else
                    {
                        num3 = 0;
                        node = type;
                    }
                    int num5 = rowArray2[index].Class = this.GetTypeDefIndex(type);
                    Interface interface2 = null;
                    interface2 = type.Interfaces[num3];
                    if (interface2 == null)
                    {
                        index--;
                    }
                    else
                    {
                        int num7 = rowArray2[index].Interface = this.GetTypeDefOrRefOrSpecEncoded(interface2);
                        for (int i = 0; i < index; i++)
                        {
                            if (rowArray2[i].Class > num5)
                            {
                                for (int j = index; j > i; j--)
                                {
                                    rowArray2[j].Class = rowArray2[j - 1].Class;
                                    rowArray2[j].Interface = rowArray2[j - 1].Interface;
                                }
                                rowArray2[i].Class = num5;
                                rowArray2[i].Interface = num7;
                                break;
                            }
                        }
                    }
                    index++;
                }
            }
        }

        private void PopulateManifestResourceTable()
        {
            ResourceList resources = this.module.Resources;
            int num = (resources == null) ? 0 : resources.Count;
            if (num != 0)
            {
                ManifestResourceRow[] rowArray2 = this.writer.manifestResourceTable = new ManifestResourceRow[num];
                for (int i = 0; i < num; i++)
                {
                    Resource resource = resources[i];
                    rowArray2[i].Flags = resource.IsPublic ? 1 : 2;
                    rowArray2[i].Name = this.GetStringIndex(resource.Name);
                    if (resource.Data != null)
                    {
                        rowArray2[i].Offset = this.GetResourceDataIndex(resource.Data);
                    }
                    else if (resource.DefiningModule is AssemblyNode)
                    {
                        rowArray2[i].Implementation = (this.GetAssemblyRefIndex((AssemblyNode) resource.DefiningModule) << 2) | 1;
                    }
                    else
                    {
                        rowArray2[i].Implementation = this.GetFileTableIndex(resource.DefiningModule) << 2;
                    }
                }
            }
        }

        private void PopulateMarshalTable()
        {
            int count = this.marshalEntries.Count;
            if (count != 0)
            {
                FieldMarshalRow[] rowArray2 = this.writer.fieldMarshalTable = new FieldMarshalRow[count];
                for (int i = 0; i < count; i++)
                {
                    MarshallingInformation marshallingInformation;
                    Field f = this.marshalEntries[i] as Field;
                    if (f != null)
                    {
                        rowArray2[i].Parent = this.GetFieldIndex(f) << 1;
                        marshallingInformation = f.MarshallingInformation;
                    }
                    else
                    {
                        Parameter p = (Parameter) this.marshalEntries[i];
                        rowArray2[i].Parent = (this.GetParamIndex(p) << 1) | 1;
                        marshallingInformation = p.MarshallingInformation;
                    }
                    int num4 = rowArray2[i].NativeType = this.GetBlobIndex(marshallingInformation);
                    int parent = rowArray2[i].Parent;
                    for (int j = 0; j < i; j++)
                    {
                        if (rowArray2[j].Parent > parent)
                        {
                            for (int k = i; k > j; k--)
                            {
                                rowArray2[k].Parent = rowArray2[k - 1].Parent;
                                rowArray2[k].NativeType = rowArray2[k - 1].NativeType;
                            }
                            rowArray2[j].Parent = parent;
                            rowArray2[j].NativeType = num4;
                            break;
                        }
                    }
                }
            }
        }

        private void PopulateMemberRefTable()
        {
            int count = this.memberRefEntries.Count;
            if (count != 0)
            {
                MemberRefRow[] rowArray2 = this.writer.memberRefTable = new MemberRefRow[count];
                for (int i = 0; i < count; i++)
                {
                    int num4;
                    Member member = this.memberRefEntries[i];
                    if ((member == null) || (member.Name == null))
                    {
                        continue;
                    }
                    rowArray2[i].Name = this.GetStringIndex(member.Name.ToString());
                    Field field = member as Field;
                    if (field != null)
                    {
                        rowArray2[i].Signature = this.GetBlobIndex(field);
                    }
                    else
                    {
                        FunctionPointer fp = member as FunctionPointer;
                        if (fp != null)
                        {
                            rowArray2[i].Signature = this.GetBlobIndex(fp);
                            if (!(fp is VarargMethodCallSignature))
                            {
                                goto Label_01BA;
                            }
                            Method m = ((VarargMethodCallSignature) member).method;
                            if (((m == null) || (m.DeclaringType.DeclaringModule != this.module)) || this.IsStructural(m.DeclaringType))
                            {
                                goto Label_01BA;
                            }
                            rowArray2[i].Class = (this.GetMethodIndex(m) << 3) | 3;
                            continue;
                        }
                        Method unspecializedMethod = (Method) member;
                        if (unspecializedMethod.IsGeneric && (unspecializedMethod.Template != null))
                        {
                            unspecializedMethod = this.GetUnspecializedMethod(unspecializedMethod);
                        }
                        rowArray2[i].Signature = this.GetBlobIndex(unspecializedMethod, false);
                        if (((unspecializedMethod.DeclaringType.DeclaringModule == this.module) && !this.IsStructural(unspecializedMethod.DeclaringType)) && !unspecializedMethod.IsGeneric)
                        {
                            rowArray2[i].Class = (this.GetMethodIndex(unspecializedMethod) << 3) | 3;
                            continue;
                        }
                    }
                Label_01BA:
                    num4 = rowArray2[i].Class = this.GetMemberRefParentEncoded(member.DeclaringType);
                    if ((num4 & 3) == 2)
                    {
                        rowArray2[i].Class = (num4 & -4) | 4;
                    }
                }
            }
        }

        private void PopulateMethodImplTable()
        {
            int count = this.methodImplEntries.Count;
            if (count != 0)
            {
                MethodImplRow[] rowArray2 = this.writer.methodImplTable = new MethodImplRow[count];
                int num2 = 0;
                Method method = null;
                for (int i = 0; i < count; i++)
                {
                    Method m = this.methodImplEntries[i];
                    if (method != m)
                    {
                        num2 = 0;
                    }
                    rowArray2[i].Class = this.GetTypeDefIndex(m.DeclaringType);
                    if (m.DeclaringType.DeclaringModule == this.module)
                    {
                        rowArray2[i].MethodBody = this.GetMethodIndex(m) << 1;
                    }
                    else
                    {
                        rowArray2[i].MethodBody = (this.GetMemberRefIndex(m) << 1) | 1;
                    }
                    Method method3 = m.ImplementedInterfaceMethods[num2++];
                    while (method3 == null)
                    {
                        method3 = m.ImplementedInterfaceMethods[num2++];
                    }
                    rowArray2[i].MethodDeclaration = this.GetMethodDefOrRefEncoded(method3);
                    method = m;
                }
            }
        }

        private void PopulateMethodSemanticsTable()
        {
            int count = this.methodSemanticsEntries.Count;
            if (count != 0)
            {
                MethodSemanticsRow[] array = this.writer.methodSemanticsTable = new MethodSemanticsRow[count];
                Member member = null;
                int num2 = -1;
                for (int i = 0; i < count; i++)
                {
                    Member member2 = this.methodSemanticsEntries[i];
                    Property p = member2 as Property;
                    if (p != null)
                    {
                        array[i].Association = (this.GetPropertyIndex(p) << 1) | 1;
                        if (member2 != member)
                        {
                            member = member2;
                            num2 = -1;
                            if (p.Getter != null)
                            {
                                array[i].Method = this.GetMethodIndex(p.Getter);
                                array[i].Semantics = 2;
                                continue;
                            }
                        }
                        if (num2 == -1)
                        {
                            num2 = 0;
                            if (p.Setter != null)
                            {
                                array[i].Method = this.GetMethodIndex(p.Setter);
                                array[i].Semantics = 1;
                                continue;
                            }
                        }
                        array[i].Method = this.GetMethodIndex(p.OtherMethods[num2]);
                        array[i].Semantics = 4;
                        num2++;
                    }
                    else
                    {
                        Event e = member2 as Event;
                        if (e != null)
                        {
                            array[i].Association = this.GetEventIndex(e) << 1;
                            if (member2 != member)
                            {
                                member = member2;
                                num2 = -2;
                                if (e.HandlerAdder != null)
                                {
                                    array[i].Method = this.GetMethodIndex(e.HandlerAdder);
                                    array[i].Semantics = 8;
                                    continue;
                                }
                            }
                            if (num2 == -2)
                            {
                                num2 = -1;
                                if (e.HandlerRemover != null)
                                {
                                    array[i].Method = this.GetMethodIndex(e.HandlerRemover);
                                    array[i].Semantics = 0x10;
                                    continue;
                                }
                            }
                            if (num2 == -1)
                            {
                                num2 = 0;
                                if (e.HandlerCaller != null)
                                {
                                    array[i].Method = this.GetMethodIndex(e.HandlerCaller);
                                    array[i].Semantics = 0x20;
                                    continue;
                                }
                            }
                            array[i].Method = this.GetMethodIndex(e.OtherMethods[i]);
                            array[i].Semantics = 4;
                            num2++;
                        }
                    }
                }
                Array.Sort(array, new MethodSemanticsRowComparer());
            }
        }

        private void PopulateMethodSpecTable()
        {
            int count = this.methodSpecEntries.Count;
            if (count != 0)
            {
                MethodSpecRow[] rowArray2 = this.writer.methodSpecTable = new MethodSpecRow[count];
                for (int i = 0; i < count; i++)
                {
                    Method method = this.methodSpecEntries[i];
                    rowArray2[i].Method = this.GetMethodDefOrRefEncoded(method.Template);
                    rowArray2[i].Instantiation = this.GetBlobIndex(method, true);
                }
            }
        }

        private void PopulateMethodTable()
        {
            int count = this.methodEntries.Count;
            if (count != 0)
            {
                MethodRow[] rowArray2 = this.writer.methodTable = new MethodRow[count];
                for (int i = 0; i < count; i++)
                {
                    Method m = this.methodEntries[i];
                    if ((m != null) && (m.Name != null))
                    {
                        if ((m.IsAbstract || (m.Body == null)) || ((m.Body.Statements == null) || (m.Body.Statements.Count == 0)))
                        {
                            rowArray2[i].RVA = -1;
                        }
                        else
                        {
                            rowArray2[i].RVA = this.GetMethodBodiesHeapIndex(m);
                        }
                        rowArray2[i].Flags = (int) m.Flags;
                        rowArray2[i].ImplFlags = (int) m.ImplFlags;
                        rowArray2[i].Name = this.GetStringIndex(m.Name.ToString());
                        rowArray2[i].Signature = this.GetBlobIndex(m, false);
                        if ((m.ReturnTypeMarshallingInformation != null) || ((m.ReturnAttributes != null) && (m.ReturnAttributes.Count > 0)))
                        {
                            rowArray2[i].ParamList = this.paramIndex[m.UniqueKey];
                        }
                        else
                        {
                            ParameterList parameters = m.Parameters;
                            if ((parameters != null) && (parameters.Count > 0))
                            {
                                rowArray2[i].ParamList = this.GetParamIndex(parameters[0]);
                            }
                            else
                            {
                                rowArray2[i].ParamList = 0;
                            }
                        }
                    }
                }
            }
        }

        private void PopulateModuleRefTable()
        {
            int count = this.moduleRefEntries.Count;
            if (count != 0)
            {
                ModuleRefRow[] rowArray2 = this.writer.moduleRefTable = new ModuleRefRow[count];
                for (int i = 0; i < count; i++)
                {
                    ModuleReference reference = this.moduleRefEntries[i];
                    rowArray2[i].Name = this.GetStringIndex(reference.Name);
                }
            }
        }

        private void PopulateModuleTable()
        {
            ModuleRow[] rowArray2 = this.writer.moduleTable = new ModuleRow[1];
            string name = this.module.Name;
            if (this.assembly != null)
            {
                if (this.assembly.ModuleName != null)
                {
                    name = this.assembly.ModuleName;
                }
                else
                {
                    string str2 = ".exe";
                    if (this.module.Kind == ModuleKindFlags.DynamicallyLinkedLibrary)
                    {
                        str2 = ".dll";
                    }
                    name = name + str2;
                }
            }
            rowArray2[0].Name = this.GetStringIndex(name);
            rowArray2[0].Mvid = this.GetGuidIndex(Guid.NewGuid());
        }

        private void PopulateNestedClassTable()
        {
            int count = this.nestedClassEntries.Count;
            if (count != 0)
            {
                NestedClassRow[] rowArray2 = this.writer.nestedClassTable = new NestedClassRow[count];
                for (int i = 0; i < count; i++)
                {
                    TypeNode type = this.nestedClassEntries[i];
                    rowArray2[i].NestedClass = this.GetTypeDefIndex(type);
                    rowArray2[i].EnclosingClass = this.GetTypeDefIndex(type.DeclaringType);
                }
            }
        }

        private void PopulateParamTable()
        {
            int count = this.paramEntries.Count;
            if (count != 0)
            {
                ParamRow[] rowArray2 = this.writer.paramTable = new ParamRow[count];
                for (int i = 0; i < count; i++)
                {
                    Parameter parameter = this.paramEntries[i];
                    if (parameter != null)
                    {
                        bool flag = (parameter.Flags & ParameterFlags.ParameterNameMissing) != ParameterFlags.None;
                        parameter.Flags &= ~ParameterFlags.ParameterNameMissing;
                        rowArray2[i].Flags = (int) parameter.Flags;
                        rowArray2[i].Sequence = parameter.ParameterListIndex + 1;
                        rowArray2[i].Name = (flag || (parameter.Name == null)) ? 0 : this.GetStringIndex(parameter.Name.ToString());
                    }
                }
            }
        }

        private void PopulatePropertyMapTable()
        {
            int count = this.propertyMapEntries.Count;
            if (count != 0)
            {
                PropertyMapRow[] rowArray2 = this.writer.propertyMapTable = new PropertyMapRow[count];
                for (int i = 0; i < count; i++)
                {
                    Property p = this.propertyMapEntries[i];
                    rowArray2[i].Parent = this.GetTypeDefIndex(p.DeclaringType);
                    rowArray2[i].PropertyList = this.GetPropertyIndex(p);
                }
            }
        }

        private void PopulatePropertyTable()
        {
            int count = this.propertyEntries.Count;
            if (count != 0)
            {
                PropertyRow[] rowArray2 = this.writer.propertyTable = new PropertyRow[count];
                for (int i = 0; i < count; i++)
                {
                    Property prop = this.propertyEntries[i];
                    if ((prop != null) && (prop.Name != null))
                    {
                        rowArray2[i].Flags = (int) prop.Flags;
                        rowArray2[i].Name = this.GetStringIndex(prop.Name.ToString());
                        rowArray2[i].Signature = this.GetBlobIndex(prop);
                    }
                }
            }
        }

        private void PopulateStandAloneSigTable()
        {
            int count = this.standAloneSignatureEntries.Count;
            if (count != 0)
            {
                StandAloneSigRow[] rowArray2 = this.writer.standAloneSigTable = new StandAloneSigRow[count];
                for (int i = 0; i < count; i++)
                {
                    System.Compiler.BinaryWriter writer = (System.Compiler.BinaryWriter) this.standAloneSignatureEntries[i];
                    rowArray2[i].Signature = this.GetBlobIndex(writer.BaseStream.ToArray());
                }
            }
        }

        private void PopulateTypeDefTable()
        {
            int count = this.typeDefEntries.Count;
            if (count != 0)
            {
                TypeDefRow[] rowArray2 = this.writer.typeDefTable = new TypeDefRow[count];
                for (int i = 0; i < count; i++)
                {
                    TypeNode node = this.typeDefEntries[i];
                    if (node != null)
                    {
                        rowArray2[i].Flags = (int) node.Flags;
                        rowArray2[i].Name = this.GetStringIndex((node.Name == null) ? "" : node.Name.ToString());
                        rowArray2[i].Namespace = (node.Namespace == null) ? 0 : this.GetStringIndex((node.Namespace == null) ? "" : node.Namespace.ToString());
                        rowArray2[i].Extends = this.GetTypeDefOrRefOrSpecEncoded(node.BaseType);
                        MemberList members = node.Members;
                        int num3 = members.Count;
                        for (int j = 0; j < num3; j++)
                        {
                            Member member = members[j];
                            if ((member != null) && (member.NodeType == NodeType.Field))
                            {
                                rowArray2[i].FieldList = this.GetFieldIndex((Field) member);
                                break;
                            }
                        }
                        for (int k = 0; k < num3; k++)
                        {
                            Member member2 = members[k];
                            if (member2 != null)
                            {
                                NodeType nodeType = member2.NodeType;
                                if (((nodeType == NodeType.InstanceInitializer) || (nodeType == NodeType.Method)) || (nodeType == NodeType.StaticInitializer))
                                {
                                    rowArray2[i].MethodList = this.GetMethodIndex((Method) member2);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void PopulateTypeRefTable()
        {
            int count = this.typeRefEntries.Count;
            if (count != 0)
            {
                TypeRefRow[] rowArray2 = this.writer.typeRefTable = new TypeRefRow[count];
                for (int i = 0; i < count; i++)
                {
                    TypeNode node = this.typeRefEntries[i];
                    if (((node != null) && (node.Name != null)) && (node.Namespace != null))
                    {
                        rowArray2[i].Name = this.GetStringIndex(node.Name.ToString());
                        rowArray2[i].Namespace = this.GetStringIndex(node.Namespace.ToString());
                        if (node.DeclaringType == null)
                        {
                            if (node.DeclaringModule is AssemblyNode)
                            {
                                rowArray2[i].ResolutionScope = (this.GetAssemblyRefIndex((AssemblyNode) node.DeclaringModule) << 2) | 2;
                            }
                            else
                            {
                                rowArray2[i].ResolutionScope = (this.GetModuleRefIndex(node.DeclaringModule) << 2) | 1;
                            }
                        }
                        else
                        {
                            rowArray2[i].ResolutionScope = (this.GetTypeRefIndex(node.DeclaringType) << 2) | 3;
                        }
                    }
                }
            }
        }

        private void PopulateTypeSpecTable()
        {
            int count = this.typeSpecEntries.Count;
            if (count != 0)
            {
                TypeSpecRow[] rowArray2 = this.writer.typeSpecTable = new TypeSpecRow[count];
                for (int i = 0; i < count; i++)
                {
                    TypeNode type = this.typeSpecEntries[i];
                    rowArray2[i].Signature = this.GetBlobIndex(type);
                }
            }
        }

        private void SetConstantTableEntryValueAndTypeCode(ConstantRow[] cr, int i, Literal defaultValue)
        {
            cr[i].Value = this.GetBlobIndex(defaultValue);
            TypeNode type = defaultValue.Type;
            if (type.NodeType == NodeType.EnumNode)
            {
                type = ((EnumNode) type).UnderlyingType;
            }
            cr[i].Type = (int) type.typeCode;
            if ((type is Reference) || Literal.IsNullLiteral(defaultValue))
            {
                cr[i].Type = 0x12;
            }
        }

        private void SetupMetadataWriter(string debugSymbolsLocation)
        {
            Version targetVersion = TargetPlatform.TargetVersion;
            this.UseGenerics = TargetPlatform.UseGenerics;
            if (debugSymbolsLocation != null)
            {
                Type typeFromProgID = null;
                if (((targetVersion.Major == 1) && (targetVersion.Minor == 0)) && (targetVersion.Build <= 0xe79))
                {
                    try
                    {
                        typeFromProgID = Type.GetTypeFromProgID("Symwriter.pdb", false);
                        this.symWriter = (ISymUnmanagedWriter) Activator.CreateInstance(typeFromProgID);
                        if (this.symWriter != null)
                        {
                            this.symWriter.Initialize(this, debugSymbolsLocation, null, true);
                        }
                    }
                    catch (Exception)
                    {
                        typeFromProgID = null;
                        this.symWriter = null;
                    }
                }
                if (typeFromProgID == null)
                {
                    typeFromProgID = Type.GetTypeFromProgID("CorSymWriter_SxS", false);
                    if (typeFromProgID == null)
                    {
                        throw new DebugSymbolsCouldNotBeWrittenException();
                    }
                    Guid gUID = typeFromProgID.GUID;
                    if (!this.UseGenerics && (typeof(object).Assembly.GetName().Version.Major >= 2))
                    {
                        object obj2 = CrossCompileActivate(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), @"..\v1.1.4322\diasymreader.dll"), gUID);
                        this.symWriter = (ISymUnmanagedWriter) obj2;
                    }
                    if (this.symWriter == null)
                    {
                        this.symWriter = (ISymUnmanagedWriter) Activator.CreateInstance(typeFromProgID);
                    }
                    if (this.symWriter != null)
                    {
                        this.symWriter.Initialize(this, debugSymbolsLocation, null, true);
                    }
                }
            }
            this.VisitModule(this.module);
            MetadataWriter writer2 = this.writer = new MetadataWriter(this.symWriter);
            writer2.UseGenerics = this.UseGenerics;
            if (this.module.EntryPoint != null)
            {
                writer2.entryPointToken = this.GetMethodToken(this.module.EntryPoint);
                if (this.symWriter != null)
                {
                    this.symWriter.SetUserEntryPoint((uint) writer2.entryPointToken);
                }
            }
            writer2.dllCharacteristics = this.module.DllCharacteristics;
            writer2.moduleKind = this.module.Kind;
            writer2.peKind = this.module.PEKind;
            writer2.TrackDebugData = this.module.TrackDebugData;
            writer2.fileAlignment = this.module.FileAlignment;
            if (writer2.fileAlignment < 0x200)
            {
                writer2.fileAlignment = 0x200;
            }
            writer2.PublicKey = this.PublicKey;
            writer2.SignatureKeyLength = this.SignatureKeyLength;
            if (this.assembly != null)
            {
                this.PopulateAssemblyTable();
            }
            this.PopulateClassLayoutTable();
            this.PopulateConstantTable();
            this.PopulateGenericParamTable();
            this.PopulateCustomAttributeTable();
            this.PopulateDeclSecurityTable();
            this.PopulateEventMapTable();
            this.PopulateEventTable();
            this.PopulateExportedTypeTable();
            this.PopulateFieldTable();
            this.PopulateFieldLayoutTable();
            this.PopulateFieldRVATable();
            this.PopulateManifestResourceTable();
            this.PopulateFileTable();
            this.PopulateGenericParamConstraintTable();
            this.PopulateImplMapTable();
            this.PopulateInterfaceImplTable();
            this.PopulateMarshalTable();
            this.PopulateMethodTable();
            this.PopulateMethodImplTable();
            this.PopulateMemberRefTable();
            this.PopulateMethodSemanticsTable();
            this.PopulateMethodSpecTable();
            this.PopulateModuleTable();
            this.PopulateModuleRefTable();
            this.PopulateNestedClassTable();
            this.PopulateParamTable();
            this.PopulatePropertyTable();
            this.PopulatePropertyMapTable();
            this.PopulateStandAloneSigTable();
            this.PopulateTypeDefTable();
            this.PopulateTypeRefTable();
            this.PopulateTypeSpecTable();
            this.PopulateGuidTable();
            this.PopulateAssemblyRefTable();
            this.writer.BlobHeap = this.blobHeap.BaseStream;
            this.writer.SdataHeap = this.sdataHeap.BaseStream;
            this.writer.TlsHeap = this.tlsHeap.BaseStream;
            this.writer.StringHeap = this.stringHeap.BaseStream;
            this.writer.UserstringHeap = this.userStringHeap.BaseStream;
            this.writer.MethodBodiesHeap = this.methodBodiesHeap.BaseStream;
            this.writer.ResourceDataHeap = this.resourceDataHeap.BaseStream;
            this.writer.Win32Resources = this.module.Win32Resources;
        }

        void IMetaDataEmit.ApplyEditAndContinue([MarshalAs(UnmanagedType.IUnknown)] object pImport)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataEmit.DefineCustomAttribute(uint tkObj, uint tkType, void* pCustomAttribute, uint cbCustomAttribute)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataEmit.DefineEvent(uint td, string szEvent, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, uint* rmdOtherMethods)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataEmit.DefineField(uint td, string szName, uint dwFieldFlags, byte* pvSigBlob, uint cbSigBlob, uint dwCPlusTypeFlag, void* pValue, uint cchValue)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataEmit.DefineImportMember(IntPtr pAssemImport, void* pbHashValue, uint cbHashValue, IMetaDataImport pImport, uint mbMember, IntPtr pAssemEmit, uint tkParent)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataEmit.DefineImportType(IntPtr pAssemImport, void* pbHashValue, uint cbHashValue, IMetaDataImport pImport, uint tdImport, IntPtr pAssemEmit)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataEmit.DefineMemberRef(uint tkImport, string szName, byte* pvSigBlob, uint cbSigBlob)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataEmit.DefineMethod(uint td, char* zName, uint dwMethodFlags, byte* pvSigBlob, uint cbSigBlob, uint ulCodeRVA, uint dwImplFlags)
        {
            throw new NotImplementedException();
        }

        void IMetaDataEmit.DefineMethodImpl(uint td, uint tkBody, uint tkDecl)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataEmit.DefineModuleRef(string szName)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataEmit.DefineNestedType(char* szTypeDef, uint dwTypeDefFlags, uint tkExtends, uint* rtkImplements, uint tdEncloser)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataEmit.DefineParam(uint md, uint ulParamSeq, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataEmit.DefinePermissionSet(uint tk, uint dwAction, void* pvPermission, uint cbPermission)
        {
            throw new NotImplementedException();
        }

        void IMetaDataEmit.DefinePinvokeMap(uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataEmit.DefineProperty(uint td, string szProperty, uint dwPropFlags, byte* pvSig, uint cbSig, uint dwCPlusTypeFlag, void* pValue, uint cchValue, uint mdSetter, uint mdGetter, uint* rmdOtherMethods)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataEmit.DefineSecurityAttributeSet(uint tkObj, IntPtr rSecAttrs, uint cSecAttrs)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataEmit.DefineTypeDef(char* szTypeDef, uint dwTypeDefFlags, uint tkExtends, uint* rtkImplements)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataEmit.DefineTypeRefByName(uint tkResolutionScope, char* szName)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataEmit.DefineUserString(string szString, uint cchString)
        {
            throw new NotImplementedException();
        }

        void IMetaDataEmit.DeleteClassLayout(uint td)
        {
            throw new NotImplementedException();
        }

        void IMetaDataEmit.DeleteFieldMarshal(uint tk)
        {
            throw new NotImplementedException();
        }

        void IMetaDataEmit.DeletePinvokeMap(uint tk)
        {
            throw new NotImplementedException();
        }

        void IMetaDataEmit.DeleteToken(uint tkObj)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataEmit.GetSaveSize(uint fSave)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataEmit.GetTokenFromSig(byte* pvSig, uint cbSig)
        {
            System.Compiler.BinaryWriter signatureWriter = new System.Compiler.BinaryWriter(new System.Compiler.MemoryStream());
            for (int i = 0; i < cbSig; i++)
            {
                signatureWriter.Write(pvSig[i]);
            }
            return (uint) (0x11000000 | this.GetStandAloneSignatureIndex(signatureWriter));
        }

        unsafe uint IMetaDataEmit.GetTokenFromTypeSpec(byte* pvSig, uint cbSig)
        {
            throw new NotImplementedException();
        }

        void IMetaDataEmit.Merge(IMetaDataImport pImport, IntPtr pHostMapToken, [MarshalAs(UnmanagedType.IUnknown)] object pHandler)
        {
            throw new NotImplementedException();
        }

        void IMetaDataEmit.MergeEnd()
        {
            throw new NotImplementedException();
        }

        void IMetaDataEmit.Save(string szFile, uint dwSaveFlags)
        {
            throw new NotImplementedException();
        }

        unsafe void IMetaDataEmit.SaveToMemory(void* pbData, uint cbData)
        {
            throw new NotImplementedException();
        }

        unsafe void IMetaDataEmit.SaveToStream(void* pIStream, uint dwSaveFlags)
        {
            throw new NotImplementedException();
        }

        unsafe void IMetaDataEmit.SetClassLayout(uint td, uint dwPackSize, COR_FIELD_OFFSET* rFieldOffsets, uint ulClassSize)
        {
            throw new NotImplementedException();
        }

        unsafe void IMetaDataEmit.SetCustomAttributeValue(uint pcv, void* pCustomAttribute, uint cbCustomAttribute)
        {
            throw new NotImplementedException();
        }

        unsafe void IMetaDataEmit.SetEventProps(uint ev, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, uint* rmdOtherMethods)
        {
            throw new NotImplementedException();
        }

        unsafe void IMetaDataEmit.SetFieldMarshal(uint tk, byte* pvNativeType, uint cbNativeType)
        {
            throw new NotImplementedException();
        }

        unsafe void IMetaDataEmit.SetFieldProps(uint fd, uint dwFieldFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue)
        {
            throw new NotImplementedException();
        }

        void IMetaDataEmit.SetFieldRVA(uint fd, uint ulRVA)
        {
            throw new NotImplementedException();
        }

        void IMetaDataEmit.SetHandler([In, MarshalAs(UnmanagedType.IUnknown)] object pUnk)
        {
            throw new NotImplementedException();
        }

        void IMetaDataEmit.SetMethodImplFlags(uint md, uint dwImplFlags)
        {
            throw new NotImplementedException();
        }

        void IMetaDataEmit.SetMethodProps(uint md, uint dwMethodFlags, uint ulCodeRVA, uint dwImplFlags)
        {
            throw new NotImplementedException();
        }

        void IMetaDataEmit.SetModuleProps(string szName)
        {
            throw new NotImplementedException();
        }

        unsafe void IMetaDataEmit.SetParamProps(uint pd, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue)
        {
            throw new NotImplementedException();
        }

        void IMetaDataEmit.SetParent(uint mr, uint tk)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataEmit.SetPermissionSetProps(uint tk, uint dwAction, void* pvPermission, uint cbPermission)
        {
            throw new NotImplementedException();
        }

        void IMetaDataEmit.SetPinvokeMap(uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL)
        {
            throw new NotImplementedException();
        }

        unsafe void IMetaDataEmit.SetPropertyProps(uint pr, uint dwPropFlags, uint dwCPlusTypeFlag, void* pValue, uint cchValue, uint mdSetter, uint mdGetter, uint* rmdOtherMethods)
        {
            throw new NotImplementedException();
        }

        void IMetaDataEmit.SetRVA(uint md, uint ulRVA)
        {
            throw new NotImplementedException();
        }

        unsafe void IMetaDataEmit.SetTypeDefProps(uint td, uint dwTypeDefFlags, uint tkExtends, uint* rtkImplements)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataEmit.TranslateSigWithScope(IntPtr pAssemImport, void* pbHashValue, uint cbHashValue, IMetaDataImport import, byte* pbSigBlob, uint cbSigBlob, IntPtr pAssemEmit, IMetaDataEmit emit, byte* pvTranslatedSig, uint cbTranslatedSigMax)
        {
            throw new NotImplementedException();
        }

        [PreserveSig]
        void IMetaDataImport.CloseEnum(uint hEnum)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.CountEnum(uint hEnum)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.EnumCustomAttributes(ref uint phEnum, uint tk, uint tkType, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rCustomAttributes, uint cMax)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataImport.EnumEvents(ref uint phEnum, uint td, uint* rEvents, uint cMax)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataImport.EnumFields(ref uint phEnum, uint cl, uint* rFields, uint cMax)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.EnumFieldsWithName(ref uint phEnum, uint cl, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rFields, uint cMax)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.EnumInterfaceImpls(ref uint phEnum, uint td, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] rImpls, uint cMax)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.EnumMemberRefs(ref uint phEnum, uint tkParent, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] rMemberRefs, uint cMax)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.EnumMembers(ref uint phEnum, uint cl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] rMembers, uint cMax)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.EnumMembersWithName(ref uint phEnum, uint cl, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rMembers, uint cMax)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.EnumMethodImpls(ref uint phEnum, uint td, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rMethodBody, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rMethodDecl, uint cMax)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataImport.EnumMethods(ref uint phEnum, uint cl, uint* rMethods, uint cMax)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.EnumMethodSemantics(ref uint phEnum, uint mb, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] rEventProp, uint cMax)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.EnumMethodsWithName(ref uint phEnum, uint cl, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rMethods, uint cMax)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.EnumModuleRefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rModuleRefs, uint cmax)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.EnumParams(ref uint phEnum, uint mb, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] rParams, uint cMax)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.EnumPermissionSets(ref uint phEnum, uint tk, uint dwActions, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] uint[] rPermission, uint cMax)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataImport.EnumProperties(ref uint phEnum, uint td, uint* rProperties, uint cMax)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.EnumSignatures(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rSignatures, uint cmax)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.EnumTypeDefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rTypeDefs, uint cMax)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.EnumTypeRefs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rTypeRefs, uint cMax)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.EnumTypeSpecs(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rTypeSpecs, uint cmax)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.EnumUnresolvedMethods(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rMethods, uint cMax)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.EnumUserStrings(ref uint phEnum, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rStrings, uint cmax)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.FindField(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pvSigBlob, uint cbSigBlob)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.FindMember(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pvSigBlob, uint cbSigBlob)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.FindMemberRef(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pvSigBlob, uint cbSigBlob)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.FindMethod(uint td, string szName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pvSigBlob, uint cbSigBlob)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.FindTypeDefByName(string szTypeDef, uint tkEnclosingClass)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.FindTypeRef(uint tkResolutionScope, string szName)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.GetClassLayout(uint td, out uint pdwPackSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] COR_FIELD_OFFSET[] rFieldOffset, uint cMax, out uint pcFieldOffset)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataImport.GetCustomAttributeByName(uint tkObj, string szName, out void* ppData)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataImport.GetCustomAttributeProps(uint cv, out uint ptkObj, out uint ptkType, out void* ppBlob)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.GetEventProps(uint ev, out uint pClass, StringBuilder szEvent, uint cchEvent, out uint pchEvent, out uint pdwEventFlags, out uint ptkEventType, out uint pmdAddOn, out uint pmdRemoveOn, out uint pmdFire, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=11)] uint[] rmdOtherMethod, uint cMax)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataImport.GetFieldMarshal(uint tk, out byte* ppvNativeType)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataImport.GetFieldProps(uint mb, out uint pClass, StringBuilder szField, uint cchField, out uint pchField, out uint pdwAttr, out byte* ppvSigBlob, out uint pcbSigBlob, out uint pdwCPlusTypeFlag, out void* ppValue)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.GetInterfaceImplProps(uint iiImpl, out uint pClass)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataImport.GetMemberProps(uint mb, out uint pClass, StringBuilder szMember, uint cchMember, out uint pchMember, out uint pdwAttr, out byte* ppvSigBlob, out uint pcbSigBlob, out uint pulCodeRVA, out uint pdwImplFlags, out uint pdwCPlusTypeFlag, out void* ppValue)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataImport.GetMemberRefProps(uint mr, ref uint ptk, StringBuilder szMember, uint cchMember, out uint pchMember, out byte* ppvSigBlob)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataImport.GetMethodProps(uint mb, out uint pClass, IntPtr szMethod, uint cchMethod, out uint pchMethod, IntPtr pdwAttr, IntPtr ppvSigBlob, IntPtr pcbSigBlob, IntPtr pulCodeRVA)
        {
            Method method = null;
            if ((mb & 0xff000000) == 0xa000000)
            {
                method = this.memberRefEntries[(((int) mb) & 0xffffff) - 1] as Method;
            }
            else
            {
                method = this.methodEntries[(((int) mb) & 0xffffff) - 1];
            }
            pchMethod = 0;
            pClass = 0;
            if ((method != null) && (method.DeclaringType != null))
            {
                pClass = (uint) this.GetTypeDefToken(method.DeclaringType);
                string str = method.Name?.ToString();
                if (str == null)
                {
                    return 0;
                }
                pchMethod = (uint) str.Length;
                char* chPtr = (char*) szMethod.ToPointer();
                for (int i = 0; i < ((ulong) pchMethod); i++)
                {
                    chPtr[i] = str[i];
                }
                chPtr[pchMethod] = '\0';
            }
            return 0;
        }

        uint IMetaDataImport.GetMethodSemantics(uint mb, uint tkEventProp)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.GetModuleFromScope()
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.GetModuleRefProps(uint mur, StringBuilder szName, uint cchName)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.GetNameFromToken(uint tk)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataImport.GetNativeCallConvFromSig(void* pvSig, uint cbSig)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.GetNestedClassProps(uint tdNestedClass)
        {
            TypeNode node = null;
            if ((tdNestedClass & 0xff000000) == 0x1b000000)
            {
                node = this.typeSpecEntries[(((int) tdNestedClass) & 0xffffff) - 1];
            }
            else
            {
                node = this.typeDefEntries[(((int) tdNestedClass) & 0xffffff) - 1];
            }
            if ((node != null) && (node.DeclaringType != null))
            {
                return (uint) this.GetTypeToken(node.DeclaringType);
            }
            return 0;
        }

        [PreserveSig]
        int IMetaDataImport.GetParamForMethodIndex(uint md, uint ulParamSeq, out uint pParam)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataImport.GetParamProps(uint tk, out uint pmd, out uint pulSequence, StringBuilder szName, uint cchName, out uint pchName, out uint pdwAttr, out uint pdwCPlusTypeFlag, out void* ppValue)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataImport.GetPermissionSetProps(uint pm, out uint pdwAction, out void* ppvPermission)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.GetPinvokeMap(uint tk, out uint pdwMappingFlags, StringBuilder szImportName, uint cchImportName, out uint pchImportName)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataImport.GetPropertyProps(uint prop, out uint pClass, StringBuilder szProperty, uint cchProperty, out uint pchProperty, out uint pdwPropFlags, out byte* ppvSig, out uint pbSig, out uint pdwCPlusTypeFlag, out void* ppDefaultValue, out uint pcchDefaultValue, out uint pmdSetter, out uint pmdGetter, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=14)] uint[] rmdOtherMethod, uint cMax)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.GetRVA(uint tk, out uint pulCodeRVA)
        {
            throw new NotImplementedException();
        }

        Guid IMetaDataImport.GetScopeProps(StringBuilder szName, uint cchName, out uint pchName)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataImport.GetSigFromToken(uint mdSig, out byte* ppvSig)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataImport.GetTypeDefProps(uint td, IntPtr szTypeDef, uint cchTypeDef, out uint pchTypeDef, IntPtr pdwTypeDefFlags)
        {
            pchTypeDef = 0;
            if (td == 0)
            {
                return 0;
            }
            TypeNode type = null;
            if ((td & 0xff000000) == 0x1b000000)
            {
                type = this.typeSpecEntries[(((int) td) & 0xffffff) - 1];
                if (type.Template != null)
                {
                    type = type.Template;
                }
            }
            else
            {
                type = this.typeDefEntries[(((int) td) & 0xffffff) - 1];
            }
            if ((type == null) || (type.Name == null))
            {
                return 0;
            }
            string properFullTypeName = GetProperFullTypeName(type);
            if (properFullTypeName == null)
            {
                return 0;
            }
            pchTypeDef = (uint) properFullTypeName.Length;
            if (pchTypeDef >= cchTypeDef)
            {
                pchTypeDef = cchTypeDef - 1;
            }
            char* chPtr = (char*) szTypeDef.ToPointer();
            for (int i = 0; i < ((ulong) pchTypeDef); i++)
            {
                chPtr[i] = properFullTypeName[i];
            }
            chPtr[pchTypeDef] = '\0';
            *((int*) pdwTypeDefFlags.ToPointer()) = type.Flags;
            TypeNode baseType = type.BaseType;
            if (baseType == null)
            {
                return 0;
            }
            return (uint) this.GetTypeToken(baseType);
        }

        uint IMetaDataImport.GetTypeRefProps(uint tr, out uint ptkResolutionScope, StringBuilder szName, uint cchName)
        {
            throw new NotImplementedException();
        }

        unsafe uint IMetaDataImport.GetTypeSpecFromToken(uint typespec, out byte* ppvSig)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.GetUserString(uint stk, StringBuilder szString, uint cchString)
        {
            throw new NotImplementedException();
        }

        int IMetaDataImport.IsGlobal(uint pd)
        {
            throw new NotImplementedException();
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [PreserveSig]
        bool IMetaDataImport.IsValidToken(uint tk)
        {
            throw new NotImplementedException();
        }

        void IMetaDataImport.ResetEnum(uint hEnum, uint ulPos)
        {
            throw new NotImplementedException();
        }

        uint IMetaDataImport.ResolveTypeRef(uint tr, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppIScope)
        {
            throw new NotImplementedException();
        }

        private void Visit(Node node)
        {
            Class class2;
            if (node != null)
            {
                switch (node.NodeType)
                {
                    case NodeType.Add:
                    case NodeType.Add_Ovf:
                    case NodeType.Add_Ovf_Un:
                    case NodeType.And:
                    case NodeType.Box:
                    case NodeType.Castclass:
                    case NodeType.Ceq:
                    case NodeType.Cgt:
                    case NodeType.Cgt_Un:
                    case NodeType.Clt:
                    case NodeType.Clt_Un:
                    case NodeType.Div:
                    case NodeType.Div_Un:
                    case NodeType.Isinst:
                    case NodeType.Ldvirtftn:
                    case NodeType.Mkrefany:
                    case NodeType.Mul:
                    case NodeType.Mul_Ovf:
                    case NodeType.Mul_Ovf_Un:
                    case NodeType.Or:
                    case NodeType.Refanyval:
                    case NodeType.Rem:
                    case NodeType.Rem_Un:
                    case NodeType.Shl:
                    case NodeType.Shr:
                    case NodeType.Shr_Un:
                    case NodeType.Sub:
                    case NodeType.Sub_Ovf:
                    case NodeType.Sub_Ovf_Un:
                    case NodeType.Unbox:
                    case NodeType.UnboxAny:
                    case NodeType.Xor:
                    case NodeType.Eq:
                    case NodeType.Ge:
                    case NodeType.Gt:
                    case NodeType.Le:
                    case NodeType.Lt:
                    case NodeType.Ne:
                    case NodeType.Is:
                        this.VisitBinaryExpression((BinaryExpression) node);
                        return;

                    case NodeType.Arglist:
                        this.VisitExpression((Expression) node);
                        return;

                    case NodeType.Branch:
                        this.VisitBranch((Branch) node);
                        return;

                    case NodeType.Call:
                    case NodeType.Calli:
                    case NodeType.Callvirt:
                    case NodeType.Jmp:
                    case NodeType.MethodCall:
                        this.VisitMethodCall((MethodCall) node);
                        return;

                    case NodeType.Ckfinite:
                    case NodeType.Conv_I:
                    case NodeType.Conv_I1:
                    case NodeType.Conv_I2:
                    case NodeType.Conv_I4:
                    case NodeType.Conv_I8:
                    case NodeType.Conv_Ovf_I:
                    case NodeType.Conv_Ovf_I_Un:
                    case NodeType.Conv_Ovf_I1:
                    case NodeType.Conv_Ovf_I1_Un:
                    case NodeType.Conv_Ovf_I2:
                    case NodeType.Conv_Ovf_I2_Un:
                    case NodeType.Conv_Ovf_I4:
                    case NodeType.Conv_Ovf_I4_Un:
                    case NodeType.Conv_Ovf_I8:
                    case NodeType.Conv_Ovf_I8_Un:
                    case NodeType.Conv_Ovf_U:
                    case NodeType.Conv_Ovf_U_Un:
                    case NodeType.Conv_Ovf_U1:
                    case NodeType.Conv_Ovf_U1_Un:
                    case NodeType.Conv_Ovf_U2:
                    case NodeType.Conv_Ovf_U2_Un:
                    case NodeType.Conv_Ovf_U4:
                    case NodeType.Conv_Ovf_U4_Un:
                    case NodeType.Conv_Ovf_U8:
                    case NodeType.Conv_Ovf_U8_Un:
                    case NodeType.Conv_R_Un:
                    case NodeType.Conv_R4:
                    case NodeType.Conv_R8:
                    case NodeType.Conv_U:
                    case NodeType.Conv_U1:
                    case NodeType.Conv_U2:
                    case NodeType.Conv_U4:
                    case NodeType.Conv_U8:
                    case NodeType.Ldftn:
                    case NodeType.Ldlen:
                    case NodeType.Ldtoken:
                    case NodeType.Localloc:
                    case NodeType.Neg:
                    case NodeType.Not:
                    case NodeType.Refanytype:
                    case NodeType.Sizeof:
                        this.VisitUnaryExpression((UnaryExpression) node);
                        return;

                    case NodeType.Cpblk:
                    case NodeType.Initblk:
                        this.VisitTernaryExpression((TernaryExpression) node);
                        return;

                    case NodeType.DebugBreak:
                        this.VisitStatement((Statement) node);
                        return;

                    case NodeType.Dup:
                        this.VisitExpression((Expression) node);
                        return;

                    case NodeType.EndFilter:
                        this.VisitEndFilter((EndFilter) node);
                        return;

                    case NodeType.EndFinally:
                        this.VisitStatement((Statement) node);
                        return;

                    case NodeType.ExceptionHandler:
                    case NodeType.SkipCheck:
                    case NodeType.Catch:
                    case NodeType.FaultHandler:
                    case NodeType.Filter:
                    case NodeType.Finally:
                    case NodeType.Identifier:
                    case NodeType.Instruction:
                    case NodeType.InterfaceExpression:
                    case NodeType.LogicalNot:
                    case NodeType.NamedArgument:
                    case NodeType.Namespace:
                    case NodeType.Try:
                    case NodeType.ArrayType:
                    case NodeType.Assembly:
                    case NodeType.AssemblyReference:
                    case NodeType.Attribute:
                    case NodeType.FunctionPointer:
                    case NodeType.Module:
                    case NodeType.ModuleReference:
                    case NodeType.OptionalModifier:
                    case NodeType.Pointer:
                    case NodeType.Reference:
                    case NodeType.RequiredModifier:
                    case NodeType.SecurityAttribute:
                    case NodeType.Array:
                    case NodeType.BlockReference:
                    case NodeType.CompilationParameters:
                    case NodeType.Document:
                    case NodeType.EndOfRecord:
                    case NodeType.Expression:
                    case NodeType.Guid:
                    case NodeType.List:
                    case NodeType.MarshallingInformation:
                    case NodeType.Member:
                    case NodeType.MemberReference:
                    case NodeType.MissingBlockReference:
                    case NodeType.MissingExpression:
                    case NodeType.MissingMemberReference:
                    case NodeType.String:
                    case NodeType.StringDictionary:
                    case NodeType.TypeNode:
                    case NodeType.Uri:
                    case NodeType.XmlNode:
                    case NodeType.AddEventHandler:
                    case NodeType.AliasDefinition:
                    case NodeType.AnonymousNestedFunction:
                    case NodeType.ApplyToAll:
                    case NodeType.ArglistArgumentExpression:
                    case NodeType.ArglistExpression:
                    case NodeType.ArrayTypeExpression:
                    case NodeType.As:
                    case NodeType.Assertion:
                    case NodeType.AssignmentExpression:
                    case NodeType.Assumption:
                    case NodeType.Base:
                    case NodeType.BoxedTypeExpression:
                    case NodeType.ClassExpression:
                    case NodeType.CoerceTuple:
                    case NodeType.CollectionEnumerator:
                    case NodeType.Comma:
                    case NodeType.Compilation:
                    case NodeType.CompilationUnit:
                    case NodeType.CompilationUnitSnippet:
                    case NodeType.Conditional:
                    case NodeType.ConstructDelegate:
                    case NodeType.ConstructFlexArray:
                    case NodeType.ConstructIterator:
                    case NodeType.ConstructTuple:
                    case NodeType.Continue:
                    case NodeType.CopyReference:
                    case NodeType.CurrentClosure:
                    case NodeType.Decrement:
                    case NodeType.DefaultValue:
                    case NodeType.DoWhile:
                    case NodeType.Exit:
                    case NodeType.ExplicitCoercion:
                    case NodeType.ExpressionSnippet:
                    case NodeType.FieldInitializerBlock:
                    case NodeType.Fixed:
                    case NodeType.FlexArrayTypeExpression:
                    case NodeType.For:
                    case NodeType.ForEach:
                    case NodeType.FunctionDeclaration:
                    case NodeType.FunctionTypeExpression:
                    case NodeType.Goto:
                    case NodeType.GotoCase:
                    case NodeType.If:
                    case NodeType.ImplicitThis:
                    case NodeType.Increment:
                    case NodeType.InvariantTypeExpression:
                    case NodeType.LabeledStatement:
                    case NodeType.LocalDeclaration:
                    case NodeType.LocalDeclarationsStatement:
                    case NodeType.Lock:
                    case NodeType.LogicalAnd:
                    case NodeType.LogicalOr:
                    case NodeType.LRExpression:
                    case NodeType.NameBinding:
                    case NodeType.NonEmptyStreamTypeExpression:
                    case NodeType.NonNullableTypeExpression:
                    case NodeType.NonNullTypeExpression:
                    case NodeType.NullableTypeExpression:
                    case NodeType.NullCoalesingExpression:
                    case NodeType.Parentheses:
                    case NodeType.PointerTypeExpression:
                    case NodeType.PostfixExpression:
                    case NodeType.PrefixExpression:
                    case NodeType.QualifiedIdentifer:
                    case NodeType.ReferenceTypeExpression:
                    case NodeType.RefTypeExpression:
                    case NodeType.RefValueExpression:
                    case NodeType.RemoveEventHandler:
                    case NodeType.Repeat:
                    case NodeType.ResourceUse:
                    case NodeType.SetterValue:
                    case NodeType.StackAlloc:
                    case NodeType.StatementSnippet:
                    case NodeType.StreamTypeExpression:
                    case NodeType.Switch:
                    case NodeType.SwitchCase:
                        goto Label_0615;

                    case NodeType.Nop:
                        this.VisitStatement((Statement) node);
                        return;

                    case NodeType.Pop:
                        this.VisitExpression((Expression) node);
                        return;

                    case NodeType.ReadOnlyAddressOf:
                    case NodeType.AddressOf:
                    case NodeType.OutAddress:
                    case NodeType.RefAddress:
                        this.VisitAddressOf((UnaryExpression) node);
                        return;

                    case NodeType.Rethrow:
                    case NodeType.Throw:
                        this.VisitThrow((Throw) node);
                        return;

                    case NodeType.SwitchInstruction:
                        this.VisitSwitchInstruction((SwitchInstruction) node);
                        return;

                    case NodeType.AddressDereference:
                        this.VisitAddressDereference((AddressDereference) node);
                        return;

                    case NodeType.AssignmentStatement:
                        this.VisitAssignmentStatement((AssignmentStatement) node);
                        return;

                    case NodeType.Block:
                        this.VisitBlock((Block) node);
                        return;

                    case NodeType.Construct:
                        this.VisitConstruct((Construct) node);
                        return;

                    case NodeType.ConstructArray:
                        this.VisitConstructArray((ConstructArray) node);
                        return;

                    case NodeType.ExpressionStatement:
                        this.VisitExpressionStatement((ExpressionStatement) node);
                        return;

                    case NodeType.Indexer:
                        this.VisitIndexer((Indexer) node);
                        return;

                    case NodeType.Literal:
                        this.VisitLiteral((Literal) node);
                        return;

                    case NodeType.MemberBinding:
                        this.VisitMemberBinding((MemberBinding) node);
                        return;

                    case NodeType.Return:
                        this.VisitReturn((Return) node);
                        return;

                    case NodeType.This:
                        this.VisitThis((This) node);
                        return;

                    case NodeType.Class:
                    case NodeType.ClassParameter:
                        this.VisitClass((Class) node);
                        return;

                    case NodeType.DelegateNode:
                        this.VisitDelegateNode((DelegateNode) node);
                        return;

                    case NodeType.EnumNode:
                        this.VisitEnumNode((EnumNode) node);
                        return;

                    case NodeType.Event:
                        this.VisitEvent((Event) node);
                        return;

                    case NodeType.Field:
                        this.VisitField((Field) node);
                        return;

                    case NodeType.InstanceInitializer:
                    case NodeType.Method:
                    case NodeType.StaticInitializer:
                        this.VisitMethod((Method) node);
                        return;

                    case NodeType.Interface:
                    case NodeType.TypeParameter:
                        this.VisitInterface((Interface) node);
                        return;

                    case NodeType.Local:
                        this.VisitLocal((Local) node);
                        return;

                    case NodeType.Parameter:
                        this.VisitParameter((Parameter) node);
                        return;

                    case NodeType.Property:
                        this.VisitProperty((Property) node);
                        return;

                    case NodeType.Struct:
                    case NodeType.TupleType:
                    case NodeType.TypeAlias:
                    case NodeType.TypeIntersection:
                    case NodeType.TypeUnion:
                        this.VisitStruct((Struct) node);
                        break;

                    case NodeType.BlockExpression:
                        this.VisitBlockExpression((BlockExpression) node);
                        return;

                    case NodeType.SwitchCaseBottom:
                        break;

                    default:
                        goto Label_0615;
                }
            }
            return;
        Label_0615:
            class2 = node as Class;
            if (class2 != null)
            {
                this.VisitClass(class2);
            }
            else
            {
                Struct struct2 = node as Struct;
                if (struct2 != null)
                {
                    this.VisitStruct(struct2);
                }
            }
        }

        private void VisitAddressDereference(AddressDereference adr)
        {
            this.Visit(adr.Address);
            if (adr.Alignment > 0)
            {
                this.methodBodyHeap.Write((byte) 0xfe);
                this.methodBodyHeap.Write((byte) 0x12);
                this.methodBodyHeap.Write((byte) adr.Alignment);
            }
            if (adr.Volatile)
            {
                this.methodBodyHeap.Write((byte) 0xfe);
                this.methodBodyHeap.Write((byte) 0x13);
            }
            switch (adr.Type.typeCode)
            {
                case ElementType.Char:
                case ElementType.UInt16:
                    this.methodBodyHeap.Write((byte) 0x49);
                    return;

                case ElementType.Int8:
                    this.methodBodyHeap.Write((byte) 70);
                    return;

                case ElementType.UInt8:
                    this.methodBodyHeap.Write((byte) 0x47);
                    return;

                case ElementType.Int16:
                    this.methodBodyHeap.Write((byte) 0x48);
                    return;

                case ElementType.Int32:
                    this.methodBodyHeap.Write((byte) 0x4a);
                    return;

                case ElementType.UInt32:
                    this.methodBodyHeap.Write((byte) 0x4b);
                    return;

                case ElementType.Int64:
                case ElementType.UInt64:
                    this.methodBodyHeap.Write((byte) 0x4c);
                    return;

                case ElementType.Single:
                    this.methodBodyHeap.Write((byte) 0x4e);
                    return;

                case ElementType.Double:
                    this.methodBodyHeap.Write((byte) 0x4f);
                    return;
            }
            if ((this.UseGenerics && (adr.Type != null)) && (adr.Type != SystemTypes.Object))
            {
                this.methodBodyHeap.Write((byte) 0x71);
                this.methodBodyHeap.Write(this.GetTypeToken(adr.Type));
            }
            else if (TypeNode.StripModifiers(adr.Type) is Pointer)
            {
                this.methodBodyHeap.Write((byte) 0x4d);
            }
            else
            {
                this.methodBodyHeap.Write((byte) 80);
            }
        }

        private void VisitAddressOf(UnaryExpression expr)
        {
            Expression operand = expr.Operand;
            if (operand != null)
            {
                switch (operand.NodeType)
                {
                    case NodeType.Indexer:
                    {
                        Indexer indexer = (Indexer) operand;
                        this.Visit(indexer.Object);
                        if ((indexer.Operands != null) && (indexer.Operands.Count >= 1))
                        {
                            this.Visit(indexer.Operands[0]);
                            if (expr.NodeType == NodeType.ReadOnlyAddressOf)
                            {
                                this.methodBodyHeap.Write((byte) 0xfe);
                                this.methodBodyHeap.Write((byte) 30);
                            }
                            this.methodBodyHeap.Write((byte) 0x8f);
                            this.methodBodyHeap.Write(this.GetTypeToken(indexer.ElementType));
                            this.stackHeight--;
                        }
                        break;
                    }
                    case NodeType.MemberBinding:
                    {
                        MemberBinding binding = (MemberBinding) operand;
                        if (binding.TargetObject != null)
                        {
                            this.Visit(binding.TargetObject);
                            this.methodBodyHeap.Write((byte) 0x7c);
                        }
                        else
                        {
                            this.methodBodyHeap.Write((byte) 0x7f);
                            this.IncrementStackHeight();
                        }
                        this.methodBodyHeap.Write(this.GetFieldToken((Field) binding.BoundMember));
                        break;
                    }
                    case NodeType.Local:
                    {
                        int localVarIndex = this.GetLocalVarIndex((Local) operand);
                        if (localVarIndex < 0x100)
                        {
                            this.methodBodyHeap.Write((byte) 0x12);
                            this.methodBodyHeap.Write((byte) localVarIndex);
                        }
                        else
                        {
                            this.methodBodyHeap.Write((byte) 0xfe);
                            this.methodBodyHeap.Write((byte) 13);
                            this.methodBodyHeap.Write((ushort) localVarIndex);
                        }
                        this.IncrementStackHeight();
                        return;
                    }
                    case NodeType.Parameter:
                    {
                        ParameterBinding binding2 = operand as ParameterBinding;
                        if (binding2 != null)
                        {
                            operand = binding2.BoundParameter;
                        }
                        int argumentListIndex = ((Parameter) operand).ArgumentListIndex;
                        if (argumentListIndex < 0x100)
                        {
                            this.methodBodyHeap.Write((byte) 15);
                            this.methodBodyHeap.Write((byte) argumentListIndex);
                        }
                        else
                        {
                            this.methodBodyHeap.Write((byte) 0xfe);
                            this.methodBodyHeap.Write((byte) 10);
                            this.methodBodyHeap.Write((ushort) argumentListIndex);
                        }
                        this.IncrementStackHeight();
                        return;
                    }
                }
            }
        }

        private void VisitAssignmentStatement(AssignmentStatement assignment)
        {
            Indexer indexer;
            byte num4;
            this.DefineSequencePoint(assignment);
            Expression target = assignment.Target;
            NodeType nodeType = assignment.Target.NodeType;
            if (nodeType <= NodeType.MemberBinding)
            {
                if (nodeType == NodeType.AddressDereference)
                {
                    AddressDereference dereference = (AddressDereference) target;
                    this.Visit(dereference.Address);
                    if (dereference.Type != null)
                    {
                        Literal source = assignment.Source as Literal;
                        if ((source != null) && (source.Value == null))
                        {
                            if (dereference.Type == SystemTypes.Object)
                            {
                                this.methodBodyHeap.Write((byte) 20);
                                this.IncrementStackHeight();
                                this.methodBodyHeap.Write((byte) 0x51);
                                this.stackHeight -= 2;
                                return;
                            }
                            this.methodBodyHeap.Write((byte) 0xfe);
                            this.methodBodyHeap.Write((byte) 0x15);
                            this.methodBodyHeap.Write(this.GetTypeToken(dereference.Type));
                            this.stackHeight--;
                            return;
                        }
                    }
                    this.Visit(assignment.Source);
                    this.stackHeight -= 2;
                    if (dereference.Alignment > 0)
                    {
                        this.methodBodyHeap.Write((byte) 0xfe);
                        this.methodBodyHeap.Write((byte) 0x12);
                        this.methodBodyHeap.Write((byte) dereference.Alignment);
                    }
                    if (dereference.Volatile)
                    {
                        this.methodBodyHeap.Write((byte) 0xfe);
                        this.methodBodyHeap.Write((byte) 0x13);
                    }
                    TypeNode type = TypeNode.StripModifiers(dereference.Type);
                    if (type != null)
                    {
                        switch (type.typeCode)
                        {
                            case ElementType.Int8:
                            case ElementType.UInt8:
                                this.methodBodyHeap.Write((byte) 0x52);
                                return;

                            case ElementType.Int16:
                            case ElementType.UInt16:
                                this.methodBodyHeap.Write((byte) 0x53);
                                return;

                            case ElementType.Int32:
                            case ElementType.UInt32:
                                this.methodBodyHeap.Write((byte) 0x54);
                                return;

                            case ElementType.Int64:
                            case ElementType.UInt64:
                                this.methodBodyHeap.Write((byte) 0x55);
                                return;

                            case ElementType.Single:
                                this.methodBodyHeap.Write((byte) 0x56);
                                return;

                            case ElementType.Double:
                                this.methodBodyHeap.Write((byte) 0x57);
                                return;

                            case ElementType.IntPtr:
                            case ElementType.UIntPtr:
                                this.methodBodyHeap.Write((byte) 0xdf);
                                return;
                        }
                        if ((this.UseGenerics && (type != null)) && (type != SystemTypes.Object))
                        {
                            this.methodBodyHeap.Write((byte) 0x81);
                            this.methodBodyHeap.Write(this.GetTypeToken(type));
                        }
                        else if (type.NodeType == NodeType.Pointer)
                        {
                            this.methodBodyHeap.Write((byte) 0xdf);
                        }
                        else
                        {
                            this.methodBodyHeap.Write((byte) 0x51);
                        }
                    }
                    return;
                }
                if (nodeType != NodeType.Indexer)
                {
                    if (nodeType == NodeType.MemberBinding)
                    {
                        MemberBinding binding = (MemberBinding) target;
                        if (binding.TargetObject != null)
                        {
                            this.Visit(binding.TargetObject);
                        }
                        this.Visit(assignment.Source);
                        if (binding.TargetObject != null)
                        {
                            if (binding.Alignment != -1)
                            {
                                this.methodBodyHeap.Write((byte) 0xfe);
                                this.methodBodyHeap.Write((byte) 0x12);
                                this.methodBodyHeap.Write((byte) binding.Alignment);
                            }
                            if (binding.Volatile)
                            {
                                this.methodBodyHeap.Write((byte) 0xfe);
                                this.methodBodyHeap.Write((byte) 0x13);
                            }
                            this.methodBodyHeap.Write((byte) 0x7d);
                        }
                        else
                        {
                            if (binding.Volatile)
                            {
                                this.methodBodyHeap.Write((byte) 0xfe);
                                this.methodBodyHeap.Write((byte) 0x13);
                            }
                            this.methodBodyHeap.Write((byte) 0x80);
                        }
                        this.methodBodyHeap.Write(this.GetFieldToken((Field) binding.BoundMember));
                        if (binding.TargetObject != null)
                        {
                            this.stackHeight -= 2;
                            return;
                        }
                        this.stackHeight--;
                    }
                    return;
                }
                indexer = (Indexer) target;
                this.Visit(indexer.Object);
                if ((indexer.Operands == null) || (indexer.Operands.Count < 1))
                {
                    return;
                }
                this.Visit(indexer.Operands[0]);
                this.Visit(assignment.Source);
                switch (indexer.ElementType.typeCode)
                {
                    case ElementType.Boolean:
                    case ElementType.Int8:
                    case ElementType.UInt8:
                        num4 = 0x9c;
                        goto Label_04B5;

                    case ElementType.Char:
                    case ElementType.Int16:
                    case ElementType.UInt16:
                        num4 = 0x9d;
                        goto Label_04B5;

                    case ElementType.Int32:
                    case ElementType.UInt32:
                        num4 = 0x9e;
                        goto Label_04B5;

                    case ElementType.Int64:
                    case ElementType.UInt64:
                        num4 = 0x9f;
                        goto Label_04B5;

                    case ElementType.Single:
                        num4 = 160;
                        goto Label_04B5;

                    case ElementType.Double:
                        num4 = 0xa1;
                        goto Label_04B5;

                    case ElementType.IntPtr:
                    case ElementType.UIntPtr:
                        num4 = 0x9b;
                        goto Label_04B5;
                }
            }
            else
            {
                if (nodeType == NodeType.This)
                {
                    this.Visit(assignment.Source);
                    this.methodBodyHeap.Write((byte) 0x10);
                    this.methodBodyHeap.Write((byte) 0);
                    this.stackHeight--;
                    return;
                }
                if (nodeType != NodeType.Local)
                {
                    if (nodeType == NodeType.Parameter)
                    {
                        ParameterBinding binding2 = target as ParameterBinding;
                        if (binding2 != null)
                        {
                            target = binding2.BoundParameter;
                        }
                        Parameter parameter = (Parameter) target;
                        this.Visit(assignment.Source);
                        int argumentListIndex = parameter.ArgumentListIndex;
                        if (argumentListIndex < 0x100)
                        {
                            this.methodBodyHeap.Write((byte) 0x10);
                            this.methodBodyHeap.Write((byte) argumentListIndex);
                        }
                        else
                        {
                            this.methodBodyHeap.Write((byte) 0xfe);
                            this.methodBodyHeap.Write((byte) 11);
                            this.methodBodyHeap.Write((ushort) argumentListIndex);
                        }
                        this.stackHeight--;
                    }
                    return;
                }
                Local loc = (Local) target;
                this.Visit(assignment.Source);
                this.stackHeight--;
                int localVarIndex = this.GetLocalVarIndex(loc);
                switch (localVarIndex)
                {
                    case 0:
                        this.methodBodyHeap.Write((byte) 10);
                        return;

                    case 1:
                        this.methodBodyHeap.Write((byte) 11);
                        return;

                    case 2:
                        this.methodBodyHeap.Write((byte) 12);
                        return;

                    case 3:
                        this.methodBodyHeap.Write((byte) 13);
                        return;

                    default:
                        if (localVarIndex < 0x100)
                        {
                            this.methodBodyHeap.Write((byte) 0x13);
                            this.methodBodyHeap.Write((byte) localVarIndex);
                            return;
                        }
                        this.methodBodyHeap.Write((byte) 0xfe);
                        this.methodBodyHeap.Write((byte) 14);
                        this.methodBodyHeap.Write((ushort) localVarIndex);
                        return;
                }
            }
            if ((this.UseGenerics && (indexer.ElementType != null)) && (indexer.ElementType != SystemTypes.Object))
            {
                num4 = 0xa4;
            }
            else if (TypeNode.StripModifiers(indexer.ElementType) is Pointer)
            {
                num4 = 0x9b;
            }
            else
            {
                num4 = 0xa2;
            }
        Label_04B5:
            this.methodBodyHeap.Write(num4);
            if (num4 == 0xa4)
            {
                this.methodBodyHeap.Write(this.GetTypeToken(indexer.ElementType));
            }
            this.stackHeight -= 3;
        }

        private void VisitAttributeList(AttributeList attrs, Node node)
        {
            if (attrs != null)
            {
                int count = attrs.Count;
                if (count != 0)
                {
                    int num2 = count;
                    for (int i = 0; i < count; i++)
                    {
                        if (attrs[i] == null)
                        {
                            num2--;
                        }
                    }
                    if (num2 != 0)
                    {
                        count = num2;
                        int customAttributeParentCodedIndex = this.GetCustomAttributeParentCodedIndex(node);
                        this.customAttributeCount += count;
                        num2 = this.nodesWithCustomAttributes.Count;
                        this.nodesWithCustomAttributes.Add(node);
                        int num5 = 0;
                        NodeList nodesWithCustomAttributes = this.nodesWithCustomAttributes;
                        num5 = num2;
                        while (num5 > 0)
                        {
                            Node node3 = nodesWithCustomAttributes[num5 - 1];
                            int num6 = this.GetCustomAttributeParentCodedIndex(node3);
                            if (num6 < customAttributeParentCodedIndex)
                            {
                                break;
                            }
                            if (this.UseGenerics)
                            {
                            }
                            num5--;
                        }
                        if (num5 != num2)
                        {
                            for (int j = num2; j > num5; j--)
                            {
                                nodesWithCustomAttributes[j] = nodesWithCustomAttributes[j - 1];
                            }
                            nodesWithCustomAttributes[num5] = node;
                        }
                    }
                }
            }
        }

        private void VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            byte num = 0;
            this.Visit(binaryExpression.Operand1);
            switch (binaryExpression.NodeType)
            {
                case NodeType.Box:
                    num = 140;
                    break;

                case NodeType.Castclass:
                    num = 0x74;
                    break;

                case NodeType.Isinst:
                    num = 0x75;
                    break;

                case NodeType.Ldvirtftn:
                    num = 7;
                    this.methodBodyHeap.Write((byte) 0xfe);
                    this.methodBodyHeap.Write(num);
                    this.methodBodyHeap.Write(this.GetMethodToken((Method) ((MemberBinding) binaryExpression.Operand2).BoundMember));
                    return;

                case NodeType.Mkrefany:
                    num = 0xc6;
                    break;

                case NodeType.Refanyval:
                    num = 0xc2;
                    break;

                case NodeType.Unbox:
                    num = 0x79;
                    break;

                case NodeType.UnboxAny:
                    num = 0xa5;
                    break;

                default:
                    this.Visit(binaryExpression.Operand2);
                    switch (binaryExpression.NodeType)
                    {
                        case NodeType.Mul:
                            num = 90;
                            break;

                        case NodeType.Mul_Ovf:
                            num = 0xd8;
                            break;

                        case NodeType.Mul_Ovf_Un:
                            num = 0xd9;
                            break;

                        case NodeType.Or:
                            num = 0x60;
                            break;

                        case NodeType.Rem:
                            num = 0x5d;
                            break;

                        case NodeType.Rem_Un:
                            num = 0x5e;
                            break;

                        case NodeType.Shl:
                            num = 0x62;
                            break;

                        case NodeType.Shr:
                            num = 0x63;
                            break;

                        case NodeType.Shr_Un:
                            num = 100;
                            break;

                        case NodeType.Sub:
                            num = 0x59;
                            break;

                        case NodeType.Sub_Ovf:
                            num = 0xda;
                            break;

                        case NodeType.Sub_Ovf_Un:
                            num = 0xdb;
                            break;

                        case NodeType.Xor:
                            num = 0x61;
                            break;

                        case NodeType.Add:
                            num = 0x58;
                            break;

                        case NodeType.Add_Ovf:
                            num = 0xd6;
                            break;

                        case NodeType.Add_Ovf_Un:
                            num = 0xd7;
                            break;

                        case NodeType.And:
                            num = 0x5f;
                            break;

                        case NodeType.Ceq:
                            num = 1;
                            this.methodBodyHeap.Write((byte) 0xfe);
                            break;

                        case NodeType.Cgt:
                            num = 2;
                            this.methodBodyHeap.Write((byte) 0xfe);
                            break;

                        case NodeType.Cgt_Un:
                            num = 3;
                            this.methodBodyHeap.Write((byte) 0xfe);
                            break;

                        case NodeType.Clt:
                            num = 4;
                            this.methodBodyHeap.Write((byte) 0xfe);
                            break;

                        case NodeType.Clt_Un:
                            num = 5;
                            this.methodBodyHeap.Write((byte) 0xfe);
                            break;

                        case NodeType.Div:
                            num = 0x5b;
                            break;

                        case NodeType.Div_Un:
                            num = 0x5c;
                            break;
                    }
                    this.methodBodyHeap.Write(num);
                    this.stackHeight--;
                    return;
            }
            this.methodBodyHeap.Write(num);
            Literal literal = binaryExpression.Operand2 as Literal;
            if (literal != null)
            {
                this.methodBodyHeap.Write(this.GetTypeToken((TypeNode) literal.Value));
            }
            else
            {
                this.methodBodyHeap.Write(this.GetTypeToken((TypeNode) ((MemberBinding) binaryExpression.Operand2).BoundMember));
            }
        }

        private void VisitBlock(Block block)
        {
            MethodInfo methodInfo = this.methodInfo;
            int position = this.methodBodyHeap.BaseStream.Position;
            this.VisitFixupList(this.methodInfo.fixupIndex[block.UniqueKey] as Fixup, position);
            methodInfo.fixupIndex[block.UniqueKey] = position;
            this.methodBodyHeap.BaseStream.Position = position;
            int stackHeight = this.stackHeight;
            if (this.exceptionBlock[block.UniqueKey] != null)
            {
                this.stackHeight = 1;
            }
            StatementList statements = block.Statements;
            if (statements != null)
            {
                if ((this.symWriter != null) && block.HasLocals)
                {
                    LocalList debugLocals = methodInfo.debugLocals;
                    Int32List signatureLengths = methodInfo.signatureLengths;
                    Int32List signatureOffsets = methodInfo.signatureOffsets;
                    methodInfo.debugLocals = new LocalList();
                    methodInfo.signatureLengths = new Int32List();
                    methodInfo.signatureOffsets = new Int32List();
                    this.symWriter.OpenScope((uint) position);
                    int num3 = 0;
                    int count = statements.Count;
                    while (num3 < count)
                    {
                        this.Visit(statements[num3]);
                        num3++;
                    }
                    if (this.stackHeight > 0)
                    {
                        this.stackHeightExitTotal += this.stackHeight;
                    }
                    this.DefineLocalVariables(position, methodInfo.debugLocals);
                    methodInfo.debugLocals = debugLocals;
                    methodInfo.signatureLengths = signatureLengths;
                    methodInfo.signatureOffsets = signatureOffsets;
                }
                else
                {
                    int num5 = 0;
                    int num6 = statements.Count;
                    while (num5 < num6)
                    {
                        this.Visit(statements[num5]);
                        num5++;
                    }
                    if (this.stackHeight > stackHeight)
                    {
                        this.stackHeightExitTotal += this.stackHeight - stackHeight;
                    }
                }
                this.stackHeight = stackHeight;
            }
        }

        private void VisitBlockExpression(BlockExpression blockExpression)
        {
            if (blockExpression.Block != null)
            {
                this.VisitBlock(blockExpression.Block);
            }
        }

        private void VisitBranch(Branch branch)
        {
            int num;
            this.DefineSequencePoint(branch);
            BinaryExpression condition = branch.Condition as BinaryExpression;
            UnaryExpression expression2 = null;
            NodeType nop = NodeType.Nop;
            if (condition == null)
            {
                expression2 = branch.Condition as UnaryExpression;
                if ((expression2 != null) && (expression2.NodeType == NodeType.LogicalNot))
                {
                    this.Visit(expression2.Operand);
                    nop = NodeType.LogicalNot;
                    this.stackHeight--;
                }
                else if (branch.Condition != null)
                {
                    nop = NodeType.Undefined;
                    this.Visit(branch.Condition);
                    this.stackHeight--;
                }
                goto Label_0164;
            }
            NodeType nodeType = condition.NodeType;
            if (nodeType <= NodeType.Xor)
            {
                switch (nodeType)
                {
                    case NodeType.Isinst:
                    case NodeType.Or:
                    case NodeType.Xor:
                    case NodeType.And:
                    case NodeType.Castclass:
                        nop = condition.NodeType;
                        break;
                }
                goto Label_00DD;
            }
            if (nodeType <= NodeType.Gt)
            {
                switch (nodeType)
                {
                    case NodeType.Ge:
                    case NodeType.Gt:
                    case NodeType.Eq:
                        goto Label_00A4;
                }
                goto Label_00DD;
            }
            if (((nodeType != NodeType.Le) && (nodeType != NodeType.Lt)) && (nodeType != NodeType.Ne))
            {
                goto Label_00DD;
            }
        Label_00A4:
            this.Visit(condition.Operand1);
            this.Visit(condition.Operand2);
            nop = condition.NodeType;
            this.stackHeight -= 2;
            goto Label_0164;
        Label_00DD:
            this.Visit(branch.Condition);
            this.stackHeight--;
        Label_0164:
            num = this.GetOffset(branch.Target, ref branch.shortOffset);
            if (!branch.ShortOffset)
            {
                switch (nop)
                {
                    case NodeType.Nop:
                        if (branch.Condition != null)
                        {
                            this.methodBodyHeap.Write((byte) 0x3a);
                            break;
                        }
                        if (!branch.LeavesExceptionBlock)
                        {
                            this.methodBodyHeap.Write((byte) 0x38);
                            break;
                        }
                        this.methodBodyHeap.Write((byte) 0xdd);
                        break;

                    case NodeType.Or:
                    case NodeType.Isinst:
                    case NodeType.Castclass:
                    case NodeType.Undefined:
                    case NodeType.And:
                    case NodeType.Xor:
                        this.methodBodyHeap.Write((byte) 0x3a);
                        break;

                    case NodeType.Eq:
                        this.methodBodyHeap.Write((byte) 0x3b);
                        break;

                    case NodeType.Ge:
                        if (!branch.BranchIfUnordered)
                        {
                            this.methodBodyHeap.Write((byte) 60);
                            break;
                        }
                        this.methodBodyHeap.Write((byte) 0x41);
                        break;

                    case NodeType.Gt:
                        if (!branch.BranchIfUnordered)
                        {
                            this.methodBodyHeap.Write((byte) 0x3d);
                            break;
                        }
                        this.methodBodyHeap.Write((byte) 0x42);
                        break;

                    case NodeType.Le:
                        if (!branch.BranchIfUnordered)
                        {
                            this.methodBodyHeap.Write((byte) 0x3e);
                            break;
                        }
                        this.methodBodyHeap.Write((byte) 0x43);
                        break;

                    case NodeType.LogicalNot:
                        this.methodBodyHeap.Write((byte) 0x39);
                        break;

                    case NodeType.Lt:
                        if (!branch.BranchIfUnordered)
                        {
                            this.methodBodyHeap.Write((byte) 0x3f);
                            break;
                        }
                        this.methodBodyHeap.Write((byte) 0x44);
                        break;

                    case NodeType.Ne:
                        this.methodBodyHeap.Write((byte) 0x40);
                        break;
                }
            }
            else
            {
                switch (nop)
                {
                    case NodeType.Nop:
                        if (branch.Condition != null)
                        {
                            this.methodBodyHeap.Write((byte) 0x2d);
                            break;
                        }
                        if (!branch.LeavesExceptionBlock)
                        {
                            this.methodBodyHeap.Write((byte) 0x2b);
                            break;
                        }
                        this.methodBodyHeap.Write((byte) 0xde);
                        break;

                    case NodeType.Or:
                    case NodeType.Isinst:
                    case NodeType.Castclass:
                    case NodeType.Undefined:
                    case NodeType.And:
                    case NodeType.Xor:
                        this.methodBodyHeap.Write((byte) 0x2d);
                        break;

                    case NodeType.Eq:
                        this.methodBodyHeap.Write((byte) 0x2e);
                        break;

                    case NodeType.Ge:
                        if (!branch.BranchIfUnordered)
                        {
                            this.methodBodyHeap.Write((byte) 0x2f);
                            break;
                        }
                        this.methodBodyHeap.Write((byte) 0x34);
                        break;

                    case NodeType.Gt:
                        if (!branch.BranchIfUnordered)
                        {
                            this.methodBodyHeap.Write((byte) 0x30);
                            break;
                        }
                        this.methodBodyHeap.Write((byte) 0x35);
                        break;

                    case NodeType.Le:
                        if (!branch.BranchIfUnordered)
                        {
                            this.methodBodyHeap.Write((byte) 0x31);
                            break;
                        }
                        this.methodBodyHeap.Write((byte) 0x36);
                        break;

                    case NodeType.LogicalNot:
                        this.methodBodyHeap.Write((byte) 0x2c);
                        break;

                    case NodeType.Lt:
                        if (!branch.BranchIfUnordered)
                        {
                            this.methodBodyHeap.Write((byte) 50);
                            break;
                        }
                        this.methodBodyHeap.Write((byte) 0x37);
                        break;

                    case NodeType.Ne:
                        this.methodBodyHeap.Write((byte) 0x33);
                        break;
                }
                this.methodBodyHeap.Write((sbyte) num);
                return;
            }
            this.methodBodyHeap.Write(num);
        }

        private void VisitClass(Class Class)
        {
            if ((!this.UseGenerics || (Class.Template == null)) || !Class.Template.IsGeneric)
            {
                this.VisitAttributeList(Class.Attributes, Class);
                this.VisitSecurityAttributeList(Class.SecurityAttributes, Class);
                if (Class.BaseClass != null)
                {
                    this.VisitReferencedType(Class.BaseClass);
                }
                int num = 0;
                int num2 = (Class.Interfaces == null) ? 0 : Class.Interfaces.Count;
                while (num < num2)
                {
                    this.GetTypeDefOrRefOrSpecEncoded(Class.Interfaces[num]);
                    if (Class.Interfaces[num] != null)
                    {
                        this.interfaceEntries.Add(Class);
                    }
                    num++;
                }
                if ((Class.NodeType == NodeType.ClassParameter) && !(Class is MethodClassParameter))
                {
                    this.interfaceEntries.Add(Class);
                }
                int num3 = 0;
                int count = Class.Members.Count;
                while (num3 < count)
                {
                    Member node = Class.Members[num3];
                    if ((node != null) && !(node is TypeNode))
                    {
                        this.Visit(node);
                    }
                    num3++;
                }
                if (((Class.Flags & TypeFlags.LayoutMask) != TypeFlags.AnsiClass) && ((Class.PackingSize != 0) || (Class.ClassSize != 0)))
                {
                    this.classLayoutEntries.Add(Class);
                }
            }
        }

        private void VisitConstruct(Construct cons)
        {
            int num = -1;
            ExpressionList operands = cons.Operands;
            if (operands != null)
            {
                this.VisitExpressionList(cons.Operands);
                num = operands.Count - 1;
            }
            if (num >= 0)
            {
                this.stackHeight -= num;
            }
            else
            {
                this.IncrementStackHeight();
            }
            this.methodBodyHeap.Write((byte) 0x73);
            Method boundMember = ((MemberBinding) cons.Constructor).BoundMember as Method;
            if (boundMember != null)
            {
                this.methodBodyHeap.Write(this.GetMethodToken(boundMember));
            }
        }

        private void VisitConstructArray(ConstructArray consArr)
        {
            if (((consArr != null) && (consArr.Operands != null)) && (consArr.Operands.Count >= 1))
            {
                this.Visit(consArr.Operands[0]);
                this.methodBodyHeap.Write((byte) 0x8d);
                this.methodBodyHeap.Write(this.GetTypeToken(consArr.ElementType));
            }
        }

        private void VisitDelegateNode(DelegateNode delegateNode)
        {
            if ((!this.UseGenerics || (delegateNode.Template == null)) || !delegateNode.Template.IsGeneric)
            {
                this.VisitAttributeList(delegateNode.Attributes, delegateNode);
                this.VisitSecurityAttributeList(delegateNode.SecurityAttributes, delegateNode);
                this.VisitReferencedType(CoreSystemTypes.MulticastDelegate);
                int num = 0;
                int num2 = (delegateNode.Interfaces == null) ? 0 : delegateNode.Interfaces.Count;
                while (num < num2)
                {
                    this.GetTypeDefOrRefOrSpecEncoded(delegateNode.Interfaces[num]);
                    if (delegateNode.Interfaces[num] != null)
                    {
                        this.interfaceEntries.Add(delegateNode);
                    }
                    num++;
                }
                int num3 = 0;
                int count = delegateNode.Members.Count;
                while (num3 < count)
                {
                    Member node = delegateNode.Members[num3];
                    if ((node != null) && !(node is TypeNode))
                    {
                        this.Visit(node);
                    }
                    num3++;
                }
            }
        }

        private void VisitEndFilter(EndFilter endFilter)
        {
            this.DefineSequencePoint(endFilter);
            this.Visit(endFilter.Value);
            this.methodBodyHeap.Write((byte) 0xfe);
            this.methodBodyHeap.Write((byte) 0x11);
            this.stackHeight--;
        }

        private void VisitEnumNode(EnumNode enumNode)
        {
            this.VisitAttributeList(enumNode.Attributes, enumNode);
            this.VisitSecurityAttributeList(enumNode.SecurityAttributes, enumNode);
            this.VisitReferencedType(CoreSystemTypes.Enum);
            int num = 0;
            int num2 = (enumNode.Interfaces == null) ? 0 : enumNode.Interfaces.Count;
            while (num < num2)
            {
                this.GetTypeDefOrRefOrSpecEncoded(enumNode.Interfaces[num]);
                if (enumNode.Interfaces[num] != null)
                {
                    this.interfaceEntries.Add(enumNode);
                }
                num++;
            }
            int num3 = 0;
            int count = enumNode.Members.Count;
            while (num3 < count)
            {
                this.Visit(enumNode.Members[num3]);
                num3++;
            }
        }

        private void VisitEvent(Event Event)
        {
            if (this.eventIndex[Event.UniqueKey] == null)
            {
                int num = this.eventEntries.Count + 1;
                this.eventEntries.Add(Event);
                this.eventIndex[Event.UniqueKey] = num;
                if (this.eventMapIndex[Event.DeclaringType.UniqueKey] == null)
                {
                    this.eventMapEntries.Add(Event);
                    this.eventMapIndex[Event.DeclaringType.UniqueKey] = this.eventMapEntries.Count;
                }
                if (Event.HandlerAdder != null)
                {
                    this.methodSemanticsEntries.Add(Event);
                }
                if (Event.HandlerRemover != null)
                {
                    this.methodSemanticsEntries.Add(Event);
                }
                if (Event.HandlerCaller != null)
                {
                    this.methodSemanticsEntries.Add(Event);
                }
                if (Event.OtherMethods != null)
                {
                    int num2 = 0;
                    int count = Event.OtherMethods.Count;
                    while (num2 < count)
                    {
                        this.methodSemanticsEntries.Add(Event);
                        num2++;
                    }
                }
                this.VisitAttributeList(Event.Attributes, Event);
            }
        }

        private void VisitExpression(Expression expression)
        {
            NodeType nodeType = expression.NodeType;
            if (nodeType != NodeType.Arglist)
            {
                if (nodeType != NodeType.Dup)
                {
                    if (nodeType != NodeType.Pop)
                    {
                        return;
                    }
                }
                else
                {
                    this.methodBodyHeap.Write((byte) 0x25);
                    this.IncrementStackHeight();
                    return;
                }
                UnaryExpression expression2 = expression as UnaryExpression;
                if (expression2 != null)
                {
                    this.Visit(expression2.Operand);
                    this.stackHeight--;
                    this.methodBodyHeap.Write((byte) 0x26);
                }
            }
            else
            {
                this.IncrementStackHeight();
                this.methodBodyHeap.Write((byte) 0xfe);
                this.methodBodyHeap.Write((byte) 0);
            }
        }

        private void VisitExpressionList(ExpressionList expressions)
        {
            if (expressions != null)
            {
                int num = 0;
                int count = expressions.Count;
                while (num < count)
                {
                    this.Visit(expressions[num]);
                    num++;
                }
            }
        }

        private void VisitExpressionStatement(ExpressionStatement statement)
        {
            if (!(statement.Expression is BlockExpression))
            {
                this.DefineSequencePoint(statement);
            }
            this.Visit(statement.Expression);
        }

        private void VisitField(Field field)
        {
            this.VisitAttributeList(field.Attributes, field);
            this.GetFieldIndex(field);
            if (field.IsVolatile)
            {
                field.Type = RequiredModifier.For(CoreSystemTypes.IsVolatile, field.Type);
            }
            this.VisitReferencedType(field.Type);
        }

        private void VisitFixupList(Fixup fixup, int targetAddress)
        {
            while (fixup != null)
            {
                this.methodBodyHeap.BaseStream.Position = fixup.fixupLocation;
                if (fixup.shortOffset)
                {
                    int num = targetAddress - fixup.addressOfNextInstruction;
                    this.methodBodyHeap.Write((byte) num);
                }
                else
                {
                    this.methodBodyHeap.Write((int) (targetAddress - fixup.addressOfNextInstruction));
                }
                fixup = fixup.nextFixUp;
            }
        }

        private void VisitGenericParameterList(Member member, TypeNodeList parameters)
        {
            if (((member != null) && (parameters != null)) && this.UseGenerics)
            {
                int num = (member is Method) ? -1 : 1;
                int num2 = 0;
                int count = parameters.Count;
                while (num2 < count)
                {
                    TypeNode element = parameters[num2];
                    if (element != null)
                    {
                        this.typeParameterNumber[element.UniqueKey] = num * (num2 + 1);
                        this.genericParamEntries.Add(member);
                        if (((ITypeParameter) element).DeclaringMember != member)
                        {
                            element = (TypeNode) element.Clone();
                        }
                        this.genericParameters.Add(element);
                        if ((element.BaseType is Class) && (element.BaseType != CoreSystemTypes.Object))
                        {
                            this.genericParamConstraintEntries.Add(element);
                        }
                        int num4 = 0;
                        int num5 = (element.Interfaces == null) ? 0 : element.Interfaces.Count;
                        while (num4 < num5)
                        {
                            this.genericParamConstraintEntries.Add(element);
                            num4++;
                        }
                    }
                    num2++;
                }
            }
        }

        private void VisitIndexer(Indexer indexer)
        {
            this.Visit(indexer.Object);
            if ((indexer.Operands != null) && (indexer.Operands.Count >= 1))
            {
                byte num;
                this.Visit(indexer.Operands[0]);
                switch (indexer.ElementType.typeCode)
                {
                    case ElementType.Boolean:
                    case ElementType.Int8:
                        num = 0x90;
                        break;

                    case ElementType.Char:
                    case ElementType.UInt16:
                        num = 0x93;
                        break;

                    case ElementType.UInt8:
                        num = 0x91;
                        break;

                    case ElementType.Int16:
                        num = 0x92;
                        break;

                    case ElementType.Int32:
                        num = 0x94;
                        break;

                    case ElementType.UInt32:
                        num = 0x95;
                        break;

                    case ElementType.Int64:
                    case ElementType.UInt64:
                        num = 150;
                        break;

                    case ElementType.Single:
                        num = 0x98;
                        break;

                    case ElementType.Double:
                        num = 0x99;
                        break;

                    case ElementType.IntPtr:
                    case ElementType.UIntPtr:
                        num = 0x97;
                        break;

                    default:
                        if ((this.UseGenerics && (indexer.ElementType != null)) && (indexer.ElementType != SystemTypes.Object))
                        {
                            num = 0xa3;
                        }
                        else if (TypeNode.StripModifiers(indexer.ElementType) is Pointer)
                        {
                            num = 0x97;
                        }
                        else
                        {
                            num = 0x9a;
                        }
                        break;
                }
                this.methodBodyHeap.Write(num);
                if (num == 0xa3)
                {
                    this.methodBodyHeap.Write(this.GetTypeToken(indexer.ElementType));
                }
                this.stackHeight--;
            }
        }

        private void VisitInterface(Interface Interface)
        {
            if ((!this.UseGenerics || (Interface.Template == null)) || !Interface.Template.IsGeneric)
            {
                this.VisitAttributeList(Interface.Attributes, Interface);
                this.VisitSecurityAttributeList(Interface.SecurityAttributes, Interface);
                InterfaceList interfaces = Interface.Interfaces;
                int num = 0;
                int num2 = (interfaces == null) ? 0 : interfaces.Count;
                while (num < num2)
                {
                    this.GetTypeDefOrRefOrSpecEncoded(interfaces[num]);
                    if (interfaces[num] != null)
                    {
                        this.interfaceEntries.Add(Interface);
                    }
                    num++;
                }
                if ((Interface.NodeType == NodeType.TypeParameter) && !(Interface is MethodTypeParameter))
                {
                    this.interfaceEntries.Add(Interface);
                }
                int num3 = 0;
                int count = Interface.Members.Count;
                while (num3 < count)
                {
                    Member node = Interface.Members[num3];
                    if ((node != null) && !(node is TypeNode))
                    {
                        this.Visit(node);
                    }
                    num3++;
                }
            }
        }

        private void VisitLiteral(Literal literal)
        {
            this.IncrementStackHeight();
            IConvertible convertible = literal.Value as IConvertible;
            if (convertible == null)
            {
                this.methodBodyHeap.Write((byte) 20);
                return;
            }
            TypeCode typeCode = convertible.GetTypeCode();
            switch (typeCode)
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                {
                    long num = convertible.ToInt64(null);
                    long num2 = num;
                    if ((num2 <= 8L) && (num2 >= -1L))
                    {
                        switch (((int) (num2 - -1L)))
                        {
                            case 0:
                                this.methodBodyHeap.Write((byte) 0x15);
                                goto Label_0244;

                            case 1:
                                this.methodBodyHeap.Write((byte) 0x16);
                                goto Label_0244;

                            case 2:
                                this.methodBodyHeap.Write((byte) 0x17);
                                goto Label_0244;

                            case 3:
                                this.methodBodyHeap.Write((byte) 0x18);
                                goto Label_0244;

                            case 4:
                                this.methodBodyHeap.Write((byte) 0x19);
                                goto Label_0244;

                            case 5:
                                this.methodBodyHeap.Write((byte) 0x1a);
                                goto Label_0244;

                            case 6:
                                this.methodBodyHeap.Write((byte) 0x1b);
                                goto Label_0244;

                            case 7:
                                this.methodBodyHeap.Write((byte) 0x1c);
                                goto Label_0244;

                            case 8:
                                this.methodBodyHeap.Write((byte) 0x1d);
                                goto Label_0244;

                            case 9:
                                this.methodBodyHeap.Write((byte) 30);
                                goto Label_0244;
                        }
                    }
                    if ((num >= -128L) && (num <= 0x7fL))
                    {
                        this.methodBodyHeap.Write((byte) 0x1f);
                        this.methodBodyHeap.Write((byte) num);
                    }
                    else if (((num >= -2147483648L) && (num <= 0x7fffffffL)) || ((num <= 0xffffffffL) && (((typeCode == TypeCode.Char) || (typeCode == TypeCode.UInt16)) || (typeCode == TypeCode.UInt32))))
                    {
                        if ((num == 0xffffffffL) && (typeCode != TypeCode.Int64))
                        {
                            this.methodBodyHeap.Write((byte) 0x15);
                        }
                        else
                        {
                            this.methodBodyHeap.Write((byte) 0x20);
                            this.methodBodyHeap.Write((int) num);
                        }
                    }
                    else
                    {
                        this.methodBodyHeap.Write((byte) 0x21);
                        this.methodBodyHeap.Write(num);
                        typeCode = TypeCode.Empty;
                    }
                    break;
                }
                case TypeCode.UInt64:
                    this.methodBodyHeap.Write((byte) 0x21);
                    this.methodBodyHeap.Write(convertible.ToUInt64(null));
                    return;

                case TypeCode.Single:
                    this.methodBodyHeap.Write((byte) 0x22);
                    this.methodBodyHeap.Write(convertible.ToSingle(null));
                    return;

                case TypeCode.Double:
                    this.methodBodyHeap.Write((byte) 0x23);
                    this.methodBodyHeap.Write(convertible.ToDouble(null));
                    return;

                case TypeCode.Decimal:
                case TypeCode.DateTime:
                case (TypeCode.DateTime | TypeCode.Object):
                    return;

                case TypeCode.String:
                    this.methodBodyHeap.Write((byte) 0x72);
                    this.methodBodyHeap.Write((int) (this.GetUserStringIndex((string) literal.Value) | 0x70000000));
                    return;

                default:
                    return;
            }
        Label_0244:
            if (typeCode == TypeCode.Int64)
            {
                this.methodBodyHeap.Write((byte) 0x6a);
            }
        }

        private void VisitLocal(Local local)
        {
            this.IncrementStackHeight();
            int localVarIndex = this.GetLocalVarIndex(local);
            switch (localVarIndex)
            {
                case 0:
                    this.methodBodyHeap.Write((byte) 6);
                    return;

                case 1:
                    this.methodBodyHeap.Write((byte) 7);
                    return;

                case 2:
                    this.methodBodyHeap.Write((byte) 8);
                    return;

                case 3:
                    this.methodBodyHeap.Write((byte) 9);
                    return;
            }
            if (localVarIndex < 0x100)
            {
                this.methodBodyHeap.Write((byte) 0x11);
                this.methodBodyHeap.Write((byte) localVarIndex);
            }
            else
            {
                this.methodBodyHeap.Write((byte) 0xfe);
                this.methodBodyHeap.Write((byte) 12);
                this.methodBodyHeap.Write((ushort) localVarIndex);
            }
        }

        private void VisitMemberBinding(MemberBinding memberBinding)
        {
            if (memberBinding.TargetObject != null)
            {
                this.Visit(memberBinding.TargetObject);
                if (memberBinding.Volatile)
                {
                    this.methodBodyHeap.Write((byte) 0xfe);
                    this.methodBodyHeap.Write((byte) 0x13);
                }
                this.methodBodyHeap.Write((byte) 0x7b);
            }
            else
            {
                this.IncrementStackHeight();
                if (memberBinding.Volatile)
                {
                    this.methodBodyHeap.Write((byte) 0xfe);
                    this.methodBodyHeap.Write((byte) 0x13);
                }
                this.methodBodyHeap.Write((byte) 0x7e);
            }
            this.methodBodyHeap.Write(this.GetFieldToken((Field) memberBinding.BoundMember));
        }

        private void VisitMethod(Method method)
        {
            if ((!this.UseGenerics || (method.Template == null)) || !method.Template.IsGeneric)
            {
                this.GetMethodIndex(method);
                this.VisitAttributeList(method.Attributes, method);
                this.VisitSecurityAttributeList(method.SecurityAttributes, method);
                int num = 0;
                int num2 = (method.Parameters == null) ? 0 : method.Parameters.Count;
                while (num < num2)
                {
                    Parameter node = method.Parameters[num];
                    if (node != null)
                    {
                        this.VisitAttributeList(node.Attributes, node);
                        this.VisitReferencedType(node.Type);
                    }
                    num++;
                }
                if (method.ReturnType != null)
                {
                    this.VisitReferencedType(method.ReturnType);
                }
                if ((!method.IsAbstract && (method.Body != null)) && ((method.Body.Statements != null) && (method.Body.Statements.Count > 0)))
                {
                    this.VisitMethodBody(method);
                }
                MethodList implementedInterfaceMethods = method.ImplementedInterfaceMethods;
                int num3 = 0;
                int num4 = (implementedInterfaceMethods == null) ? 0 : implementedInterfaceMethods.Count;
                while (num3 < num4)
                {
                    if (implementedInterfaceMethods[num3] != null)
                    {
                        this.methodImplEntries.Add(method);
                    }
                    num3++;
                }
                if ((((method.Flags & MethodFlags.PInvokeImpl) != MethodFlags.CompilerControlled) && (method.PInvokeImportName != null)) && (method.PInvokeModule != null))
                {
                    this.implMapEntries.Add(method);
                    this.GetStringIndex(method.PInvokeImportName);
                    this.GetModuleRefIndex(method.PInvokeModule);
                }
            }
        }

        private void VisitMethodBody(Method method)
        {
            this.methodBodyHeap = new System.Compiler.BinaryWriter(new System.Compiler.MemoryStream());
            this.methodInfo = new MethodInfo();
            this.stackHeightMax = 0;
            this.stackHeightExitTotal = 0;
            if (this.symWriter != null)
            {
                this.methodInfo.debugLocals = new LocalList();
                this.methodInfo.signatureLengths = new Int32List();
                this.methodInfo.signatureOffsets = new Int32List();
                this.methodInfo.statementNodes = new NodeList();
                this.methodInfo.statementOffsets = new Int32List();
                this.symWriter.OpenMethod((uint) this.GetMethodDefToken(method));
                this.symWriter.OpenScope(0);
            }
            int startAddress = 0;
            if (method.LocalList != null)
            {
                int num2 = 0;
                int count = method.LocalList.Count;
                while (num2 < count)
                {
                    Local loc = method.LocalList[num2];
                    if (loc != null)
                    {
                        this.GetLocalVarIndex(loc);
                    }
                    num2++;
                }
                if (this.symWriter != null)
                {
                    int num4 = this.methodBodyHeap.BaseStream.Position;
                    startAddress = num4;
                    this.symWriter.OpenScope((uint) num4);
                }
            }
            int num5 = (method.ExceptionHandlers == null) ? 0 : method.ExceptionHandlers.Count;
            if (num5 > 0)
            {
                this.exceptionBlock = new TrivialHashtable();
                for (int i = 0; i < num5; i++)
                {
                    ExceptionHandler handler = method.ExceptionHandlers[i];
                    if (((handler != null) && (handler.HandlerStartBlock != null)) && ((handler.HandlerType == NodeType.Catch) || (handler.HandlerType == NodeType.Filter)))
                    {
                        this.exceptionBlock[handler.HandlerStartBlock.UniqueKey] = handler;
                    }
                }
            }
            this.VisitBlock(method.Body);
            if ((method.LocalList != null) && (this.symWriter != null))
            {
                this.DefineLocalVariables(startAddress, method.LocalList);
            }
            this.methodBodiesHeapIndex[method.UniqueKey] = this.methodBodiesHeap.BaseStream.Position;
            int num7 = this.stackHeightExitTotal + this.stackHeightMax;
            if ((num5 > 0) && (num7 == 0))
            {
                num7 = 1;
            }
            int position = this.methodBodyHeap.BaseStream.Position;
            int localVarSigTok = this.methodInfo.localVarSigTok;
            bool flag = (((position >= 0x40) || (num5 > 0)) || (num7 > 8)) || (localVarSigTok != 0);
            if (flag)
            {
                byte num10 = 3;
                if (method.InitLocals)
                {
                    num10 = (byte) (num10 | 0x10);
                }
                if (num5 > 0)
                {
                    num10 = (byte) (num10 | 8);
                }
                this.methodBodiesHeap.Write(num10);
                this.methodBodiesHeap.Write((byte) 0x30);
                this.methodBodiesHeap.Write((short) num7);
                this.methodBodiesHeap.Write(position);
                if (localVarSigTok != 0)
                {
                    if (this.methodInfo.localVarIndex.Count > 0x7f)
                    {
                        this.methodInfo.localVarSignature.Write((byte) 0);
                        byte[] buffer = this.methodInfo.localVarSignature.BaseStream.Buffer;
                        int length = buffer.Length;
                        for (int j = length - 2; j > 1; j--)
                        {
                            buffer[j + 1] = buffer[j];
                        }
                    }
                    this.methodInfo.localVarSignature.BaseStream.Position = 0;
                    this.methodInfo.localVarSignature.Write((byte) 7);
                    WriteCompressedInt(this.methodInfo.localVarSignature, this.methodInfo.localVarIndex.Count);
                }
                this.methodBodiesHeap.Write(localVarSigTok);
            }
            else
            {
                this.methodBodiesHeap.Write((byte) ((position << 2) | 2));
            }
            this.methodBodyHeap.BaseStream.WriteTo(this.methodBodiesHeap.BaseStream);
            int num13 = this.methodBodiesHeap.BaseStream.Position;
            while ((num13 % 4) != 0)
            {
                num13++;
                this.methodBodiesHeap.Write((byte) 0);
            }
            if (flag)
            {
                int[] numArray = new int[num5];
                int[] numArray2 = new int[num5];
                int[] numArray3 = new int[num5];
                int[] numArray4 = new int[num5];
                bool flag2 = false;
                for (int k = 0; k < num5; k++)
                {
                    ExceptionHandler handler2 = method.ExceptionHandlers[k];
                    int num16 = numArray[k] = (int) this.methodInfo.fixupIndex[handler2.TryStartBlock.UniqueKey];
                    int num18 = numArray2[k] = ((int) this.methodInfo.fixupIndex[handler2.BlockAfterTryEnd.UniqueKey]) - num16;
                    int num20 = numArray3[k] = (int) this.methodInfo.fixupIndex[handler2.HandlerStartBlock.UniqueKey];
                    int num22 = numArray4[k] = ((int) this.methodInfo.fixupIndex[handler2.BlockAfterHandlerEnd.UniqueKey]) - num20;
                    if (((num16 > 0xffff) || (num18 > 0xff)) || ((num20 > 0xffff) || (num22 > 0xff)))
                    {
                        flag2 = true;
                    }
                }
                if (((num5 * 12) + 4) > 0xff)
                {
                    flag2 = true;
                }
                if (flag2)
                {
                    int num23 = (num5 * 0x18) + 4;
                    this.methodBodiesHeap.Write((byte) 0x41);
                    this.methodBodiesHeap.Write((byte) (num23 & 0xff));
                    this.methodBodiesHeap.Write((short) ((num23 >> 8) & 0xffff));
                }
                else
                {
                    int num24 = (num5 * 12) + 4;
                    this.methodBodiesHeap.Write((byte) 1);
                    this.methodBodiesHeap.Write((byte) num24);
                    this.methodBodiesHeap.Write((short) 0);
                }
                for (int m = 0; m < num5; m++)
                {
                    ExceptionHandler handler3 = method.ExceptionHandlers[m];
                    byte num26 = 0;
                    switch (handler3.HandlerType)
                    {
                        case NodeType.FaultHandler:
                            num26 = 4;
                            break;

                        case NodeType.Filter:
                            num26 = 1;
                            break;

                        case NodeType.Finally:
                            num26 = 2;
                            break;
                    }
                    if (flag2)
                    {
                        this.methodBodiesHeap.Write((int) num26);
                        this.methodBodiesHeap.Write(numArray[m]);
                        this.methodBodiesHeap.Write(numArray2[m]);
                        this.methodBodiesHeap.Write(numArray3[m]);
                        this.methodBodiesHeap.Write(numArray4[m]);
                    }
                    else
                    {
                        this.methodBodiesHeap.Write((short) num26);
                        this.methodBodiesHeap.Write((ushort) numArray[m]);
                        this.methodBodiesHeap.Write((byte) numArray2[m]);
                        this.methodBodiesHeap.Write((ushort) numArray3[m]);
                        this.methodBodiesHeap.Write((byte) numArray4[m]);
                    }
                    if (handler3.FilterType != null)
                    {
                        this.methodBodiesHeap.Write(this.GetTypeToken(handler3.FilterType));
                    }
                    else if (handler3.FilterExpression != null)
                    {
                        this.methodBodiesHeap.Write((int) this.methodInfo.fixupIndex[handler3.FilterExpression.UniqueKey]);
                    }
                    else
                    {
                        this.methodBodiesHeap.Write(0);
                    }
                }
            }
            if (this.symWriter != null)
            {
                MethodInfo methodInfo = this.methodInfo;
                NodeList statementNodes = methodInfo.statementNodes;
                Int32List statementOffsets = methodInfo.statementOffsets;
                if (statementNodes.Count == 0)
                {
                    statementNodes.Add(new Statement(NodeType.Nop, new SourceContext(HiddenDocument.Document)));
                }
                int num27 = statementNodes.Count;
                int start = 0;
                int num29 = 0;
                Document doc = null;
                ISymUnmanagedDocumentWriter documentWriter = null;
                for (int n = 0; n < num27; n++)
                {
                    Document document = statementNodes[n].SourceContext.Document;
                    if (document != null)
                    {
                        if (document != doc)
                        {
                            doc = document;
                            if (documentWriter != null)
                            {
                                this.DefineSequencePoints(statementNodes, statementOffsets, start, num29, documentWriter);
                            }
                            documentWriter = this.GetDocumentWriter(doc);
                            start = n;
                            num29 = 0;
                        }
                        num29++;
                    }
                }
                this.DefineSequencePoints(statementNodes, statementOffsets, start, num29, documentWriter);
                this.symWriter.CloseScope((uint) this.methodBodyHeap.BaseStream.Position);
                this.symWriter.CloseMethod();
            }
        }

        private void VisitMethodCall(MethodCall call)
        {
            MemberBinding callee = (MemberBinding) call.Callee;
            TypeNode constraint = call.Constraint;
            this.Visit(callee.TargetObject);
            ExpressionList operands = call.Operands;
            int count = 0;
            if (operands != null)
            {
                this.VisitExpressionList(operands);
                count = operands.Count;
            }
            if (call.Type != CoreSystemTypes.Void)
            {
                this.VisitReferencedType(call.Type);
                count--;
            }
            if (count >= 0)
            {
                this.stackHeight -= count;
            }
            else
            {
                this.IncrementStackHeight();
            }
            if (call.IsTailCall)
            {
                this.methodBodyHeap.Write((byte) 0xfe);
                this.methodBodyHeap.Write((byte) 20);
            }
            else if (constraint != null)
            {
                this.methodBodyHeap.Write((byte) 0xfe);
                this.methodBodyHeap.Write((byte) 0x16);
                this.methodBodyHeap.Write(this.GetTypeToken(constraint));
            }
            switch (call.NodeType)
            {
                case NodeType.Calli:
                {
                    this.methodBodyHeap.Write((byte) 0x29);
                    System.Compiler.BinaryWriter target = new System.Compiler.BinaryWriter(new System.Compiler.MemoryStream());
                    this.WriteMethodSignature(target, (FunctionPointer) callee.BoundMember);
                    this.methodBodyHeap.Write((int) (0x11000000 | this.GetStandAloneSignatureIndex(target)));
                    return;
                }
                case NodeType.Callvirt:
                    this.methodBodyHeap.Write((byte) 0x6f);
                    break;

                case NodeType.Jmp:
                    this.methodBodyHeap.Write((byte) 0x27);
                    break;

                default:
                    this.methodBodyHeap.Write((byte) 40);
                    break;
            }
            Method boundMember = (Method) callee.BoundMember;
            if (((boundMember.CallingConvention & CallingConventionFlags.ArgumentConvention) == CallingConventionFlags.VarArg) || ((boundMember.CallingConvention & CallingConventionFlags.ArgumentConvention) == CallingConventionFlags.C))
            {
                this.methodBodyHeap.Write(this.GetMemberRefToken(boundMember, operands));
            }
            else
            {
                this.methodBodyHeap.Write(this.GetMethodToken(boundMember));
            }
        }

        private void VisitModule(Module module)
        {
            this.VisitAttributeList(module.Attributes, module);
            if (this.assembly != null)
            {
                Module module2 = new Module {
                    Attributes = this.assembly.ModuleAttributes
                };
                this.VisitAttributeList(module2.Attributes, module2);
                this.VisitSecurityAttributeList(this.assembly.SecurityAttributes, this.assembly);
            }
            TypeNodeList list = module.Types.Clone();
            int num = 0;
            while (num < list.Count)
            {
                TypeNode node;
                int count = module.Types.Count;
                int num3 = num;
                int num4 = num;
                int num5 = list.Count;
                goto Label_0158;
            Label_0085:
                node = list[num3];
                if (node != null)
                {
                    if ((this.UseGenerics && (node.Template != null)) && node.Template.IsGeneric)
                    {
                        list[num3] = null;
                    }
                    else
                    {
                        this.GetTypeDefIndex(node);
                        if (num3 >= num5)
                        {
                            this.nestedClassEntries.Add(node);
                        }
                        MemberList members = node.Members;
                        if (members != null)
                        {
                            int num6 = 0;
                            int num7 = members.Count;
                            while (num6 < num7)
                            {
                                TypeNode element = members[num6] as TypeNode;
                                if (element != null)
                                {
                                    list.Add(element);
                                }
                                num6++;
                            }
                        }
                    }
                }
                num3++;
            Label_014F:
                if (num3 < num4)
                {
                    goto Label_0085;
                }
            Label_0158:
                if (num3 < (num4 = list.Count))
                {
                    goto Label_014F;
                }
                int num8 = num;
                int num9 = list.Count;
                while (num8 < num9)
                {
                    TypeNode node3 = list[num8];
                    if (node3 != null)
                    {
                        if ((this.UseGenerics && (node3.Template != null)) && node3.Template.IsGeneric)
                        {
                            list[num8] = null;
                        }
                        else
                        {
                            MemberList list3 = node3.Members;
                            if (node3 is EnumNode)
                            {
                                int num10 = 0;
                                int num11 = list3.Count;
                                while (num10 < num11)
                                {
                                    Field field = list3[num10] as Field;
                                    if ((field != null) && !field.IsStatic)
                                    {
                                        list3[num10] = list3[0];
                                        list3[0] = field;
                                        break;
                                    }
                                    num10++;
                                }
                            }
                            int num12 = 0;
                            int num13 = list3.Count;
                            while (num12 < num13)
                            {
                                Method method;
                                Member member = list3[num12];
                                if (member != null)
                                {
                                    switch (member.NodeType)
                                    {
                                        case NodeType.Field:
                                            this.GetFieldIndex((Field) member);
                                            break;

                                        case NodeType.InstanceInitializer:
                                        case NodeType.Method:
                                        case NodeType.StaticInitializer:
                                            goto Label_02B2;
                                    }
                                }
                                goto Label_02FA;
                            Label_02B2:
                                method = (Method) member;
                                if ((this.UseGenerics && (method.Template != null)) && method.Template.IsGeneric)
                                {
                                    this.GetMethodSpecIndex(method);
                                }
                                else
                                {
                                    this.GetMethodIndex(method);
                                }
                            Label_02FA:
                                num12++;
                            }
                        }
                    }
                    num8++;
                }
                int num14 = num;
                int num15 = list.Count;
                while (num14 < num15)
                {
                    TypeNode node4 = list[num14];
                    if (node4 != null)
                    {
                        this.Visit(node4);
                    }
                    num14++;
                    num++;
                }
                int num16 = count;
                int num17 = module.Types.Count;
                while (num16 < num17)
                {
                    TypeNode node5 = module.Types[num16];
                    bool flag1 = node5 == null;
                    num16++;
                }
            }
        }

        private void VisitParameter(Parameter parameter)
        {
            this.IncrementStackHeight();
            ParameterBinding binding = parameter as ParameterBinding;
            if (binding != null)
            {
                parameter = binding.BoundParameter;
            }
            int argumentListIndex = parameter.ArgumentListIndex;
            switch (argumentListIndex)
            {
                case 0:
                    this.methodBodyHeap.Write((byte) 2);
                    return;

                case 1:
                    this.methodBodyHeap.Write((byte) 3);
                    return;

                case 2:
                    this.methodBodyHeap.Write((byte) 4);
                    return;

                case 3:
                    this.methodBodyHeap.Write((byte) 5);
                    return;
            }
            if (argumentListIndex < 0x100)
            {
                this.methodBodyHeap.Write((byte) 14);
                this.methodBodyHeap.Write((byte) argumentListIndex);
            }
            else
            {
                this.methodBodyHeap.Write((byte) 0xfe);
                this.methodBodyHeap.Write((byte) 9);
                this.methodBodyHeap.Write((ushort) argumentListIndex);
            }
        }

        private void VisitProperty(Property property)
        {
            if (this.propertyIndex[property.UniqueKey] == null)
            {
                int num = this.propertyEntries.Count + 1;
                this.propertyEntries.Add(property);
                this.propertyIndex[property.UniqueKey] = num;
                if (this.propertyMapIndex[property.DeclaringType.UniqueKey] == null)
                {
                    this.propertyMapEntries.Add(property);
                    this.propertyMapIndex[property.DeclaringType.UniqueKey] = this.propertyMapEntries.Count;
                }
                if (property.Getter != null)
                {
                    this.methodSemanticsEntries.Add(property);
                }
                if (property.Setter != null)
                {
                    this.methodSemanticsEntries.Add(property);
                }
                if (property.OtherMethods != null)
                {
                    int num2 = 0;
                    int count = property.OtherMethods.Count;
                    while (num2 < count)
                    {
                        this.methodSemanticsEntries.Add(property);
                        num2++;
                    }
                }
                this.VisitAttributeList(property.Attributes, property);
            }
        }

        private void VisitReferencedType(TypeNode type)
        {
            if (type != null)
            {
                if (type.IsGeneric && (type.Template == null))
                {
                    TypeNodeList consolidatedTemplateParameters = type.ConsolidatedTemplateParameters;
                    int num = 0;
                    int num2 = (consolidatedTemplateParameters == null) ? 0 : consolidatedTemplateParameters.Count;
                    while (num < num2)
                    {
                        this.typeParameterNumber[consolidatedTemplateParameters[num].UniqueKey] = num + 1;
                        num++;
                    }
                }
                switch (type.typeCode)
                {
                    case ElementType.Pointer:
                        this.VisitReferencedType(((Pointer) type).ElementType);
                        return;

                    case ElementType.Reference:
                        this.VisitReferencedType(((Reference) type).ElementType);
                        return;

                    case ElementType.ValueType:
                    case ElementType.Class:
                        if (!this.IsStructural(type))
                        {
                            if (type.DeclaringModule == this.module)
                            {
                                this.GetTypeDefIndex(type);
                            }
                            else if (type.DeclaringModule != null)
                            {
                                this.GetTypeRefIndex(type);
                            }
                            else if (((type.typeCode == ElementType.ValueType) || (type.typeCode == ElementType.Class)) && (!this.UseGenerics || (this.typeParameterNumber[type.UniqueKey] == null)))
                            {
                                type.DeclaringModule = this.module;
                                this.GetTypeDefIndex(type);
                            }
                            return;
                        }
                        this.GetTypeSpecIndex(type);
                        return;

                    case ElementType.TypeParameter:
                    case ElementType.Object:
                    case ElementType.MethodParameter:
                        return;

                    case ElementType.Array:
                    case ElementType.SzArray:
                        this.VisitReferencedType(((ArrayType) type).ElementType);
                        return;

                    case ElementType.FunctionPointer:
                    {
                        FunctionPointer pointer = (FunctionPointer) type;
                        this.VisitReferencedType(pointer.ReturnType);
                        int num3 = 0;
                        int num4 = (pointer.ParameterTypes == null) ? 0 : pointer.ParameterTypes.Count;
                        while (num3 < num4)
                        {
                            this.VisitReferencedType(pointer.ParameterTypes[num3]);
                            num3++;
                        }
                        return;
                    }
                    case ElementType.RequiredModifier:
                    case ElementType.OptionalModifier:
                    {
                        TypeModifier modifier = (TypeModifier) type;
                        this.VisitReferencedType(modifier.Modifier);
                        this.VisitReferencedType(modifier.ModifiedType);
                        return;
                    }
                }
            }
        }

        private void VisitReturn(Return Return)
        {
            this.DefineSequencePoint(Return);
            if (Return.Expression != null)
            {
                this.Visit(Return.Expression);
                this.stackHeight--;
            }
            this.methodBodyHeap.Write((byte) 0x2a);
        }

        private void VisitSecurityAttributeList(SecurityAttributeList attrs, Node node)
        {
            if (attrs != null)
            {
                int count = attrs.Count;
                if (count != 0)
                {
                    int num2 = count;
                    for (int i = 0; i < count; i++)
                    {
                        if (attrs[i] == null)
                        {
                            num2--;
                        }
                    }
                    if (num2 != 0)
                    {
                        count = num2;
                        int securityAttributeParentCodedIndex = this.GetSecurityAttributeParentCodedIndex(node);
                        this.securityAttributeCount += count;
                        num2 = this.nodesWithSecurityAttributes.Count;
                        this.nodesWithSecurityAttributes.Add(node);
                        int num5 = 0;
                        NodeList nodesWithSecurityAttributes = this.nodesWithSecurityAttributes;
                        num5 = num2;
                        while (num5 > 0)
                        {
                            Node node2 = nodesWithSecurityAttributes[num5 - 1];
                            if (this.GetSecurityAttributeParentCodedIndex(node2) < securityAttributeParentCodedIndex)
                            {
                                break;
                            }
                            num5--;
                        }
                        if (num5 != num2)
                        {
                            for (int j = num2; j > num5; j--)
                            {
                                nodesWithSecurityAttributes[j] = nodesWithSecurityAttributes[j - 1];
                            }
                            nodesWithSecurityAttributes[num5] = node;
                        }
                    }
                }
            }
        }

        private void VisitStatement(Statement statement)
        {
            this.DefineSequencePoint(statement);
            NodeType nodeType = statement.NodeType;
            switch (nodeType)
            {
                case NodeType.DebugBreak:
                    this.methodBodyHeap.Write((byte) 1);
                    return;

                case NodeType.EndFinally:
                    this.methodBodyHeap.Write((byte) 220);
                    return;
            }
            if (nodeType == NodeType.Nop)
            {
                this.methodBodyHeap.Write((byte) 0);
            }
        }

        private void VisitStruct(Struct Struct)
        {
            if ((!this.UseGenerics || (Struct.Template == null)) || !Struct.Template.IsGeneric)
            {
                this.VisitAttributeList(Struct.Attributes, Struct);
                this.VisitSecurityAttributeList(Struct.SecurityAttributes, Struct);
                this.VisitReferencedType(CoreSystemTypes.ValueType);
                InterfaceList interfaces = Struct.Interfaces;
                int num = 0;
                int num2 = (interfaces == null) ? 0 : interfaces.Count;
                while (num < num2)
                {
                    this.GetTypeDefOrRefOrSpecEncoded(interfaces[num]);
                    if (interfaces[num] != null)
                    {
                        this.interfaceEntries.Add(Struct);
                    }
                    num++;
                }
                int num3 = 0;
                int count = Struct.Members.Count;
                while (num3 < count)
                {
                    Member node = Struct.Members[num3];
                    if (!(node is TypeNode))
                    {
                        this.Visit(node);
                    }
                    num3++;
                }
                if (((Struct.Flags & TypeFlags.LayoutMask) != TypeFlags.AnsiClass) && ((Struct.PackingSize != 0) || (Struct.ClassSize != 0)))
                {
                    this.classLayoutEntries.Add(Struct);
                }
            }
        }

        private void VisitSwitchInstruction(SwitchInstruction switchInstruction)
        {
            this.Visit(switchInstruction.Expression);
            this.stackHeight--;
            BlockList targets = switchInstruction.Targets;
            int num = (targets != null) ? targets.Count : 0;
            int addressOfNextInstruction = (this.methodBodyHeap.BaseStream.Position + 5) + (4 * num);
            this.methodBodyHeap.Write((byte) 0x45);
            this.methodBodyHeap.Write((uint) num);
            for (int i = 0; i < num; i++)
            {
                this.methodBodyHeap.Write(this.GetOffset(targets[i], addressOfNextInstruction));
            }
        }

        private void VisitTernaryExpression(TernaryExpression expression)
        {
            this.Visit(expression.Operand1);
            this.Visit(expression.Operand2);
            this.Visit(expression.Operand3);
            this.methodBodyHeap.Write((byte) 0xfe);
            if (expression.NodeType == NodeType.Cpblk)
            {
                this.methodBodyHeap.Write((byte) 0x17);
            }
            else
            {
                this.methodBodyHeap.Write((byte) 0x18);
            }
            this.stackHeight -= 3;
        }

        private void VisitThis(This This)
        {
            this.IncrementStackHeight();
            this.methodBodyHeap.Write((byte) 2);
        }

        private void VisitThrow(Throw Throw)
        {
            this.DefineSequencePoint(Throw);
            if (Throw.NodeType == NodeType.Rethrow)
            {
                this.methodBodyHeap.Write((byte) 0xfe);
                this.methodBodyHeap.Write((byte) 0x1a);
            }
            else
            {
                this.Visit(Throw.Expression);
                this.methodBodyHeap.Write((byte) 0x7a);
            }
            this.stackHeight--;
        }

        private void VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            switch (unaryExpression.NodeType)
            {
                case NodeType.Ldftn:
                    this.methodBodyHeap.Write((byte) 0xfe);
                    this.methodBodyHeap.Write((byte) 6);
                    this.methodBodyHeap.Write(this.GetMethodToken((Method) ((MemberBinding) unaryExpression.Operand).BoundMember));
                    this.IncrementStackHeight();
                    return;

                case NodeType.Ldtoken:
                {
                    this.methodBodyHeap.Write((byte) 0xd0);
                    Literal operand = unaryExpression.Operand as Literal;
                    if (operand == null)
                    {
                        if (unaryExpression.Operand == null)
                        {
                            return;
                        }
                        Member boundMember = ((MemberBinding) unaryExpression.Operand).BoundMember;
                        if (boundMember == null)
                        {
                            return;
                        }
                        Method m = boundMember as Method;
                        if (m != null)
                        {
                            this.methodBodyHeap.Write(this.GetMethodToken(m));
                        }
                        else
                        {
                            this.methodBodyHeap.Write(this.GetFieldToken((Field) boundMember));
                        }
                        break;
                    }
                    if (operand.Value != null)
                    {
                        this.methodBodyHeap.Write(this.GetTypeDefToken((TypeNode) operand.Value));
                        break;
                    }
                    return;
                }
                case NodeType.Sizeof:
                    this.methodBodyHeap.Write((byte) 0xfe);
                    this.methodBodyHeap.Write((byte) 0x1c);
                    this.methodBodyHeap.Write(this.GetTypeToken((TypeNode) ((Literal) unaryExpression.Operand).Value));
                    this.IncrementStackHeight();
                    return;

                case NodeType.SkipCheck:
                {
                    this.methodBodyHeap.Write((byte) 0xfe);
                    this.methodBodyHeap.Write((byte) 0x19);
                    NodeType nodeType = unaryExpression.Operand.NodeType;
                    if ((nodeType != NodeType.Castclass) && (nodeType != NodeType.Unbox))
                    {
                        this.methodBodyHeap.Write((byte) 0);
                    }
                    else
                    {
                        this.methodBodyHeap.Write((byte) 1);
                    }
                    this.VisitExpression(unaryExpression.Operand);
                    return;
                }
                default:
                {
                    this.Visit(unaryExpression.Operand);
                    byte num = 0;
                    switch (unaryExpression.NodeType)
                    {
                        case NodeType.Ckfinite:
                            num = 0xc3;
                            break;

                        case NodeType.Conv_I:
                            num = 0xd3;
                            break;

                        case NodeType.Conv_I1:
                            num = 0x67;
                            break;

                        case NodeType.Conv_I2:
                            num = 0x68;
                            break;

                        case NodeType.Conv_I4:
                            num = 0x69;
                            break;

                        case NodeType.Conv_I8:
                            num = 0x6a;
                            break;

                        case NodeType.Conv_Ovf_I:
                            num = 0xd4;
                            break;

                        case NodeType.Conv_Ovf_I_Un:
                            num = 0x8a;
                            break;

                        case NodeType.Conv_Ovf_I1:
                            num = 0xb3;
                            break;

                        case NodeType.Conv_Ovf_I1_Un:
                            num = 130;
                            break;

                        case NodeType.Conv_Ovf_I2:
                            num = 0xb5;
                            break;

                        case NodeType.Conv_Ovf_I2_Un:
                            num = 0x83;
                            break;

                        case NodeType.Conv_Ovf_I4:
                            num = 0xb7;
                            break;

                        case NodeType.Conv_Ovf_I4_Un:
                            num = 0x84;
                            break;

                        case NodeType.Conv_Ovf_I8:
                            num = 0xb9;
                            break;

                        case NodeType.Conv_Ovf_I8_Un:
                            num = 0x85;
                            break;

                        case NodeType.Conv_Ovf_U:
                            num = 0xd5;
                            break;

                        case NodeType.Conv_Ovf_U_Un:
                            num = 0x8b;
                            break;

                        case NodeType.Conv_Ovf_U1:
                            num = 180;
                            break;

                        case NodeType.Conv_Ovf_U1_Un:
                            num = 0x86;
                            break;

                        case NodeType.Conv_Ovf_U2:
                            num = 0xb6;
                            break;

                        case NodeType.Conv_Ovf_U2_Un:
                            num = 0x87;
                            break;

                        case NodeType.Conv_Ovf_U4:
                            num = 0xb8;
                            break;

                        case NodeType.Conv_Ovf_U4_Un:
                            num = 0x88;
                            break;

                        case NodeType.Conv_Ovf_U8:
                            num = 0xba;
                            break;

                        case NodeType.Conv_Ovf_U8_Un:
                            num = 0x89;
                            break;

                        case NodeType.Conv_R_Un:
                            num = 0x76;
                            break;

                        case NodeType.Conv_R4:
                            num = 0x6b;
                            break;

                        case NodeType.Conv_R8:
                            num = 0x6c;
                            break;

                        case NodeType.Conv_U:
                            num = 0xe0;
                            break;

                        case NodeType.Conv_U1:
                            num = 210;
                            break;

                        case NodeType.Conv_U2:
                            num = 0xd1;
                            break;

                        case NodeType.Conv_U4:
                            num = 0x6d;
                            break;

                        case NodeType.Conv_U8:
                            num = 110;
                            break;

                        case NodeType.Ldlen:
                            num = 0x8e;
                            break;

                        case NodeType.Localloc:
                            num = 15;
                            this.methodBodyHeap.Write((byte) 0xfe);
                            break;

                        case NodeType.Neg:
                            num = 0x65;
                            break;

                        case NodeType.Not:
                            num = 0x66;
                            break;

                        case NodeType.Refanytype:
                            num = 0x1d;
                            this.methodBodyHeap.Write((byte) 0xfe);
                            break;
                    }
                    this.methodBodyHeap.Write(num);
                    return;
                }
            }
            this.IncrementStackHeight();
        }

        private static void WriteArrayShape(System.Compiler.BinaryWriter target, ArrayType arrayType)
        {
            WriteCompressedInt(target, arrayType.Rank);
            int val = (arrayType.Sizes == null) ? 0 : arrayType.Sizes.Length;
            WriteCompressedInt(target, val);
            for (int i = 0; i < val; i++)
            {
                WriteCompressedInt(target, arrayType.Sizes[i]);
            }
            val = (arrayType.LowerBounds == null) ? 0 : arrayType.LowerBounds.Length;
            WriteCompressedInt(target, val);
            for (int j = 0; j < val; j++)
            {
                WriteCompressedInt(target, arrayType.LowerBounds[j]);
            }
        }

        internal static void WriteCompressedInt(System.Compiler.BinaryWriter target, int val)
        {
            if (val <= 0x7f)
            {
                target.Write((byte) val);
            }
            else if (val < 0x3fff)
            {
                target.Write((byte) ((val >> 8) | 0x80));
                target.Write((byte) (val & 0xff));
            }
            else if (val < 0x1fffffff)
            {
                target.Write((byte) ((val >> 0x18) | 0xc0));
                target.Write((byte) ((val & 0xff0000) >> 0x10));
                target.Write((byte) ((val & 0xff00) >> 8));
                target.Write((byte) (val & 0xff));
            }
        }

        private void WriteCustomAttributeLiteral(System.Compiler.BinaryWriter writer, Literal literal, bool needsTag)
        {
            object obj2;
            Literal literal3;
            TypeNode typeNode;
            if (literal.Type == null)
            {
                return;
            }
            ElementType typeCode = literal.Type.typeCode;
            if (needsTag)
            {
                switch (typeCode)
                {
                    case ElementType.ValueType:
                        writer.Write((byte) 0x55);
                        this.WriteSerializedTypeName(writer, literal.Type);
                        goto Label_006B;

                    case ElementType.Class:
                        writer.Write((byte) 80);
                        goto Label_006B;
                }
                if (typeCode != ElementType.Object)
                {
                    writer.Write((byte) typeCode);
                }
            }
        Label_006B:
            obj2 = literal.Value;
            switch (typeCode)
            {
                case ElementType.Boolean:
                    writer.Write((bool) obj2);
                    return;

                case ElementType.Char:
                    writer.Write((ushort) ((char) obj2));
                    return;

                case ElementType.Int8:
                    writer.Write((sbyte) obj2);
                    return;

                case ElementType.UInt8:
                    writer.Write((byte) obj2);
                    return;

                case ElementType.Int16:
                    writer.Write((short) obj2);
                    return;

                case ElementType.UInt16:
                    writer.Write((ushort) obj2);
                    return;

                case ElementType.Int32:
                    writer.Write((int) obj2);
                    return;

                case ElementType.UInt32:
                    writer.Write((uint) obj2);
                    return;

                case ElementType.Int64:
                    writer.Write((long) obj2);
                    return;

                case ElementType.UInt64:
                    writer.Write((ulong) obj2);
                    return;

                case ElementType.Single:
                    writer.Write((float) obj2);
                    return;

                case ElementType.Double:
                    writer.Write((double) obj2);
                    return;

                case ElementType.String:
                    writer.Write((string) obj2, false);
                    return;

                case ElementType.Pointer:
                case ElementType.Reference:
                case ElementType.TypeParameter:
                case ElementType.Array:
                case ElementType.GenericTypeInstance:
                case ElementType.DynamicallyTypedReference:
                case (ElementType.DynamicallyTypedReference | ElementType.Void):
                case ElementType.IntPtr:
                case ElementType.UIntPtr:
                case (ElementType.IntPtr | ElementType.Boolean):
                case ElementType.FunctionPointer:
                    return;

                case ElementType.ValueType:
                    this.WriteCustomAttributeLiteral(writer, new Literal(obj2, ((EnumNode) literal.Type).UnderlyingType), false);
                    return;

                case ElementType.Class:
                    if ((obj2 != null) || (literal.Type != CoreSystemTypes.Type))
                    {
                        this.WriteSerializedTypeName(writer, (TypeNode) obj2);
                        return;
                    }
                    writer.Write((byte) 0xff);
                    return;

                case ElementType.Object:
                    literal3 = (Literal) literal.Clone();
                    typeNode = null;
                    switch (Convert.GetTypeCode(literal3.Value))
                    {
                        case TypeCode.Empty:
                        case TypeCode.Object:
                        {
                            Array array2 = literal3.Value as Array;
                            if (array2 != null)
                            {
                                typeNode = TypeNode.GetTypeNode(array2.GetType());
                            }
                            else
                            {
                                typeNode = CoreSystemTypes.Type;
                            }
                            goto Label_03C8;
                        }
                        case TypeCode.Boolean:
                            typeNode = CoreSystemTypes.Boolean;
                            goto Label_03C8;

                        case TypeCode.Char:
                            typeNode = CoreSystemTypes.Char;
                            goto Label_03C8;

                        case TypeCode.SByte:
                            typeNode = CoreSystemTypes.Int8;
                            goto Label_03C8;

                        case TypeCode.Byte:
                            typeNode = CoreSystemTypes.UInt8;
                            goto Label_03C8;

                        case TypeCode.Int16:
                            typeNode = CoreSystemTypes.Int16;
                            goto Label_03C8;

                        case TypeCode.UInt16:
                            typeNode = CoreSystemTypes.UInt16;
                            goto Label_03C8;

                        case TypeCode.Int32:
                            typeNode = CoreSystemTypes.Int32;
                            goto Label_03C8;

                        case TypeCode.UInt32:
                            typeNode = CoreSystemTypes.UInt32;
                            goto Label_03C8;

                        case TypeCode.Int64:
                            typeNode = CoreSystemTypes.Int64;
                            goto Label_03C8;

                        case TypeCode.UInt64:
                            typeNode = CoreSystemTypes.UInt64;
                            goto Label_03C8;

                        case TypeCode.Single:
                            typeNode = CoreSystemTypes.Single;
                            goto Label_03C8;

                        case TypeCode.Double:
                            typeNode = CoreSystemTypes.Double;
                            goto Label_03C8;

                        case TypeCode.String:
                            typeNode = CoreSystemTypes.String;
                            goto Label_03C8;
                    }
                    break;

                case ElementType.SzArray:
                {
                    TypeNode elementType = ((ArrayType) literal.Type).ElementType;
                    if (needsTag)
                    {
                        writer.Write((byte) elementType.typeCode);
                    }
                    Array array = (Array) obj2;
                    int num = (array == null) ? -1 : array.Length;
                    writer.Write(num);
                    bool flag = elementType == CoreSystemTypes.Object;
                    for (int i = 0; i < num; i++)
                    {
                        object obj3 = array.GetValue(i);
                        Literal literal2 = obj3 as Literal;
                        if (literal2 == null)
                        {
                            literal2 = new Literal(obj3, elementType);
                        }
                        this.WriteCustomAttributeLiteral(writer, literal2, flag);
                    }
                    return;
                }
                default:
                    return;
            }
        Label_03C8:
            if (typeNode == null)
            {
                return;
            }
            literal3.Type = typeNode;
            this.WriteCustomAttributeLiteral(writer, literal3, true);
        }

        private void WriteCustomAttributeSignature(ExpressionList expressions, ParameterList parameters, bool onlyWriteNamedArguments, System.Compiler.BinaryWriter signature)
        {
            int num = (parameters == null) ? 0 : parameters.Count;
            int num2 = (expressions == null) ? 0 : expressions.Count;
            int val = (num2 > num) ? (num2 - num) : 0;
            if (onlyWriteNamedArguments)
            {
                WriteCompressedInt(signature, val);
            }
            else
            {
                signature.Write((short) 1);
                if ((parameters != null) && (expressions != null))
                {
                    for (int i = 0; i < num; i++)
                    {
                        Parameter parameter = parameters[i];
                        Expression expression = expressions[i];
                        if ((parameter != null) && (expression != null))
                        {
                            Literal literal = expression as Literal;
                            if (literal != null)
                            {
                                this.WriteCustomAttributeLiteral(signature, literal, parameter.Type == CoreSystemTypes.Object);
                            }
                        }
                    }
                }
                signature.Write((short) val);
            }
            if (expressions != null)
            {
                for (int j = num; j < num2; j++)
                {
                    Expression expression2 = expressions[j];
                    NamedArgument argument = expression2 as NamedArgument;
                    if (argument != null)
                    {
                        signature.Write(argument.IsCustomAttributeProperty ? ((byte) 0x54) : ((byte) 0x53));
                        if (argument.ValueIsBoxed)
                        {
                            signature.Write((byte) 0x51);
                        }
                        else if (argument.Value.Type is EnumNode)
                        {
                            signature.Write((byte) 0x55);
                            this.WriteSerializedTypeName(signature, argument.Value.Type);
                        }
                        else if (argument.Value.Type == CoreSystemTypes.Type)
                        {
                            signature.Write((byte) 80);
                        }
                        else if (argument.Value.Type is ArrayType)
                        {
                            ArrayType type = (ArrayType) argument.Value.Type;
                            if (type.ElementType == CoreSystemTypes.Type)
                            {
                                signature.Write((byte) 0x1d);
                                signature.Write((byte) 80);
                            }
                            else if (type.ElementType is EnumNode)
                            {
                                signature.Write((byte) 0x1d);
                                signature.Write((byte) 0x55);
                                this.WriteSerializedTypeName(signature, type.ElementType);
                            }
                            else
                            {
                                this.WriteTypeSignature(signature, argument.Value.Type);
                            }
                        }
                        else
                        {
                            this.WriteTypeSignature(signature, argument.Value.Type);
                        }
                        signature.Write(argument.Name.Name, false);
                        this.WriteCustomAttributeLiteral(signature, (Literal) argument.Value, argument.ValueIsBoxed);
                    }
                }
            }
        }

        private TypeNode WriteCustomModifiers(System.Compiler.BinaryWriter target, TypeNode type)
        {
            NodeType nodeType = type.NodeType;
            if ((nodeType != NodeType.OptionalModifier) && (nodeType != NodeType.RequiredModifier))
            {
                return type;
            }
            TypeModifier modifier = (TypeModifier) type;
            target.Write((byte) modifier.typeCode);
            this.WriteTypeDefOrRefEncoded(target, modifier.Modifier);
            return this.WriteCustomModifiers(target, modifier.ModifiedType);
        }

        private void WriteMethodSignature(System.Compiler.BinaryWriter target, FunctionPointer fp)
        {
            target.Write((byte) fp.CallingConvention);
            TypeNodeList parameterTypes = fp.ParameterTypes;
            int val = (parameterTypes == null) ? 0 : parameterTypes.Count;
            WriteCompressedInt(target, val);
            if (fp.ReturnType != null)
            {
                this.WriteTypeSignature(target, fp.ReturnType);
            }
            int varArgStart = fp.VarArgStart;
            for (int i = 0; i < val; i++)
            {
                if (i == varArgStart)
                {
                    target.Write((byte) 0x41);
                }
                this.WriteTypeSignature(target, parameterTypes[i]);
            }
        }

        private void WriteMethodSignature(System.Compiler.BinaryWriter target, Method method)
        {
            if (this.UseGenerics)
            {
                if ((method.Template != null) && method.Template.IsGeneric)
                {
                    TypeNodeList templateArguments = method.TemplateArguments;
                    int num = (templateArguments == null) ? 0 : templateArguments.Count;
                    target.Write((byte) (method.CallingConvention | CallingConventionFlags.Generic));
                    WriteCompressedInt(target, num);
                }
                else
                {
                    if ((method.DeclaringType.Template != null) && method.DeclaringType.Template.IsGeneric)
                    {
                        Method unspecializedMethod = this.GetUnspecializedMethod(method);
                        this.WriteMethodSignature(target, unspecializedMethod);
                        return;
                    }
                    if (method.IsGeneric)
                    {
                        TypeNodeList templateParameters = method.TemplateParameters;
                        int num2 = (templateParameters == null) ? 0 : templateParameters.Count;
                        target.Write((byte) (method.CallingConvention | CallingConventionFlags.Generic));
                        WriteCompressedInt(target, num2);
                    }
                    else
                    {
                        target.Write((byte) method.CallingConvention);
                    }
                }
            }
            else
            {
                target.Write((byte) method.CallingConvention);
            }
            ParameterList parameters = method.Parameters;
            int val = (parameters == null) ? 0 : parameters.Count;
            WriteCompressedInt(target, val);
            TypeNode returnType = method.ReturnType;
            if (returnType == null)
            {
                returnType = SystemTypes.Object;
            }
            this.WriteTypeSignature(target, returnType, true);
            for (int i = 0; i < val; i++)
            {
                Parameter parameter = parameters[i];
                if (parameter != null)
                {
                    TypeNode type = parameter.Type;
                    if (type == null)
                    {
                        type = SystemTypes.Object;
                    }
                    this.WriteTypeSignature(target, type);
                }
            }
        }

        private void WriteMethodSpecSignature(System.Compiler.BinaryWriter target, Method method)
        {
            target.Write((byte) 10);
            TypeNodeList templateArguments = method.TemplateArguments;
            int val = (templateArguments == null) ? 0 : templateArguments.Count;
            WriteCompressedInt(target, val);
            for (int i = 0; i < val; i++)
            {
                this.WriteTypeSignature(target, templateArguments[i]);
            }
        }

        internal static void WritePE(Module module, string debugSymbolsLocation, System.Compiler.BinaryWriter writer)
        {
            Ir2md irmd = new Ir2md(module);
            try
            {
                irmd.SetupMetadataWriter(debugSymbolsLocation);
                irmd.writer.WritePE(writer);
            }
            finally
            {
                if (irmd.symWriter != null)
                {
                    irmd.symWriter.Close();
                }
                irmd.assembly = null;
                irmd.assemblyRefEntries = null;
                irmd.assemblyRefIndex = null;
                irmd.blobHeap = null;
                irmd.blobHeapIndex = null;
                irmd.blobHeapStringIndex = null;
                irmd.classLayoutEntries = null;
                irmd.constantTableEntries = null;
                irmd.documentMap = null;
                irmd.eventEntries = null;
                irmd.eventIndex = null;
                irmd.eventMapEntries = null;
                irmd.eventMapIndex = null;
                irmd.exceptionBlock = null;
                irmd.fieldEntries = null;
                irmd.fieldIndex = null;
                irmd.fieldLayoutEntries = null;
                irmd.fieldRvaEntries = null;
                irmd.fileTableEntries = null;
                irmd.fileTableIndex = null;
                irmd.genericParamConstraintEntries = null;
                irmd.genericParamEntries = null;
                irmd.genericParameters = null;
                irmd.genericParamIndex = null;
                irmd.guidEntries = null;
                irmd.guidIndex = null;
                irmd.implMapEntries = null;
                irmd.interfaceEntries = null;
                irmd.marshalEntries = null;
                irmd.memberRefEntries = null;
                irmd.memberRefIndex = null;
                irmd.methodBodiesHeap = null;
                irmd.methodBodiesHeapIndex = null;
                irmd.methodBodyHeap = null;
                irmd.methodEntries = null;
                irmd.methodImplEntries = null;
                irmd.methodIndex = null;
                irmd.methodInfo = null;
                irmd.methodSemanticsEntries = null;
                irmd.methodSpecEntries = null;
                irmd.methodSpecIndex = null;
                irmd.module = null;
                irmd.moduleRefEntries = null;
                irmd.moduleRefIndex = null;
                irmd.nestedClassEntries = null;
                irmd.nodesWithCustomAttributes = null;
                irmd.nodesWithSecurityAttributes = null;
                irmd.paramEntries = null;
                irmd.paramIndex = null;
                irmd.propertyEntries = null;
                irmd.propertyIndex = null;
                irmd.propertyMapEntries = null;
                irmd.propertyMapIndex = null;
                irmd.PublicKey = null;
                irmd.resourceDataHeap = null;
                irmd.sdataHeap = null;
                irmd.standAloneSignatureEntries = null;
                irmd.stringHeap = null;
                irmd.stringHeapIndex = null;
                irmd.symWriter = null;
                irmd.tlsHeap = null;
                irmd.typeDefEntries = null;
                irmd.typeDefIndex = null;
                irmd.typeParameterNumber = null;
                irmd.typeRefEntries = null;
                irmd.typeRefIndex = null;
                irmd.typeSpecEntries = null;
                irmd.typeSpecIndex = null;
                irmd.unspecializedFieldFor = null;
                irmd.unspecializedMethodFor = null;
                irmd.userStringHeap = null;
                irmd.userStringHeapIndex = null;
                irmd.writer = null;
                irmd = null;
            }
        }

        private void WritePropertySignature(System.Compiler.BinaryWriter target, Property prop)
        {
            byte num = 8;
            if (!prop.IsStatic)
            {
                num = (byte) (num | 0x20);
            }
            target.Write(num);
            ParameterList parameters = prop.Parameters;
            int val = (parameters == null) ? 0 : parameters.Count;
            WriteCompressedInt(target, val);
            if (prop.Type != null)
            {
                this.WriteTypeSignature(target, prop.Type);
            }
            for (int i = 0; i < val; i++)
            {
                Parameter parameter = parameters[i];
                if ((parameter != null) && (parameter.Type != null))
                {
                    this.WriteTypeSignature(target, parameter.Type);
                }
            }
        }

        private void WriteSecurityAttribute(System.Compiler.BinaryWriter signature, AttributeNode attr)
        {
            bool isAssemblyQualified = true;
            string serializedTypeName = this.GetSerializedTypeName(attr.Type, ref isAssemblyQualified);
            if (!isAssemblyQualified)
            {
                serializedTypeName = serializedTypeName + ", " + attr.Type.DeclaringModule.ContainingAssembly.StrongName;
            }
            signature.Write(serializedTypeName);
            System.Compiler.MemoryStream output = new System.Compiler.MemoryStream();
            System.Compiler.BinaryWriter writer = new System.Compiler.BinaryWriter(output);
            MemberBinding constructor = attr.Constructor as MemberBinding;
            if (constructor != null)
            {
                InstanceInitializer boundMember = constructor.BoundMember as InstanceInitializer;
                if (boundMember != null)
                {
                    this.WriteCustomAttributeSignature(attr.Expressions, boundMember.Parameters, true, writer);
                    byte[] buffer = output.ToArray();
                    int length = buffer.Length;
                    WriteCompressedInt(signature, length);
                    signature.BaseStream.Write(buffer, 0, length);
                }
            }
        }

        private void WriteSerializedTypeName(System.Compiler.BinaryWriter target, TypeNode type)
        {
            if ((target != null) && (type != null))
            {
                target.Write(this.GetSerializedTypeName(type), false);
            }
        }

        private void WriteTypeDefEncoded(System.Compiler.BinaryWriter target, TypeNode type)
        {
            int typeDefIndex = this.GetTypeDefIndex(type);
            WriteCompressedInt(target, typeDefIndex << 2);
        }

        private void WriteTypeDefOrRefEncoded(System.Compiler.BinaryWriter target, TypeNode type)
        {
            if ((!type.IsGeneric && this.IsStructural(type)) && !(type is ITypeParameter))
            {
                this.WriteTypeSpecEncoded(target, type);
            }
            else if (type.DeclaringModule == this.module)
            {
                this.WriteTypeDefEncoded(target, type);
            }
            else if (type.DeclaringModule != null)
            {
                this.WriteTypeRefEncoded(target, type);
            }
        }

        private void WriteTypeRefEncoded(System.Compiler.BinaryWriter target, TypeNode type)
        {
            int typeRefIndex = this.GetTypeRefIndex(type);
            WriteCompressedInt(target, (typeRefIndex << 2) | 1);
        }

        private void WriteTypeSignature(System.Compiler.BinaryWriter target, TypeNode type)
        {
            this.WriteTypeSignature(target, type, false);
        }

        private void WriteTypeSignature(System.Compiler.BinaryWriter target, TypeNode type, bool instantiateGenericTypes)
        {
            if (type != null)
            {
                TypeNode node = this.WriteCustomModifiers(target, type);
                if (this.UseGenerics)
                {
                    if (((node.Template != null) && node.Template.IsGeneric) && (node.TemplateParameters == null))
                    {
                        target.Write((byte) 0x15);
                        TypeNode template = node.Template;
                        while (template.Template != null)
                        {
                            template = template.Template;
                        }
                        this.WriteTypeSignature(target, template);
                        TypeNodeList consolidatedTemplateArguments = node.ConsolidatedTemplateArguments;
                        int val = (consolidatedTemplateArguments == null) ? 0 : consolidatedTemplateArguments.Count;
                        WriteCompressedInt(target, val);
                        for (int i = 0; i < val; i++)
                        {
                            TypeNode node3 = consolidatedTemplateArguments[i];
                            if (node3 != null)
                            {
                                this.WriteTypeSignature(target, node3);
                            }
                        }
                        return;
                    }
                    if (node.IsGeneric && instantiateGenericTypes)
                    {
                        while (node.Template != null)
                        {
                            node = node.Template;
                        }
                        target.Write((byte) 0x15);
                        this.WriteTypeSignature(target, node);
                        TypeNodeList consolidatedTemplateParameters = node.ConsolidatedTemplateParameters;
                        int num3 = (consolidatedTemplateParameters == null) ? 0 : consolidatedTemplateParameters.Count;
                        WriteCompressedInt(target, num3);
                        for (int j = 0; j < num3; j++)
                        {
                            TypeNode node4 = consolidatedTemplateParameters[j];
                            if (node4 != null)
                            {
                                this.WriteTypeSignature(target, node4);
                            }
                        }
                        return;
                    }
                    if (node is ITypeParameter)
                    {
                        object obj2 = this.typeParameterNumber[node.UniqueKey];
                        if (obj2 == null)
                        {
                            if ((node is MethodTypeParameter) || (node is MethodClassParameter))
                            {
                                obj2 = -(((ITypeParameter) node).ParameterListIndex + 1);
                            }
                            else
                            {
                                obj2 = ((ITypeParameter) node).ParameterListIndex + 1;
                            }
                        }
                        if (obj2 is int)
                        {
                            int num5 = (int) obj2;
                            if (num5 < 0)
                            {
                                target.Write((byte) 30);
                                num5 = -num5;
                            }
                            else
                            {
                                target.Write((byte) 0x13);
                            }
                            WriteCompressedInt(target, num5 - 1);
                            return;
                        }
                    }
                }
                target.Write((byte) node.typeCode);
                switch (node.typeCode)
                {
                    case ElementType.Pointer:
                        this.WriteTypeSignature(target, ((Pointer) node).ElementType);
                        return;

                    case ElementType.Reference:
                        this.WriteTypeSignature(target, ((Reference) node).ElementType);
                        return;

                    case ElementType.ValueType:
                    case ElementType.Class:
                        this.WriteTypeDefOrRefEncoded(target, node);
                        return;

                    case ElementType.TypeParameter:
                    case ElementType.Object:
                        return;

                    case ElementType.Array:
                        this.WriteTypeSignature(target, ((ArrayType) node).ElementType);
                        WriteArrayShape(target, (ArrayType) node);
                        return;

                    case ElementType.FunctionPointer:
                        this.WriteMethodSignature(target, (FunctionPointer) node);
                        return;

                    case ElementType.SzArray:
                        this.WriteTypeSignature(target, ((ArrayType) node).ElementType);
                        return;
                }
            }
        }

        private void WriteTypeSpecEncoded(System.Compiler.BinaryWriter target, TypeNode type)
        {
            int typeSpecIndex = this.GetTypeSpecIndex(type);
            WriteCompressedInt(target, (typeSpecIndex << 2) | 2);
        }

        private bool StripOptionalModifiersFromLocals =>
            this.module.StripOptionalModifiersFromLocals;

        private delegate int GetClassObjectDelegate([In] ref Guid refclsid, [In] ref Guid refiid, [MarshalAs(UnmanagedType.Interface)] out Ir2md.IClassFactory ppUnk);

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000001-0000-0000-C000-000000000046")]
        private interface IClassFactory
        {
            int CreateInstance([In, MarshalAs(UnmanagedType.Interface)] object unused, [In] ref Guid refiid, [MarshalAs(UnmanagedType.Interface)] out object ppunk);
            int LockServer(int fLock);
        }

        private class MethodSemanticsRowComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                MethodSemanticsRow row = (MethodSemanticsRow) x;
                MethodSemanticsRow row2 = (MethodSemanticsRow) y;
                int num = row.Association - row2.Association;
                if (num == 0)
                {
                    num = row.Method - row2.Method;
                }
                return num;
            }
        }

        private class VarargMethodCallSignature : FunctionPointer
        {
            internal Method method;

            internal VarargMethodCallSignature(Method method, TypeNodeList parameterTypes) : base(parameterTypes, method.ReturnType, method.Name)
            {
                this.method = method;
                this.DeclaringType = method.DeclaringType;
            }
        }
    }
}

