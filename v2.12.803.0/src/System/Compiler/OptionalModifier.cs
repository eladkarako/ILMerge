namespace System.Compiler
{
    using System;
    using System.Text;

    internal class OptionalModifier : TypeModifier
    {
        internal OptionalModifier(TypeNode modifier, TypeNode modified) : base(NodeType.OptionalModifier, modifier, modified)
        {
        }

        internal override void AppendDocumentIdMangledName(StringBuilder sb, TypeNodeList methodTypeParameters, TypeNodeList typeParameters)
        {
            base.ModifiedType.AppendDocumentIdMangledName(sb, methodTypeParameters, typeParameters);
            sb.Append('!');
            base.Modifier.AppendDocumentIdMangledName(sb, methodTypeParameters, typeParameters);
        }

        public static OptionalModifier For(TypeNode modifier, TypeNode modified)
        {
            if ((modified != null) && (modifier != null))
            {
                return (OptionalModifier) modified.GetModified(modifier, true);
            }
            return null;
        }
    }
}

