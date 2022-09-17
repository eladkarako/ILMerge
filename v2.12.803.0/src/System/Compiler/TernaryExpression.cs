namespace System.Compiler
{
    using System;

    internal class TernaryExpression : Expression
    {
        private Expression operand1;
        private Expression operand2;
        private Expression operand3;

        public TernaryExpression() : base(NodeType.Nop)
        {
        }

        public TernaryExpression(Expression operand1, Expression operand2, Expression operand3, NodeType nodeType, TypeNode resultType) : base(nodeType)
        {
            this.operand1 = operand1;
            this.operand2 = operand2;
            this.operand3 = operand3;
            this.Type = resultType;
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

        public Expression Operand3
        {
            get => 
                this.operand3;
            set
            {
                this.operand3 = value;
            }
        }
    }
}

