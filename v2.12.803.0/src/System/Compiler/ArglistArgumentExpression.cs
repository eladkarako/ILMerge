namespace System.Compiler
{
    using System;

    internal class ArglistArgumentExpression : NaryExpression
    {
        public ArglistArgumentExpression(ExpressionList args, SourceContext sctx) : base(args, NodeType.ArglistArgumentExpression)
        {
            base.SourceContext = sctx;
        }
    }
}

