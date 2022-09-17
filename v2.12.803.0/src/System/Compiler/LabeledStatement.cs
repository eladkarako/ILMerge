namespace System.Compiler
{
    using System;

    internal class LabeledStatement : Block
    {
        public Identifier Label;
        public System.Compiler.Statement Statement;

        public LabeledStatement()
        {
            base.NodeType = NodeType.LabeledStatement;
        }
    }
}

