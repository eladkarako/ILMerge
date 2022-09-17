namespace System.Compiler
{
    using System;

    internal class FunctionDeclaration : Statement
    {
        public Block Body;
        public System.Compiler.Method Method;
        public Identifier Name;
        public ParameterList Parameters;
        public TypeNode ReturnType;
        public TypeNode ReturnTypeExpression;

        public FunctionDeclaration() : base(NodeType.FunctionDeclaration)
        {
        }

        public FunctionDeclaration(Identifier name, ParameterList parameters, TypeNode returnType, Block body) : base(NodeType.FunctionDeclaration)
        {
            this.Name = name;
            this.Parameters = parameters;
            this.ReturnType = returnType;
            this.Body = body;
        }
    }
}

