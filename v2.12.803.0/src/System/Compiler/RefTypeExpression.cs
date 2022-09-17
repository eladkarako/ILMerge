namespace System.Compiler
{
    using System;

    internal class RefTypeExpression : UnaryExpression
    {
        public RefTypeExpression(Expression typedreference, SourceContext sctx) : base(typedreference, NodeType.RefTypeExpression)
        {
            base.SourceContext = sctx;
        }
    }
}

