namespace System.Compiler.Metadata
{
    using System;

    internal class MetadataHeader
    {
        internal int flags;
        internal ushort majorVersion;
        internal ushort minorVersion;
        internal int reserved;
        internal int signature;
        internal StreamHeader[] streamHeaders;
        internal string versionString;
    }
}

