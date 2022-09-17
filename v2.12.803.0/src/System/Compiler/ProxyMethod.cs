namespace System.Compiler
{
    using System;

    internal class ProxyMethod : Method
    {
        public Method ProxyFor;

        public ProxyMethod(TypeNode declaringType, AttributeList attributes, Identifier name, ParameterList parameters, TypeNode returnType, Block body) : base(declaringType, attributes, name, parameters, returnType, body)
        {
        }
    }
}

