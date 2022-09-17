namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ImplMapRow
    {
        internal int MappingFlags;
        internal int MemberForwarded;
        internal int ImportName;
        internal int ImportScope;
    }
}

