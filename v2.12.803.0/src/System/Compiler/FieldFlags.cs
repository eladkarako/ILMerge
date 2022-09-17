namespace System.Compiler
{
    using System;

    [Flags]
    internal enum FieldFlags
    {
        Assembly = 3,
        CompilerControlled = 0,
        FamANDAssem = 2,
        Family = 4,
        FamORAssem = 5,
        FieldAccessMask = 7,
        HasDefault = 0x8000,
        HasFieldMarshal = 0x1000,
        HasFieldRVA = 0x100,
        InitOnly = 0x20,
        Literal = 0x40,
        None = 0,
        NotSerialized = 0x80,
        PinvokeImpl = 0x2000,
        Private = 1,
        Public = 6,
        ReservedMask = 0x9500,
        RTSpecialName = 0x400,
        SpecialName = 0x200,
        Static = 0x10
    }
}

