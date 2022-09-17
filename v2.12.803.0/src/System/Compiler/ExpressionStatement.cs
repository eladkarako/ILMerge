namespace System.Compiler
{
    using System;

    internal class ExpressionStatement : Statement
    {
        private System.Compiler.Expression expression;

        public ExpressionStatement() : base(NodeType.ExpressionStatement)
        {
        }

        public ExpressionStatement(System.Compiler.Expression expression) : base(NodeType.ExpressionStatement)
        {
            this.Expression = expression;
        }

        public ExpressionStatement(System.Compiler.Expression expression, SourceContext sctx) : base(NodeType.ExpressionStatement)
        {
            this.Expression = expression;
            base.SourceContext = sctx;
        }

        public System.Compiler.Expression Expression
        {
            get => 
                this.expression;
            set
            {
                this.expression = value;
            }
        }
    }
}

