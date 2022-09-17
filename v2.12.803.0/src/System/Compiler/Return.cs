namespace System.Compiler
{
    using System;

    internal class Return : ExpressionStatement
    {
        public Return()
        {
            base.NodeType = NodeType.Return;
        }

        public Return(Expression expression) : base(expression)
        {
            base.NodeType = NodeType.Return;
        }

        public Return(SourceContext sctx)
        {
            base.NodeType = NodeType.Return;
            base.SourceContext = sctx;
        }

        public Return(Expression expression, SourceContext sctx) : base(expression)
        {
            base.NodeType = NodeType.Return;
            base.SourceContext = sctx;
        }
    }
}

