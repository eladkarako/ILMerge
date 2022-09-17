namespace System.Compiler
{
    using System;

    internal class AnonymousNestedFunction : Expression
    {
        public Block Body;
        public Expression Invocation;
        public System.Compiler.Method Method;
        public ParameterList Parameters;

        public AnonymousNestedFunction() : base(NodeType.AnonymousNestedFunction)
        {
        }

        public AnonymousNestedFunction(ParameterList parameters, Block body) : base(NodeType.AnonymousNestedFunction)
        {
            this.Parameters = parameters;
            this.Body = body;
        }

        public AnonymousNestedFunction(ParameterList parameters, Block body, SourceContext sctx) : base(NodeType.AnonymousNestedFunction)
        {
            this.Parameters = parameters;
            this.Body = body;
            base.SourceContext = sctx;
        }
    }
}

