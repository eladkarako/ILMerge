namespace System.Compiler
{
    using System;

    internal class Filter : Statement
    {
        private System.Compiler.Block block;
        private System.Compiler.Expression expression;

        public Filter() : base(NodeType.Filter)
        {
        }

        public Filter(System.Compiler.Block block, System.Compiler.Expression expression) : base(NodeType.Filter)
        {
            this.block = block;
            this.expression = expression;
        }

        public System.Compiler.Block Block
        {
            get => 
                this.block;
            set
            {
                this.block = value;
            }
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

