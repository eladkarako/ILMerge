namespace System.Compiler
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct COR_FIELD_OFFSET
    {
        public uint ridOfField;
        public uint ulOffset;
    }
}

