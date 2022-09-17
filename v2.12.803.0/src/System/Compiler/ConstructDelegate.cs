namespace System.Compiler
{
    using System;

    internal class ConstructDelegate : Expression
    {
        public TypeNode DelegateType;
        public TypeNode DelegateTypeExpression;
        public Identifier MethodName;
        public Expression TargetObject;

        public ConstructDelegate() : base(NodeType.ConstructDelegate)
        {
        }

        public ConstructDelegate(TypeNode delegateType, Expression targetObject, Identifier methodName) : base(NodeType.ConstructDelegate)
        {
            this.DelegateType = delegateType;
            this.MethodName = methodName;
            this.TargetObject = targetObject;
        }

        public ConstructDelegate(TypeNode delegateType, Expression targetObject, Identifier methodName, SourceContext sctx) : base(NodeType.ConstructDelegate)
        {
            this.DelegateType = delegateType;
            this.MethodName = methodName;
            this.TargetObject = targetObject;
            base.SourceContext = sctx;
        }
    }
}

