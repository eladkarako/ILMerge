namespace System.Compiler
{
    using System;

    internal class OldExpression : Expression
    {
        private int copyLevel;
        public Expression expression;

        public OldExpression() : base(NodeType.OldExpression)
        {
        }

        public OldExpression(Expression expression) : base(NodeType.OldExpression)
        {
            this.expression = expression;
        }

        public int ShallowCopyUptoDimension
        {
            get => 
                this.copyLevel;
            set
            {
                this.copyLevel = value;
            }
        }
    }
}

