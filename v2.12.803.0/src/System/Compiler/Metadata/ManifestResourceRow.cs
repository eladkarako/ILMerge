namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ManifestResourceRow
    {
        internal int Offset;
        internal int Flags;
        internal int Name;
        internal int Implementation;
    }
}

