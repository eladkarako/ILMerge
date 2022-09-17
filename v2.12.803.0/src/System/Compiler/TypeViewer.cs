namespace System.Compiler
{
    using System;

    internal class TypeViewer
    {
        protected virtual TypeNode GetTypeView(TypeNode type) => 
            type.EffectiveTypeNode;

        public static TypeNode GetTypeView(TypeViewer typeViewer, TypeNode type)
        {
            if (typeViewer != null)
            {
                return typeViewer.GetTypeView(type);
            }
            return type.EffectiveTypeNode;
        }
    }
}

