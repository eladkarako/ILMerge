namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SectionHeader
    {
        internal string name;
        internal int virtualSize;
        internal int virtualAddress;
        internal int sizeOfRawData;
        internal int pointerToRawData;
        internal int pointerToRelocations;
        internal int pointerToLinenumbers;
        internal ushort numberOfRelocations;
        internal ushort numberOfLinenumbers;
        internal int characteristics;
    }
}

