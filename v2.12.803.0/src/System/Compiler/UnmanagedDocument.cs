namespace System.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Compiler.Metadata;
    using System.Runtime.InteropServices;

    internal class UnmanagedDocument : Document
    {
        private Int32List columnList = new Int32List(8);
        private Int32List lineList = new Int32List(8);

        private UnmanagedDocument(IntPtr ptrToISymUnmanagedDocument)
        {
            ISymUnmanagedDocument typedObjectForIUnknown = (ISymUnmanagedDocument) Marshal.GetTypedObjectForIUnknown(ptrToISymUnmanagedDocument, typeof(ISymUnmanagedDocument));
            if (typedObjectForIUnknown != null)
            {
                try
                {
                    typedObjectForIUnknown.GetDocumentType(out this.DocumentType);
                    typedObjectForIUnknown.GetLanguage(out this.Language);
                    typedObjectForIUnknown.GetLanguageVendor(out this.LanguageVendor);
                    char[] url = null;
                    uint length = 0;
                    typedObjectForIUnknown.GetURL(0, out length, url);
                    if (length > 0)
                    {
                        url = new char[length];
                        uint num2 = 0;
                        typedObjectForIUnknown.GetURL(length, out num2, url);
                    }
                    if (length > 0)
                    {
                        base.Name = new string(url, 0, ((int) length) - 1);
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(typedObjectForIUnknown);
                }
            }
        }

        private static int BinarySearch(Int32List list, int value)
        {
            int num = 0;
            int num2 = 0;
            int num3 = list.Count - 1;
            while (num2 < num3)
            {
                num = num2 + ((num3 - num2) / 2);
                if (list[num] <= value)
                {
                    if (list[num + 1] > value)
                    {
                        return num;
                    }
                    num2 = num + 1;
                }
                else
                {
                    num3 = num;
                }
            }
            return num2;
        }

        internal static UnmanagedDocument For(Dictionary<IntPtr, UnmanagedDocument> documentCache, IntPtr intPtr)
        {
            UnmanagedDocument document;
            if (!documentCache.TryGetValue(intPtr, out document))
            {
                document = new UnmanagedDocument(intPtr);
                documentCache[intPtr] = document;
            }
            return document;
        }

        public override int GetColumn(int offset) => 
            this.columnList[offset];

        public override int GetLine(int offset) => 
            this.lineList[offset];

        internal int GetOffset(uint line, uint column)
        {
            this.lineList.Add((int) line);
            this.columnList.Add((int) column);
            return (this.lineList.Count - 1);
        }

        public override void GetOffsets(int startLine, int startColumn, int endLine, int endColumn, out int startCol, out int endCol)
        {
            int num = BinarySearch(this.lineList, startLine);
            Int32List columnList = this.columnList;
            startCol = 0;
            int num2 = num;
            int count = columnList.Count;
            while (num2 < count)
            {
                if (columnList[num2] >= startColumn)
                {
                    startCol = num2;
                    break;
                }
                num2++;
            }
            endCol = 0;
            int num4 = BinarySearch(this.lineList, endLine);
            int num5 = columnList.Count;
            while (num4 < num5)
            {
                if (columnList[num4] >= endColumn)
                {
                    endCol = num4;
                    return;
                }
                num4++;
            }
        }

        public override void InsertOrDeleteLines(int offset, int lineCount)
        {
        }
    }
}

