namespace System.Compiler
{
    using System;

    [Flags]
    internal enum MethodFlags
    {
        Abstract = 0x400,
        Assembly = 3,
        CheckAccessOnOverride = 0x200,
        CompilerControlled = 0,
        Extend = 0x1000000,
        FamANDAssem = 2,
        Family = 4,
        FamORAssem = 5,
        Final = 0x20,
        HasSecurity = 0x4000,
        HideBySig = 0x80,
        MethodAccessMask = 7,
        NewSlot = 0x100,
        PInvokeImpl = 0x2000,
        Private = 1,
        Public = 6,
        RequireSecObject = 0x8000,
        ReservedMask = 0xd000,
        ReuseSlot = 0,
        RTSpecialName = 0x1000,
        SpecialName = 0x800,
        Static = 0x10,
        UnmanagedExport = 0xd000,
        Virtual = 0x40,
        VtableLayoutMask = 0x100
    }
}

