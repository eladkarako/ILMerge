namespace System.Compiler
{
    using System;

    internal class ClassExpression : Class, ISymbolicTypeReference
    {
        public System.Compiler.Expression Expression;

        public ClassExpression(System.Compiler.Expression expression)
        {
            base.NodeType = NodeType.ClassExpression;
            this.Expression = expression;
        }

        public ClassExpression(System.Compiler.Expression expression, SourceContext sctx)
        {
            base.NodeType = NodeType.ClassExpression;
            this.Expression = expression;
            base.SourceContext = sctx;
        }

        public ClassExpression(System.Compiler.Expression expression, TypeNodeList templateArguments)
        {
            base.NodeType = NodeType.ClassExpression;
            this.Expression = expression;
            this.TemplateArguments = templateArguments;
            if (templateArguments != null)
            {
                base.TemplateArgumentExpressions = templateArguments.Clone();
            }
        }

        public ClassExpression(System.Compiler.Expression expression, TypeNodeList templateArguments, SourceContext sctx)
        {
            base.NodeType = NodeType.ClassExpression;
            this.Expression = expression;
            this.TemplateArguments = base.TemplateArgumentExpressions = templateArguments;
            if (templateArguments != null)
            {
                base.TemplateArgumentExpressions = templateArguments.Clone();
            }
            base.SourceContext = sctx;
        }
    }
}

