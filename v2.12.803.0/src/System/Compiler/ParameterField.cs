namespace System.Compiler
{
    using System;

    internal class ParameterField : Field
    {
        protected System.Compiler.Parameter parameter;

        public ParameterField()
        {
        }

        public ParameterField(TypeNode declaringType, AttributeList attributes, FieldFlags flags, Identifier name, TypeNode Type, Literal defaultValue) : base(declaringType, attributes, flags, name, Type, defaultValue)
        {
        }

        public virtual System.Compiler.Parameter Parameter
        {
            get => 
                this.parameter;
            set
            {
                this.parameter = value;
            }
        }
    }
}

