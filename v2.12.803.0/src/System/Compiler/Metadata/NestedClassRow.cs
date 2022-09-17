namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct NestedClassRow
    {
        internal int NestedClass;
        internal int EnclosingClass;
    }
}

