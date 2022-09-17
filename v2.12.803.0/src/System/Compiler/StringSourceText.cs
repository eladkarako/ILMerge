namespace System.Compiler
{
    using System;

    internal sealed class StringSourceText : ISourceText
    {
        public bool IsSameAsFileContents;
        public readonly string SourceText;

        public StringSourceText(string sourceText, bool isSameAsFileContents)
        {
            this.SourceText = sourceText;
            this.IsSameAsFileContents = isSameAsFileContents;
        }

        void ISourceText.MakeCollectible()
        {
        }

        string ISourceText.Substring(int startIndex, int length) => 
            this.SourceText.Substring(startIndex, length);

        char ISourceText.this[int index] =>
            this.SourceText[index];

        int ISourceText.Length =>
            this.SourceText.Length;
    }
}

