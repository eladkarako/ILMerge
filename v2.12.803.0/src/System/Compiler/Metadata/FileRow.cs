namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct FileRow
    {
        internal int Flags;
        internal int Name;
        internal int HashValue;
    }
}

