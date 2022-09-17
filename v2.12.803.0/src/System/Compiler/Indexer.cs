namespace System.Compiler
{
    using System;

    internal class Indexer : NaryExpression
    {
        public bool ArgumentListIsIncomplete;
        public Property CorrespondingDefaultIndexedProperty;
        private TypeNode elementType;
        private Expression @object;

        public Indexer()
        {
            base.NodeType = NodeType.Indexer;
        }

        public Indexer(Expression @object, ExpressionList arguments) : base(arguments, NodeType.Indexer)
        {
            this.@object = @object;
        }

        public Indexer(Expression Object, ExpressionList arguments, SourceContext sctx) : base(arguments, NodeType.Indexer)
        {
            this.@object = Object;
            base.SourceContext = sctx;
        }

        public Indexer(Expression Object, ExpressionList arguments, TypeNode elementType) : base(arguments, NodeType.Indexer)
        {
            this.@object = Object;
            this.elementType = this.Type = elementType;
        }

        public Indexer(Expression Object, ExpressionList arguments, TypeNode elementType, SourceContext sctx) : base(arguments, NodeType.Indexer)
        {
            this.@object = Object;
            this.elementType = this.Type = elementType;
            base.SourceContext = sctx;
        }

        public TypeNode ElementType
        {
            get => 
                this.elementType;
            set
            {
                this.elementType = value;
            }
        }

        public Expression Object
        {
            get => 
                this.@object;
            set
            {
                this.@object = value;
            }
        }
    }
}

