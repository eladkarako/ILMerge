namespace System.Compiler.Metadata
{
    using System;
    using System.Collections;
    using System.Compiler;
    using System.Text;

    internal class MetadataWriter
    {
        internal AssemblyRefRow[] assemblyRefTable;
        internal AssemblyRow[] assemblyTable;
        internal MemoryStream BlobHeap;
        private int blobRefSize;
        internal ClassLayoutRow[] classLayoutTable;
        private CLIHeader cliHeader = new CLIHeader();
        private int constantParentRefSize;
        internal ConstantRow[] constantTable;
        private int customAttributeConstructorRefSize;
        private int customAttributeParentRefSize;
        internal CustomAttributeRow[] customAttributeTable;
        private int declSecurityParentRefSize;
        internal DeclSecurityRow[] declSecurityTable;
        internal ushort dllCharacteristics;
        private static readonly byte[] dosHeader = new byte[] { 
            0x4d, 90, 0x90, 0, 3, 0, 0, 0, 4, 0, 0, 0, 0xff, 0xff, 0, 0,
            0xb8, 0, 0, 0, 0, 0, 0, 0, 0x40, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x80, 0, 0, 0,
            14, 0x1f, 0xba, 14, 0, 180, 9, 0xcd, 0x21, 0xb8, 1, 0x4c, 0xcd, 0x21, 0x54, 0x68,
            0x69, 0x73, 0x20, 0x70, 0x72, 0x6f, 0x67, 0x72, 0x61, 0x6d, 0x20, 0x63, 0x61, 110, 110, 0x6f,
            0x74, 0x20, 0x62, 0x65, 0x20, 0x72, 0x75, 110, 0x20, 0x69, 110, 0x20, 0x44, 0x4f, 0x53, 0x20,
            0x6d, 0x6f, 100, 0x65, 0x2e, 13, 13, 10, 0x24, 0, 0, 0, 0, 0, 0, 0
        };
        internal int entryPointToken;
        internal EventMapRow[] eventMapTable;
        internal EventRow[] eventTable;
        internal ExportedTypeRow[] exportedTypeTable;
        internal FieldLayoutRow[] fieldLayoutTable;
        private int fieldMarshalParentRefSize;
        internal FieldMarshalRow[] fieldMarshalTable;
        internal FieldRvaRow[] fieldRvaTable;
        internal FieldRow[] fieldTable;
        internal int fileAlignment;
        internal FileRow[] fileTable;
        internal GenericParamConstraintRow[] genericParamConstraintTable;
        internal GenericParamRow[] genericParamTable;
        internal Guid[] GuidHeap;
        private int guidRefSize;
        private int hasSemanticRefSize;
        private int implementationRefSize;
        internal ImplMapRow[] implMapTable;
        internal InterfaceImplRow[] interfaceImplTable;
        internal ManifestResourceRow[] manifestResourceTable;
        private int memberForwardedRefSize;
        private int memberRefParentSize;
        internal MemberRefRow[] memberRefTable;
        internal MemoryStream MethodBodiesHeap;
        private int methodDefOrRefSize;
        internal MethodImplRow[] methodImplTable;
        internal MethodSemanticsRow[] methodSemanticsTable;
        internal MethodSpecRow[] methodSpecTable;
        internal MethodRow[] methodTable;
        internal ModuleKindFlags moduleKind;
        internal ModuleRefRow[] moduleRefTable;
        internal ModuleRow[] moduleTable;
        internal NestedClassRow[] nestedClassTable;
        private static readonly DateTime NineteenSeventy = new DateTime(0x7b2, 1, 1);
        private NTHeader ntHeader = new NTHeader();
        internal ParamRow[] paramTable;
        internal PEKindFlags peKind;
        internal PropertyMapRow[] propertyMapTable;
        internal PropertyRow[] propertyTable;
        internal byte[] PublicKey;
        private int resolutionScopeRefSize;
        internal MemoryStream ResourceDataHeap;
        internal MemoryStream SdataHeap;
        private SectionHeader[] sectionHeaders;
        internal int SignatureKeyLength;
        internal StandAloneSigRow[] standAloneSigTable;
        internal MemoryStream StringHeap;
        private int stringRefSize;
        private ISymUnmanagedWriter symWriter;
        private int[] tableRefSize;
        private int[] tableSize;
        internal MemoryStream TlsHeap;
        internal bool TrackDebugData;
        private int typeDefOrMethodDefSize;
        private int typeDefOrRefOrSpecSize;
        internal TypeDefRow[] typeDefTable;
        internal TypeRefRow[] typeRefTable;
        internal TypeSpecRow[] typeSpecTable;
        internal bool UseGenerics;
        internal MemoryStream UserstringHeap;
        private long validMask;
        internal Win32ResourceList Win32Resources;

        internal MetadataWriter(ISymUnmanagedWriter symWriter)
        {
            this.symWriter = symWriter;
        }

        private int ComputeStrongNameSignatureSize()
        {
            int signatureKeyLength = this.SignatureKeyLength;
            if (signatureKeyLength == 0)
            {
                signatureKeyLength = this.PublicKey.Length;
            }
            if (signatureKeyLength == 0)
            {
                return 0;
            }
            if (signatureKeyLength >= 160)
            {
                return (signatureKeyLength - 0x20);
            }
            return 0x80;
        }

        private void SerializeAssemblyRefTable(BinaryWriter writer)
        {
            int num = this.tableSize[0x23];
            for (int i = 0; i < num; i++)
            {
                AssemblyRefRow row = this.assemblyRefTable[i];
                writer.Write((short) row.MajorVersion);
                writer.Write((short) row.MinorVersion);
                writer.Write((short) row.BuildNumber);
                writer.Write((short) row.RevisionNumber);
                writer.Write(row.Flags);
                this.WriteReference(writer, row.PublicKeyOrToken, this.blobRefSize);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.Culture, this.stringRefSize);
                this.WriteReference(writer, row.HashValue, this.blobRefSize);
            }
        }

        private void SerializeAssemblyTable(BinaryWriter writer)
        {
            int num = this.tableSize[0x20];
            for (int i = 0; i < num; i++)
            {
                AssemblyRow row = this.assemblyTable[i];
                writer.Write(row.HashAlgId);
                writer.Write((short) row.MajorVersion);
                writer.Write((short) row.MinorVersion);
                writer.Write((short) row.BuildNumber);
                writer.Write((short) row.RevisionNumber);
                writer.Write(row.Flags);
                this.WriteReference(writer, row.PublicKey, this.blobRefSize);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.Culture, this.stringRefSize);
            }
        }

        private void SerializeClassLayoutTable(BinaryWriter writer)
        {
            int num = this.tableSize[15];
            for (int i = 0; i < num; i++)
            {
                ClassLayoutRow row = this.classLayoutTable[i];
                writer.Write((short) row.PackingSize);
                writer.Write(row.ClassSize);
                this.WriteReference(writer, row.Parent, this.tableRefSize[2]);
            }
        }

        private void SerializeConstantTable(BinaryWriter writer)
        {
            int num = this.tableSize[11];
            for (int i = 0; i < num; i++)
            {
                ConstantRow row = this.constantTable[i];
                writer.Write((byte) row.Type);
                writer.Write((byte) 0);
                this.WriteReference(writer, row.Parent, this.constantParentRefSize);
                this.WriteReference(writer, row.Value, this.blobRefSize);
            }
        }

        private void SerializeCustomAttributeTable(BinaryWriter writer)
        {
            int num = this.tableSize[12];
            for (int i = 0; i < num; i++)
            {
                CustomAttributeRow row = this.customAttributeTable[i];
                this.WriteReference(writer, row.Parent, this.customAttributeParentRefSize);
                this.WriteReference(writer, row.Constructor, this.customAttributeConstructorRefSize);
                this.WriteReference(writer, row.Value, this.blobRefSize);
            }
        }

        private void SerializeDeclSecurityTable(BinaryWriter writer)
        {
            int num = this.tableSize[14];
            for (int i = 0; i < num; i++)
            {
                DeclSecurityRow row = this.declSecurityTable[i];
                writer.Write((short) row.Action);
                this.WriteReference(writer, row.Parent, this.declSecurityParentRefSize);
                this.WriteReference(writer, row.PermissionSet, this.blobRefSize);
            }
        }

        private void SerializeEventMapTable(BinaryWriter writer)
        {
            int num = this.tableSize[0x12];
            for (int i = 0; i < num; i++)
            {
                EventMapRow row = this.eventMapTable[i];
                this.WriteReference(writer, row.Parent, this.tableRefSize[2]);
                this.WriteReference(writer, row.EventList, this.tableRefSize[20]);
            }
        }

        private void SerializeEventTable(BinaryWriter writer)
        {
            int num = this.tableSize[20];
            for (int i = 0; i < num; i++)
            {
                EventRow row = this.eventTable[i];
                writer.Write((short) row.Flags);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.EventType, this.typeDefOrRefOrSpecSize);
            }
        }

        private void SerializeExportedTypeTable(BinaryWriter writer)
        {
            int num = this.tableSize[0x27];
            for (int i = 0; i < num; i++)
            {
                ExportedTypeRow row = this.exportedTypeTable[i];
                writer.Write(row.Flags);
                writer.Write(row.TypeDefId);
                this.WriteReference(writer, row.TypeName, this.stringRefSize);
                this.WriteReference(writer, row.TypeNamespace, this.stringRefSize);
                this.WriteReference(writer, row.Implementation, this.implementationRefSize);
            }
        }

        private void SerializeFieldLayoutTable(BinaryWriter writer)
        {
            int num = this.tableSize[0x10];
            for (int i = 0; i < num; i++)
            {
                FieldLayoutRow row = this.fieldLayoutTable[i];
                writer.Write(row.Offset);
                this.WriteReference(writer, row.Field, this.tableRefSize[4]);
            }
        }

        private void SerializeFieldMarshalTable(BinaryWriter writer)
        {
            int num = this.tableSize[13];
            for (int i = 0; i < num; i++)
            {
                FieldMarshalRow row = this.fieldMarshalTable[i];
                this.WriteReference(writer, row.Parent, this.fieldMarshalParentRefSize);
                this.WriteReference(writer, row.NativeType, this.blobRefSize);
            }
        }

        private void SerializeFieldRvaTable(BinaryWriter writer, int mbRVAOffset, Fixup sdataFixup, Fixup tlsFixup)
        {
            int num = this.tableSize[0x1d];
            for (int i = 0; i < num; i++)
            {
                Fixup fixup;
                FieldRvaRow row = this.fieldRvaTable[i];
                switch (row.TargetSection)
                {
                    case PESection.Text:
                        writer.Write((int) (row.RVA + mbRVAOffset));
                        goto Label_00AE;

                    case PESection.SData:
                    case PESection.TLS:
                        fixup = new Fixup {
                            fixupLocation = writer.BaseStream.Position,
                            addressOfNextInstruction = row.RVA
                        };
                        if (row.TargetSection != PESection.SData)
                        {
                            break;
                        }
                        sdataFixup.nextFixUp = fixup;
                        sdataFixup = fixup;
                        goto Label_0093;

                    default:
                        goto Label_00AE;
                }
                sdataFixup.nextFixUp = fixup;
                sdataFixup = fixup;
            Label_0093:
                writer.Write(0);
            Label_00AE:
                this.WriteReference(writer, row.Field, this.tableRefSize[4]);
            }
        }

        private void SerializeFieldTable(BinaryWriter writer)
        {
            int num = this.tableSize[4];
            for (int i = 0; i < num; i++)
            {
                FieldRow row = this.fieldTable[i];
                writer.Write((short) row.Flags);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.Signature, this.blobRefSize);
            }
        }

        private void SerializeFileTable(BinaryWriter writer)
        {
            int num = this.tableSize[0x26];
            for (int i = 0; i < num; i++)
            {
                FileRow row = this.fileTable[i];
                writer.Write(row.Flags);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.HashValue, this.blobRefSize);
            }
        }

        private void SerializeGenericParamConstraintTable(BinaryWriter writer)
        {
            int num = this.tableSize[0x2c];
            for (int i = 0; i < num; i++)
            {
                GenericParamConstraintRow row = this.genericParamConstraintTable[i];
                this.WriteReference(writer, row.Param, this.tableRefSize[0x2a]);
                this.WriteReference(writer, row.Constraint, this.typeDefOrRefOrSpecSize);
            }
        }

        private void SerializeGenericParamTable(BinaryWriter writer)
        {
            int num = this.tableSize[0x2a];
            bool flag = (TargetPlatform.MajorVersion == 1) && (TargetPlatform.MinorVersion == 0);
            bool flag2 = (TargetPlatform.MajorVersion == 1) && (TargetPlatform.MinorVersion == 1);
            for (int i = 0; i < num; i++)
            {
                GenericParamRow row = this.genericParamTable[i];
                writer.Write((short) row.Number);
                writer.Write((short) row.Flags);
                this.WriteReference(writer, row.Owner, this.typeDefOrMethodDefSize);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                if (flag2)
                {
                    this.WriteReference(writer, 0, this.typeDefOrRefOrSpecSize);
                }
                if (flag)
                {
                    writer.Write((short) 0);
                }
            }
        }

        private void SerializeImplMapTable(BinaryWriter writer)
        {
            int num = this.tableSize[0x1c];
            for (int i = 0; i < num; i++)
            {
                ImplMapRow row = this.implMapTable[i];
                writer.Write((short) row.MappingFlags);
                this.WriteReference(writer, row.MemberForwarded, this.memberForwardedRefSize);
                this.WriteReference(writer, row.ImportName, this.stringRefSize);
                this.WriteReference(writer, row.ImportScope, this.tableRefSize[0x1a]);
            }
        }

        private void SerializeInterfaceImplTable(BinaryWriter writer)
        {
            int num = this.tableSize[9];
            for (int i = 0; i < num; i++)
            {
                InterfaceImplRow row = this.interfaceImplTable[i];
                this.WriteReference(writer, row.Class, this.tableRefSize[2]);
                this.WriteReference(writer, row.Interface, this.typeDefOrRefOrSpecSize);
            }
        }

        private void SerializeManifestResourceTable(BinaryWriter writer)
        {
            int num = this.tableSize[40];
            for (int i = 0; i < num; i++)
            {
                ManifestResourceRow row = this.manifestResourceTable[i];
                writer.Write(row.Offset);
                writer.Write(row.Flags);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.Implementation, this.implementationRefSize);
            }
        }

        private void SerializeMemberRefTable(BinaryWriter writer)
        {
            int num = this.tableSize[10];
            for (int i = 0; i < num; i++)
            {
                MemberRefRow row = this.memberRefTable[i];
                this.WriteReference(writer, row.Class, this.memberRefParentSize);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.Signature, this.blobRefSize);
            }
        }

        private void SerializeMetadata(BinaryWriter writer, int virtualAddressBase, Fixup sdataFixup, Fixup tlsFixup)
        {
            int num9;
            int num = 0;
            num += this.MethodBodiesHeap.Length;
            this.MethodBodiesHeap.WriteTo(writer.BaseStream);
            while ((num % 4) != 0)
            {
                writer.Write((byte) 0);
                num++;
            }
            if ((this.PublicKey != null) && (0 < this.PublicKey.Length))
            {
                this.cliHeader.strongNameSignature.virtualAddress = (virtualAddressBase + 0x48) + num;
                int num2 = this.ComputeStrongNameSignatureSize();
                this.cliHeader.strongNameSignature.size = num2;
                num += num2;
                writer.BaseStream.Position += num2;
            }
            if (this.ResourceDataHeap.Length > 0)
            {
                this.cliHeader.resources.virtualAddress = (virtualAddressBase + 0x48) + num;
                this.ResourceDataHeap.WriteTo(writer.BaseStream);
                int num3 = this.ResourceDataHeap.Length;
                while ((num3 % 4) != 0)
                {
                    writer.Write((byte) 0);
                    num3++;
                }
                this.cliHeader.resources.size = num3;
                num += num3;
            }
            this.cliHeader.metaData.virtualAddress = (virtualAddressBase + 0x48) + num;
            int position = writer.BaseStream.Position;
            writer.Write(0x424a5342);
            writer.Write((short) 1);
            writer.Write((short) 1);
            writer.Write(0);
            writer.Write(12);
            char[] destinationArray = new char[12];
            char[] sourceArray = TargetPlatform.TargetRuntimeVersion.ToCharArray();
            Array.Copy(sourceArray, 0, destinationArray, 0, Math.Min(12, sourceArray.Length));
            writer.Write(destinationArray);
            writer.Write((short) 0);
            writer.Write((short) 5);
            int num5 = 0x6c;
            writer.Write(num5);
            int num6 = 0;
            num5 += this.StringHeap.Length;
            while ((num5 % 4) != 0)
            {
                num5++;
                num6++;
            }
            writer.Write((int) (this.StringHeap.Length + num6));
            writer.Write(new char[] { '#', 'S', 't', 'r', 'i', 'n', 'g', 's', '\0', '\0', '\0', '\0' });
            writer.Write(num5);
            num5 += this.UserstringHeap.Length;
            int num7 = 0;
            while ((num5 % 4) != 0)
            {
                num5++;
                num7++;
            }
            writer.Write((int) (this.UserstringHeap.Length + num7));
            writer.Write(new char[] { '#', 'U', 'S', '\0' });
            writer.Write(num5);
            writer.Write(this.BlobHeap.Length);
            writer.Write(new char[] { '#', 'B', 'l', 'o', 'b', '\0', '\0', '\0' });
            num5 += this.BlobHeap.Length;
            while ((num5 % 4) != 0)
            {
                num5++;
            }
            writer.Write(num5);
            writer.Write((int) (this.GuidHeap.Length * 0x10));
            writer.Write(new char[] { '#', 'G', 'U', 'I', 'D', '\0', '\0', '\0' });
            num5 += this.GuidHeap.Length * 0x10;
            writer.Write(num5);
            int num8 = this.TablesLength();
            writer.Write(num8);
            char[] chars = new char[4];
            chars[0] = '#';
            chars[1] = '~';
            writer.Write(chars);
            this.StringHeap.WriteTo(writer.BaseStream);
            for (num9 = this.StringHeap.Length; (num9 % 4) != 0; num9++)
            {
                writer.Write((byte) 0);
            }
            this.UserstringHeap.WriteTo(writer.BaseStream);
            for (num9 = this.UserstringHeap.Length; (num9 % 4) != 0; num9++)
            {
                writer.Write((byte) 0);
            }
            this.BlobHeap.WriteTo(writer.BaseStream);
            for (num9 = this.BlobHeap.Length; (num9 % 4) != 0; num9++)
            {
                writer.Write((byte) 0);
            }
            int index = 0;
            int length = this.GuidHeap.Length;
            while (index < length)
            {
                writer.Write(this.GuidHeap[index].ToByteArray());
                index++;
            }
            this.SerializeTables(writer, virtualAddressBase + 0x48, sdataFixup, tlsFixup);
            this.cliHeader.metaData.size = writer.BaseStream.Position - position;
        }

        private void SerializeMethodImplTable(BinaryWriter writer)
        {
            int num = this.tableSize[0x19];
            for (int i = 0; i < num; i++)
            {
                MethodImplRow row = this.methodImplTable[i];
                this.WriteReference(writer, row.Class, this.tableRefSize[2]);
                this.WriteReference(writer, row.MethodBody, this.methodDefOrRefSize);
                this.WriteReference(writer, row.MethodDeclaration, this.methodDefOrRefSize);
            }
        }

        private void SerializeMethodSemanticsTable(BinaryWriter writer)
        {
            int num = this.tableSize[0x18];
            for (int i = 0; i < num; i++)
            {
                MethodSemanticsRow row = this.methodSemanticsTable[i];
                writer.Write((short) row.Semantics);
                this.WriteReference(writer, row.Method, this.tableRefSize[6]);
                this.WriteReference(writer, row.Association, this.hasSemanticRefSize);
            }
        }

        private void SerializeMethodSpecTable(BinaryWriter writer)
        {
            int num = this.tableSize[0x2b];
            for (int i = 0; i < num; i++)
            {
                MethodSpecRow row = this.methodSpecTable[i];
                this.WriteReference(writer, row.Method, this.methodDefOrRefSize);
                this.WriteReference(writer, row.Instantiation, this.blobRefSize);
            }
        }

        private void SerializeMethodTable(BinaryWriter writer, int mbRVAOffset)
        {
            int num = this.tableSize[6];
            int paramList = (this.paramTable == null) ? 1 : (this.paramTable.Length + 1);
            for (int i = num - 1; i >= 0; i--)
            {
                MethodRow row = this.methodTable[i];
                if (row.ParamList != 0)
                {
                    paramList = row.ParamList;
                }
                else
                {
                    this.methodTable[i].ParamList = paramList;
                }
            }
            for (int j = 0; j < num; j++)
            {
                MethodRow row2 = this.methodTable[j];
                if (row2.RVA < 0)
                {
                    writer.Write(0);
                }
                else
                {
                    writer.Write((int) (row2.RVA + mbRVAOffset));
                }
                writer.Write((short) row2.ImplFlags);
                writer.Write((short) row2.Flags);
                this.WriteReference(writer, row2.Name, this.stringRefSize);
                this.WriteReference(writer, row2.Signature, this.blobRefSize);
                this.WriteReference(writer, row2.ParamList, this.tableRefSize[8]);
            }
        }

        private void SerializeModuleRefTable(BinaryWriter writer)
        {
            int num = this.tableSize[0x1a];
            for (int i = 0; i < num; i++)
            {
                ModuleRefRow row = this.moduleRefTable[i];
                this.WriteReference(writer, row.Name, this.stringRefSize);
            }
        }

        private void SerializeModuleTable(BinaryWriter writer)
        {
            int num = this.tableSize[0];
            for (int i = 0; i < num; i++)
            {
                ModuleRow row = this.moduleTable[i];
                writer.Write((short) row.Generation);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.Mvid, this.guidRefSize);
                this.WriteReference(writer, row.EncId, this.guidRefSize);
                this.WriteReference(writer, row.EncBaseId, this.guidRefSize);
            }
        }

        private void SerializeNestedClassTable(BinaryWriter writer)
        {
            int num = this.tableSize[0x29];
            for (int i = 0; i < num; i++)
            {
                NestedClassRow row = this.nestedClassTable[i];
                this.WriteReference(writer, row.NestedClass, this.tableRefSize[2]);
                this.WriteReference(writer, row.EnclosingClass, this.tableRefSize[2]);
            }
        }

        private void SerializeParamTable(BinaryWriter writer)
        {
            int num = this.tableSize[8];
            for (int i = 0; i < num; i++)
            {
                ParamRow row = this.paramTable[i];
                writer.Write((short) row.Flags);
                writer.Write((short) row.Sequence);
                this.WriteReference(writer, row.Name, this.stringRefSize);
            }
        }

        private void SerializePropertyMapTable(BinaryWriter writer)
        {
            int num = this.tableSize[0x15];
            for (int i = 0; i < num; i++)
            {
                PropertyMapRow row = this.propertyMapTable[i];
                this.WriteReference(writer, row.Parent, this.tableRefSize[2]);
                this.WriteReference(writer, row.PropertyList, this.tableRefSize[0x17]);
            }
        }

        private void SerializePropertyTable(BinaryWriter writer)
        {
            int num = this.tableSize[0x17];
            for (int i = 0; i < num; i++)
            {
                PropertyRow row = this.propertyTable[i];
                writer.Write((short) row.Flags);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.Signature, this.blobRefSize);
            }
        }

        private void SerializeStandAloneSigTable(BinaryWriter writer)
        {
            int num = this.tableSize[0x11];
            for (int i = 0; i < num; i++)
            {
                StandAloneSigRow row = this.standAloneSigTable[i];
                this.WriteReference(writer, row.Signature, this.blobRefSize);
            }
        }

        private void SerializeTables(BinaryWriter writer, int mbRVAOffset, Fixup sdataFixup, Fixup tlsFixup)
        {
            writer.Write(0);
            if (this.UseGenerics)
            {
                writer.Write((byte) 2);
                writer.Write((byte) 0);
            }
            else
            {
                writer.Write((byte) 1);
                writer.Write((byte) 1);
            }
            byte num = 0;
            if (this.StringHeap.Length >= 0x10000)
            {
                num = (byte) (num | 1);
            }
            if (this.GuidHeap.Length >= 0x10000)
            {
                num = (byte) (num | 2);
            }
            if (this.BlobHeap.Length >= 0x10000)
            {
                num = (byte) (num | 4);
            }
            writer.Write(num);
            writer.Write((byte) 0);
            writer.Write(this.validMask);
            if (this.UseGenerics)
            {
                writer.Write((long) 0x16003301fa00L);
            }
            else
            {
                writer.Write((long) 0x2003301fa00L);
            }
            int[] tableSize = this.tableSize;
            int index = 0;
            int num3 = 0;
            while (index < 0x2d)
            {
                num3 = tableSize[index];
                if (num3 > 0)
                {
                    writer.Write(num3);
                }
                index++;
            }
            if (this.moduleTable != null)
            {
                this.SerializeModuleTable(writer);
            }
            if (this.typeRefTable != null)
            {
                this.SerializeTypeRefTable(writer);
            }
            if (this.typeDefTable != null)
            {
                this.SerializeTypeDefTable(writer);
            }
            if (this.fieldTable != null)
            {
                this.SerializeFieldTable(writer);
            }
            if (this.methodTable != null)
            {
                this.SerializeMethodTable(writer, mbRVAOffset);
            }
            if (this.paramTable != null)
            {
                this.SerializeParamTable(writer);
            }
            if (this.interfaceImplTable != null)
            {
                this.SerializeInterfaceImplTable(writer);
            }
            if (this.memberRefTable != null)
            {
                this.SerializeMemberRefTable(writer);
            }
            if (this.constantTable != null)
            {
                this.SerializeConstantTable(writer);
            }
            if (this.customAttributeTable != null)
            {
                this.SerializeCustomAttributeTable(writer);
            }
            if (this.fieldMarshalTable != null)
            {
                this.SerializeFieldMarshalTable(writer);
            }
            if (this.declSecurityTable != null)
            {
                this.SerializeDeclSecurityTable(writer);
            }
            if (this.classLayoutTable != null)
            {
                this.SerializeClassLayoutTable(writer);
            }
            if (this.fieldLayoutTable != null)
            {
                this.SerializeFieldLayoutTable(writer);
            }
            if (this.standAloneSigTable != null)
            {
                this.SerializeStandAloneSigTable(writer);
            }
            if (this.eventMapTable != null)
            {
                this.SerializeEventMapTable(writer);
            }
            if (this.eventTable != null)
            {
                this.SerializeEventTable(writer);
            }
            if (this.propertyMapTable != null)
            {
                this.SerializePropertyMapTable(writer);
            }
            if (this.propertyTable != null)
            {
                this.SerializePropertyTable(writer);
            }
            if (this.methodSemanticsTable != null)
            {
                this.SerializeMethodSemanticsTable(writer);
            }
            if (this.methodImplTable != null)
            {
                this.SerializeMethodImplTable(writer);
            }
            if (this.moduleRefTable != null)
            {
                this.SerializeModuleRefTable(writer);
            }
            if (this.typeSpecTable != null)
            {
                this.SerializeTypeSpecTable(writer);
            }
            if (this.implMapTable != null)
            {
                this.SerializeImplMapTable(writer);
            }
            if (this.fieldRvaTable != null)
            {
                this.SerializeFieldRvaTable(writer, mbRVAOffset, sdataFixup, tlsFixup);
            }
            if (this.assemblyTable != null)
            {
                this.SerializeAssemblyTable(writer);
            }
            if (this.assemblyRefTable != null)
            {
                this.SerializeAssemblyRefTable(writer);
            }
            if (this.fileTable != null)
            {
                this.SerializeFileTable(writer);
            }
            if (this.exportedTypeTable != null)
            {
                this.SerializeExportedTypeTable(writer);
            }
            if (this.manifestResourceTable != null)
            {
                this.SerializeManifestResourceTable(writer);
            }
            if (this.nestedClassTable != null)
            {
                this.SerializeNestedClassTable(writer);
            }
            if (this.genericParamTable != null)
            {
                this.SerializeGenericParamTable(writer);
            }
            if (this.methodSpecTable != null)
            {
                this.SerializeMethodSpecTable(writer);
            }
            if (this.genericParamConstraintTable != null)
            {
                this.SerializeGenericParamConstraintTable(writer);
            }
        }

        private void SerializeTypeDefTable(BinaryWriter writer)
        {
            int num = this.tableSize[2];
            int fieldList = (this.fieldTable == null) ? 1 : (this.fieldTable.Length + 1);
            int methodList = (this.methodTable == null) ? 1 : (this.methodTable.Length + 1);
            for (int i = num - 1; i >= 0; i--)
            {
                TypeDefRow row = this.typeDefTable[i];
                if (row.FieldList != 0)
                {
                    fieldList = row.FieldList;
                }
                else
                {
                    this.typeDefTable[i].FieldList = fieldList;
                }
                if (row.MethodList != 0)
                {
                    methodList = row.MethodList;
                }
                else
                {
                    this.typeDefTable[i].MethodList = methodList;
                }
            }
            for (int j = 0; j < num; j++)
            {
                TypeDefRow row2 = this.typeDefTable[j];
                writer.Write(row2.Flags);
                this.WriteReference(writer, row2.Name, this.stringRefSize);
                this.WriteReference(writer, row2.Namespace, this.stringRefSize);
                this.WriteReference(writer, row2.Extends, this.typeDefOrRefOrSpecSize);
                this.WriteReference(writer, row2.FieldList, this.tableRefSize[4]);
                this.WriteReference(writer, row2.MethodList, this.tableRefSize[6]);
            }
        }

        private void SerializeTypeRefTable(BinaryWriter writer)
        {
            int num = this.tableSize[1];
            for (int i = 0; i < num; i++)
            {
                TypeRefRow row = this.typeRefTable[i];
                this.WriteReference(writer, row.ResolutionScope, this.resolutionScopeRefSize);
                this.WriteReference(writer, row.Name, this.stringRefSize);
                this.WriteReference(writer, row.Namespace, this.stringRefSize);
            }
        }

        private void SerializeTypeSpecTable(BinaryWriter writer)
        {
            int num = this.tableSize[0x1b];
            for (int i = 0; i < num; i++)
            {
                TypeSpecRow row = this.typeSpecTable[i];
                this.WriteReference(writer, row.Signature, this.blobRefSize);
            }
        }

        private int SizeOfDirectory(Directory directory)
        {
            int count = directory.Entries.Count;
            int num2 = 0x10 + (8 * count);
            for (int i = 0; i < count; i++)
            {
                Directory directory2 = directory.Entries[i] as Directory;
                if (directory2 != null)
                {
                    num2 += 0x10 + (8 * directory2.Entries.Count);
                }
            }
            return num2;
        }

        private int TablesLength()
        {
            int[] numArray2 = this.tableSize = new int[0x2d];
            int[] numArray4 = this.tableRefSize = new int[0x2d];
            int num = 0;
            long num2 = 0L;
            for (int i = 0; i < 0x2d; i++)
            {
                int length = 0;
                switch (((TableIndices) i))
                {
                    case TableIndices.Module:
                        if (this.moduleTable != null)
                        {
                            length = this.moduleTable.Length;
                        }
                        break;

                    case TableIndices.TypeRef:
                        if (this.typeRefTable != null)
                        {
                            length = this.typeRefTable.Length;
                        }
                        break;

                    case TableIndices.TypeDef:
                        if (this.typeDefTable != null)
                        {
                            length = this.typeDefTable.Length;
                        }
                        break;

                    case TableIndices.Field:
                        if (this.fieldTable != null)
                        {
                            length = this.fieldTable.Length;
                        }
                        break;

                    case TableIndices.Method:
                        if (this.methodTable != null)
                        {
                            length = this.methodTable.Length;
                        }
                        break;

                    case TableIndices.Param:
                        if (this.paramTable != null)
                        {
                            length = this.paramTable.Length;
                        }
                        break;

                    case TableIndices.InterfaceImpl:
                        if (this.interfaceImplTable != null)
                        {
                            length = this.interfaceImplTable.Length;
                        }
                        break;

                    case TableIndices.MemberRef:
                        if (this.memberRefTable != null)
                        {
                            length = this.memberRefTable.Length;
                        }
                        break;

                    case TableIndices.Constant:
                        if (this.constantTable != null)
                        {
                            length = this.constantTable.Length;
                        }
                        break;

                    case TableIndices.CustomAttribute:
                        if (this.customAttributeTable != null)
                        {
                            length = this.customAttributeTable.Length;
                        }
                        break;

                    case TableIndices.FieldMarshal:
                        if (this.fieldMarshalTable != null)
                        {
                            length = this.fieldMarshalTable.Length;
                        }
                        break;

                    case TableIndices.DeclSecurity:
                        if (this.declSecurityTable != null)
                        {
                            length = this.declSecurityTable.Length;
                        }
                        break;

                    case TableIndices.ClassLayout:
                        if (this.classLayoutTable != null)
                        {
                            length = this.classLayoutTable.Length;
                        }
                        break;

                    case TableIndices.FieldLayout:
                        if (this.fieldLayoutTable != null)
                        {
                            length = this.fieldLayoutTable.Length;
                        }
                        break;

                    case TableIndices.StandAloneSig:
                        if (this.standAloneSigTable != null)
                        {
                            length = this.standAloneSigTable.Length;
                        }
                        break;

                    case TableIndices.EventMap:
                        if (this.eventMapTable != null)
                        {
                            length = this.eventMapTable.Length;
                        }
                        break;

                    case TableIndices.Event:
                        if (this.eventTable != null)
                        {
                            length = this.eventTable.Length;
                        }
                        break;

                    case TableIndices.PropertyMap:
                        if (this.propertyMapTable != null)
                        {
                            length = this.propertyMapTable.Length;
                        }
                        break;

                    case TableIndices.Property:
                        if (this.propertyTable != null)
                        {
                            length = this.propertyTable.Length;
                        }
                        break;

                    case TableIndices.MethodSemantics:
                        if (this.methodSemanticsTable != null)
                        {
                            length = this.methodSemanticsTable.Length;
                        }
                        break;

                    case TableIndices.MethodImpl:
                        if (this.methodImplTable != null)
                        {
                            length = this.methodImplTable.Length;
                        }
                        break;

                    case TableIndices.ModuleRef:
                        if (this.moduleRefTable != null)
                        {
                            length = this.moduleRefTable.Length;
                        }
                        break;

                    case TableIndices.TypeSpec:
                        if (this.typeSpecTable != null)
                        {
                            length = this.typeSpecTable.Length;
                        }
                        break;

                    case TableIndices.ImplMap:
                        if (this.implMapTable != null)
                        {
                            length = this.implMapTable.Length;
                        }
                        break;

                    case TableIndices.FieldRva:
                        if (this.fieldRvaTable != null)
                        {
                            length = this.fieldRvaTable.Length;
                        }
                        break;

                    case TableIndices.Assembly:
                        if (this.assemblyTable != null)
                        {
                            length = this.assemblyTable.Length;
                        }
                        break;

                    case TableIndices.AssemblyRef:
                        if (this.assemblyRefTable != null)
                        {
                            length = this.assemblyRefTable.Length;
                        }
                        break;

                    case TableIndices.File:
                        if (this.fileTable != null)
                        {
                            length = this.fileTable.Length;
                        }
                        break;

                    case TableIndices.ExportedType:
                        if (this.exportedTypeTable != null)
                        {
                            length = this.exportedTypeTable.Length;
                        }
                        break;

                    case TableIndices.ManifestResource:
                        if (this.manifestResourceTable != null)
                        {
                            length = this.manifestResourceTable.Length;
                        }
                        break;

                    case TableIndices.NestedClass:
                        if (this.nestedClassTable != null)
                        {
                            length = this.nestedClassTable.Length;
                        }
                        break;

                    case TableIndices.GenericParam:
                        if (this.genericParamTable != null)
                        {
                            length = this.genericParamTable.Length;
                        }
                        break;

                    case TableIndices.MethodSpec:
                        if (this.methodSpecTable != null)
                        {
                            length = this.methodSpecTable.Length;
                        }
                        break;

                    case TableIndices.GenericParamConstraint:
                        if (this.genericParamConstraintTable != null)
                        {
                            length = this.genericParamConstraintTable.Length;
                        }
                        break;
                }
                numArray2[i] = length;
                if (length > 0)
                {
                    num++;
                    num2 |= ((long) 1L) << i;
                }
            }
            this.validMask = num2;
            for (int j = 0; j < 0x2d; j++)
            {
                numArray4[j] = (numArray2[j] < 0x10000) ? 2 : 4;
            }
            int num7 = this.blobRefSize = (this.BlobHeap.Length < 0x10000) ? 2 : 4;
            int num9 = this.constantParentRefSize = (((numArray2[8] < 0x4000) && (numArray2[4] < 0x4000)) && (numArray2[0x17] < 0x4000)) ? 2 : 4;
            int num11 = this.customAttributeParentRefSize = (((((numArray2[6] < 0x800) && (numArray2[4] < 0x800)) && ((numArray2[1] < 0x800) && (numArray2[2] < 0x800))) && (((numArray2[8] < 0x800) && (numArray2[9] < 0x800)) && ((numArray2[10] < 0x800) && (numArray2[0] < 0x800)))) && (((((numArray2[14] < 0x800) && (numArray2[0x17] < 0x800)) && ((numArray2[20] < 0x800) && (numArray2[0x11] < 0x800))) && (((numArray2[0x1a] < 0x800) && (numArray2[0x1b] < 0x800)) && ((numArray2[0x20] < 0x800) && (numArray2[0x26] < 0x800)))) && (((numArray2[0x27] < 0x800) && (numArray2[40] < 0x800)) && (((numArray2[0x2a] < 0x800) && (numArray2[0x2b] < 0x800)) && (numArray2[0x2c] < 0x800))))) ? 2 : 4;
            int num13 = this.customAttributeConstructorRefSize = ((numArray2[6] < 0x2000) && (numArray2[10] < 0x2000)) ? 2 : 4;
            int num15 = this.declSecurityParentRefSize = (((numArray2[2] < 0x4000) && (numArray2[6] < 0x4000)) && (numArray2[0x20] < 0x4000)) ? 2 : 4;
            int num17 = this.fieldMarshalParentRefSize = ((numArray2[4] < 0x8000) && (numArray2[8] < 0x8000)) ? 2 : 4;
            int num19 = this.guidRefSize = (this.GuidHeap.Length < 0x10000) ? 2 : 4;
            int num21 = this.hasSemanticRefSize = ((numArray2[20] < 0x8000) && (numArray2[0x17] < 0x8000)) ? 2 : 4;
            int num23 = this.implementationRefSize = (((numArray2[0x26] < 0x4000) && (numArray2[0x23] < 0x4000)) && (numArray2[0x27] < 0x4000)) ? 2 : 4;
            int num25 = this.methodDefOrRefSize = ((numArray2[6] < 0x8000) && (numArray2[10] < 0x8000)) ? 2 : 4;
            int num27 = this.memberRefParentSize = (((numArray2[2] < 0x2000) && (numArray2[1] < 0x2000)) && (((numArray2[0x1a] < 0x2000) && (numArray2[6] < 0x2000)) && (numArray2[0x1b] < 0x2000))) ? 2 : 4;
            int num29 = this.memberForwardedRefSize = ((numArray2[4] < 0x8000) && (numArray2[6] < 0x8000)) ? 2 : 4;
            int num31 = this.typeDefOrMethodDefSize = ((numArray2[2] < 0x8000) && (numArray2[6] < 0x8000)) ? 2 : 4;
            int num33 = this.typeDefOrRefOrSpecSize = (((numArray2[2] < 0x4000) && (numArray2[1] < 0x4000)) && (numArray2[0x1b] < 0x4000)) ? 2 : 4;
            int num35 = this.resolutionScopeRefSize = (((numArray2[0] < 0x4000) && (numArray2[0x1a] < 0x4000)) && ((numArray2[0x23] < 0x4000) && (numArray2[1] < 0x4000))) ? 2 : 4;
            int num37 = this.stringRefSize = (this.StringHeap.Length < 0x10000) ? 2 : 4;
            int num38 = 0;
            for (int k = 0; k < 0x2d; k++)
            {
                int num40 = numArray2[k];
                if (num40 != 0)
                {
                    switch (((TableIndices) k))
                    {
                        case TableIndices.Module:
                            num38 += num40 * ((2 + num37) + (3 * num19));
                            break;

                        case TableIndices.TypeRef:
                            num38 += num40 * (num35 + (2 * num37));
                            break;

                        case TableIndices.TypeDef:
                            num38 += num40 * ((((4 + (2 * num37)) + num33) + numArray4[4]) + numArray4[6]);
                            break;

                        case TableIndices.Field:
                            num38 += num40 * ((2 + num37) + num7);
                            break;

                        case TableIndices.Method:
                            num38 += num40 * (((8 + num37) + num7) + numArray4[8]);
                            break;

                        case TableIndices.Param:
                            num38 += num40 * (4 + num37);
                            break;

                        case TableIndices.InterfaceImpl:
                            num38 += num40 * (numArray4[2] + num33);
                            break;

                        case TableIndices.MemberRef:
                            num38 += num40 * ((num27 + num37) + num7);
                            break;

                        case TableIndices.Constant:
                            num38 += num40 * ((2 + num9) + num7);
                            break;

                        case TableIndices.CustomAttribute:
                            num38 += num40 * ((num11 + num13) + num7);
                            break;

                        case TableIndices.FieldMarshal:
                            num38 += num40 * (num17 + num7);
                            break;

                        case TableIndices.DeclSecurity:
                            num38 += num40 * ((2 + num15) + num7);
                            break;

                        case TableIndices.ClassLayout:
                            num38 += num40 * (6 + numArray4[2]);
                            break;

                        case TableIndices.FieldLayout:
                            num38 += num40 * (4 + numArray4[4]);
                            break;

                        case TableIndices.StandAloneSig:
                            num38 += num40 * num7;
                            break;

                        case TableIndices.EventMap:
                            num38 += num40 * (numArray4[2] + numArray4[20]);
                            break;

                        case TableIndices.Event:
                            num38 += num40 * ((2 + num37) + num33);
                            break;

                        case TableIndices.PropertyMap:
                            num38 += num40 * (numArray4[2] + numArray4[0x17]);
                            break;

                        case TableIndices.Property:
                            num38 += num40 * ((2 + num37) + num7);
                            break;

                        case TableIndices.MethodSemantics:
                            num38 += num40 * ((2 + numArray4[6]) + num21);
                            break;

                        case TableIndices.MethodImpl:
                            num38 += num40 * (numArray4[2] + (2 * num25));
                            break;

                        case TableIndices.ModuleRef:
                            num38 += num40 * num37;
                            break;

                        case TableIndices.TypeSpec:
                            num38 += num40 * num7;
                            break;

                        case TableIndices.ImplMap:
                            num38 += num40 * (((2 + num29) + num37) + numArray4[0x1a]);
                            break;

                        case TableIndices.FieldRva:
                            num38 += num40 * (4 + numArray4[4]);
                            break;

                        case TableIndices.EncLog:
                            throw new InvalidMetadataException(ExceptionStrings.ENCLogTableEncountered);

                        case TableIndices.EncMap:
                            throw new InvalidMetadataException(ExceptionStrings.ENCMapTableEncountered);

                        case TableIndices.Assembly:
                            goto Label_0C09;

                        case TableIndices.AssemblyRef:
                            goto Label_0C20;

                        case TableIndices.File:
                            goto Label_0C39;

                        case TableIndices.ExportedType:
                            goto Label_0C4D;

                        case TableIndices.ManifestResource:
                            goto Label_0C63;

                        case TableIndices.NestedClass:
                            goto Label_0C77;

                        case TableIndices.GenericParam:
                            goto Label_0C89;

                        case TableIndices.MethodSpec:
                            num38 += num40 * (num25 + num7);
                            break;

                        case TableIndices.GenericParamConstraint:
                            num38 += num40 * (numArray4[0x2a] + num33);
                            break;
                    }
                }
                continue;
            Label_0C09:
                num38 += num40 * ((0x10 + num7) + (2 * num37));
                continue;
            Label_0C20:
                num38 += num40 * ((12 + (2 * num7)) + (2 * num37));
                continue;
            Label_0C39:
                num38 += num40 * ((4 + num37) + num7);
                continue;
            Label_0C4D:
                num38 += num40 * ((8 + (2 * num37)) + num23);
                continue;
            Label_0C63:
                num38 += num40 * ((8 + num37) + num23);
                continue;
            Label_0C77:
                num38 += num40 * (2 * numArray4[2]);
                continue;
            Label_0C89:
                if ((TargetPlatform.MajorVersion == 1) && (TargetPlatform.MinorVersion == 0))
                {
                    num38 += num40 * (((6 + num31) + num37) + num33);
                }
                else if ((TargetPlatform.MajorVersion == 1) && (TargetPlatform.MinorVersion == 1))
                {
                    num38 += num40 * (((4 + num31) + num37) + num33);
                }
                else
                {
                    num38 += num40 * ((4 + num31) + num37);
                }
            }
            return (num38 + (0x18 + (num * 4)));
        }

        private void WriteCLIHeader(BinaryWriter writer)
        {
            CLIHeader cliHeader = this.cliHeader;
            writer.Write(cliHeader.cb);
            writer.Write((ushort) 2);
            if (this.UseGenerics)
            {
                writer.Write((ushort) 5);
            }
            else
            {
                writer.Write((ushort) 0);
            }
            writer.Write(cliHeader.metaData.virtualAddress);
            writer.Write(cliHeader.metaData.size);
            if ((this.peKind & PEKindFlags.Requires32bits) != 0)
            {
                cliHeader.flags |= 2;
            }
            if ((this.peKind & PEKindFlags.Prefers32bits) != 0)
            {
                cliHeader.flags |= 0x20000;
            }
            if ((this.peKind & PEKindFlags.ILonly) != 0)
            {
                cliHeader.flags |= 1;
            }
            if (this.TrackDebugData)
            {
                cliHeader.flags |= 0x10000;
            }
            writer.Write(cliHeader.flags);
            writer.Write(cliHeader.entryPointToken);
            writer.Write(cliHeader.resources.virtualAddress);
            writer.Write(cliHeader.resources.size);
            writer.Write(cliHeader.strongNameSignature.virtualAddress);
            writer.Write(cliHeader.strongNameSignature.size);
            writer.Write(cliHeader.codeManagerTable.virtualAddress);
            writer.Write(cliHeader.codeManagerTable.size);
            writer.Write(cliHeader.vtableFixups.virtualAddress);
            writer.Write(cliHeader.vtableFixups.size);
        }

        private void WriteDirectory(Directory directory, BinaryWriter writer, int offset, int level, int sizeOfDirectoryTree, int virtualAddressBase, BinaryWriter dataHeap)
        {
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write((short) directory.NumberOfNamedEntries);
            writer.Write((short) directory.NumberOfIdEntries);
            int count = directory.Entries.Count;
            int num2 = (offset + 0x10) + (count * 8);
            for (int i = 0; i < count; i++)
            {
                int iD = -2147483648;
                string name = null;
                int num5 = dataHeap.BaseStream.Position + sizeOfDirectoryTree;
                int num6 = num2;
                Directory directory2 = directory.Entries[i] as Directory;
                if (directory2 != null)
                {
                    iD = directory2.ID;
                    name = directory2.Name;
                    if (level == 0)
                    {
                        num2 += this.SizeOfDirectory(directory2);
                    }
                    else
                    {
                        num2 += 0x10 + (8 * directory2.Entries.Count);
                    }
                }
                else
                {
                    Win32Resource resource = (Win32Resource) directory.Entries[i];
                    iD = (level == 0) ? resource.TypeId : ((level == 1) ? resource.Id : resource.LanguageId);
                    name = (level == 0) ? resource.TypeName : ((level == 1) ? resource.Name : null);
                    dataHeap.Write((int) (((virtualAddressBase + sizeOfDirectoryTree) + 0x10) + dataHeap.BaseStream.Position));
                    dataHeap.Write(resource.Data.Length);
                    dataHeap.Write(resource.CodePage);
                    dataHeap.Write(0);
                    dataHeap.Write(resource.Data);
                }
                if (iD >= 0)
                {
                    writer.Write(iD);
                }
                else
                {
                    if (name == null)
                    {
                        name = "";
                    }
                    writer.Write((uint) (num5 | -2147483648));
                    dataHeap.Write((ushort) name.Length);
                    dataHeap.Write(name.ToCharArray());
                }
                if (directory2 != null)
                {
                    writer.Write((uint) (num6 | -2147483648));
                }
                else
                {
                    writer.Write(num5);
                }
            }
            num2 = (offset + 0x10) + (count * 8);
            for (int j = 0; j < count; j++)
            {
                Directory directory3 = directory.Entries[j] as Directory;
                if (directory3 != null)
                {
                    this.WriteDirectory(directory3, writer, num2, level + 1, sizeOfDirectoryTree, virtualAddressBase, dataHeap);
                    if (level == 0)
                    {
                        num2 += this.SizeOfDirectory(directory3);
                    }
                    else
                    {
                        num2 += 0x10 + (8 * directory3.Entries.Count);
                    }
                }
            }
        }

        private int WriteImportTableAndEntryPointStub(BinaryWriter writer, ref SectionHeader textSection)
        {
            bool flag = (this.peKind & PEKindFlags.Requires64bits) == 0;
            int position = writer.BaseStream.Position;
            while ((position % 4) != 0)
            {
                position++;
                writer.Write((byte) 0);
            }
            int num2 = (textSection.virtualAddress + position) - textSection.pointerToRawData;
            int num3 = num2 + 40;
            int num4 = 12;
            int num5 = num3 + (flag ? 8 : 0x10);
            int num6 = num5 + 14;
            int num7 = ((num6 + 14) + 4) + num4;
            this.ntHeader.addressOfEntryPoint = (num7 - num4) - 2;
            this.ntHeader.importTable.virtualAddress = num2;
            this.ntHeader.importTable.size = (this.ntHeader.addressOfEntryPoint - num2) - 2;
            this.ntHeader.importAddressTable.virtualAddress = num7;
            this.ntHeader.importAddressTable.size = 8;
            writer.Write(num3);
            writer.Write(0);
            writer.Write(0);
            writer.Write(num6);
            writer.Write(num7);
            writer.BaseStream.Position += 20;
            if (flag)
            {
                writer.Write(num5);
                writer.Write(0);
            }
            else
            {
                writer.Write((long) num5);
                writer.Write((long) 0L);
            }
            writer.Write((short) 0);
            string str = (this.moduleKind == ModuleKindFlags.DynamicallyLinkedLibrary) ? "_CorDllMain" : "_CorExeMain";
            foreach (char ch in str.ToCharArray())
            {
                writer.Write((byte) ch);
            }
            writer.Write((byte) 0);
            foreach (char ch2 in "mscoree.dll".ToCharArray())
            {
                writer.Write((byte) ch2);
            }
            writer.Write((byte) 0);
            writer.Write((short) 0);
            writer.Write((short) 0);
            writer.Write((byte) 0xff);
            writer.Write((byte) 0x25);
            writer.Write((int) (num7 + ((int) this.ntHeader.imageBase)));
            writer.Write(0);
            writer.Write(0);
            if (flag)
            {
                writer.Write(num5);
                writer.Write(0);
            }
            else
            {
                writer.Write((long) num5);
                writer.Write((long) 0L);
            }
            return (num7 - num4);
        }

        private void WriteNTHeader(BinaryWriter writer)
        {
            NTHeader ntHeader = this.ntHeader;
            writer.Write(ntHeader.signature);
            if ((this.peKind & PEKindFlags.Requires64bits) == 0)
            {
                if ((this.peKind & PEKindFlags.Requires32bits) != 0)
                {
                    ntHeader.characteristics = (ushort) (ntHeader.characteristics | 0x100);
                }
                ntHeader.magic = 0x10b;
                ntHeader.machine = 0x14c;
            }
            else
            {
                ntHeader.characteristics = (ushort) (ntHeader.characteristics | 0x20);
                ntHeader.magic = 0x20b;
                if ((this.peKind & PEKindFlags.AMD) != 0)
                {
                    ntHeader.machine = 0x8664;
                }
                else
                {
                    ntHeader.machine = 0x200;
                }
                ntHeader.sizeOfOptionalHeader = (ushort) (ntHeader.sizeOfOptionalHeader + 0x10);
            }
            writer.Write(ntHeader.machine);
            writer.Write(ntHeader.numberOfSections);
            writer.Write(ntHeader.timeDateStamp);
            writer.Write(ntHeader.pointerToSymbolTable);
            writer.Write(ntHeader.numberOfSymbols);
            writer.Write(ntHeader.sizeOfOptionalHeader);
            writer.Write(ntHeader.characteristics);
            writer.Write(ntHeader.magic);
            writer.Write((byte) TargetPlatform.LinkerMajorVersion);
            writer.Write((byte) TargetPlatform.LinkerMinorVersion);
            writer.Write(this.sectionHeaders[0].sizeOfRawData);
            writer.Write(ntHeader.sizeOfInitializedData);
            writer.Write(ntHeader.sizeOfUninitializedData);
            writer.Write(ntHeader.addressOfEntryPoint);
            writer.Write(this.sectionHeaders[0].virtualAddress);
            if (ntHeader.magic == 0x10b)
            {
                if (this.sectionHeaders.Length > 1)
                {
                    writer.Write(this.sectionHeaders[1].virtualAddress);
                }
                else
                {
                    writer.Write(0);
                }
                writer.Write((int) ntHeader.imageBase);
            }
            else
            {
                writer.Write(ntHeader.imageBase);
            }
            writer.Write(ntHeader.sectionAlignment);
            writer.Write(this.fileAlignment);
            writer.Write(ntHeader.majorOperatingSystemVersion);
            writer.Write(ntHeader.minorOperatingSystemVersion);
            writer.Write(ntHeader.majorImageVersion);
            writer.Write(ntHeader.minorImageVersion);
            writer.Write(ntHeader.majorSubsystemVersion);
            writer.Write(ntHeader.minorSubsystemVersion);
            writer.Write(ntHeader.win32VersionValue);
            int num = (int) (Math.Ceiling((double) (((double) this.sectionHeaders[this.sectionHeaders.Length - 1].virtualSize) / ((double) ntHeader.sectionAlignment))) * ntHeader.sectionAlignment);
            writer.Write((int) (this.sectionHeaders[this.sectionHeaders.Length - 1].virtualAddress + num));
            writer.Write(this.sectionHeaders[0].pointerToRawData);
            writer.Write(ntHeader.checkSum);
            writer.Write(ntHeader.subsystem);
            writer.Write(ntHeader.dllCharacteristics);
            if (ntHeader.magic == 0x10b)
            {
                writer.Write((int) ntHeader.sizeOfStackReserve);
                writer.Write((int) ntHeader.sizeOfStackCommit);
                writer.Write((int) ntHeader.sizeOfHeapReserve);
                writer.Write((int) ntHeader.sizeOfHeapCommit);
            }
            else
            {
                writer.Write(ntHeader.sizeOfStackReserve);
                writer.Write(ntHeader.sizeOfStackCommit);
                writer.Write(ntHeader.sizeOfHeapReserve);
                writer.Write(ntHeader.sizeOfHeapCommit);
            }
            writer.Write(ntHeader.loaderFlags);
            writer.Write(ntHeader.numberOfDataDirectories);
            writer.Write(ntHeader.exportTable.virtualAddress);
            writer.Write(ntHeader.exportTable.size);
            writer.Write(ntHeader.importTable.virtualAddress);
            writer.Write(ntHeader.importTable.size);
            writer.Write(ntHeader.resourceTable.virtualAddress);
            writer.Write(ntHeader.resourceTable.size);
            writer.Write(ntHeader.exceptionTable.virtualAddress);
            writer.Write(ntHeader.exceptionTable.size);
            writer.Write(ntHeader.certificateTable.virtualAddress);
            writer.Write(ntHeader.certificateTable.size);
            writer.Write(ntHeader.baseRelocationTable.virtualAddress);
            writer.Write(ntHeader.baseRelocationTable.size);
            writer.Write(ntHeader.debugTable.virtualAddress);
            writer.Write(ntHeader.debugTable.size);
            writer.Write(ntHeader.copyrightTable.virtualAddress);
            writer.Write(ntHeader.copyrightTable.size);
            writer.Write(ntHeader.globalPointerTable.virtualAddress);
            writer.Write(ntHeader.globalPointerTable.size);
            writer.Write(ntHeader.threadLocalStorageTable.virtualAddress);
            writer.Write(ntHeader.threadLocalStorageTable.size);
            writer.Write(ntHeader.loadConfigTable.virtualAddress);
            writer.Write(ntHeader.loadConfigTable.size);
            writer.Write(ntHeader.boundImportTable.virtualAddress);
            writer.Write(ntHeader.boundImportTable.size);
            writer.Write(ntHeader.importAddressTable.virtualAddress);
            writer.Write(ntHeader.importAddressTable.size);
            writer.Write(ntHeader.delayImportTable.virtualAddress);
            writer.Write(ntHeader.delayImportTable.size);
            writer.Write(ntHeader.cliHeaderTable.virtualAddress);
            writer.Write(ntHeader.cliHeaderTable.size);
            writer.Write((long) 0L);
        }

        internal void WritePE(BinaryWriter writer)
        {
            this.cliHeader.entryPointToken = this.entryPointToken;
            switch (this.moduleKind)
            {
                case ModuleKindFlags.ConsoleApplication:
                    this.ntHeader.subsystem = 3;
                    break;

                case ModuleKindFlags.WindowsApplication:
                    this.ntHeader.subsystem = 2;
                    break;

                case ModuleKindFlags.DynamicallyLinkedLibrary:
                    this.ntHeader.characteristics = (ushort) (this.ntHeader.characteristics | 0x2000);
                    this.ntHeader.subsystem = 3;
                    break;
            }
            this.ntHeader.dllCharacteristics = (ushort) (this.dllCharacteristics | 0x400);
            int num = 2;
            if (this.SdataHeap.Length > 0)
            {
                num++;
            }
            if (this.TlsHeap.Length > 0)
            {
                num++;
            }
            if ((this.Win32Resources != null) && (this.Win32Resources.Count > 0))
            {
                num++;
            }
            this.sectionHeaders = new SectionHeader[num];
            this.ntHeader.numberOfSections = (ushort) num;
            TimeSpan span = (TimeSpan) (DateTime.Now.ToUniversalTime() - NineteenSeventy);
            this.ntHeader.timeDateStamp = (int) span.TotalSeconds;
            Fixup sdataFixup = new Fixup();
            Fixup tlsFixup = new Fixup();
            SectionHeader textSection = new SectionHeader {
                name = ".text",
                virtualAddress = 0x2000
            };
            int num2 = 0x178 + (40 * num);
            if ((this.peKind & PEKindFlags.Requires64bits) != 0)
            {
                num2 += 0x10;
            }
            textSection.pointerToRawData = ((int) Math.Ceiling((double) (((double) num2) / ((double) this.fileAlignment)))) * this.fileAlignment;
            textSection.characteristics = 0x60000020;
            writer.BaseStream.Position = textSection.pointerToRawData + 0x48;
            this.SerializeMetadata(writer, textSection.virtualAddress, sdataFixup, tlsFixup);
            int num3 = this.WriteImportTableAndEntryPointStub(writer, ref textSection);
            if (this.symWriter != null)
            {
                this.WriteReferenceToPDBFile(writer, textSection.virtualAddress, textSection.pointerToRawData);
            }
            int num5 = textSection.virtualSize = writer.BaseStream.Position - textSection.pointerToRawData;
            textSection.sizeOfRawData = ((int) Math.Ceiling((double) (((double) num5) / ((double) this.fileAlignment)))) * this.fileAlignment;
            this.sectionHeaders[0] = textSection;
            writer.BaseStream.Position = textSection.pointerToRawData;
            this.ntHeader.cliHeaderTable.virtualAddress = textSection.virtualAddress;
            this.ntHeader.cliHeaderTable.size = 0x48;
            this.WriteCLIHeader(writer);
            int index = 1;
            SectionHeader header2 = textSection;
            int sectionAlignment = this.ntHeader.sectionAlignment;
            int fileAlignment = this.fileAlignment;
            if (this.SdataHeap.Length > 0)
            {
                SectionHeader header3 = new SectionHeader {
                    name = ".sdata"
                };
                int num10 = header3.virtualAddress = header2.virtualAddress + (sectionAlignment * ((int) Math.Ceiling((double) (((double) header2.sizeOfRawData) / ((double) sectionAlignment)))));
                header3.virtualSize = this.SdataHeap.Length;
                header3.pointerToRawData = header2.pointerToRawData + (fileAlignment * ((int) Math.Ceiling((double) (((double) header2.sizeOfRawData) / ((double) fileAlignment)))));
                header3.characteristics = -1073741760;
                writer.BaseStream.Position = header3.pointerToRawData;
                this.SdataHeap.WriteTo(writer.BaseStream);
                num5 = header3.virtualSize = writer.BaseStream.Position - header3.pointerToRawData;
                writer.BaseStream.Position += (fileAlignment - (num5 % this.fileAlignment)) - 1;
                writer.Write((byte) 0);
                header3.sizeOfRawData = ((int) Math.Ceiling((double) (((double) num5) / ((double) this.fileAlignment)))) * this.fileAlignment;
                for (sdataFixup = sdataFixup.nextFixUp; sdataFixup != null; sdataFixup = sdataFixup.nextFixUp)
                {
                    writer.BaseStream.Position = sdataFixup.fixupLocation;
                    writer.Write((int) (num10 + sdataFixup.addressOfNextInstruction));
                }
                this.sectionHeaders[index++] = header3;
                header2 = header3;
            }
            if (this.TlsHeap.Length > 0)
            {
                SectionHeader header4 = new SectionHeader {
                    name = ".tls"
                };
                int num13 = header4.virtualAddress = header2.virtualAddress + (sectionAlignment * ((int) Math.Ceiling((double) (((double) header2.sizeOfRawData) / ((double) sectionAlignment)))));
                header4.virtualSize = this.SdataHeap.Length;
                header4.pointerToRawData = header2.pointerToRawData + (fileAlignment * ((int) Math.Ceiling((double) (((double) header2.sizeOfRawData) / ((double) fileAlignment)))));
                header4.characteristics = -1073741760;
                writer.BaseStream.Position = header4.pointerToRawData;
                this.TlsHeap.WriteTo(writer.BaseStream);
                num5 = header4.virtualSize = writer.BaseStream.Position - header4.pointerToRawData;
                writer.BaseStream.Position += (fileAlignment - (num5 % this.fileAlignment)) - 1;
                writer.Write((byte) 0);
                header4.sizeOfRawData = ((int) Math.Ceiling((double) (((double) num5) / ((double) this.fileAlignment)))) * this.fileAlignment;
                for (tlsFixup = tlsFixup.nextFixUp; tlsFixup != null; tlsFixup = tlsFixup.nextFixUp)
                {
                    writer.BaseStream.Position = tlsFixup.fixupLocation;
                    writer.Write((int) (num13 + tlsFixup.addressOfNextInstruction));
                }
                this.sectionHeaders[index++] = header4;
                header2 = header4;
            }
            if ((this.Win32Resources != null) && (this.Win32Resources.Count > 0))
            {
                SectionHeader header5 = new SectionHeader {
                    name = ".rsrc",
                    virtualAddress = header2.virtualAddress + (sectionAlignment * ((int) Math.Ceiling((double) (((double) header2.sizeOfRawData) / ((double) sectionAlignment))))),
                    pointerToRawData = header2.pointerToRawData + (fileAlignment * ((int) Math.Ceiling((double) (((double) header2.sizeOfRawData) / ((double) fileAlignment))))),
                    characteristics = 0x40000040
                };
                writer.BaseStream.Position = header5.pointerToRawData;
                this.WriteWin32Resources(writer, header5.virtualAddress);
                num5 = header5.virtualSize = writer.BaseStream.Position - header5.pointerToRawData;
                writer.BaseStream.Position += (fileAlignment - (num5 % this.fileAlignment)) - 1;
                writer.Write((byte) 0);
                header5.sizeOfRawData = ((int) Math.Ceiling((double) (((double) num5) / ((double) this.fileAlignment)))) * this.fileAlignment;
                this.sectionHeaders[index++] = header5;
                this.ntHeader.resourceTable.virtualAddress = header5.virtualAddress;
                this.ntHeader.resourceTable.size = header5.virtualSize;
                this.ntHeader.sizeOfInitializedData += header5.sizeOfRawData;
                header2 = header5;
            }
            bool flag = ((this.peKind & PEKindFlags.Requires64bits) != 0) && ((this.peKind & PEKindFlags.AMD) == 0);
            SectionHeader header6 = new SectionHeader {
                name = ".reloc",
                virtualAddress = header2.virtualAddress + (sectionAlignment * ((int) Math.Ceiling((double) (((double) header2.sizeOfRawData) / ((double) sectionAlignment))))),
                virtualSize = flag ? 14 : 12,
                pointerToRawData = header2.pointerToRawData + (fileAlignment * ((int) Math.Ceiling((double) (((double) header2.sizeOfRawData) / ((double) fileAlignment))))),
                sizeOfRawData = fileAlignment,
                characteristics = 0x42000040
            };
            writer.BaseStream.Position = header6.pointerToRawData;
            writer.Write((int) ((num3 / 0x1000) * 0x1000));
            writer.Write(flag ? 14 : 12);
            int num16 = num3 % 0x1000;
            int num17 = ((this.peKind & PEKindFlags.Requires64bits) != 0) ? 10 : 3;
            short num18 = (short) ((num17 << 12) | num16);
            writer.Write(num18);
            if (flag)
            {
                writer.Write((short) (num17 << 12));
            }
            writer.Write((short) 0);
            writer.BaseStream.Position += fileAlignment - 13;
            writer.Write((byte) 0);
            this.sectionHeaders[index] = header6;
            this.ntHeader.baseRelocationTable.virtualAddress = header6.virtualAddress;
            this.ntHeader.baseRelocationTable.size = header6.virtualSize;
            this.ntHeader.sizeOfInitializedData += header6.sizeOfRawData;
            writer.BaseStream.Position = 0;
            writer.Write(dosHeader);
            this.WriteNTHeader(writer);
            this.WriteSectionHeaders(writer);
        }

        private void WriteReference(BinaryWriter writer, int index, int refSize)
        {
            if (refSize == 2)
            {
                writer.Write((short) index);
            }
            else
            {
                writer.Write(index);
            }
        }

        private unsafe void WriteReferenceToPDBFile(BinaryWriter writer, int virtualAddressBase, int fileBase)
        {
            int position = writer.BaseStream.Position;
            this.ntHeader.debugTable.virtualAddress = (position - fileBase) + virtualAddressBase;
            this.ntHeader.debugTable.size = 0x1c;
            ImageDebugDirectory pIDD = new ImageDebugDirectory(true);
            uint pcData = 0;
            this.symWriter.GetDebugInfo(ref pIDD, 0, out pcData, IntPtr.Zero);
            byte[] buffer = new byte[pcData];
            fixed (byte* numRef = buffer)
            {
                this.symWriter.GetDebugInfo(ref pIDD, pcData, out pcData, (IntPtr) numRef);
            }
            writer.Write(pIDD.Characteristics);
            writer.Write(this.ntHeader.timeDateStamp);
            writer.Write((ushort) pIDD.MajorVersion);
            writer.Write((ushort) pIDD.MinorVersion);
            writer.Write(pIDD.Type);
            writer.Write(pIDD.SizeOfData);
            writer.Write((int) (((position + 0x1c) - fileBase) + virtualAddressBase));
            writer.Write((int) (position + 0x1c));
            writer.Write(buffer);
        }

        private void WriteSectionHeaders(BinaryWriter writer)
        {
            SectionHeader[] sectionHeaders = this.sectionHeaders;
            int index = 0;
            int length = this.sectionHeaders.Length;
            while (index < length)
            {
                SectionHeader header = sectionHeaders[index];
                int num3 = 0;
                int num4 = header.name.Length;
                while (num3 < 8)
                {
                    if (num3 < num4)
                    {
                        writer.Write(header.name[num3]);
                    }
                    else
                    {
                        writer.Write((byte) 0);
                    }
                    num3++;
                }
                writer.Write(header.virtualSize);
                writer.Write(header.virtualAddress);
                writer.Write(header.sizeOfRawData);
                writer.Write(header.pointerToRawData);
                writer.Write(header.pointerToRelocations);
                writer.Write(header.pointerToLinenumbers);
                writer.Write(header.numberOfRelocations);
                writer.Write(header.numberOfLinenumbers);
                writer.Write(header.characteristics);
                index++;
            }
        }

        private void WriteWin32Resources(BinaryWriter writer, int virtualAddressBase)
        {
            Win32ResourceList list = this.Win32Resources;
            BinaryWriter dataHeap = new BinaryWriter(new MemoryStream(), Encoding.Unicode);
            Directory directory = new Directory("", 0);
            Directory directory2 = null;
            Directory directory3 = null;
            int iD = -2147483648;
            string name = null;
            int id = -2147483648;
            string str2 = null;
            int sizeOfDirectoryTree = 0x10;
            int num4 = 0;
            int count = list.Count;
            while (num4 < count)
            {
                Win32Resource resource = list[num4];
                bool flag = ((resource.TypeId < 0) && (resource.TypeName != name)) || (resource.TypeId > iD);
                if (flag)
                {
                    iD = resource.TypeId;
                    name = resource.TypeName;
                    if (iD < 0)
                    {
                        directory.NumberOfNamedEntries++;
                    }
                    else
                    {
                        directory.NumberOfIdEntries++;
                    }
                    sizeOfDirectoryTree += 0x18;
                    directory.Entries.Add(directory2 = new Directory(name, iD));
                }
                if ((flag || ((resource.Id < 0) && (resource.Name != str2))) || (resource.Id > id))
                {
                    id = resource.Id;
                    str2 = resource.Name;
                    if (id < 0)
                    {
                        directory2.NumberOfNamedEntries++;
                    }
                    else
                    {
                        directory2.NumberOfIdEntries++;
                    }
                    sizeOfDirectoryTree += 0x18;
                    directory2.Entries.Add(directory3 = new Directory(str2, id));
                }
                directory3.NumberOfIdEntries++;
                sizeOfDirectoryTree += 8;
                directory3.Entries.Add(resource);
                num4++;
            }
            this.WriteDirectory(directory, writer, 0, 0, sizeOfDirectoryTree, virtualAddressBase, dataHeap);
            dataHeap.BaseStream.WriteTo(writer.BaseStream);
        }

        private class Directory
        {
            internal ArrayList Entries;
            internal int ID;
            internal string Name;
            internal int NumberOfIdEntries;
            internal int NumberOfNamedEntries;

            internal Directory(string Name, int ID)
            {
                this.Name = Name;
                this.ID = ID;
                this.Entries = new ArrayList();
            }
        }
    }
}

