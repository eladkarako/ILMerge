namespace System.Compiler
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ImageDebugDirectory
    {
        internal int Characteristics;
        internal int TimeDateStamp;
        internal short MajorVersion;
        internal short MinorVersion;
        internal int Type;
        internal int SizeOfData;
        internal int AddressOfRawData;
        internal int PointerToRawData;
        public ImageDebugDirectory(bool zeroFill)
        {
            this.Characteristics = 0;
            this.TimeDateStamp = 0;
            this.MajorVersion = 0;
            this.MinorVersion = 0;
            this.Type = 0;
            this.SizeOfData = 0;
            this.AddressOfRawData = 0;
            this.PointerToRawData = 0;
        }
    }
}

