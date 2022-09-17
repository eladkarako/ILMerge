namespace System.Compiler
{
    using System;

    internal class TupleTypeExpression : TypeNode, ISymbolicTypeReference
    {
        public FieldList Domains;

        public TupleTypeExpression(FieldList domains) : base(NodeType.TupleTypeExpression)
        {
            this.Domains = domains;
        }

        public TupleTypeExpression(FieldList domains, SourceContext sctx) : base(NodeType.TupleTypeExpression)
        {
            this.Domains = domains;
            base.SourceContext = sctx;
        }
    }
}

