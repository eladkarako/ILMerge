namespace System.Compiler
{
    using System;
    using System.Text;

    internal class RequiredModifier : TypeModifier
    {
        internal RequiredModifier(TypeNode modifier, TypeNode modified) : base(NodeType.RequiredModifier, modifier, modified)
        {
        }

        internal override void AppendDocumentIdMangledName(StringBuilder sb, TypeNodeList methodTypeParameters, TypeNodeList typeParameters)
        {
            base.ModifiedType.AppendDocumentIdMangledName(sb, methodTypeParameters, typeParameters);
            sb.Append('|');
            base.Modifier.AppendDocumentIdMangledName(sb, methodTypeParameters, typeParameters);
        }

        public static RequiredModifier For(TypeNode modifier, TypeNode modified) => 
            ((RequiredModifier) modified.GetModified(modifier, false));
    }
}

