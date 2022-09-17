namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ClassLayoutRow
    {
        internal int PackingSize;
        internal int ClassSize;
        internal int Parent;
    }
}

