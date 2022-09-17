namespace System.Compiler
{
    using System;

    internal class Branch : Statement
    {
        public bool BranchIfUnordered;
        private Expression condition;
        private bool leavesExceptionBlock;
        internal bool shortOffset;
        private Block target;

        public Branch() : base(NodeType.Branch)
        {
        }

        public Branch(Expression condition, Block target) : this(condition, target, false, false, false)
        {
        }

        public Branch(Expression condition, Block target, SourceContext sourceContext) : this(condition, target, false, false, false)
        {
            base.SourceContext = sourceContext;
        }

        public Branch(Expression condition, Block target, SourceContext sourceContext, bool unordered) : this(condition, target, false, false, false)
        {
            this.BranchIfUnordered = unordered;
            base.SourceContext = sourceContext;
        }

        public Branch(Expression condition, Block target, bool shortOffset, bool unordered, bool leavesExceptionBlock) : base(NodeType.Branch)
        {
            this.BranchIfUnordered = unordered;
            this.condition = condition;
            this.leavesExceptionBlock = leavesExceptionBlock;
            this.shortOffset = shortOffset;
            this.target = target;
        }

        public Expression Condition
        {
            get => 
                this.condition;
            set
            {
                this.condition = value;
            }
        }

        public bool LeavesExceptionBlock
        {
            get => 
                this.leavesExceptionBlock;
            set
            {
                this.leavesExceptionBlock = value;
            }
        }

        public bool ShortOffset
        {
            get => 
                this.shortOffset;
            set
            {
                this.shortOffset = value;
            }
        }

        public Block Target
        {
            get => 
                this.target;
            set
            {
                this.target = value;
            }
        }
    }
}

