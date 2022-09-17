namespace System.Compiler
{
    using System;

    [Flags]
    internal enum PEKindFlags
    {
        AMD = 8,
        ILonly = 1,
        Prefers32bits = 0x20000,
        Requires32bits = 2,
        Requires64bits = 4
    }
}

