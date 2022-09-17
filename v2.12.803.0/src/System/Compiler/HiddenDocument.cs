namespace System.Compiler
{
    using System;

    internal class HiddenDocument : System.Compiler.Document
    {
        public static readonly HiddenDocument Document = new HiddenDocument();

        private HiddenDocument()
        {
        }

        public override int GetColumn(int position) => 
            1;

        public override int GetLine(int position) => 
            0xfeefee;
    }
}

