namespace System.Compiler
{
    using System;

    [Flags]
    internal enum AssemblyFlags
    {
        CompatibilityMask = 240,
        ContainsForeignTypes = 0x200,
        DisableJITcompileOptimizer = 0x4000,
        EnableJITcompileTracking = 0x8000,
        Library = 2,
        None = 0,
        NonSideBySideCompatible = 0x10,
        NonSideBySideMachine = 0x30,
        NonSideBySideProcess = 0x20,
        NowPlatform = 6,
        Platform = 4,
        PublicKey = 1,
        Retargetable = 0x100,
        SideBySideCompatible = 0
    }
}

