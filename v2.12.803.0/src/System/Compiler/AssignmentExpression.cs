namespace System.Compiler
{
    using System;

    internal class AssignmentExpression : Expression
    {
        public Statement AssignmentStatement;

        public AssignmentExpression() : base(NodeType.AssignmentExpression)
        {
        }

        public AssignmentExpression(System.Compiler.AssignmentStatement assignment) : base(NodeType.AssignmentExpression)
        {
            this.AssignmentStatement = assignment;
        }
    }
}

