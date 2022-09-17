namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ExportedTypeRow
    {
        internal int Flags;
        internal int TypeDefId;
        internal int TypeName;
        internal int TypeNamespace;
        internal int Implementation;
    }
}

