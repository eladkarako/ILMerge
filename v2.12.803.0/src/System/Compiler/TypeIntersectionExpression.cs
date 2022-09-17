namespace System.Compiler
{
    using System;

    internal class TypeIntersectionExpression : TypeNode, ISymbolicTypeReference
    {
        public TypeNodeList Types;

        public TypeIntersectionExpression(TypeNodeList types) : base(NodeType.TypeIntersectionExpression)
        {
            this.Types = types;
        }

        public TypeIntersectionExpression(TypeNodeList types, SourceContext sctx) : base(NodeType.TypeIntersectionExpression)
        {
            this.Types = types;
            base.SourceContext = sctx;
        }
    }
}

