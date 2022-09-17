namespace System.Compiler
{
    using System;

    internal class Block : Statement
    {
        public bool Checked;
        public bool HasLocals;
        private StatementList statements;
        public bool SuppressCheck;

        public Block() : base(NodeType.Block)
        {
        }

        public Block(StatementList statements) : base(NodeType.Block)
        {
            this.statements = statements;
        }

        public StatementList Statements
        {
            get => 
                this.statements;
            set
            {
                this.statements = value;
            }
        }
    }
}

