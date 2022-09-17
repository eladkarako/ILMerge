namespace System.Compiler
{
    using System;

    internal class OptionalModifierTypeExpression : TypeNode, ISymbolicTypeReference
    {
        public TypeNode ModifiedType;
        public TypeNode Modifier;

        public OptionalModifierTypeExpression(TypeNode elementType, TypeNode modifier) : base(NodeType.OptionalModifierTypeExpression)
        {
            this.ModifiedType = elementType;
            this.Modifier = modifier;
        }

        public OptionalModifierTypeExpression(TypeNode elementType, TypeNode modifier, SourceContext sctx) : this(elementType, modifier)
        {
            base.SourceContext = sctx;
        }

        public override bool IsUnmanaged =>
            ((this.ModifiedType != null) && this.ModifiedType.IsUnmanaged);
    }
}

