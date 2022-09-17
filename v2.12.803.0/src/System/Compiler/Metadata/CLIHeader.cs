namespace System.Compiler.Metadata
{
    using System;

    internal class CLIHeader
    {
        internal int cb = 0x48;
        internal DirectoryEntry codeManagerTable;
        internal int entryPointToken;
        internal DirectoryEntry exportAddressTableJumps;
        internal int flags;
        internal ushort majorRuntimeVersion = 2;
        internal DirectoryEntry metaData;
        internal ushort minorRuntimeVersion = 5;
        internal DirectoryEntry resources;
        internal DirectoryEntry strongNameSignature;
        internal DirectoryEntry vtableFixups;

        internal CLIHeader()
        {
        }
    }
}

