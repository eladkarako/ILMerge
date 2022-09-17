namespace System.Compiler
{
    using System;

    internal class LocalBinding : Local, IUniqueKey
    {
        public Local BoundLocal;

        public LocalBinding(Local boundLocal, SourceContext sctx)
        {
            if (boundLocal == null)
            {
                throw new ArgumentNullException("boundLocal");
            }
            this.BoundLocal = boundLocal;
            base.SourceContext = sctx;
            this.Type = boundLocal.Type;
            base.Name = boundLocal.Name;
            base.TypeExpression = boundLocal.TypeExpression;
            base.DeclaringBlock = boundLocal.DeclaringBlock;
            base.Pinned = boundLocal.Pinned;
            base.InitOnly = boundLocal.InitOnly;
            base.Index = boundLocal.Index;
        }

        public override bool Equals(object obj)
        {
            LocalBinding binding = obj as LocalBinding;
            if (binding != null)
            {
                return this.BoundLocal.Equals(binding.BoundLocal);
            }
            return this.BoundLocal.Equals(obj);
        }

        public override int GetHashCode() => 
            this.BoundLocal.GetHashCode();

        int IUniqueKey.UniqueId =>
            this.BoundLocal.UniqueKey;
    }
}

