namespace System.Compiler
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class AssemblyName
    {
        private IAssemblyName assemblyName;

        internal AssemblyName(IAssemblyName assemblyName)
        {
            this.assemblyName = assemblyName;
        }

        [DllImport("fusion.dll", CharSet=CharSet.Auto)]
        private static extern int CreateAssemblyCache(out IAssemblyCache ppAsmCache, uint dwReserved);
        internal string GetLocation()
        {
            IAssemblyCache cache;
            CreateAssemblyCache(out cache, 0);
            if (cache == null)
            {
                return null;
            }
            ASSEMBLY_INFO pAsmInfo = new ASSEMBLY_INFO {
                cbAssemblyInfo = (uint) Marshal.SizeOf(typeof(ASSEMBLY_INFO))
            };
            cache.QueryAssemblyInfo(3, this.StrongName, ref pAsmInfo);
            if (pAsmInfo.cbAssemblyInfo == 0)
            {
                return null;
            }
            pAsmInfo.pszCurrentAssemblyPathBuf = new string(new char[pAsmInfo.cchBuf]);
            cache.QueryAssemblyInfo(3, this.StrongName, ref pAsmInfo);
            return pAsmInfo.pszCurrentAssemblyPathBuf;
        }

        private byte[] ReadBytes(uint assemblyNameProperty)
        {
            uint pcbProperty = 0;
            this.assemblyName.GetProperty(assemblyNameProperty, IntPtr.Zero, ref pcbProperty);
            IntPtr pvProperty = Marshal.AllocHGlobal((int) pcbProperty);
            this.assemblyName.GetProperty(assemblyNameProperty, pvProperty, ref pcbProperty);
            byte[] destination = new byte[pcbProperty];
            Marshal.Copy(pvProperty, destination, 0, (int) pcbProperty);
            Marshal.FreeHGlobal(pvProperty);
            return destination;
        }

        private string ReadString(uint assemblyNameProperty)
        {
            uint pcbProperty = 0;
            this.assemblyName.GetProperty(assemblyNameProperty, IntPtr.Zero, ref pcbProperty);
            if ((pcbProperty == 0) || (pcbProperty > 0x7fffL))
            {
                return string.Empty;
            }
            IntPtr pvProperty = Marshal.AllocHGlobal((int) pcbProperty);
            this.assemblyName.GetProperty(assemblyNameProperty, pvProperty, ref pcbProperty);
            string str = Marshal.PtrToStringUni(pvProperty);
            Marshal.FreeHGlobal(pvProperty);
            return str;
        }

        private ushort ReadUInt16(uint assemblyNameProperty)
        {
            this.assemblyName.GetProperty(assemblyNameProperty, IntPtr.Zero, 0);
            IntPtr pvProperty = Marshal.AllocHGlobal((int) pcbProperty);
            this.assemblyName.GetProperty(assemblyNameProperty, pvProperty, ref pcbProperty);
            ushort num2 = (ushort) Marshal.ReadInt16(pvProperty);
            Marshal.FreeHGlobal(pvProperty);
            return num2;
        }

        public override string ToString() => 
            this.StrongName;

        internal string CodeBase =>
            this.ReadString(13);

        internal string Culture =>
            this.ReadString(8);

        internal string Name =>
            this.ReadString(3);

        internal byte[] PublicKeyToken =>
            this.ReadBytes(1);

        internal string StrongName
        {
            get
            {
                uint pccDisplayName = 0;
                this.assemblyName.GetDisplayName(null, ref pccDisplayName, 0xa7);
                if (pccDisplayName == 0)
                {
                    return "";
                }
                StringBuilder szDisplayName = new StringBuilder((int) pccDisplayName);
                this.assemblyName.GetDisplayName(szDisplayName, ref pccDisplayName, 0xa7);
                return szDisplayName.ToString();
            }
        }

        internal System.Version Version
        {
            get
            {
                int major = this.ReadUInt16(4);
                int minor = this.ReadUInt16(5);
                int build = this.ReadUInt16(6);
                return new System.Version(major, minor, build, this.ReadUInt16(7));
            }
        }

        private class ASM_NAME
        {
            public const uint _32_BIT_ONLY = 20;
            public const uint ALIAS = 12;
            public const uint BUILD_NUMBER = 6;
            public const uint CODEBASE_LASTMOD = 14;
            public const uint CODEBASE_URL = 13;
            public const uint CULTURE = 8;
            public const uint CUSTOM = 0x11;
            public const uint HASH_ALGID = 11;
            public const uint HASH_VALUE = 2;
            public const uint MAJOR_VERSION = 4;
            public const uint MINOR_VERSION = 5;
            public const uint MVID = 0x13;
            public const uint NAME = 3;
            public const uint NULL_CUSTOM = 0x12;
            public const uint NULL_PUBLIC_KEY = 15;
            public const uint NULL_PUBLIC_KEY_TOKEN = 0x10;
            public const uint OSINFO_ARRAY = 10;
            public const uint PROCESSOR_ID_ARRAY = 9;
            public const uint PUBLIC_KEY = 0;
            public const uint PUBLIC_KEY_TOKEN = 1;
            public const uint REVISION_NUMBER = 7;

            private ASM_NAME()
            {
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ASSEMBLY_INFO
        {
            public uint cbAssemblyInfo;
            public uint dwAssemblyFlags;
            public ulong uliAssemblySizeInKB;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszCurrentAssemblyPathBuf;
            public uint cchBuf;
        }

        private class ASSEMBLYINFO_FLAG
        {
            public const uint GETSIZE = 2;
            public const uint VALIDATE = 1;

            private ASSEMBLYINFO_FLAG()
            {
            }
        }

        [Flags]
        internal enum AssemblyNameDisplayFlags
        {
            ALL = 0xa7,
            CULTURE = 2,
            PROCESSORARCHITECTURE = 0x20,
            PUBLIC_KEY_TOKEN = 4,
            RETARGETABLE = 0x80,
            VERSION = 1
        }

        private class CREATE_ASM_NAME_OBJ_FLAGS
        {
            public const uint CANOF_PARSE_DISPLAY_NAME = 1;
            public const uint CANOF_SET_DEFAULT_VALUES = 2;

            private CREATE_ASM_NAME_OBJ_FLAGS()
            {
            }
        }

        [ComImport, Guid("E707DCDE-D1CD-11D2-BAB9-00C04F8ECEAE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAssemblyCache
        {
            [PreserveSig]
            int UninstallAssembly(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName, IntPtr pvReserved, int pulDisposition);
            [PreserveSig]
            int QueryAssemblyInfo(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName, ref AssemblyName.ASSEMBLY_INFO pAsmInfo);
            [PreserveSig]
            int CreateAssemblyCacheItem(uint dwFlags, IntPtr pvReserved, out object ppAsmItem, [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName);
            [PreserveSig]
            int CreateAssemblyScavenger(out object ppAsmScavenger);
            [PreserveSig]
            int InstallAssembly(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string pszManifestFilePath, IntPtr pvReserved);
        }
    }
}

