namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct PropertyRow
    {
        internal int Flags;
        internal int Name;
        internal int Signature;
    }
}

