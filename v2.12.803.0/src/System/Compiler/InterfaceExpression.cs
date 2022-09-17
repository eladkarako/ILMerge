namespace System.Compiler
{
    using System;

    internal class InterfaceExpression : Interface, ISymbolicTypeReference
    {
        private System.Compiler.Expression expression;

        public InterfaceExpression(System.Compiler.Expression expression) : base(null)
        {
            base.NodeType = NodeType.InterfaceExpression;
            this.Expression = expression;
        }

        public InterfaceExpression(System.Compiler.Expression expression, SourceContext sctx) : base(null)
        {
            base.NodeType = NodeType.InterfaceExpression;
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

