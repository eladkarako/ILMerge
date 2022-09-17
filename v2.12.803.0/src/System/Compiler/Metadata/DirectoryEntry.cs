namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct DirectoryEntry
    {
        internal int virtualAddress;
        internal int size;
    }
}

