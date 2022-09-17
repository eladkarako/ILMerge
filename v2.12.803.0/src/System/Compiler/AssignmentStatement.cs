namespace System.Compiler
{
    using System;

    internal class AssignmentStatement : Statement
    {
        private NodeType @operator;
        public Method OperatorOverload;
        private Expression source;
        private Expression target;
        public TypeNode UnifiedType;

        public AssignmentStatement() : base(NodeType.AssignmentStatement)
        {
            this.Operator = NodeType.Nop;
        }

        public AssignmentStatement(Expression target, Expression source) : this(target, source, NodeType.Nop)
        {
        }

        public AssignmentStatement(Expression target, Expression source, NodeType @operator) : base(NodeType.AssignmentStatement)
        {
            this.target = target;
            this.source = source;
            this.@operator = @operator;
        }

        public AssignmentStatement(Expression target, Expression source, SourceContext context) : this(target, source, NodeType.Nop)
        {
            base.SourceContext = context;
        }

        public AssignmentStatement(Expression target, Expression source, NodeType Operator, SourceContext context) : this(target, source, Operator)
        {
            base.SourceContext = context;
        }

        public NodeType Operator
        {
            get => 
                this.@operator;
            set
            {
                this.@operator = value;
            }
        }

        public Expression Source
        {
            get => 
                this.source;
            set
            {
                this.source = value;
            }
        }

        public Expression Target
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

