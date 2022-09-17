namespace System.Compiler
{
    using System;

    internal class Statement : Node
    {
        public int ILOffset;

        public Statement(NodeType nodeType) : base(nodeType)
        {
        }

        public Statement(NodeType nodeType, SourceContext sctx) : base(nodeType)
        {
            base.SourceContext = sctx;
        }
    }
}

