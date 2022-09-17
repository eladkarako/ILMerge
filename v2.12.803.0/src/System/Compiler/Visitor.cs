namespace System.Compiler
{
    using System;

    internal abstract class Visitor
    {
        protected Visitor()
        {
        }

        public virtual void TransferStateTo(Visitor targetVisitor)
        {
        }

        public abstract Node Visit(Node node);
        public virtual ExpressionList VisitExpressionList(ExpressionList list)
        {
            if (list == null)
            {
                return null;
            }
            int num = 0;
            int count = list.Count;
            while (num < count)
            {
                list[num] = (Expression) this.Visit(list[num]);
                num++;
            }
            return list;
        }
    }
}

