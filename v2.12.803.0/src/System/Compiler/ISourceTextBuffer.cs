namespace System.Compiler
{
    using System;

    internal interface ISourceTextBuffer : ISourceText
    {
        byte* Buffer { get; }
    }
}

