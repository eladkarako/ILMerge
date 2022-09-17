namespace System.Compiler
{
    using System;

    internal class EndFilter : Statement
    {
        private Expression value;

        public EndFilter() : base(NodeType.EndFilter)
        {
        }

        public EndFilter(Expression value) : base(NodeType.EndFilter)
        {
            this.value = value;
        }

        public Expression Value
        {
            get => 
                this.value;
            set
            {
                this.value = value;
            }
        }
    }
}

