namespace System.Compiler
{
    using System;

    internal class ReturnValue : Expression
    {
        public ReturnValue() : base(NodeType.ReturnValue)
        {
        }

        public ReturnValue(SourceContext sc) : this()
        {
            base.SourceContext = sc;
        }

        public ReturnValue(TypeNode returnType) : this()
        {
            this.Type = returnType;
        }

        public ReturnValue(TypeNode returnType, SourceContext sc) : this(sc)
        {
            this.Type = returnType;
        }
    }
}

