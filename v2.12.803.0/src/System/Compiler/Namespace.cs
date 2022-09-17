namespace System.Compiler
{
    using System;
    using System.Runtime.CompilerServices;

    internal class Namespace : Member
    {
        protected string fullName;
        public Identifier FullNameId;
        protected internal bool isPublic;
        public object ProviderHandle;
        public TypeProvider ProvideTypes;
        protected TypeNodeList types;

        public Namespace() : base(NodeType.Namespace)
        {
        }

        public Namespace(Identifier name) : base(NodeType.Namespace)
        {
            base.Name = name;
            this.FullNameId = name;
            if (name != null)
            {
                this.fullName = name.ToString();
            }
        }

        public override string FullName
        {
            get
            {
                if (this.fullName != null)
                {
                    return this.fullName;
                }
                return "";
            }
        }

        public override bool IsAssembly =>
            false;

        public override bool IsCompilerControlled =>
            false;

        public override bool IsFamily =>
            false;

        public override bool IsFamilyAndAssembly =>
            false;

        public override bool IsFamilyOrAssembly =>
            false;

        public override bool IsPrivate =>
            !this.IsPublic;

        public override bool IsPublic =>
            this.isPublic;

        public override bool IsSpecialName =>
            false;

        public override bool IsStatic =>
            false;

        public override bool IsVisibleOutsideAssembly =>
            false;

        public TypeNodeList Types
        {
            get
            {
                if (this.types == null)
                {
                    if (this.ProvideTypes != null)
                    {
                        lock (this)
                        {
                            if (this.types == null)
                            {
                                this.ProvideTypes(this, this.ProviderHandle);
                            }
                            goto Label_0052;
                        }
                    }
                    this.types = new TypeNodeList();
                }
            Label_0052:
                return this.types;
            }
            set
            {
                this.types = value;
            }
        }

        public delegate void TypeProvider(Namespace @namespace, object handle);
    }
}

