namespace System.Compiler.Metadata
{
    using System;

    internal enum CorOpenFlags : uint
    {
        ofCacheImage = 4,
        ofCopyMemory = 2,
        ofNoTypeLib = 0x80,
        ofRead = 0,
        ofWrite = 1
    }
}

