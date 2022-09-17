namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct CustomAttributeRow
    {
        internal int Parent;
        internal int Constructor;
        internal int Value;
    }
}

