namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct FieldLayoutRow
    {
        internal int Offset;
        internal int Field;
    }
}

