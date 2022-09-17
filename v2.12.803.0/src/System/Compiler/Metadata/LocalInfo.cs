namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct LocalInfo
    {
        public readonly string Name;
        public readonly uint Attributes;
        public LocalInfo(string name, uint attributes)
        {
            this.Name = name;
            this.Attributes = attributes;
        }
    }
}

