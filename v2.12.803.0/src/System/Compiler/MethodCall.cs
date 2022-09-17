namespace System.Compiler
{
    using System;

    internal class MethodCall : NaryExpression
    {
        public bool ArgumentListIsIncomplete;
        private Expression callee;
        public Expression CalleeExpression;
        private TypeNode constraint;
        public bool GiveErrorIfSpecialNameMethod;
        private bool isTailCall;

        public MethodCall()
        {
            base.NodeType = NodeType.MethodCall;
        }

        public MethodCall(Expression callee, ExpressionList arguments) : base(arguments, NodeType.MethodCall)
        {
            this.callee = this.CalleeExpression = callee;
            this.isTailCall = false;
        }

        public MethodCall(Expression callee, ExpressionList arguments, NodeType typeOfCall) : base(arguments, typeOfCall)
        {
            this.callee = callee;
            this.CalleeExpression = callee;
        }

        public MethodCall(Expression callee, ExpressionList arguments, NodeType typeOfCall, TypeNode resultType) : this(callee, arguments, typeOfCall)
        {
            this.Type = resultType;
        }

        public MethodCall(Expression callee, ExpressionList arguments, NodeType typeOfCall, TypeNode resultType, SourceContext sctx) : this(callee, arguments, typeOfCall, resultType)
        {
            base.SourceContext = sctx;
        }

        public Expression Callee
        {
            get => 
                this.callee;
            set
            {
                this.callee = value;
            }
        }

        public TypeNode Constraint
        {
            get => 
                this.constraint;
            set
            {
                this.constraint = value;
            }
        }

        public bool IsTailCall
        {
            get => 
                this.isTailCall;
            set
            {
                this.isTailCall = value;
            }
        }
    }
}

