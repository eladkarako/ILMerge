namespace System.Compiler.Metadata
{
    using System;
    using System.Compiler;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct AssemblyRefRow
    {
        internal int MajorVersion;
        internal int MinorVersion;
        internal int BuildNumber;
        internal int RevisionNumber;
        internal int Flags;
        internal int PublicKeyOrToken;
        internal int Name;
        internal int Culture;
        internal int HashValue;
        internal System.Compiler.AssemblyReference AssemblyReference;
    }
}

