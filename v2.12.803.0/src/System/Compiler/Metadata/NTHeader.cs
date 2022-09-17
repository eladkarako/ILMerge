namespace System.Compiler.Metadata
{
    using System;

    internal class NTHeader
    {
        internal int addressOfEntryPoint;
        internal int baseOfCode = 0x2000;
        internal int baseOfData;
        internal DirectoryEntry baseRelocationTable;
        internal DirectoryEntry boundImportTable;
        internal DirectoryEntry certificateTable;
        internal ushort characteristics = 2;
        internal int checkSum;
        internal DirectoryEntry cliHeaderTable;
        internal DirectoryEntry copyrightTable;
        internal DirectoryEntry debugTable;
        internal DirectoryEntry delayImportTable;
        internal ushort dllCharacteristics = 0x400;
        internal DirectoryEntry exceptionTable;
        internal DirectoryEntry exportTable;
        internal int fileAlignment = 0x200;
        internal DirectoryEntry globalPointerTable;
        internal long imageBase = 0x400000L;
        internal DirectoryEntry importAddressTable;
        internal DirectoryEntry importTable;
        internal DirectoryEntry loadConfigTable;
        internal int loaderFlags;
        internal ushort machine = 0x14c;
        internal ushort magic = 0x10b;
        internal ushort majorImageVersion;
        internal byte majorLinkerVersion = 6;
        internal ushort majorOperatingSystemVersion = 4;
        internal ushort majorSubsystemVersion = 4;
        internal ushort minorImageVersion;
        internal byte minorLinkerVersion;
        internal ushort minorOperatingSystemVersion;
        internal ushort minorSubsystemVersion;
        internal int numberOfDataDirectories = 0x10;
        internal ushort numberOfSections;
        internal int numberOfSymbols;
        internal int pointerToSymbolTable;
        internal DirectoryEntry reserved;
        internal DirectoryEntry resourceTable;
        internal int sectionAlignment = 0x2000;
        internal int signature = 0x4550;
        internal int sizeOfCode;
        internal int sizeOfHeaders;
        internal long sizeOfHeapCommit = 0x1000L;
        internal long sizeOfHeapReserve = 0x100000L;
        internal int sizeOfImage;
        internal int sizeOfInitializedData;
        internal ushort sizeOfOptionalHeader = 0xe0;
        internal long sizeOfStackCommit = 0x1000L;
        internal long sizeOfStackReserve = 0x100000L;
        internal int sizeOfUninitializedData;
        internal ushort subsystem;
        internal DirectoryEntry threadLocalStorageTable;
        internal int timeDateStamp;
        internal int win32VersionValue;

        internal NTHeader()
        {
        }
    }
}

