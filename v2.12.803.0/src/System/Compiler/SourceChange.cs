namespace System.Compiler
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SourceChange
    {
        public System.Compiler.SourceContext SourceContext;
        public string ChangedText;
    }
}

