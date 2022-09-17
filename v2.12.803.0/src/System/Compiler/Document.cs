namespace System.Compiler
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class Document
    {
        public Guid DocumentType;
        public bool Hidden;
        public Guid Language;
        public Guid LanguageVendor;
        public int LineNumber;
        private int[] lineOffsets;
        private int lines;
        public string Name;
        public DocumentText Text;
        private int uniqueKey;
        private static int uniqueKeyCounter;

        public Document()
        {
            this.Name = "";
        }

        public Document(string name, int lineNumber, DocumentText text, Guid documentType, Guid language, Guid languageVendor)
        {
            this.DocumentType = documentType;
            this.Language = language;
            this.LanguageVendor = languageVendor;
            this.LineNumber = lineNumber;
            this.Name = name;
            this.Text = text;
        }

        public Document(string name, int lineNumber, string text, Guid documentType, Guid language, Guid languageVendor) : this(name, lineNumber, new DocumentText(text), documentType, language, languageVendor)
        {
        }

        private void AddOffset(int offset)
        {
            if ((this.lineOffsets != null) && (this.lines >= 0))
            {
                if (this.lines >= this.lineOffsets.Length)
                {
                    int length = this.lineOffsets.Length;
                    if (length <= 0)
                    {
                        length = 0x10;
                    }
                    int[] destinationArray = new int[length * 2];
                    Array.Copy(this.lineOffsets, destinationArray, this.lineOffsets.Length);
                    this.lineOffsets = destinationArray;
                }
                this.lineOffsets[this.lines++] = offset;
            }
        }

        private void ComputeLineOffsets()
        {
            if (this.Text != null)
            {
                int length = this.Text.Length;
                this.lineOffsets = new int[(length / 10) + 1];
                this.lines = 0;
                this.AddOffset(0);
                for (int i = 0; i < length; i++)
                {
                    switch (this.Text[i])
                    {
                        case '\u2028':
                        case '\u2029':
                        case '\n':
                            this.AddOffset(i + 1);
                            break;

                        case '\r':
                            if (((i + 1) < length) && (this.Text[i + 1] == '\n'))
                            {
                                i++;
                            }
                            this.AddOffset(i + 1);
                            break;
                    }
                }
                this.AddOffset(length + 1);
                this.AddOffset(length + 2);
            }
        }

        private void DeleteLines(int offset, int lineCount)
        {
            int num = this.Search(offset);
            if ((num >= 0) && (num < this.lines))
            {
                for (int i = num + 1; (i + lineCount) < this.lines; i++)
                {
                    this.lineOffsets[i] = this.lineOffsets[i + lineCount];
                }
                this.lines -= lineCount;
                if (this.lines <= num)
                {
                    this.lines = num + 1;
                }
            }
        }

        public virtual int GetColumn(int position)
        {
            int line = 0;
            int column = 0;
            this.GetPosition(position, out line, out column);
            return (column + 1);
        }

        public virtual int GetLine(int position)
        {
            int line = 0;
            int column = 0;
            this.GetPosition(position, out line, out column);
            return (line + this.LineNumber);
        }

        protected static int GetLineCount(string text)
        {
            int num = (text == null) ? 0 : text.Length;
            int num2 = 0;
            for (int i = 0; i < num; i++)
            {
                switch (text[i])
                {
                    case '\u2028':
                    case '\u2029':
                    case '\n':
                        num2++;
                        break;

                    case '\r':
                        if (((i + 1) < num) && (text[i + 1] == '\n'))
                        {
                            i++;
                        }
                        num2++;
                        break;
                }
            }
            return num2;
        }

        public virtual void GetOffsets(int startLine, int startColumn, int endLine, int endColumn, out int startPos, out int endPos)
        {
            lock (this)
            {
                if (this.lineOffsets == null)
                {
                    this.ComputeLineOffsets();
                }
                startPos = (this.lineOffsets[startLine - this.LineNumber] + startColumn) - 1;
                endPos = (this.lineOffsets[endLine - this.LineNumber] + endColumn) - 1;
            }
        }

        private void GetPosition(int offset, out int line, out int column)
        {
            line = 0;
            column = 0;
            if (((offset >= 0) && (this.Text != null)) && (offset <= this.Text.Length))
            {
                lock (this)
                {
                    if (this.lineOffsets == null)
                    {
                        this.ComputeLineOffsets();
                    }
                    if (this.lineOffsets != null)
                    {
                        int index = this.Search(offset);
                        if ((index >= 0) && (index < this.lineOffsets.Length))
                        {
                            line = index;
                            column = offset - this.lineOffsets[index];
                        }
                    }
                }
            }
        }

        private void InsertLines(int offset, int lineCount)
        {
            int num = this.Search(offset);
            if ((num >= 0) && (num < this.lines))
            {
                int num2 = this.lineOffsets[this.lines - 1];
                for (int i = 0; i < lineCount; i++)
                {
                    this.AddOffset(++num2);
                }
                for (int j = lineCount; j > 0; j--)
                {
                    this.lineOffsets[(num + j) + 1] = this.lineOffsets[num + 1];
                }
            }
        }

        public virtual void InsertOrDeleteLines(int offset, int lineCount)
        {
            if ((lineCount != 0) && (((offset >= 0) && (this.Text != null)) && (offset <= this.Text.Length)))
            {
                lock (this)
                {
                    if ((this.lineOffsets == null) && (this.lineOffsets == null))
                    {
                        this.ComputeLineOffsets();
                    }
                    if (lineCount < 0)
                    {
                        this.DeleteLines(offset, -lineCount);
                    }
                    else
                    {
                        this.InsertLines(offset, lineCount);
                    }
                }
            }
        }

        private int Search(int offset)
        {
            int[] lineOffsets;
            int num3;
            do
            {
                lineOffsets = this.lineOffsets;
                int lines = this.lines;
                if (lineOffsets == null)
                {
                    return -1;
                }
                if (offset < 0)
                {
                    return -1;
                }
                int index = 0;
                num3 = 0;
                int num4 = lines - 1;
                while (num3 < num4)
                {
                    index = (num3 + num4) / 2;
                    if (lineOffsets[index] <= offset)
                    {
                        if (offset < lineOffsets[index + 1])
                        {
                            return index;
                        }
                        num3 = index + 1;
                    }
                    else
                    {
                        num4 = index;
                    }
                }
            }
            while (lineOffsets != this.lineOffsets);
            return num3;
        }

        public virtual string Substring(int position, int length) => 
            this.Text?.Substring(position, length);

        public int UniqueKey
        {
            get
            {
                if (this.uniqueKey == 0)
                {
                    int uniqueKeyCounter;
                    int num2;
                    do
                    {
                        uniqueKeyCounter = Document.uniqueKeyCounter;
                        num2 = (uniqueKeyCounter == 0x7fffffff) ? 1 : (uniqueKeyCounter + 1);
                    }
                    while (Interlocked.CompareExchange(ref Document.uniqueKeyCounter, num2, uniqueKeyCounter) != uniqueKeyCounter);
                    this.uniqueKey = num2;
                }
                return this.uniqueKey;
            }
        }
    }
}

