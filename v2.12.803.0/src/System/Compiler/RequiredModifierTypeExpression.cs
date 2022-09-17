namespace System.Compiler
{
    using System;

    internal class RequiredModifierTypeExpression : TypeNode, ISymbolicTypeReference
    {
        public TypeNode ModifiedType;
        public TypeNode Modifier;

        public RequiredModifierTypeExpression(TypeNode elementType, TypeNode modifier) : base(NodeType.RequiredModifierTypeExpression)
        {
            this.ModifiedType = elementType;
            this.Modifier = modifier;
        }

        public RequiredModifierTypeExpression(TypeNode elementType, TypeNode modifier, SourceContext sctx) : this(elementType, modifier)
        {
            base.SourceContext = sctx;
        }

        public override bool IsUnmanaged =>
            ((this.ModifiedType != null) && this.ModifiedType.IsUnmanaged);
    }
}

