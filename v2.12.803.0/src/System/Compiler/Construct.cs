namespace System.Compiler
{
    using System;

    internal class Construct : NaryExpression
    {
        private Expression constructor;
        public Expression Owner;

        public Construct()
        {
            base.NodeType = NodeType.Construct;
        }

        public Construct(Expression constructor, ExpressionList arguments) : base(arguments, NodeType.Construct)
        {
            this.constructor = constructor;
        }

        public Construct(Expression constructor, ExpressionList arguments, SourceContext sctx) : base(arguments, NodeType.Construct)
        {
            this.constructor = constructor;
            base.SourceContext = sctx;
        }

        public Construct(Expression constructor, ExpressionList arguments, TypeNode type) : base(arguments, NodeType.Construct)
        {
            this.constructor = constructor;
            this.Type = type;
        }

        public Construct(Expression constructor, ExpressionList arguments, TypeNode type, SourceContext sctx) : base(arguments, NodeType.Construct)
        {
            this.constructor = constructor;
            this.Type = type;
            base.SourceContext = sctx;
        }

        public Expression Constructor
        {
            get => 
                this.constructor;
            set
            {
                this.constructor = value;
            }
        }
    }
}

