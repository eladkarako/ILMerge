namespace System.Compiler
{
    using System;

    internal class FunctionTypeExpression : TypeNode, ISymbolicTypeReference
    {
        public ParameterList Parameters;
        public TypeNode ReturnType;

        public FunctionTypeExpression(TypeNode returnType, ParameterList parameters) : base(NodeType.FunctionTypeExpression)
        {
            this.ReturnType = returnType;
            this.Parameters = parameters;
        }

        public FunctionTypeExpression(TypeNode returnType, ParameterList parameters, SourceContext sctx) : base(NodeType.FunctionTypeExpression)
        {
            this.ReturnType = returnType;
            this.Parameters = parameters;
            base.SourceContext = sctx;
        }
    }
}

