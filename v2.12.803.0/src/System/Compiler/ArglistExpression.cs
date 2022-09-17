namespace System.Compiler
{
    using System;

    internal class ArglistExpression : Expression
    {
        public ArglistExpression(SourceContext sctx) : base(NodeType.ArglistExpression)
        {
            base.SourceContext = sctx;
        }
    }
}

