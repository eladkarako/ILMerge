namespace System.Compiler
{
    using System;
    using System.IO;

    internal sealed class CollectibleSourceText : ISourceText
    {
        private WeakReference fileContent;
        private string filePath;
        private int length;

        public CollectibleSourceText(string filePath, int length)
        {
            this.filePath = filePath;
            this.fileContent = new WeakReference(null);
            this.length = length;
        }

        public CollectibleSourceText(string filePath, string fileContent)
        {
            this.filePath = filePath;
            this.fileContent = new WeakReference(fileContent);
            this.length = (fileContent == null) ? 0 : fileContent.Length;
        }

        public string GetSourceText()
        {
            string target = (string) this.fileContent.Target;
            if (target == null)
            {
                target = this.ReadFile();
                this.fileContent.Target = target;
            }
            return target;
        }

        private string ReadFile()
        {
            string str = string.Empty;
            try
            {
                StreamReader reader = new StreamReader(this.filePath);
                str = reader.ReadToEnd();
                this.length = str.Length;
                reader.Close();
            }
            catch
            {
            }
            return str;
        }

        void ISourceText.MakeCollectible()
        {
            this.fileContent.Target = null;
        }

        string ISourceText.Substring(int startIndex, int length) => 
            this.GetSourceText().Substring(startIndex, length);

        char ISourceText.this[int index] =>
            this.GetSourceText()[index];

        int ISourceText.Length =>
            this.length;
    }
}

