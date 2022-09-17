namespace System.Compiler.Metadata
{
    using System;
    using System.Compiler;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct FieldRow
    {
        internal int Flags;
        internal int Name;
        internal int Signature;
        internal System.Compiler.Field Field;
    }
}

