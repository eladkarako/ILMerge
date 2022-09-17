namespace System.Compiler
{
    using System;

    internal class CollectionEnumerator : Expression
    {
        public Expression Collection;
        public Method DefaultIndexerGetter;
        public Expression ElementCoercion;
        public Local ElementLocal;
        public Method GetCurrent;
        public Method GetEnumerator;
        public Method LengthPropertyGetter;
        public Method MoveNext;

        public CollectionEnumerator() : base(NodeType.CollectionEnumerator)
        {
        }
    }
}

