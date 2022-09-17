namespace System.Compiler
{
    using System;

    internal class SwitchInstruction : Statement
    {
        private System.Compiler.Expression expression;
        private BlockList targets;

        public SwitchInstruction() : base(NodeType.SwitchInstruction)
        {
        }

        public SwitchInstruction(System.Compiler.Expression expression, BlockList targets) : base(NodeType.SwitchInstruction)
        {
            this.expression = expression;
            this.targets = targets;
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

        public BlockList Targets
        {
            get => 
                this.targets;
            set
            {
                this.targets = value;
            }
        }
    }
}

