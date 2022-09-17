namespace System.Compiler
{
    using System;

    internal abstract class MethodContractElement : Node
    {
        public SourceContext DefSite;
        public int ILOffset;
        public bool Inherited;
        public Literal SourceConditionText;
        public Expression UserMessage;
        public bool UsesModels;

        protected MethodContractElement(NodeType nodeType) : base(nodeType)
        {
        }

        public abstract Expression Assertion { get; }
    }
}

