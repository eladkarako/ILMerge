namespace System.Compiler
{
    using System;

    internal class ParameterBinding : Parameter, IUniqueKey
    {
        public Parameter BoundParameter;

        public ParameterBinding(Parameter boundParameter, SourceContext sctx)
        {
            if (boundParameter == null)
            {
                throw new ArgumentNullException("boundParameter");
            }
            this.BoundParameter = boundParameter;
            base.SourceContext = sctx;
            this.Type = boundParameter.Type;
            base.Name = boundParameter.Name;
            base.TypeExpression = boundParameter.TypeExpression;
            base.Attributes = boundParameter.Attributes;
            base.DefaultValue = boundParameter.DefaultValue;
            base.Flags = boundParameter.Flags;
            base.MarshallingInformation = boundParameter.MarshallingInformation;
            base.DeclaringMethod = boundParameter.DeclaringMethod;
            base.ParameterListIndex = boundParameter.ParameterListIndex;
            base.ArgumentListIndex = boundParameter.ArgumentListIndex;
        }

        public override bool Equals(object obj)
        {
            ParameterBinding binding = obj as ParameterBinding;
            if (binding != null)
            {
                return this.BoundParameter.Equals(binding.BoundParameter);
            }
            return this.BoundParameter.Equals(obj);
        }

        public override int GetHashCode() => 
            this.BoundParameter.GetHashCode();

        int IUniqueKey.UniqueId =>
            this.BoundParameter.UniqueKey;
    }
}

