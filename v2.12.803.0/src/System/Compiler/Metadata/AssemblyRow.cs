namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct AssemblyRow
    {
        internal int HashAlgId;
        internal int MajorVersion;
        internal int MinorVersion;
        internal int BuildNumber;
        internal int RevisionNumber;
        internal int Flags;
        internal int PublicKey;
        internal int Name;
        internal int Culture;
    }
}

