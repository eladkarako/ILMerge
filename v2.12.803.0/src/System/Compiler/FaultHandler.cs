namespace System.Compiler
{
    using System;

    internal class FaultHandler : Statement
    {
        private System.Compiler.Block block;

        public FaultHandler() : base(NodeType.FaultHandler)
        {
        }

        public FaultHandler(System.Compiler.Block block) : base(NodeType.FaultHandler)
        {
            this.block = block;
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
    }
}

