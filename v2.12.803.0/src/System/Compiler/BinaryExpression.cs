namespace System.Compiler
{
    using System;

    internal class BinaryExpression : Expression
    {
        private Expression operand1;
        private Expression operand2;

        public BinaryExpression() : base(NodeType.Nop)
        {
        }

        public BinaryExpression(Expression operand1, Expression operand2, NodeType nodeType) : base(nodeType)
        {
            this.operand1 = operand1;
            this.operand2 = operand2;
        }

        public BinaryExpression(Expression operand1, Expression operand2, NodeType nodeType, SourceContext ctx) : base(nodeType)
        {
            this.operand1 = operand1;
            this.operand2 = operand2;
            base.SourceContext = ctx;
        }

        public BinaryExpression(Expression operand1, Expression operand2, NodeType nodeType, TypeNode resultType) : base(nodeType)
        {
            this.operand1 = operand1;
            this.operand2 = operand2;
            this.Type = resultType;
        }

        public BinaryExpression(Expression operand1, Expression operand2, NodeType nodeType, TypeNode resultType, SourceContext ctx) : base(nodeType)
        {
            this.operand1 = operand1;
            this.operand2 = operand2;
            this.Type = resultType;
            base.SourceContext = ctx;
        }

        public Expression Operand1
        {
            get => 
                this.operand1;
            set
            {
                this.operand1 = value;
            }
        }

        public Expression Operand2
        {
            get => 
                this.operand2;
            set
            {
                this.operand2 = value;
            }
        }
    }
}

