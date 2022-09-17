namespace System.Compiler
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SourceContext
    {
        public System.Compiler.Document Document;
        public int EndPos;
        public int StartPos;
        public SourceContext(System.Compiler.Document document) : this(document, 0, (document == null) ? 0 : ((document.Text == null) ? 0 : document.Text.Length))
        {
        }

        public SourceContext(System.Compiler.Document document, int startPos, int endPos)
        {
            this.Document = document;
            this.StartPos = startPos;
            this.EndPos = endPos;
        }

        public SourceContext(System.Compiler.Document document, int startLine, int startColumn, int endLine, int endColumn)
        {
            this.Document = document;
            this.Document.GetOffsets(startLine, startColumn, endLine, endColumn, out this.StartPos, out this.EndPos);
        }

        public int StartLine =>
            this.Document?.GetLine(this.StartPos);
        public int StartColumn =>
            this.Document?.GetColumn(this.StartPos);
        public int EndLine =>
            this.Document.GetLine(this.EndPos);
        public int EndColumn =>
            this.Document.GetColumn(this.EndPos);
        public bool Encloses(int line, int column)
        {
            if ((line < this.StartLine) || (line > this.EndLine))
            {
                return false;
            }
            if (line == this.StartLine)
            {
                if (column < this.StartColumn)
                {
                    return false;
                }
                if (column > this.EndColumn)
                {
                    return (line < this.EndLine);
                }
                return true;
            }
            if (line == this.EndLine)
            {
                return (column <= this.EndColumn);
            }
            return true;
        }

        public bool Encloses(SourceContext sourceContext) => 
            (((this.StartPos <= sourceContext.StartPos) && (this.EndPos >= sourceContext.EndPos)) && (this.EndPos > sourceContext.StartPos));

        public bool IsValid =>
            ((this.Document != null) && (this.StartLine != 0xfeefee));
        public string SourceText
        {
            get
            {
                if (!this.IsValid)
                {
                    return null;
                }
                return this.Document.Substring(this.StartPos, this.EndPos - this.StartPos);
            }
        }
    }
}

