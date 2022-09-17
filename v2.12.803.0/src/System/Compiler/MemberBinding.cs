namespace System.Compiler
{
    using System;

    internal class MemberBinding : Expression
    {
        private int alignment;
        private Member boundMember;
        public Expression BoundMemberExpression;
        private Expression targetObject;
        private bool volatile;

        public MemberBinding() : base(NodeType.MemberBinding)
        {
        }

        public MemberBinding(Expression targetObject, Member boundMember) : this(targetObject, boundMember, false, -1)
        {
            if (boundMember is Field)
            {
                this.Volatile = ((Field) boundMember).IsVolatile;
            }
        }

        public MemberBinding(Expression targetObject, Member boundMember, Expression boundMemberExpression) : this(targetObject, boundMember, false, -1)
        {
            if (boundMember is Field)
            {
                this.Volatile = ((Field) boundMember).IsVolatile;
            }
            this.BoundMemberExpression = boundMemberExpression;
        }

        public MemberBinding(Expression targetObject, Member boundMember, SourceContext sctx) : this(targetObject, boundMember, false, -1)
        {
            if (boundMember is Field)
            {
                this.Volatile = ((Field) boundMember).IsVolatile;
            }
            base.SourceContext = sctx;
        }

        public MemberBinding(Expression targetObject, Member boundMember, bool volatile, int alignment) : base(NodeType.MemberBinding)
        {
            this.alignment = alignment;
            this.boundMember = boundMember;
            this.targetObject = targetObject;
            this.volatile = volatile;
            switch (boundMember.NodeType)
            {
                case NodeType.Event:
                    this.Type = ((Event) boundMember).HandlerType;
                    return;

                case NodeType.Field:
                    this.Type = ((Field) boundMember).Type;
                    return;

                case NodeType.Method:
                    this.Type = ((Method) boundMember).ReturnType;
                    return;
            }
            this.Type = boundMember as TypeNode;
        }

        public MemberBinding(Expression targetObject, Member boundMember, SourceContext sctx, Expression boundMemberExpression) : this(targetObject, boundMember, false, -1)
        {
            if (boundMember is Field)
            {
                this.Volatile = ((Field) boundMember).IsVolatile;
            }
            base.SourceContext = sctx;
            this.BoundMemberExpression = boundMemberExpression;
        }

        public int Alignment
        {
            get => 
                this.alignment;
            set
            {
                this.alignment = value;
            }
        }

        public Member BoundMember
        {
            get => 
                this.boundMember;
            set
            {
                this.boundMember = value;
            }
        }

        public Expression TargetObject
        {
            get => 
                this.targetObject;
            set
            {
                this.targetObject = value;
            }
        }

        public bool Volatile
        {
            get => 
                this.volatile;
            set
            {
                this.volatile = value;
            }
        }
    }
}

