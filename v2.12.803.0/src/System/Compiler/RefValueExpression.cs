namespace System.Compiler
{
    using System;

    internal class RefValueExpression : BinaryExpression
    {
        public RefValueExpression(Expression typedreference, Expression type, SourceContext sctx) : base(typedreference, type, NodeType.RefValueExpression)
        {
            base.SourceContext = sctx;
        }
    }
}

