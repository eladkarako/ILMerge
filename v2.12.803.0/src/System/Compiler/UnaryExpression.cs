namespace System.Compiler
{
    using System;

    internal class UnaryExpression : Expression
    {
        private Expression operand;

        public UnaryExpression() : base(NodeType.Nop)
        {
        }

        public UnaryExpression(Expression operand, NodeType nodeType) : base(nodeType)
        {
            this.Operand = operand;
        }

        public UnaryExpression(Expression operand, NodeType nodeType, SourceContext sctx) : base(nodeType)
        {
            this.operand = operand;
            base.SourceContext = sctx;
        }

        public UnaryExpression(Expression operand, NodeType nodeType, TypeNode type) : base(nodeType)
        {
            this.operand = operand;
            this.Type = type;
        }

        public UnaryExpression(Expression operand, NodeType nodeType, TypeNode type, SourceContext sctx) : base(nodeType)
        {
            this.operand = operand;
            this.Type = type;
            base.SourceContext = sctx;
        }

        public Expression Operand
        {
            get => 
                this.operand;
            set
            {
                this.operand = value;
            }
        }
    }
}

