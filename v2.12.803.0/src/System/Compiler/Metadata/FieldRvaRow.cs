namespace System.Compiler.Metadata
{
    using System;
    using System.Compiler;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct FieldRvaRow
    {
        internal int RVA;
        internal int Field;
        internal PESection TargetSection;
    }
}

