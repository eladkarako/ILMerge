namespace System.Compiler
{
    using System;
    using System.CodeDom.Compiler;
    using System.Compiler.Metadata;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class Writer
    {
        private Writer()
        {
        }

        internal static unsafe void AddWin32Icon(Module module, Stream win32IconStream)
        {
            if ((module != null) && (win32IconStream != null))
            {
                long length = win32IconStream.Length;
                if (length > 0x7fffffffL)
                {
                    throw new FileLoadException();
                }
                int count = (int) length;
                byte[] buffer = new byte[count];
                win32IconStream.Read(buffer, 0, count);
                byte* numPtr = (byte*) Marshal.AllocHGlobal(count);
                for (int i = 0; i < count; i++)
                {
                    numPtr[i] = buffer[i];
                }
                MemoryCursor c = new MemoryCursor(numPtr, count);
                if (module.Win32Resources == null)
                {
                    module.Win32Resources = new Win32ResourceList();
                }
                if (c.ReadUInt16() != 0)
                {
                    throw new NullReferenceException();
                }
                if (c.ReadUInt16() != 1)
                {
                    throw new NullReferenceException();
                }
                int num6 = c.ReadUInt16();
                System.Compiler.BinaryWriter writer = new System.Compiler.BinaryWriter(new System.Compiler.MemoryStream());
                writer.Write((ushort) 0);
                writer.Write((ushort) 1);
                writer.Write((ushort) num6);
                Win32Resource element = new Win32Resource();
                for (int j = 0; j < num6; j++)
                {
                    element = new Win32Resource {
                        CodePage = 0,
                        Id = module.Win32Resources.Count + 2,
                        LanguageId = 0,
                        Name = null,
                        TypeId = 3,
                        TypeName = null
                    };
                    writer.Write(c.ReadByte());
                    writer.Write(c.ReadByte());
                    writer.Write(c.ReadByte());
                    writer.Write(c.ReadByte());
                    writer.Write(c.ReadUInt16());
                    writer.Write(c.ReadUInt16());
                    int num8 = c.ReadInt32();
                    int num9 = c.ReadInt32();
                    writer.Write(num8);
                    writer.Write((int) (module.Win32Resources.Count + 2));
                    element.Data = new MemoryCursor(c) { Position = num9 }.ReadBytes(num8);
                    module.Win32Resources.Add(element);
                }
                element.CodePage = 0;
                element.Data = writer.BaseStream.ToArray();
                element.Id = 0x7f00;
                element.LanguageId = 0;
                element.Name = null;
                element.TypeId = 14;
                element.TypeName = null;
                module.Win32Resources.Add(element);
            }
        }

        internal static void AddWin32Icon(Module module, string win32IconFilePath)
        {
            if ((module != null) && (win32IconFilePath != null))
            {
                using (FileStream stream = File.OpenRead(win32IconFilePath))
                {
                    AddWin32Icon(module, stream);
                }
            }
        }

        internal static unsafe void AddWin32ResourceFileToModule(Module module, Stream win32ResourceStream)
        {
            if ((module != null) && (win32ResourceStream != null))
            {
                long length = win32ResourceStream.Length;
                if (length > 0x7fffffffL)
                {
                    throw new FileLoadException();
                }
                int count = (int) length;
                byte[] buffer = new byte[count];
                win32ResourceStream.Read(buffer, 0, count);
                byte* numPtr = (byte*) Marshal.AllocHGlobal(count);
                for (int i = 0; i < count; i++)
                {
                    numPtr[i] = buffer[i];
                }
                MemoryCursor cursor = new MemoryCursor(numPtr, count);
                if (module.Win32Resources == null)
                {
                    module.Win32Resources = new Win32ResourceList();
                }
                while (cursor.Position < count)
                {
                    Win32Resource element = new Win32Resource {
                        CodePage = 0
                    };
                    int c = cursor.ReadInt32();
                    cursor.ReadUInt32();
                    if (cursor.Int16(0) == -1)
                    {
                        cursor.ReadInt16();
                        element.TypeId = cursor.ReadUInt16();
                        element.TypeName = null;
                    }
                    else
                    {
                        element.TypeId = 0;
                        element.TypeName = cursor.ReadUTF16();
                    }
                    if (cursor.Int16(0) == -1)
                    {
                        cursor.ReadInt16();
                        element.Id = cursor.ReadUInt16();
                        element.Name = null;
                    }
                    else
                    {
                        element.Id = 0;
                        element.Name = cursor.ReadUTF16();
                    }
                    cursor.ReadUInt32();
                    cursor.ReadUInt16();
                    element.LanguageId = cursor.ReadUInt16();
                    cursor.ReadUInt32();
                    cursor.ReadUInt32();
                    element.Data = cursor.ReadBytes(c);
                    if (element.Data != null)
                    {
                        module.Win32Resources.Add(element);
                    }
                }
            }
        }

        internal static void AddWin32ResourceFileToModule(Module module, string win32ResourceFilePath)
        {
            if ((module != null) && (win32ResourceFilePath != null))
            {
                using (FileStream stream = File.OpenRead(win32ResourceFilePath))
                {
                    AddWin32ResourceFileToModule(module, stream);
                }
            }
        }

        internal static void AddWin32VersionInfo(Module module, CompilerOptions options)
        {
            if ((module != null) && (options != null))
            {
                Win32Resource element = new Win32Resource {
                    CodePage = 0,
                    Id = 1,
                    LanguageId = 0,
                    Name = null,
                    TypeId = 0x10,
                    TypeName = null,
                    Data = FillInVsVersionStructure(module, options)
                };
                if (module.Win32Resources == null)
                {
                    module.Win32Resources = new Win32ResourceList();
                }
                module.Win32Resources.Add(element);
            }
        }

        private static string ConvertToString(Version version)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(version.Major.ToString());
            if (((version.Minor != 0) || (version.Build != 0)) || (version.Revision != 0))
            {
                builder.Append('.');
                builder.Append(version.Minor.ToString());
            }
            if ((version.Build != 0) || (version.Revision != 0))
            {
                builder.Append('.');
                builder.Append(version.Build.ToString());
            }
            if (version.Revision != 0)
            {
                builder.Append('.');
                builder.Append(version.Revision.ToString());
            }
            return builder.ToString();
        }

        private static ushort DaysSince2000()
        {
            TimeSpan span = (TimeSpan) (DateTime.Now - new DateTime(0x7d0, 1, 1));
            return (ushort) span.Days;
        }

        private static byte[] FillInVsVersionStructure(Module module, CompilerOptions options)
        {
            AssemblyNode node = module as AssemblyNode;
            System.Compiler.BinaryWriter data = new System.Compiler.BinaryWriter(new System.Compiler.MemoryStream(), Encoding.Unicode);
            data.Write((ushort) 0);
            data.Write((ushort) 0x34);
            data.Write((ushort) 0);
            data.Write("VS_VERSION_INFO", true);
            data.Write((ushort) 0);
            data.Write((uint) 0xfeef04bd);
            data.Write((uint) 0x10000);
            Version version = ParseVersion(options.TargetInformation.Version, true);
            if ((version == null) && (node != null))
            {
                version = node.Version;
            }
            if (version == null)
            {
                version = new Version();
            }
            data.Write((ushort) version.Minor);
            data.Write((ushort) version.Major);
            data.Write((ushort) version.Revision);
            data.Write((ushort) version.Build);
            Version version2 = ParseVersion(options.TargetInformation.ProductVersion, true);
            if (version2 == null)
            {
                version2 = version;
            }
            data.Write((ushort) version2.Minor);
            data.Write((ushort) version2.Major);
            data.Write((ushort) version2.Revision);
            data.Write((ushort) version2.Build);
            data.Write((uint) 0x3f);
            data.Write((uint) 0);
            data.Write((uint) 4);
            if (options.GenerateExecutable)
            {
                data.Write((uint) 1);
            }
            else
            {
                data.Write((uint) 2);
            }
            data.Write((uint) 0);
            data.Write((ulong) 0L);
            data.Write((ushort) 0x44);
            data.Write((ushort) 0);
            data.Write((ushort) 1);
            data.Write("VarFileInfo", true);
            data.Write((ushort) 0);
            data.Write((ushort) 0x24);
            data.Write((ushort) 4);
            data.Write((ushort) 0);
            data.Write("Translation", true);
            data.Write((uint) 0);
            data.Write((ushort) 0x4b0);
            int position = data.BaseStream.Position;
            data.Write((ushort) 0);
            data.Write((ushort) 0);
            data.Write((ushort) 1);
            data.Write("StringFileInfo", true);
            int num2 = data.BaseStream.Position;
            data.Write((ushort) 0);
            data.Write((ushort) 0);
            data.Write((ushort) 1);
            data.Write("000004b0", true);
            WriteVersionString(data, options.TargetInformation.Description, "Comments");
            WriteVersionString(data, options.TargetInformation.Company, "CompanyName");
            WriteVersionString(data, options.TargetInformation.Title, "FileDescription");
            WriteVersionString(data, ConvertToString(version), "FileVersion");
            string str = module.Name + (options.GenerateExecutable ? ".exe" : ".dll");
            WriteVersionString(data, str, "InternalName");
            WriteVersionString(data, options.TargetInformation.Copyright, "LegalCopyright");
            WriteVersionString(data, options.TargetInformation.Trademark, "LegalTrademarks");
            WriteVersionString(data, str, "OriginalFilename");
            WriteVersionString(data, options.TargetInformation.Product, "ProductName");
            WriteVersionString(data, ConvertToString(version2), "ProductVersion");
            if (node != null)
            {
                WriteVersionString(data, (node.Version == null) ? "" : node.Version.ToString(), "Assembly Version");
            }
            int num3 = data.BaseStream.Position;
            data.BaseStream.Position = num2;
            data.Write((ushort) (num3 - num2));
            data.BaseStream.Position = 0;
            data.Write((ushort) num3);
            data.BaseStream.Position = position;
            data.Write((int) (((ushort) num3) - position));
            return data.BaseStream.ToArray();
        }

        private static unsafe byte[] GetPublicKey(AssemblyNode assem)
        {
            int num;
            IntPtr zero = IntPtr.Zero;
            try
            {
                if (assem.KeyBlob != null)
                {
                    StrongNameGetPublicKey(null, assem.KeyBlob, assem.KeyBlob.Length, out zero, out num);
                    if (zero == IntPtr.Zero)
                    {
                        return assem.KeyBlob;
                    }
                }
                else if (assem.KeyContainerName != null)
                {
                    StrongNameGetPublicKey(assem.KeyContainerName, null, 0, out zero, out num);
                    if (zero == IntPtr.Zero)
                    {
                        return null;
                    }
                }
                else
                {
                    return assem.PublicKeyOrToken;
                }
                byte[] buffer2 = new byte[num];
                byte* numPtr = (byte*) zero;
                for (int j = 0; j < num; j++)
                {
                    numPtr++;
                    buffer2[j] = numPtr[0];
                }
                return buffer2;
            }
            catch
            {
            }
            if (assem.KeyBlob != null)
            {
                MscorsnStrongNameGetPublicKeyUsing(null, assem.KeyBlob, assem.KeyBlob.Length, out zero, out num);
                if (zero == IntPtr.Zero)
                {
                    return assem.KeyBlob;
                }
            }
            else if (assem.KeyContainerName != null)
            {
                MscorsnStrongNameGetPublicKeyUsing(assem.KeyContainerName, null, 0, out zero, out num);
                if (zero == IntPtr.Zero)
                {
                    return null;
                }
            }
            else
            {
                return assem.PublicKeyOrToken;
            }
            byte[] buffer3 = new byte[num];
            byte* numPtr2 = (byte*) zero;
            for (int i = 0; i < num; i++)
            {
                numPtr2++;
                buffer3[i] = numPtr2[0];
            }
            return buffer3;
        }

        [DllImport("mscorsn.dll", EntryPoint="StrongNameGetPublicKey", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        private static extern bool MscorsnStrongNameGetPublicKeyUsing(string wszKeyContainer, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pbKeyBlob, int cbKeyBlob, out IntPtr ppbPublicKeyBlob, out int pcbPublicKeyBlob);
        [DllImport("mscorsn.dll", EntryPoint="StrongNameSignatureGeneration", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        private static extern bool MscorsnStrongNameSignatureGeneration(string wszFilePath, string wszKeyContainer, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] byte[] pbKeyBlob, int cbKeyBlob, IntPtr ppbSignatureBlob, IntPtr pcbSignatureBlob);
        private static Version ParseVersion(string vString, bool allowWildcards)
        {
            if (vString == null)
            {
                return null;
            }
            ushort major = 1;
            ushort minor = 0;
            ushort build = 0;
            ushort revision = 0;
            try
            {
                int length = vString.Length;
                int index = vString.IndexOf('.', 0);
                if (index < 0)
                {
                    throw new FormatException();
                }
                major = ushort.Parse(vString.Substring(0, index), CultureInfo.InvariantCulture);
                int num7 = vString.IndexOf('.', index + 1);
                if (num7 < (index + 1))
                {
                    minor = ushort.Parse(vString.Substring(index + 1, (length - index) - 1), CultureInfo.InvariantCulture);
                }
                else
                {
                    minor = ushort.Parse(vString.Substring(index + 1, (num7 - index) - 1), CultureInfo.InvariantCulture);
                    if ((vString[num7 + 1] == '*') && allowWildcards)
                    {
                        if ((num7 + 1) < (length - 1))
                        {
                            return null;
                        }
                        build = DaysSince2000();
                        revision = SecondsSinceMidnight();
                    }
                    else
                    {
                        int num8 = vString.IndexOf('.', num7 + 1);
                        if (num8 < (num7 + 1))
                        {
                            build = ushort.Parse(vString.Substring(num7 + 1, (length - num7) - 1), CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            build = ushort.Parse(vString.Substring(num7 + 1, (num8 - num7) - 1), CultureInfo.InvariantCulture);
                            if ((vString[num8 + 1] == '*') && allowWildcards)
                            {
                                if ((num7 + 1) < (length - 1))
                                {
                                    return null;
                                }
                                revision = SecondsSinceMidnight();
                            }
                            else
                            {
                                revision = ushort.Parse(vString.Substring(num8 + 1, (length - num8) - 1), CultureInfo.InvariantCulture);
                            }
                        }
                    }
                }
            }
            catch (FormatException)
            {
                major = minor = build = (ushort) (revision = 0xffff);
            }
            catch (OverflowException)
            {
                major = minor = build = (ushort) (revision = 0xffff);
            }
            if (((major == 0xffff) && (minor == 0xffff)) && ((build == 0xffff) && (revision == 0xffff)))
            {
                return null;
            }
            return new Version(major, minor, build, revision);
        }

        private static ushort SecondsSinceMidnight()
        {
            TimeSpan span = (TimeSpan) (DateTime.Now - DateTime.Today);
            return (ushort) (((((span.Hours * 60) * 60) + (span.Minutes * 60)) + span.Seconds) / 2);
        }

        [DllImport("mscoree.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        private static extern bool StrongNameGetPublicKey(string wszKeyContainer, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pbKeyBlob, int cbKeyBlob, out IntPtr ppbPublicKeyBlob, out int pcbPublicKeyBlob);
        [DllImport("mscoree.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        private static extern bool StrongNameSignatureGeneration(string wszFilePath, string wszKeyContainer, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] byte[] pbKeyBlob, int cbKeyBlob, IntPtr ppbSignatureBlob, IntPtr pcbSignatureBlob);
        internal static void WritePE(CompilerParameters compilerParameters, Module module)
        {
            if (compilerParameters != null)
            {
                CompilerOptions options = compilerParameters as CompilerOptions;
                if (options == null)
                {
                    WritePE(module.Location, compilerParameters.IncludeDebugInformation, module, false, null, null);
                }
                else
                {
                    if (options.FileAlignment > 0x200)
                    {
                        module.FileAlignment = options.FileAlignment;
                    }
                    WritePE(module.Location, options.IncludeDebugInformation, module, options.DelaySign, options.AssemblyKeyFile, options.AssemblyKeyName);
                }
            }
        }

        internal static void WritePE(out byte[] executable, Module module)
        {
            System.Compiler.MemoryStream output = new System.Compiler.MemoryStream();
            Ir2md.WritePE(module, null, new System.Compiler.BinaryWriter(output));
            executable = output.ToArray();
        }

        internal static void WritePE(Stream executable, Stream debugSymbols, Module module)
        {
            System.Compiler.MemoryStream output = new System.Compiler.MemoryStream();
            Ir2md.WritePE(module, null, new System.Compiler.BinaryWriter(output));
            output.WriteTo(executable);
        }

        internal static void WritePE(string location, bool writeDebugSymbols, Module module)
        {
            WritePE(location, writeDebugSymbols, module, false, null, null);
        }

        internal static void WritePE(out byte[] executable, out byte[] debugSymbols, Module module)
        {
            System.Compiler.MemoryStream output = new System.Compiler.MemoryStream();
            Ir2md.WritePE(module, null, new System.Compiler.BinaryWriter(output));
            executable = output.ToArray();
            debugSymbols = null;
        }

        private static void WritePE(string location, bool writeDebugSymbols, Module module, bool delaySign, string keyFileName, string keyName)
        {
            AssemblyNode assem = module as AssemblyNode;
            location = Path.GetFullPath(location);
            module.Directory = Path.GetDirectoryName(location);
            bool flag = false;
            if (assem == null)
            {
                goto Label_00C8;
            }
            assem.KeyContainerName = keyName;
            if ((keyFileName != null) && (keyFileName.Length > 0))
            {
                if (!File.Exists(keyFileName))
                {
                    keyFileName = Path.Combine(module.Directory, keyFileName);
                }
                if (File.Exists(keyFileName))
                {
                    using (FileStream stream = File.OpenRead(keyFileName))
                    {
                        long length = stream.Length;
                        if (length > 0x7fffffffL)
                        {
                            throw new FileLoadException();
                        }
                        int count = (int) length;
                        byte[] buffer = new byte[count];
                        stream.Read(buffer, 0, count);
                        assem.KeyBlob = buffer;
                        goto Label_00BC;
                    }
                }
                flag = true;
            }
        Label_00BC:
            assem.PublicKeyOrToken = GetPublicKey(assem);
        Label_00C8:
            using (FileStream stream2 = new FileStream(location, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                string path = writeDebugSymbols ? Path.ChangeExtension(location, "pdb") : null;
                if ((path != null) && File.Exists(path))
                {
                    File.Delete(path);
                }
                System.Compiler.MemoryStream output = new System.Compiler.MemoryStream(0x493e0);
                Ir2md.WritePE(module, path, new System.Compiler.BinaryWriter(output));
                output.WriteTo(stream2);
            }
            if (flag)
            {
                throw new KeyFileNotFoundException();
            }
            if ((!delaySign && (assem != null)) && ((assem.KeyBlob != null) || ((assem.KeyContainerName != null) && (assem.KeyContainerName.Length > 0))))
            {
                try
                {
                    if (!StrongNameSignatureGeneration(location, keyName, assem.KeyBlob, (assem.KeyBlob == null) ? 0 : assem.KeyBlob.Length, IntPtr.Zero, IntPtr.Zero))
                    {
                        throw new AssemblyCouldNotBeSignedException();
                    }
                }
                catch
                {
                    if (!MscorsnStrongNameSignatureGeneration(location, keyName, assem.KeyBlob, (assem.KeyBlob == null) ? 0 : assem.KeyBlob.Length, IntPtr.Zero, IntPtr.Zero))
                    {
                        throw new AssemblyCouldNotBeSignedException();
                    }
                }
            }
        }

        private static void WriteVersionString(System.Compiler.BinaryWriter data, string value, string key)
        {
            if (value != null)
            {
                int num = 6;
                num += key.Length * 2;
                num += 4 - (num % 4);
                num += value.Length * 2;
                num += 4 - (num % 4);
                data.Write((ushort) num);
                data.Write((ushort) (value.Length + 1));
                data.Write((ushort) 1);
                data.Write(key, true);
                if ((data.BaseStream.Position % 4) != 0)
                {
                    data.Write('\0');
                }
                data.Write(value, true);
                if ((data.BaseStream.Position % 4) != 0)
                {
                    data.Write('\0');
                }
            }
        }
    }
}

