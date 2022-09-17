namespace System.Compiler.Metadata
{
    using System;
    using System.Compiler;
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal class MetadataReader : IDisposable
    {
        private AssemblyRefRow[] assemblyRefTable;
        private AssemblyRow[] assemblyTable;
        private StreamHeader blobHeap;
        private int blobRefSize;
        private ClassLayoutRow[] classLayoutTable;
        private int constantParentRefSize;
        private ConstantRow[] constantTable;
        private MemoryCursor cursor;
        private int customAttributeConstructorRefSize;
        private int customAttributeParentRefSize;
        private CustomAttributeRow[] customAttributeTable;
        private int declSecurityParentRefSize;
        private DeclSecurityRow[] declSecurityTable;
        internal ushort dllCharacteristics;
        internal byte[] emptyBlob;
        internal int entryPointToken;
        private EventMapRow[] eventMapTable;
        private EventPtrRow[] eventPtrTable;
        private EventRow[] eventTable;
        private ExportedTypeRow[] exportedTypeTable;
        private FieldLayoutRow[] fieldLayoutTable;
        private int fieldMarshalParentRefSize;
        private FieldMarshalRow[] fieldMarshalTable;
        private FieldPtrRow[] fieldPtrTable;
        private FieldRvaRow[] fieldRvaTable;
        private FieldRow[] fieldTable;
        internal int fileAlignment;
        private FileRow[] fileTable;
        private StreamHeader generalStringHeap;
        private GenericParamConstraintRow[] genericParamConstraintTable;
        private GenericParamRow[] genericParamTable;
        private StreamHeader guidHeap;
        private int guidRefSize;
        internal byte[] HashValue;
        private int hasSemanticRefSize;
        private StreamHeader identifierStringHeap;
        private int implementationRefSize;
        private ImplMapRow[] implMapTable;
        private InterfaceImplRow[] interfaceImplTable;
        internal int linkerMajorVersion;
        internal int linkerMinorVersion;
        private ManifestResourceRow[] manifestResourceTable;
        private int mdOffset;
        private int memberForwardedRefSize;
        private int memberRefParentSize;
        private MemberRefRow[] memberRefTable;
        private MemoryMappedFile memmap;
        internal int metadataFormatMajorVersion;
        internal int metadataFormatMinorVersion;
        private int methodDefOrRefSize;
        private MethodImplRow[] methodImplTable;
        private MethodPtrRow[] methodPtrTable;
        private MethodSemanticsRow[] methodSemanticsTable;
        private MethodSpecRow[] methodSpecTable;
        private MethodRow[] methodTable;
        internal ModuleKindFlags moduleKind;
        private ModuleRefRow[] moduleRefTable;
        private ModuleRow[] moduleTable;
        private NestedClassRow[] nestedClassTable;
        private ParamPtrRow[] paramPtrTable;
        private ParamRow[] paramTable;
        internal PEKindFlags peKind;
        private PropertyMapRow[] propertyMapTable;
        private PropertyPtrRow[] propertyPtrTable;
        private PropertyRow[] propertyTable;
        private int resolutionScopeRefSize;
        private int resourcesOffset;
        private SectionHeader[] sectionHeaders;
        private StandAloneSigRow[] standAloneSigTable;
        private int stringRefSize;
        private int[] tableOffset;
        private int[] tableRefSize;
        private StreamHeader tables;
        internal TablesHeader tablesHeader;
        private int[] tableSize;
        internal string targetRuntimeVersion;
        internal bool TrackDebugData;
        private int typeDefOrMethodDefSize;
        private int typeDefOrRefOrSpecSize;
        private TypeDefRow[] typeDefTable;
        private TypeRefRow[] typeRefTable;
        private TypeSpecRow[] typeSpecTable;
        private int win32ResourcesOffset;

        internal MetadataReader(string path)
        {
            MemoryMappedFile memoryMap = this.memmap = new MemoryMappedFile(path);
            try
            {
                this.cursor = new MemoryCursor(memoryMap);
                this.ReadHeader();
            }
            catch
            {
                this.Dispose();
                throw;
            }
        }

        internal unsafe MetadataReader(byte* buffer, int length)
        {
            this.cursor = new MemoryCursor(buffer, length);
            this.ReadHeader();
        }

        internal void AlignTo32BitBoundary()
        {
            this.cursor.Align(4);
        }

        public void Dispose()
        {
            if (this.memmap != null)
            {
                this.memmap.Dispose();
            }
            this.memmap = null;
            this.sectionHeaders = null;
            this.identifierStringHeap = null;
            this.generalStringHeap = null;
            this.blobHeap = null;
            this.guidHeap = null;
            this.tables = null;
            this.tablesHeader = null;
            this.targetRuntimeVersion = null;
            this.tableSize = null;
            this.tableRefSize = null;
            this.tableOffset = null;
            this.HashValue = null;
        }

        internal byte[] GetBlob(int blobIndex)
        {
            if ((this.blobHeap == null) && (blobIndex == 0))
            {
                return this.EmptyBlob;
            }
            MemoryCursor cursor = this.cursor;
            cursor.Position = this.PositionOfBlob(blobIndex);
            return cursor.ReadBytes(cursor.ReadCompressedInt());
        }

        internal MemoryCursor GetBlobCursor(int blobIndex)
        {
            MemoryCursor c = this.cursor;
            c.Position = this.PositionOfBlob(blobIndex);
            c.ReadCompressedInt();
            return new MemoryCursor(c);
        }

        internal MemoryCursor GetBlobCursor(int blobIndex, out int blobLength)
        {
            MemoryCursor c = this.cursor;
            c.Position = this.PositionOfBlob(blobIndex);
            blobLength = c.ReadCompressedInt();
            return new MemoryCursor(c);
        }

        internal string GetBlobString(int blobIndex)
        {
            MemoryCursor cursor = this.cursor;
            cursor.Position = this.PositionOfBlob(blobIndex);
            int num = cursor.ReadCompressedInt();
            return cursor.ReadUTF16(num / 2);
        }

        internal byte GetByte() => 
            this.cursor.ReadByte();

        internal int GetCurrentPosition() => 
            this.cursor.Position;

        internal Guid GetGuid(int guidIndex)
        {
            int num = guidIndex * 0x10;
            if ((num < 0x10) || (this.guidHeap.size < num))
            {
                throw new ArgumentOutOfRangeException("guidIndex", ExceptionStrings.BadGuidHeapIndex);
            }
            MemoryCursor cursor = this.cursor;
            cursor.Position = ((this.mdOffset + this.guidHeap.offset) + num) - 0x10;
            return new Guid(cursor.ReadBytes(0x10));
        }

        internal Identifier GetIdentifier(int stringHeapIndex)
        {
            int offset = (this.mdOffset + this.identifierStringHeap.offset) + stringHeapIndex;
            return Identifier.For(this.cursor.GetBuffer(), offset);
        }

        internal short GetInt16() => 
            this.cursor.ReadInt16();

        internal int GetInt32() => 
            this.cursor.ReadInt32();

        internal byte GetMethodBodyHeaderByte(int RVA)
        {
            MemoryCursor cursor = this.cursor;
            cursor.Position = this.RvaToOffset(RVA);
            return cursor.ReadByte();
        }

        internal MemoryCursor GetNewCursor() => 
            new MemoryCursor(this.cursor);

        internal MemoryCursor GetNewCursor(int RVA, out PESection targetSection) => 
            new MemoryCursor(this.cursor) { Position = this.RvaToOffset(RVA, out targetSection) };

        internal int GetOffsetToEndOfSection(int virtualAddress)
        {
            foreach (SectionHeader header in this.sectionHeaders)
            {
                if ((virtualAddress >= header.virtualAddress) && (virtualAddress < (header.virtualAddress + header.sizeOfRawData)))
                {
                    return (header.sizeOfRawData - (virtualAddress - header.virtualAddress));
                }
            }
            return -1;
        }

        internal byte[] GetResourceData(int resourceOffset)
        {
            this.cursor.Position = this.resourcesOffset + resourceOffset;
            int c = this.cursor.ReadInt32();
            return this.cursor.ReadBytes(c);
        }

        internal int GetSignatureLength(int blobIndex)
        {
            MemoryCursor cursor = this.cursor;
            cursor.Position = this.PositionOfBlob(blobIndex);
            return cursor.ReadCompressedInt();
        }

        internal string GetString(int stringHeapIndex)
        {
            if ((stringHeapIndex < 0) || (this.identifierStringHeap.size <= stringHeapIndex))
            {
                throw new ArgumentOutOfRangeException("stringHeapIndex", ExceptionStrings.BadStringHeapIndex);
            }
            MemoryCursor cursor = this.cursor;
            cursor.Position = (this.mdOffset + this.identifierStringHeap.offset) + stringHeapIndex;
            return cursor.ReadUTF8();
        }

        internal ushort GetUInt16() => 
            this.cursor.ReadUInt16();

        internal string GetUserString(int stringHeapIndex)
        {
            if ((stringHeapIndex < 0) || (this.generalStringHeap.size <= stringHeapIndex))
            {
                throw new ArgumentOutOfRangeException("stringHeapIndex", ExceptionStrings.BadUserStringHeapIndex);
            }
            MemoryCursor cursor = this.cursor;
            cursor.Position = (this.mdOffset + this.generalStringHeap.offset) + stringHeapIndex;
            int num = cursor.ReadCompressedInt();
            return cursor.ReadUTF16(num / 2);
        }

        internal object GetValueFromBlob(int type, int blobIndex)
        {
            MemoryCursor cursor = this.cursor;
            cursor.Position = this.PositionOfBlob(blobIndex);
            int num = cursor.ReadCompressedInt();
            switch (((ElementType) type))
            {
                case ElementType.Boolean:
                    return cursor.ReadBoolean();

                case ElementType.Char:
                    return (char) cursor.ReadUInt16();

                case ElementType.Int8:
                    return cursor.ReadSByte();

                case ElementType.UInt8:
                    return cursor.ReadByte();

                case ElementType.Int16:
                    return cursor.ReadInt16();

                case ElementType.UInt16:
                    return cursor.ReadUInt16();

                case ElementType.Int32:
                    return cursor.ReadInt32();

                case ElementType.UInt32:
                    return cursor.ReadUInt32();

                case ElementType.Int64:
                    return cursor.ReadInt64();

                case ElementType.UInt64:
                    return cursor.ReadUInt64();

                case ElementType.Single:
                    return cursor.ReadSingle();

                case ElementType.Double:
                    return cursor.ReadDouble();

                case ElementType.String:
                    return cursor.ReadUTF16(num / 2);

                case ElementType.Class:
                    return null;
            }
            throw new InvalidMetadataException(ExceptionStrings.UnknownConstantType);
        }

        internal bool NoOffsetFor(int virtualAddress)
        {
            foreach (SectionHeader header in this.sectionHeaders)
            {
                if ((virtualAddress >= header.virtualAddress) && (virtualAddress < (header.virtualAddress + header.sizeOfRawData)))
                {
                    return false;
                }
            }
            return true;
        }

        private int PositionOfBlob(int blobIndex)
        {
            if ((blobIndex < 0) || (this.blobHeap.size <= blobIndex))
            {
                throw new ArgumentOutOfRangeException("blobIndex", ExceptionStrings.BadBlobHeapIndex);
            }
            return ((this.mdOffset + this.blobHeap.offset) + blobIndex);
        }

        private void ReadAssemblyRefTable()
        {
            int num = this.tableSize[0x23];
            AssemblyRefRow[] rowArray2 = this.assemblyRefTable = new AssemblyRefRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x23];
                for (int i = 0; i < num; i++)
                {
                    AssemblyRefRow row;
                    row.MajorVersion = cursor.ReadUInt16();
                    row.MinorVersion = cursor.ReadUInt16();
                    row.BuildNumber = cursor.ReadUInt16();
                    row.RevisionNumber = cursor.ReadUInt16();
                    row.Flags = cursor.ReadInt32();
                    row.PublicKeyOrToken = cursor.ReadReference(this.blobRefSize);
                    row.Name = cursor.ReadReference(this.stringRefSize);
                    row.Culture = cursor.ReadReference(this.stringRefSize);
                    row.HashValue = cursor.ReadReference(this.blobRefSize);
                    row.AssemblyReference = null;
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadAssemblyTable()
        {
            int num = this.tableSize[0x20];
            AssemblyRow[] rowArray2 = this.assemblyTable = new AssemblyRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x20];
                for (int i = 0; i < num; i++)
                {
                    AssemblyRow row;
                    row.HashAlgId = cursor.ReadInt32();
                    row.MajorVersion = cursor.ReadUInt16();
                    row.MinorVersion = cursor.ReadUInt16();
                    row.BuildNumber = cursor.ReadUInt16();
                    row.RevisionNumber = cursor.ReadUInt16();
                    row.Flags = cursor.ReadInt32();
                    row.PublicKey = cursor.ReadReference(this.blobRefSize);
                    row.Name = cursor.ReadReference(this.stringRefSize);
                    row.Culture = cursor.ReadReference(this.stringRefSize);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadClassLayoutTable()
        {
            int num = this.tableSize[15];
            ClassLayoutRow[] rowArray2 = this.classLayoutTable = new ClassLayoutRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[15];
                for (int i = 0; i < num; i++)
                {
                    ClassLayoutRow row;
                    row.PackingSize = cursor.ReadUInt16();
                    row.ClassSize = cursor.ReadInt32();
                    row.Parent = cursor.ReadReference(this.tableRefSize[2]);
                    rowArray2[i] = row;
                }
            }
        }

        private static CLIHeader ReadCLIHeader(MemoryCursor c)
        {
            CLIHeader header = new CLIHeader {
                cb = c.Int32(0)
            };
            c.SkipInt32(1);
            header.majorRuntimeVersion = c.UInt16(0);
            header.minorRuntimeVersion = c.UInt16(1);
            c.SkipUInt16(2);
            header.metaData = ReadDirectoryEntry(c);
            header.flags = c.Int32(0);
            header.entryPointToken = c.Int32(1);
            c.SkipInt32(2);
            header.resources = ReadDirectoryEntry(c);
            header.strongNameSignature = ReadDirectoryEntry(c);
            header.codeManagerTable = ReadDirectoryEntry(c);
            header.vtableFixups = ReadDirectoryEntry(c);
            header.exportAddressTableJumps = ReadDirectoryEntry(c);
            if (header.majorRuntimeVersion < 2)
            {
                throw new InvalidMetadataException(ExceptionStrings.BadCLIHeader);
            }
            return header;
        }

        private void ReadConstantTable()
        {
            int num = this.tableSize[11];
            ConstantRow[] rowArray2 = this.constantTable = new ConstantRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[11];
                for (int i = 0; i < num; i++)
                {
                    ConstantRow row;
                    row.Type = cursor.ReadByte();
                    cursor.ReadByte();
                    row.Parent = cursor.ReadReference(this.constantParentRefSize);
                    row.Value = cursor.ReadReference(this.blobRefSize);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadCustomAttributeTable()
        {
            int num = this.tableSize[12];
            CustomAttributeRow[] rowArray2 = this.customAttributeTable = new CustomAttributeRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[12];
                for (int i = 0; i < num; i++)
                {
                    CustomAttributeRow row;
                    row.Parent = cursor.ReadReference(this.customAttributeParentRefSize);
                    row.Constructor = cursor.ReadReference(this.customAttributeConstructorRefSize);
                    row.Value = cursor.ReadReference(this.blobRefSize);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadDeclSecurityTable()
        {
            int num = this.tableSize[14];
            DeclSecurityRow[] rowArray2 = this.declSecurityTable = new DeclSecurityRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[14];
                for (int i = 0; i < num; i++)
                {
                    DeclSecurityRow row;
                    row.Action = cursor.ReadUInt16();
                    row.Parent = cursor.ReadReference(this.declSecurityParentRefSize);
                    row.PermissionSet = cursor.ReadReference(this.blobRefSize);
                    rowArray2[i] = row;
                }
            }
        }

        private static DirectoryEntry ReadDirectoryEntry(MemoryCursor c)
        {
            DirectoryEntry entry = new DirectoryEntry {
                virtualAddress = c.Int32(0),
                size = c.Int32(1)
            };
            c.SkipInt32(2);
            return entry;
        }

        internal static void ReadDOSHeader(MemoryCursor c)
        {
            c.Position = 0;
            if (c.UInt16(0) != 0x5a4d)
            {
                throw new InvalidMetadataException(ExceptionStrings.BadMagicNumber);
            }
            c.Position = 60;
            int num2 = c.Int32(0);
            c.Position = num2;
        }

        private void ReadEventMapTable()
        {
            int num = this.tableSize[0x12];
            EventMapRow[] rowArray2 = this.eventMapTable = new EventMapRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x12];
                for (int i = 0; i < num; i++)
                {
                    EventMapRow row;
                    row.Parent = cursor.ReadReference(this.tableRefSize[2]);
                    row.EventList = cursor.ReadReference(this.tableRefSize[20]);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadEventPtrTable()
        {
            int num = this.tableSize[0x13];
            EventPtrRow[] rowArray2 = this.eventPtrTable = new EventPtrRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x13];
                for (int i = 0; i < num; i++)
                {
                    EventPtrRow row;
                    row.Event = cursor.ReadReference(this.tableRefSize[20]);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadEventTable()
        {
            int num = this.tableSize[20];
            EventRow[] rowArray2 = this.eventTable = new EventRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[20];
                for (int i = 0; i < num; i++)
                {
                    EventRow row;
                    row.Flags = cursor.ReadUInt16();
                    row.Name = cursor.ReadReference(this.stringRefSize);
                    row.EventType = cursor.ReadReference(this.typeDefOrRefOrSpecSize);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadExportedTypeTable()
        {
            int num = this.tableSize[0x27];
            ExportedTypeRow[] rowArray2 = this.exportedTypeTable = new ExportedTypeRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x27];
                for (int i = 0; i < num; i++)
                {
                    ExportedTypeRow row;
                    row.Flags = cursor.ReadInt32();
                    row.TypeDefId = cursor.ReadInt32();
                    row.TypeName = cursor.ReadReference(this.stringRefSize);
                    row.TypeNamespace = cursor.ReadReference(this.stringRefSize);
                    row.Implementation = cursor.ReadReference(this.implementationRefSize);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadFieldLayoutTable()
        {
            int num = this.tableSize[0x10];
            FieldLayoutRow[] rowArray2 = this.fieldLayoutTable = new FieldLayoutRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x10];
                for (int i = 0; i < num; i++)
                {
                    FieldLayoutRow row;
                    row.Offset = cursor.ReadInt32();
                    row.Field = cursor.ReadReference(this.tableRefSize[4]);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadFieldMarshalTable()
        {
            int num = this.tableSize[13];
            FieldMarshalRow[] rowArray2 = this.fieldMarshalTable = new FieldMarshalRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[13];
                for (int i = 0; i < num; i++)
                {
                    FieldMarshalRow row;
                    row.Parent = cursor.ReadReference(this.fieldMarshalParentRefSize);
                    row.NativeType = cursor.ReadReference(this.blobRefSize);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadFieldPtrTable()
        {
            int num = this.tableSize[3];
            FieldPtrRow[] rowArray2 = this.fieldPtrTable = new FieldPtrRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[3];
                for (int i = 0; i < num; i++)
                {
                    FieldPtrRow row;
                    row.Field = cursor.ReadReference(this.tableRefSize[4]);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadFieldRvaTable()
        {
            int num = this.tableSize[0x1d];
            FieldRvaRow[] rowArray2 = this.fieldRvaTable = new FieldRvaRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x1d];
                for (int i = 0; i < num; i++)
                {
                    FieldRvaRow row;
                    row.RVA = cursor.ReadInt32();
                    row.Field = cursor.ReadReference(this.tableRefSize[4]);
                    row.TargetSection = PESection.Text;
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadFieldTable()
        {
            int num = this.tableSize[4];
            FieldRow[] rowArray2 = this.fieldTable = new FieldRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[4];
                for (int i = 0; i < num; i++)
                {
                    FieldRow row;
                    row.Flags = cursor.ReadUInt16();
                    row.Name = cursor.ReadReference(this.stringRefSize);
                    row.Signature = cursor.ReadReference(this.blobRefSize);
                    row.Field = null;
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadFileTable()
        {
            int num = this.tableSize[0x26];
            FileRow[] rowArray2 = this.fileTable = new FileRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x26];
                for (int i = 0; i < num; i++)
                {
                    FileRow row;
                    row.Flags = cursor.ReadInt32();
                    row.Name = cursor.ReadReference(this.stringRefSize);
                    row.HashValue = cursor.ReadReference(this.blobRefSize);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadGenericParamConstraintTable()
        {
            int num = this.tableSize[0x2c];
            GenericParamConstraintRow[] rowArray2 = this.genericParamConstraintTable = new GenericParamConstraintRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x2c];
                for (int i = 0; i < num; i++)
                {
                    GenericParamConstraintRow row;
                    row.Param = cursor.ReadReference(this.tableRefSize[0x2a]);
                    row.Constraint = cursor.ReadReference(this.typeDefOrRefOrSpecSize);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadGenericParamTable()
        {
            int num = this.tableSize[0x2a];
            GenericParamRow[] rowArray2 = this.genericParamTable = new GenericParamRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x2a];
                bool flag = (this.metadataFormatMajorVersion == 1) && (this.metadataFormatMinorVersion == 0);
                bool flag2 = (this.metadataFormatMajorVersion == 1) && (this.metadataFormatMinorVersion == 1);
                for (int i = 0; i < num; i++)
                {
                    GenericParamRow row;
                    row.Number = cursor.ReadUInt16();
                    row.Flags = cursor.ReadUInt16();
                    row.Owner = cursor.ReadReference(this.typeDefOrMethodDefSize);
                    row.Name = cursor.ReadReference(this.stringRefSize);
                    row.GenericParameter = null;
                    if (flag2)
                    {
                        cursor.ReadReference(this.typeDefOrRefOrSpecSize);
                    }
                    if (flag)
                    {
                        cursor.ReadInt16();
                    }
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadHeader()
        {
            MemoryCursor c = this.cursor;
            c.Position = 0;
            ReadDOSHeader(c);
            NTHeader header = ReadNTHeader(c);
            this.dllCharacteristics = header.dllCharacteristics;
            this.linkerMajorVersion = header.majorLinkerVersion;
            this.linkerMinorVersion = header.minorLinkerVersion;
            this.fileAlignment = header.fileAlignment;
            if ((header.characteristics & 0x2000) != 0)
            {
                this.moduleKind = ModuleKindFlags.DynamicallyLinkedLibrary;
            }
            else
            {
                this.moduleKind = (header.subsystem == 3) ? ModuleKindFlags.ConsoleApplication : ModuleKindFlags.WindowsApplication;
            }
            int numberOfSections = header.numberOfSections;
            SectionHeader[] headerArray2 = this.sectionHeaders = new SectionHeader[numberOfSections];
            int index = -1;
            for (int i = 0; i < numberOfSections; i++)
            {
                headerArray2[i] = ReadSectionHeader(c);
                if (headerArray2[i].name == ".rsrc")
                {
                    index = i;
                }
            }
            if (index >= 0)
            {
                this.win32ResourcesOffset = headerArray2[index].pointerToRawData;
            }
            else
            {
                this.win32ResourcesOffset = -1;
            }
            DirectoryEntry cliHeaderTable = header.cliHeaderTable;
            int num4 = this.RvaToOffset(cliHeaderTable.virtualAddress);
            c.Position = num4;
            CLIHeader header2 = ReadCLIHeader(c);
            this.entryPointToken = header2.entryPointToken;
            if ((header2.flags & 1) != 0)
            {
                this.peKind = PEKindFlags.ILonly;
            }
            if ((header2.flags & 0x10) != 0)
            {
                this.entryPointToken = 0;
            }
            switch (header.machine)
            {
                case 0x200:
                    this.peKind |= PEKindFlags.Requires64bits;
                    break;

                case 0x8664:
                    this.peKind |= PEKindFlags.AMD | PEKindFlags.Requires64bits;
                    break;

                default:
                    if (header.magic == 0x20b)
                    {
                        this.peKind |= PEKindFlags.Requires64bits;
                    }
                    else
                    {
                        if ((header2.flags & 2) != 0)
                        {
                            this.peKind |= PEKindFlags.Requires32bits;
                        }
                        if ((header2.flags & 0x20000) != 0)
                        {
                            this.peKind |= PEKindFlags.Prefers32bits;
                        }
                    }
                    break;
            }
            this.TrackDebugData = (header2.flags & 0x10000) != 0;
            if (header2.resources.size > 0)
            {
                this.resourcesOffset = this.RvaToOffset(header2.resources.virtualAddress);
            }
            int size = header2.strongNameSignature.size;
            if (size > 0)
            {
                long num7 = this.RvaToOffset(header2.strongNameSignature.virtualAddress);
                c.Position = (int) num7;
                this.HashValue = c.ReadBytes(size);
                bool flag = true;
                for (int k = 0; k < size; k++)
                {
                    if (this.HashValue[k] != 0)
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    this.HashValue = null;
                }
            }
            long num10 = this.mdOffset = this.RvaToOffset(header2.metaData.virtualAddress);
            c.Position = (int) num10;
            MetadataHeader header3 = ReadMetadataHeader(c);
            this.targetRuntimeVersion = header3.versionString;
            foreach (StreamHeader header4 in header3.streamHeaders)
            {
                switch (header4.name)
                {
                    case "#Strings":
                        this.identifierStringHeap = header4;
                        break;

                    case "#US":
                        this.generalStringHeap = header4;
                        break;

                    case "#Blob":
                        this.blobHeap = header4;
                        break;

                    case "#GUID":
                        this.guidHeap = header4;
                        break;

                    case "#~":
                        this.tables = header4;
                        break;

                    case "#-":
                        this.tables = header4;
                        break;
                }
            }
            if (this.tables == null)
            {
                throw new InvalidMetadataException(ExceptionStrings.NoMetadataStream);
            }
            c.Position = ((int) num10) + this.tables.offset;
            TablesHeader header6 = this.tablesHeader = ReadTablesHeader(c);
            this.metadataFormatMajorVersion = header6.majorVersion;
            this.metadataFormatMinorVersion = header6.minorVersion;
            int[] numArray2 = this.tableSize = new int[0x2d];
            int[] numArray4 = this.tableRefSize = new int[0x2d];
            long maskValid = header6.maskValid;
            int[] countArray = header6.countArray;
            int num14 = 0;
            int num15 = 0;
            while (num14 < 0x2d)
            {
                if ((maskValid % 2L) == 1L)
                {
                    int num17 = numArray2[num14] = countArray[num15++];
                    numArray4[num14] = (num17 < 0x10000) ? 2 : 4;
                }
                else
                {
                    numArray4[num14] = 2;
                }
                maskValid /= 2L;
                num14++;
            }
            int num19 = this.blobRefSize = ((header6.heapSizes & 4) == 0) ? 2 : 4;
            int num21 = this.constantParentRefSize = (((numArray2[8] < 0x4000) && (numArray2[4] < 0x4000)) && (numArray2[0x17] < 0x4000)) ? 2 : 4;
            int num22 = 0;
            if ((this.metadataFormatMajorVersion > 1) || (this.metadataFormatMinorVersion > 0))
            {
                num22 = this.customAttributeParentRefSize = (((((numArray2[6] < 0x800) && (numArray2[4] < 0x800)) && ((numArray2[1] < 0x800) && (numArray2[2] < 0x800))) && (((numArray2[8] < 0x800) && (numArray2[9] < 0x800)) && ((numArray2[10] < 0x800) && (numArray2[0] < 0x800)))) && (((((numArray2[14] < 0x800) && (numArray2[0x17] < 0x800)) && ((numArray2[20] < 0x800) && (numArray2[0x11] < 0x800))) && (((numArray2[0x1a] < 0x800) && (numArray2[0x1b] < 0x800)) && ((numArray2[0x20] < 0x800) && (numArray2[0x26] < 0x800)))) && (((numArray2[0x27] < 0x800) && (numArray2[40] < 0x800)) && (((numArray2[0x2a] < 0x800) && (numArray2[0x2b] < 0x800)) && (numArray2[0x2c] < 0x800))))) ? 2 : 4;
            }
            else
            {
                num22 = this.customAttributeParentRefSize = (((((numArray2[6] < 0x800) && (numArray2[4] < 0x800)) && ((numArray2[1] < 0x800) && (numArray2[2] < 0x800))) && (((numArray2[8] < 0x800) && (numArray2[9] < 0x800)) && ((numArray2[10] < 0x800) && (numArray2[0] < 0x800)))) && ((((numArray2[14] < 0x800) && (numArray2[0x17] < 0x800)) && ((numArray2[20] < 0x800) && (numArray2[0x11] < 0x800))) && ((((numArray2[0x1a] < 0x800) && (numArray2[0x1b] < 0x800)) && ((numArray2[0x20] < 0x800) && (numArray2[0x26] < 0x800))) && ((numArray2[0x27] < 0x800) && (numArray2[40] < 0x800))))) ? 2 : 4;
            }
            int num26 = this.customAttributeConstructorRefSize = ((numArray2[6] < 0x2000) && (numArray2[10] < 0x2000)) ? 2 : 4;
            int num28 = this.declSecurityParentRefSize = (((numArray2[2] < 0x4000) && (numArray2[6] < 0x4000)) && (numArray2[0x20] < 0x4000)) ? 2 : 4;
            int num29 = this.fieldMarshalParentRefSize = ((numArray2[4] < 0x8000) && (numArray2[8] < 0x8000)) ? 2 : 4;
            int num30 = this.guidRefSize = ((header6.heapSizes & 2) == 0) ? 2 : 4;
            int num31 = this.hasSemanticRefSize = ((numArray2[20] < 0x8000) && (numArray2[0x17] < 0x8000)) ? 2 : 4;
            int num32 = this.implementationRefSize = (((numArray2[0x26] < 0x4000) && (numArray2[0x23] < 0x4000)) && (numArray2[0x27] < 0x4000)) ? 2 : 4;
            int num33 = this.methodDefOrRefSize = ((numArray2[6] < 0x8000) && (numArray2[10] < 0x8000)) ? 2 : 4;
            int num34 = this.memberRefParentSize = (((numArray2[2] < 0x2000) && (numArray2[1] < 0x2000)) && (((numArray2[0x1a] < 0x2000) && (numArray2[6] < 0x2000)) && (numArray2[0x1b] < 0x2000))) ? 2 : 4;
            int num35 = this.memberForwardedRefSize = ((numArray2[4] < 0x8000) && (numArray2[6] < 0x8000)) ? 2 : 4;
            int num36 = this.typeDefOrMethodDefSize = ((numArray2[2] < 0x8000) && (numArray2[6] < 0x8000)) ? 2 : 4;
            int num37 = this.typeDefOrRefOrSpecSize = (((numArray2[2] < 0x4000) && (numArray2[1] < 0x4000)) && (numArray2[0x1b] < 0x4000)) ? 2 : 4;
            int num38 = this.resolutionScopeRefSize = (((numArray2[0] < 0x4000) && (numArray2[0x1a] < 0x4000)) && ((numArray2[0x23] < 0x4000) && (numArray2[1] < 0x4000))) ? 2 : 4;
            int num39 = this.stringRefSize = ((header6.heapSizes & 1) == 0) ? 2 : 4;
            int[] numArray6 = this.tableOffset = new int[0x2d];
            int num40 = ((this.mdOffset + this.tables.offset) + 0x18) + (countArray.Length * 4);
            for (int j = 0; j < 0x2d; j++)
            {
                int num42 = numArray2[j];
                if (num42 != 0)
                {
                    numArray6[j] = num40;
                    switch (((TableIndices) j))
                    {
                        case TableIndices.Module:
                        {
                            num40 += num42 * ((2 + num39) + (3 * num30));
                            continue;
                        }
                        case TableIndices.TypeRef:
                        {
                            num40 += num42 * (num38 + (2 * num39));
                            continue;
                        }
                        case TableIndices.TypeDef:
                        {
                            num40 += num42 * ((((4 + (2 * num39)) + num37) + numArray4[4]) + numArray4[6]);
                            continue;
                        }
                        case TableIndices.FieldPtr:
                        {
                            num40 += num42 * numArray4[4];
                            continue;
                        }
                        case TableIndices.Field:
                        {
                            num40 += num42 * ((2 + num39) + num19);
                            continue;
                        }
                        case TableIndices.MethodPtr:
                        {
                            num40 += num42 * numArray4[6];
                            continue;
                        }
                        case TableIndices.Method:
                        {
                            num40 += num42 * (((8 + num39) + num19) + numArray4[8]);
                            continue;
                        }
                        case TableIndices.ParamPtr:
                        {
                            num40 += num42 * numArray4[8];
                            continue;
                        }
                        case TableIndices.Param:
                        {
                            num40 += num42 * (4 + num39);
                            continue;
                        }
                        case TableIndices.InterfaceImpl:
                        {
                            num40 += num42 * (numArray4[2] + num37);
                            continue;
                        }
                        case TableIndices.MemberRef:
                        {
                            num40 += num42 * ((num34 + num39) + num19);
                            continue;
                        }
                        case TableIndices.Constant:
                        {
                            num40 += num42 * ((2 + num21) + num19);
                            continue;
                        }
                        case TableIndices.CustomAttribute:
                        {
                            num40 += num42 * ((num22 + num26) + num19);
                            continue;
                        }
                        case TableIndices.FieldMarshal:
                        {
                            num40 += num42 * (num29 + num19);
                            continue;
                        }
                        case TableIndices.DeclSecurity:
                        {
                            num40 += num42 * ((2 + num28) + num19);
                            continue;
                        }
                        case TableIndices.ClassLayout:
                        {
                            num40 += num42 * (6 + numArray4[2]);
                            continue;
                        }
                        case TableIndices.FieldLayout:
                        {
                            num40 += num42 * (4 + numArray4[4]);
                            continue;
                        }
                        case TableIndices.StandAloneSig:
                        {
                            num40 += num42 * num19;
                            continue;
                        }
                        case TableIndices.EventMap:
                        {
                            num40 += num42 * (numArray4[2] + numArray4[20]);
                            continue;
                        }
                        case TableIndices.EventPtr:
                        {
                            num40 += num42 * numArray4[20];
                            continue;
                        }
                        case TableIndices.Event:
                        {
                            num40 += num42 * ((2 + num39) + num37);
                            continue;
                        }
                        case TableIndices.PropertyMap:
                        {
                            num40 += num42 * (numArray4[2] + numArray4[0x17]);
                            continue;
                        }
                        case TableIndices.PropertyPtr:
                        {
                            num40 += num42 * numArray4[0x17];
                            continue;
                        }
                        case TableIndices.Property:
                        {
                            num40 += num42 * ((2 + num39) + num19);
                            continue;
                        }
                        case TableIndices.MethodSemantics:
                        {
                            num40 += num42 * ((2 + numArray4[6]) + num31);
                            continue;
                        }
                        case TableIndices.MethodImpl:
                        {
                            num40 += num42 * (numArray4[2] + (2 * num33));
                            continue;
                        }
                        case TableIndices.ModuleRef:
                        {
                            num40 += num42 * num39;
                            continue;
                        }
                        case TableIndices.TypeSpec:
                        {
                            num40 += num42 * num19;
                            continue;
                        }
                        case TableIndices.ImplMap:
                        {
                            num40 += num42 * (((2 + num35) + num39) + numArray4[0x1a]);
                            continue;
                        }
                        case TableIndices.FieldRva:
                        {
                            num40 += num42 * (4 + numArray4[4]);
                            continue;
                        }
                        case TableIndices.EncLog:
                            throw new InvalidMetadataException(ExceptionStrings.ENCLogTableEncountered);

                        case TableIndices.EncMap:
                            throw new InvalidMetadataException(ExceptionStrings.ENCMapTableEncountered);

                        case TableIndices.Assembly:
                        {
                            num40 += num42 * ((0x10 + num19) + (2 * num39));
                            continue;
                        }
                        case TableIndices.AssemblyProcessor:
                        {
                            num40 += num42 * 4;
                            continue;
                        }
                        case TableIndices.AssemblyOS:
                        {
                            num40 += num42 * 12;
                            continue;
                        }
                        case TableIndices.AssemblyRef:
                        {
                            num40 += num42 * ((12 + (2 * num19)) + (2 * num39));
                            continue;
                        }
                        case TableIndices.AssemblyRefProcessor:
                        {
                            num40 += num42 * (4 + numArray4[0x23]);
                            continue;
                        }
                        case TableIndices.AssemblyRefOS:
                        {
                            num40 += num42 * (12 + numArray4[0x23]);
                            continue;
                        }
                        case TableIndices.File:
                        {
                            num40 += num42 * ((4 + num39) + num19);
                            continue;
                        }
                        case TableIndices.ExportedType:
                        {
                            num40 += num42 * ((8 + (2 * num39)) + num32);
                            continue;
                        }
                        case TableIndices.ManifestResource:
                        {
                            num40 += num42 * ((8 + num39) + num32);
                            continue;
                        }
                        case TableIndices.NestedClass:
                        {
                            num40 += num42 * (2 * numArray4[2]);
                            continue;
                        }
                        case TableIndices.GenericParam:
                        {
                            if ((this.metadataFormatMajorVersion == 1) && (this.metadataFormatMinorVersion == 0))
                            {
                                num40 += num42 * (((6 + num36) + num39) + num37);
                            }
                            else if ((this.metadataFormatMajorVersion == 1) && (this.metadataFormatMinorVersion == 1))
                            {
                                num40 += num42 * (((4 + num36) + num39) + num37);
                            }
                            else
                            {
                                num40 += num42 * ((4 + num36) + num39);
                            }
                            continue;
                        }
                        case TableIndices.MethodSpec:
                        {
                            num40 += num42 * (num33 + num19);
                            continue;
                        }
                        case TableIndices.GenericParamConstraint:
                        {
                            num40 += num42 * (numArray4[0x2a] + num37);
                            continue;
                        }
                    }
                    throw new InvalidMetadataException(ExceptionStrings.UnsupportedTableEncountered);
                }
            }
        }

        private void ReadImplMapTable()
        {
            int num = this.tableSize[0x1c];
            ImplMapRow[] rowArray2 = this.implMapTable = new ImplMapRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x1c];
                for (int i = 0; i < num; i++)
                {
                    ImplMapRow row;
                    row.MappingFlags = cursor.ReadUInt16();
                    row.MemberForwarded = cursor.ReadReference(this.memberForwardedRefSize);
                    row.ImportName = cursor.ReadReference(this.stringRefSize);
                    row.ImportScope = cursor.ReadReference(this.tableRefSize[0x1a]);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadInterfaceImplTable()
        {
            int num = this.tableSize[9];
            InterfaceImplRow[] rowArray2 = this.interfaceImplTable = new InterfaceImplRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[9];
                for (int i = 0; i < num; i++)
                {
                    InterfaceImplRow row;
                    row.Class = cursor.ReadReference(this.tableRefSize[2]);
                    row.Interface = cursor.ReadReference(this.typeDefOrRefOrSpecSize);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadManifestResourceTable()
        {
            int num = this.tableSize[40];
            ManifestResourceRow[] rowArray2 = this.manifestResourceTable = new ManifestResourceRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[40];
                for (int i = 0; i < num; i++)
                {
                    ManifestResourceRow row;
                    row.Offset = cursor.ReadInt32();
                    row.Flags = cursor.ReadInt32();
                    row.Name = cursor.ReadReference(this.stringRefSize);
                    row.Implementation = cursor.ReadReference(this.implementationRefSize);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadMemberRefTable()
        {
            int num = this.tableSize[10];
            MemberRefRow[] rowArray2 = this.memberRefTable = new MemberRefRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[10];
                for (int i = 0; i < num; i++)
                {
                    MemberRefRow row;
                    row.Class = cursor.ReadReference(this.memberRefParentSize);
                    row.Name = cursor.ReadReference(this.stringRefSize);
                    row.Signature = cursor.ReadReference(this.blobRefSize);
                    row.Member = null;
                    row.VarargTypes = null;
                    rowArray2[i] = row;
                }
            }
        }

        private static MetadataHeader ReadMetadataHeader(MemoryCursor c)
        {
            MetadataHeader header = new MetadataHeader {
                signature = c.ReadInt32()
            };
            if (header.signature != 0x424a5342)
            {
                throw new InvalidMetadataException(ExceptionStrings.BadMetadataHeaderSignature);
            }
            header.majorVersion = c.ReadUInt16();
            header.minorVersion = c.ReadUInt16();
            header.reserved = c.ReadInt32();
            int bytesToRead = c.ReadInt32();
            header.versionString = c.ReadASCII(bytesToRead);
            while ((bytesToRead++ % 4) != 0)
            {
                c.ReadByte();
            }
            header.flags = c.ReadUInt16();
            int num2 = c.ReadUInt16();
            StreamHeader[] headerArray2 = header.streamHeaders = new StreamHeader[num2];
            for (int i = 0; i < num2; i++)
            {
                headerArray2[i] = ReadStreamHeader(c);
            }
            return header;
        }

        private void ReadMethodImplTable()
        {
            int num = this.tableSize[0x19];
            MethodImplRow[] rowArray2 = this.methodImplTable = new MethodImplRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x19];
                for (int i = 0; i < num; i++)
                {
                    MethodImplRow row;
                    row.Class = cursor.ReadReference(this.tableRefSize[2]);
                    row.MethodBody = cursor.ReadReference(this.methodDefOrRefSize);
                    row.MethodDeclaration = cursor.ReadReference(this.methodDefOrRefSize);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadMethodPtrTable()
        {
            int num = this.tableSize[5];
            MethodPtrRow[] rowArray2 = this.methodPtrTable = new MethodPtrRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[5];
                for (int i = 0; i < num; i++)
                {
                    MethodPtrRow row;
                    row.Method = cursor.ReadReference(this.tableRefSize[6]);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadMethodSemanticsTable()
        {
            int num = this.tableSize[0x18];
            MethodSemanticsRow[] rowArray2 = this.methodSemanticsTable = new MethodSemanticsRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x18];
                for (int i = 0; i < num; i++)
                {
                    MethodSemanticsRow row;
                    row.Semantics = cursor.ReadUInt16();
                    row.Method = cursor.ReadReference(this.tableRefSize[6]);
                    row.Association = cursor.ReadReference(this.hasSemanticRefSize);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadMethodSpecTable()
        {
            int num = this.tableSize[0x2b];
            MethodSpecRow[] rowArray2 = this.methodSpecTable = new MethodSpecRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x2b];
                for (int i = 0; i < num; i++)
                {
                    MethodSpecRow row;
                    row.Method = cursor.ReadReference(this.methodDefOrRefSize);
                    row.Instantiation = cursor.ReadReference(this.blobRefSize);
                    row.InstantiatedMethod = null;
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadMethodTable()
        {
            int num = this.tableSize[6];
            MethodRow[] rowArray2 = this.methodTable = new MethodRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[6];
                for (int i = 0; i < num; i++)
                {
                    MethodRow row;
                    row.RVA = cursor.ReadInt32();
                    row.ImplFlags = cursor.ReadUInt16();
                    row.Flags = cursor.ReadUInt16();
                    row.Name = cursor.ReadReference(this.stringRefSize);
                    row.Signature = cursor.ReadReference(this.blobRefSize);
                    row.ParamList = cursor.ReadReference(this.tableRefSize[8]);
                    row.Method = null;
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadModuleRefTable()
        {
            int num = this.tableSize[0x1a];
            ModuleRefRow[] rowArray2 = this.moduleRefTable = new ModuleRefRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x1a];
                for (int i = 0; i < num; i++)
                {
                    ModuleRefRow row;
                    row.Name = cursor.ReadReference(this.stringRefSize);
                    row.Module = null;
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadModuleTable()
        {
            int num = this.tableSize[0];
            ModuleRow[] rowArray2 = this.moduleTable = new ModuleRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0];
                for (int i = 0; i < num; i++)
                {
                    ModuleRow row;
                    row.Generation = cursor.ReadUInt16();
                    row.Name = cursor.ReadReference(this.stringRefSize);
                    row.Mvid = cursor.ReadReference(this.guidRefSize);
                    row.EncId = cursor.ReadReference(this.guidRefSize);
                    row.EncBaseId = cursor.ReadReference(this.guidRefSize);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadNestedClassTable()
        {
            int num = this.tableSize[0x29];
            NestedClassRow[] rowArray2 = this.nestedClassTable = new NestedClassRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x29];
                for (int i = 0; i < num; i++)
                {
                    NestedClassRow row;
                    row.NestedClass = cursor.ReadReference(this.tableRefSize[2]);
                    row.EnclosingClass = cursor.ReadReference(this.tableRefSize[2]);
                    rowArray2[i] = row;
                }
            }
        }

        internal static NTHeader ReadNTHeader(MemoryCursor c)
        {
            NTHeader header = new NTHeader {
                signature = c.ReadInt32(),
                machine = c.ReadUInt16(),
                numberOfSections = c.ReadUInt16(),
                timeDateStamp = c.ReadInt32(),
                pointerToSymbolTable = c.ReadInt32(),
                numberOfSymbols = c.ReadInt32(),
                sizeOfOptionalHeader = c.ReadUInt16(),
                characteristics = c.ReadUInt16(),
                magic = c.ReadUInt16(),
                majorLinkerVersion = c.ReadByte(),
                minorLinkerVersion = c.ReadByte(),
                sizeOfCode = c.ReadInt32(),
                sizeOfInitializedData = c.ReadInt32(),
                sizeOfUninitializedData = c.ReadInt32(),
                addressOfEntryPoint = c.ReadInt32(),
                baseOfCode = c.ReadInt32()
            };
            if (header.magic == 0x10b)
            {
                header.baseOfData = c.ReadInt32();
                header.imageBase = c.ReadInt32();
            }
            else
            {
                header.baseOfData = 0;
                header.imageBase = c.ReadInt64();
            }
            header.sectionAlignment = c.ReadInt32();
            header.fileAlignment = c.ReadInt32();
            header.majorOperatingSystemVersion = c.ReadUInt16();
            header.minorOperatingSystemVersion = c.ReadUInt16();
            header.majorImageVersion = c.ReadUInt16();
            header.minorImageVersion = c.ReadUInt16();
            header.majorSubsystemVersion = c.ReadUInt16();
            header.minorSubsystemVersion = c.ReadUInt16();
            header.win32VersionValue = c.ReadInt32();
            header.sizeOfImage = c.ReadInt32();
            header.sizeOfHeaders = c.ReadInt32();
            header.checkSum = c.ReadInt32();
            header.subsystem = c.ReadUInt16();
            header.dllCharacteristics = c.ReadUInt16();
            if (header.magic == 0x10b)
            {
                header.sizeOfStackReserve = c.ReadInt32();
                header.sizeOfStackCommit = c.ReadInt32();
                header.sizeOfHeapReserve = c.ReadInt32();
                header.sizeOfHeapCommit = c.ReadInt32();
            }
            else
            {
                header.sizeOfStackReserve = c.ReadInt64();
                header.sizeOfStackCommit = c.ReadInt64();
                header.sizeOfHeapReserve = c.ReadInt64();
                header.sizeOfHeapCommit = c.ReadInt64();
            }
            header.loaderFlags = c.ReadInt32();
            header.numberOfDataDirectories = c.ReadInt32();
            if (header.signature != 0x4550)
            {
                throw new InvalidMetadataException(ExceptionStrings.BadCOFFHeaderSignature);
            }
            if ((header.magic != 0x10b) && (header.magic != 0x20b))
            {
                throw new InvalidMetadataException(ExceptionStrings.BadPEHeaderMagicNumber);
            }
            header.exportTable = ReadDirectoryEntry(c);
            header.importTable = ReadDirectoryEntry(c);
            header.resourceTable = ReadDirectoryEntry(c);
            header.exceptionTable = ReadDirectoryEntry(c);
            header.certificateTable = ReadDirectoryEntry(c);
            header.baseRelocationTable = ReadDirectoryEntry(c);
            header.debugTable = ReadDirectoryEntry(c);
            header.copyrightTable = ReadDirectoryEntry(c);
            header.globalPointerTable = ReadDirectoryEntry(c);
            header.threadLocalStorageTable = ReadDirectoryEntry(c);
            header.loadConfigTable = ReadDirectoryEntry(c);
            header.boundImportTable = ReadDirectoryEntry(c);
            header.importAddressTable = ReadDirectoryEntry(c);
            header.delayImportTable = ReadDirectoryEntry(c);
            header.cliHeaderTable = ReadDirectoryEntry(c);
            header.reserved = ReadDirectoryEntry(c);
            return header;
        }

        private void ReadParamPtrTable()
        {
            int num = this.tableSize[7];
            ParamPtrRow[] rowArray2 = this.paramPtrTable = new ParamPtrRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[7];
                for (int i = 0; i < num; i++)
                {
                    ParamPtrRow row;
                    row.Param = cursor.ReadReference(this.tableRefSize[8]);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadParamTable()
        {
            int num = this.tableSize[8];
            ParamRow[] rowArray2 = this.paramTable = new ParamRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[8];
                for (int i = 0; i < num; i++)
                {
                    ParamRow row;
                    row.Flags = cursor.ReadUInt16();
                    row.Sequence = cursor.ReadUInt16();
                    row.Name = cursor.ReadReference(this.stringRefSize);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadPropertyMapTable()
        {
            int num = this.tableSize[0x15];
            PropertyMapRow[] rowArray2 = this.propertyMapTable = new PropertyMapRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x15];
                for (int i = 0; i < num; i++)
                {
                    PropertyMapRow row;
                    row.Parent = cursor.ReadReference(this.tableRefSize[2]);
                    row.PropertyList = cursor.ReadReference(this.tableRefSize[0x17]);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadPropertyPtrTable()
        {
            int num = this.tableSize[0x16];
            PropertyPtrRow[] rowArray2 = this.propertyPtrTable = new PropertyPtrRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x16];
                for (int i = 0; i < num; i++)
                {
                    PropertyPtrRow row;
                    row.Property = cursor.ReadReference(this.tableRefSize[0x17]);
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadPropertyTable()
        {
            int num = this.tableSize[0x17];
            PropertyRow[] rowArray2 = this.propertyTable = new PropertyRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x17];
                for (int i = 0; i < num; i++)
                {
                    PropertyRow row;
                    row.Flags = cursor.ReadUInt16();
                    row.Name = cursor.ReadReference(this.stringRefSize);
                    row.Signature = cursor.ReadReference(this.blobRefSize);
                    rowArray2[i] = row;
                }
            }
        }

        internal static SectionHeader ReadSectionHeader(MemoryCursor c)
        {
            SectionHeader header = new SectionHeader {
                name = c.ReadASCII(8),
                virtualSize = c.Int32(0),
                virtualAddress = c.Int32(1),
                sizeOfRawData = c.Int32(2),
                pointerToRawData = c.Int32(3),
                pointerToRelocations = c.Int32(4),
                pointerToLinenumbers = c.Int32(5)
            };
            c.SkipInt32(6);
            header.numberOfRelocations = c.UInt16(0);
            header.numberOfLinenumbers = c.UInt16(1);
            c.SkipInt16(2);
            header.characteristics = c.Int32(0);
            c.SkipInt32(1);
            return header;
        }

        private void ReadStandAloneSigTable()
        {
            int num = this.tableSize[0x11];
            StandAloneSigRow[] rowArray2 = this.standAloneSigTable = new StandAloneSigRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x11];
                for (int i = 0; i < num; i++)
                {
                    StandAloneSigRow row;
                    row.Signature = cursor.ReadReference(this.blobRefSize);
                    rowArray2[i] = row;
                }
            }
        }

        private static StreamHeader ReadStreamHeader(MemoryCursor c)
        {
            StreamHeader header = new StreamHeader {
                offset = c.ReadInt32(),
                size = c.ReadInt32(),
                name = c.ReadASCII()
            };
            int num = header.name.Length + 1;
            c.Position += (4 - (num % 4)) % 4;
            return header;
        }

        private static TablesHeader ReadTablesHeader(MemoryCursor c)
        {
            TablesHeader header = new TablesHeader {
                reserved = c.ReadInt32(),
                majorVersion = c.ReadByte(),
                minorVersion = c.ReadByte(),
                heapSizes = c.ReadByte(),
                rowId = c.ReadByte(),
                maskValid = c.ReadInt64(),
                maskSorted = c.ReadInt64()
            };
            int num = 0;
            for (ulong i = (ulong) header.maskValid; i != 0L; i /= (ulong) 2L)
            {
                if ((i % ((ulong) 2L)) == 1L)
                {
                    num++;
                }
            }
            int[] numArray2 = header.countArray = new int[num];
            for (int j = 0; j < num; j++)
            {
                numArray2[j] = c.ReadInt32();
            }
            return header;
        }

        private void ReadTypeDefTable()
        {
            int num = this.tableSize[2];
            TypeDefRow[] rowArray2 = this.typeDefTable = new TypeDefRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[2];
                for (int i = 0; i < num; i++)
                {
                    TypeDefRow row;
                    row.Flags = cursor.ReadInt32();
                    row.Name = cursor.ReadReference(this.stringRefSize);
                    row.Namespace = cursor.ReadReference(this.stringRefSize);
                    row.Extends = cursor.ReadReference(this.typeDefOrRefOrSpecSize);
                    row.FieldList = cursor.ReadReference(this.tableRefSize[4]);
                    row.MethodList = cursor.ReadReference(this.tableRefSize[6]);
                    row.Type = null;
                    row.NameKey = 0;
                    row.NamespaceId = null;
                    row.NamespaceKey = 0;
                    rowArray2[i] = row;
                }
                for (int j = 0; j < num; j++)
                {
                    rowArray2[j].NameKey = this.GetIdentifier(rowArray2[j].Name).UniqueIdKey;
                    rowArray2[j].NamespaceId = this.GetIdentifier(rowArray2[j].Namespace);
                    rowArray2[j].NamespaceKey = rowArray2[j].NamespaceId.UniqueIdKey;
                }
            }
        }

        private void ReadTypeRefTable()
        {
            int num = this.tableSize[1];
            TypeRefRow[] rowArray2 = this.typeRefTable = new TypeRefRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[1];
                for (int i = 0; i < num; i++)
                {
                    TypeRefRow row;
                    row.ResolutionScope = cursor.ReadReference(this.resolutionScopeRefSize);
                    row.Name = cursor.ReadReference(this.stringRefSize);
                    row.Namespace = cursor.ReadReference(this.stringRefSize);
                    row.Type = null;
                    rowArray2[i] = row;
                }
            }
        }

        private void ReadTypeSpecTable()
        {
            int num = this.tableSize[0x1b];
            TypeSpecRow[] rowArray2 = this.typeSpecTable = new TypeSpecRow[num];
            if (num != 0)
            {
                MemoryCursor cursor = this.cursor;
                cursor.Position = this.tableOffset[0x1b];
                for (int i = 0; i < num; i++)
                {
                    TypeSpecRow row;
                    row.Signature = cursor.ReadReference(this.blobRefSize);
                    row.Type = null;
                    rowArray2[i] = row;
                }
            }
        }

        private Win32Resource ReadWin32ResourceDataEntry(MemoryCursor c, int position, string TypeName, int TypeID, string Name, int ID, int LanguageID)
        {
            Win32Resource resource = new Win32Resource {
                TypeName = TypeName,
                TypeId = TypeID,
                Name = Name,
                Id = ID,
                LanguageId = LanguageID
            };
            c = new MemoryCursor(c);
            c.Position = position;
            int virtualAddress = c.ReadInt32();
            int num2 = c.ReadInt32();
            resource.CodePage = c.ReadInt32();
            c.Position = this.RvaToOffset(virtualAddress);
            resource.Data = c.ReadBytes(num2);
            return resource;
        }

        private static int ReadWin32ResourceDirectoryHeader(MemoryCursor c)
        {
            c.ReadInt32();
            c.ReadInt32();
            c.ReadInt32();
            int num = c.ReadUInt16();
            int num2 = c.ReadUInt16();
            return (num + num2);
        }

        internal Win32ResourceList ReadWin32Resources()
        {
            Win32ResourceList list = new Win32ResourceList();
            int num = this.win32ResourcesOffset;
            if (num >= 0)
            {
                MemoryCursor c = this.cursor;
                c.Position = num;
                int num2 = ReadWin32ResourceDirectoryHeader(c);
                for (int i = 0; i < num2; i++)
                {
                    string typeName = null;
                    int typeID = c.ReadInt32();
                    if (typeID < 0)
                    {
                        MemoryCursor cursor2 = new MemoryCursor(c) {
                            Position = num + (typeID & 0x7fffffff)
                        };
                        int charsToRead = cursor2.ReadUInt16();
                        typeName = cursor2.ReadUTF16(charsToRead);
                    }
                    int num6 = c.ReadInt32();
                    if (num6 >= 0)
                    {
                        list.Add(this.ReadWin32ResourceDataEntry(c, num + num6, typeName, typeID, null, 0, 0));
                    }
                    else
                    {
                        MemoryCursor cursor3 = new MemoryCursor(c) {
                            Position = num + (num6 & 0x7fffffff)
                        };
                        int num7 = ReadWin32ResourceDirectoryHeader(cursor3);
                        for (int j = 0; j < num7; j++)
                        {
                            string name = null;
                            int iD = cursor3.ReadInt32();
                            if (iD < 0)
                            {
                                MemoryCursor cursor4 = new MemoryCursor(c);
                                int num10 = cursor4.ReadUInt16();
                                name = cursor4.ReadUTF16(num10);
                            }
                            num6 = cursor3.ReadInt32();
                            if (num6 >= 0)
                            {
                                list.Add(this.ReadWin32ResourceDataEntry(c, num + num6, typeName, typeID, name, iD, 0));
                            }
                            else
                            {
                                MemoryCursor cursor5 = new MemoryCursor(c) {
                                    Position = num + (num6 & 0x7fffffff)
                                };
                                int num11 = ReadWin32ResourceDirectoryHeader(cursor5);
                                for (int k = 0; k < num11; k++)
                                {
                                    int languageID = cursor5.ReadInt32();
                                    num6 = cursor5.ReadInt32();
                                    list.Add(this.ReadWin32ResourceDataEntry(c, num + num6, typeName, typeID, name, iD, languageID));
                                }
                            }
                        }
                    }
                }
            }
            return list;
        }

        private int RvaToOffset(int virtualAddress)
        {
            foreach (SectionHeader header in this.sectionHeaders)
            {
                if ((virtualAddress >= header.virtualAddress) && (virtualAddress < (header.virtualAddress + header.sizeOfRawData)))
                {
                    return ((virtualAddress - header.virtualAddress) + header.pointerToRawData);
                }
            }
            throw new InvalidMetadataException(string.Format(CultureInfo.CurrentCulture, ExceptionStrings.UnknownVirtualAddress, new object[] { virtualAddress }));
        }

        private int RvaToOffset(int virtualAddress, out PESection targetSection)
        {
            foreach (SectionHeader header in this.sectionHeaders)
            {
                if ((virtualAddress >= header.virtualAddress) && (virtualAddress < (header.virtualAddress + header.sizeOfRawData)))
                {
                    if (header.name == ".tls")
                    {
                        targetSection = PESection.TLS;
                    }
                    else if (header.name == ".sdata")
                    {
                        targetSection = PESection.SData;
                    }
                    else
                    {
                        targetSection = PESection.Text;
                    }
                    return ((virtualAddress - header.virtualAddress) + header.pointerToRawData);
                }
            }
            throw new InvalidMetadataException(string.Format(CultureInfo.CurrentCulture, ExceptionStrings.UnknownVirtualAddress, new object[] { virtualAddress }));
        }

        internal void SetCurrentPosition(int pos)
        {
            this.cursor.Position = pos;
        }

        internal void Skip(int bytes)
        {
            this.cursor.SkipByte(bytes);
        }

        internal AssemblyRefRow[] AssemblyRefTable
        {
            get
            {
                if (this.assemblyRefTable == null)
                {
                    this.ReadAssemblyRefTable();
                }
                return this.assemblyRefTable;
            }
        }

        internal AssemblyRow[] AssemblyTable
        {
            get
            {
                if (this.assemblyTable == null)
                {
                    this.ReadAssemblyTable();
                }
                return this.assemblyTable;
            }
        }

        internal ClassLayoutRow[] ClassLayoutTable
        {
            get
            {
                if (this.classLayoutTable == null)
                {
                    this.ReadClassLayoutTable();
                }
                return this.classLayoutTable;
            }
        }

        internal ConstantRow[] ConstantTable
        {
            get
            {
                if (this.constantTable == null)
                {
                    this.ReadConstantTable();
                }
                return this.constantTable;
            }
        }

        internal CustomAttributeRow[] CustomAttributeTable
        {
            get
            {
                if (this.customAttributeTable == null)
                {
                    this.ReadCustomAttributeTable();
                }
                return this.customAttributeTable;
            }
        }

        internal DeclSecurityRow[] DeclSecurityTable
        {
            get
            {
                if (this.declSecurityTable == null)
                {
                    this.ReadDeclSecurityTable();
                }
                return this.declSecurityTable;
            }
        }

        internal byte[] EmptyBlob
        {
            get
            {
                if (this.emptyBlob == null)
                {
                    this.emptyBlob = new byte[0];
                }
                return this.emptyBlob;
            }
        }

        internal EventMapRow[] EventMapTable
        {
            get
            {
                if (this.eventMapTable == null)
                {
                    this.ReadEventMapTable();
                }
                return this.eventMapTable;
            }
        }

        internal EventPtrRow[] EventPtrTable
        {
            get
            {
                if (this.eventPtrTable == null)
                {
                    this.ReadEventPtrTable();
                }
                return this.eventPtrTable;
            }
        }

        internal EventRow[] EventTable
        {
            get
            {
                if (this.eventTable == null)
                {
                    this.ReadEventTable();
                }
                return this.eventTable;
            }
        }

        internal ExportedTypeRow[] ExportedTypeTable
        {
            get
            {
                if (this.exportedTypeTable == null)
                {
                    this.ReadExportedTypeTable();
                }
                return this.exportedTypeTable;
            }
        }

        internal FieldLayoutRow[] FieldLayoutTable
        {
            get
            {
                if (this.fieldLayoutTable == null)
                {
                    this.ReadFieldLayoutTable();
                }
                return this.fieldLayoutTable;
            }
        }

        internal FieldMarshalRow[] FieldMarshalTable
        {
            get
            {
                if (this.fieldMarshalTable == null)
                {
                    this.ReadFieldMarshalTable();
                }
                return this.fieldMarshalTable;
            }
        }

        internal FieldPtrRow[] FieldPtrTable
        {
            get
            {
                if (this.fieldPtrTable == null)
                {
                    this.ReadFieldPtrTable();
                }
                return this.fieldPtrTable;
            }
        }

        internal FieldRvaRow[] FieldRvaTable
        {
            get
            {
                if (this.fieldRvaTable == null)
                {
                    this.ReadFieldRvaTable();
                }
                return this.fieldRvaTable;
            }
        }

        internal FieldRow[] FieldTable
        {
            get
            {
                if (this.fieldTable == null)
                {
                    this.ReadFieldTable();
                }
                return this.fieldTable;
            }
        }

        internal FileRow[] FileTable
        {
            get
            {
                if (this.fileTable == null)
                {
                    this.ReadFileTable();
                }
                return this.fileTable;
            }
        }

        internal GenericParamConstraintRow[] GenericParamConstraintTable
        {
            get
            {
                if (this.genericParamConstraintTable == null)
                {
                    this.ReadGenericParamConstraintTable();
                }
                return this.genericParamConstraintTable;
            }
        }

        internal GenericParamRow[] GenericParamTable
        {
            get
            {
                if (this.genericParamTable == null)
                {
                    this.ReadGenericParamTable();
                }
                return this.genericParamTable;
            }
        }

        internal ImplMapRow[] ImplMapTable
        {
            get
            {
                if (this.implMapTable == null)
                {
                    this.ReadImplMapTable();
                }
                return this.implMapTable;
            }
        }

        internal InterfaceImplRow[] InterfaceImplTable
        {
            get
            {
                if (this.interfaceImplTable == null)
                {
                    this.ReadInterfaceImplTable();
                }
                return this.interfaceImplTable;
            }
        }

        internal ManifestResourceRow[] ManifestResourceTable
        {
            get
            {
                if (this.manifestResourceTable == null)
                {
                    this.ReadManifestResourceTable();
                }
                return this.manifestResourceTable;
            }
        }

        internal MemberRefRow[] MemberRefTable
        {
            get
            {
                if (this.memberRefTable == null)
                {
                    this.ReadMemberRefTable();
                }
                return this.memberRefTable;
            }
        }

        internal MethodImplRow[] MethodImplTable
        {
            get
            {
                if (this.methodImplTable == null)
                {
                    this.ReadMethodImplTable();
                }
                return this.methodImplTable;
            }
        }

        internal MethodPtrRow[] MethodPtrTable
        {
            get
            {
                if (this.methodPtrTable == null)
                {
                    this.ReadMethodPtrTable();
                }
                return this.methodPtrTable;
            }
        }

        internal MethodSemanticsRow[] MethodSemanticsTable
        {
            get
            {
                if (this.methodSemanticsTable == null)
                {
                    this.ReadMethodSemanticsTable();
                }
                return this.methodSemanticsTable;
            }
        }

        internal MethodSpecRow[] MethodSpecTable
        {
            get
            {
                if (this.methodSpecTable == null)
                {
                    this.ReadMethodSpecTable();
                }
                return this.methodSpecTable;
            }
        }

        internal MethodRow[] MethodTable
        {
            get
            {
                if (this.methodTable == null)
                {
                    this.ReadMethodTable();
                }
                return this.methodTable;
            }
        }

        internal ModuleRefRow[] ModuleRefTable
        {
            get
            {
                if (this.moduleRefTable == null)
                {
                    this.ReadModuleRefTable();
                }
                return this.moduleRefTable;
            }
        }

        internal ModuleRow[] ModuleTable
        {
            get
            {
                if (this.moduleTable == null)
                {
                    this.ReadModuleTable();
                }
                return this.moduleTable;
            }
        }

        internal NestedClassRow[] NestedClassTable
        {
            get
            {
                if (this.nestedClassTable == null)
                {
                    this.ReadNestedClassTable();
                }
                return this.nestedClassTable;
            }
        }

        internal ParamPtrRow[] ParamPtrTable
        {
            get
            {
                if (this.paramPtrTable == null)
                {
                    this.ReadParamPtrTable();
                }
                return this.paramPtrTable;
            }
        }

        internal ParamRow[] ParamTable
        {
            get
            {
                if (this.paramTable == null)
                {
                    this.ReadParamTable();
                }
                return this.paramTable;
            }
        }

        internal PropertyMapRow[] PropertyMapTable
        {
            get
            {
                if (this.propertyMapTable == null)
                {
                    this.ReadPropertyMapTable();
                }
                return this.propertyMapTable;
            }
        }

        internal PropertyPtrRow[] PropertyPtrTable
        {
            get
            {
                if (this.propertyPtrTable == null)
                {
                    this.ReadPropertyPtrTable();
                }
                return this.propertyPtrTable;
            }
        }

        internal PropertyRow[] PropertyTable
        {
            get
            {
                if (this.propertyTable == null)
                {
                    this.ReadPropertyTable();
                }
                return this.propertyTable;
            }
        }

        internal StandAloneSigRow[] StandAloneSigTable
        {
            get
            {
                if (this.standAloneSigTable == null)
                {
                    this.ReadStandAloneSigTable();
                }
                return this.standAloneSigTable;
            }
        }

        internal TypeDefRow[] TypeDefTable
        {
            get
            {
                if (this.typeDefTable == null)
                {
                    this.ReadTypeDefTable();
                }
                return this.typeDefTable;
            }
        }

        internal TypeRefRow[] TypeRefTable
        {
            get
            {
                if (this.typeRefTable == null)
                {
                    this.ReadTypeRefTable();
                }
                return this.typeRefTable;
            }
        }

        internal TypeSpecRow[] TypeSpecTable
        {
            get
            {
                if (this.typeSpecTable == null)
                {
                    this.ReadTypeSpecTable();
                }
                return this.typeSpecTable;
            }
        }
    }
}

