namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ParamRow
    {
        internal int Flags;
        internal int Sequence;
        internal int Name;
    }
}

