namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct PropertyMapRow
    {
        internal int Parent;
        internal int PropertyList;
    }
}

