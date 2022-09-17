namespace System.Compiler
{
    using System;

    internal class TypeUnionExpression : TypeNode, ISymbolicTypeReference
    {
        public TypeNodeList Types;

        public TypeUnionExpression(TypeNodeList types) : base(NodeType.TypeUnionExpression)
        {
            this.Types = types;
        }

        public TypeUnionExpression(TypeNodeList types, SourceContext sctx) : base(NodeType.TypeUnionExpression)
        {
            this.Types = types;
            base.SourceContext = sctx;
        }
    }
}

