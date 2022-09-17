namespace System.Compiler
{
    using System;
    using System.Reflection;

    internal interface ISourceText
    {
        void MakeCollectible();
        string Substring(int startIndex, int length);

        char this[int position] { get; }

        int Length { get; }
    }
}

