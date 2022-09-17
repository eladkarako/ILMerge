namespace System.Compiler
{
    using System;

    internal class TypeExpression : TypeNode, ISymbolicTypeReference
    {
        public int Arity;
        public System.Compiler.Expression Expression;

        public TypeExpression(System.Compiler.Expression expression) : base(NodeType.TypeExpression)
        {
            this.Expression = expression;
        }

        public TypeExpression(System.Compiler.Expression expression, SourceContext sctx) : base(NodeType.TypeExpression)
        {
            this.Expression = expression;
            base.SourceContext = sctx;
        }

        public TypeExpression(System.Compiler.Expression expression, TypeNodeList templateArguments) : base(NodeType.TypeExpression)
        {
            this.Expression = expression;
            base.templateArguments = base.TemplateArgumentExpressions = templateArguments;
        }

        public TypeExpression(System.Compiler.Expression expression, int arity) : base(NodeType.TypeExpression)
        {
            this.Expression = expression;
            this.Arity = arity;
        }

        public TypeExpression(System.Compiler.Expression expression, TypeNodeList templateArguments, SourceContext sctx) : base(NodeType.TypeExpression)
        {
            this.Expression = expression;
            base.templateArguments = base.TemplateArgumentExpressions = templateArguments;
            base.SourceContext = sctx;
        }

        public TypeExpression(System.Compiler.Expression expression, int arity, SourceContext sctx) : base(NodeType.TypeExpression)
        {
            this.Expression = expression;
            this.Arity = arity;
            base.SourceContext = sctx;
        }

        public override bool IsUnmanaged
        {
            get
            {
                Literal expression = this.Expression as Literal;
                if (expression != null)
                {
                    TypeNode node = expression.Value as TypeNode;
                    if (node != null)
                    {
                        return node.IsUnmanaged;
                    }
                    if (expression.Value is TypeCode)
                    {
                        return true;
                    }
                }
                return true;
            }
        }
    }
}

