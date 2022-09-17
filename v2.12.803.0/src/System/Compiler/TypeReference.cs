namespace System.Compiler
{
    using System;

    internal class TypeReference : Node
    {
        public TypeNode Expression;
        public TypeNode Type;

        public TypeReference(TypeNode typeExpression) : base(NodeType.TypeReference)
        {
            this.Expression = typeExpression;
            if (typeExpression != null)
            {
                base.SourceContext = typeExpression.SourceContext;
            }
        }

        public TypeReference(TypeNode typeExpression, TypeNode type) : base(NodeType.TypeReference)
        {
            this.Expression = typeExpression;
            this.Type = type;
            if (typeExpression != null)
            {
                base.SourceContext = typeExpression.SourceContext;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj != this)
            {
                return (obj == this.Type);
            }
            return true;
        }

        public override int GetHashCode() => 
            base.GetHashCode();

        public static bool operator ==(TypeNode type, TypeReference typeReference)
        {
            if (typeReference != null)
            {
                return (typeReference.Type == type);
            }
            return (null == type);
        }

        public static bool operator ==(TypeReference typeReference, TypeNode type)
        {
            if (typeReference != null)
            {
                return (typeReference.Type == type);
            }
            return (null == type);
        }

        public static explicit operator TypeNode(TypeReference typeReference)
        {
            if (typeReference != null)
            {
                return typeReference.Type;
            }
            return null;
        }

        public static bool operator !=(TypeNode type, TypeReference typeReference)
        {
            if (typeReference != null)
            {
                return (typeReference.Type != type);
            }
            return (null != type);
        }

        public static bool operator !=(TypeReference typeReference, TypeNode type)
        {
            if (typeReference != null)
            {
                return (typeReference.Type != type);
            }
            return (null != type);
        }
    }
}

