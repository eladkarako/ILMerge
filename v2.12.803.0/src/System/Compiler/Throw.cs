namespace System.Compiler
{
    using System;

    internal class Throw : Statement
    {
        private System.Compiler.Expression expression;

        public Throw() : base(NodeType.Throw)
        {
        }

        public Throw(System.Compiler.Expression expression) : base(NodeType.Throw)
        {
            this.expression = expression;
        }

        public Throw(System.Compiler.Expression expression, SourceContext context) : base(NodeType.Throw)
        {
            this.expression = expression;
            base.SourceContext = context;
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

