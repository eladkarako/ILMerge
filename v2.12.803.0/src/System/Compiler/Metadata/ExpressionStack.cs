namespace System.Compiler.Metadata
{
    using System;
    using System.Compiler;

    internal class ExpressionStack
    {
        internal Expression[] elements = new Expression[0x10];
        internal int top = -1;

        internal ExpressionStack()
        {
        }

        private void Grow()
        {
            int length = this.elements.Length;
            Expression[] expressionArray = new Expression[length + 0x40];
            for (int i = 0; i < length; i++)
            {
                expressionArray[i] = this.elements[i];
            }
            this.elements = expressionArray;
        }

        internal Expression Pop()
        {
            if (this.top < 0)
            {
                return new Expression(NodeType.Pop);
            }
            return this.elements[this.top--];
        }

        internal void Push(Expression e)
        {
            if (++this.top >= this.elements.Length)
            {
                this.Grow();
            }
            this.elements[this.top] = e;
        }
    }
}

