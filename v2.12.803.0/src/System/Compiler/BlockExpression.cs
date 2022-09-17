namespace System.Compiler
{
    using System;

    internal class BlockExpression : Expression
    {
        public System.Compiler.Block Block;

        public BlockExpression() : base(NodeType.BlockExpression)
        {
        }

        public BlockExpression(System.Compiler.Block block) : base(NodeType.BlockExpression)
        {
            this.Block = block;
        }

        public BlockExpression(System.Compiler.Block block, TypeNode type) : base(NodeType.BlockExpression)
        {
            this.Block = block;
            this.Type = type;
        }

        public BlockExpression(System.Compiler.Block block, TypeNode type, SourceContext sctx) : base(NodeType.BlockExpression)
        {
            this.Block = block;
            this.Type = type;
            base.SourceContext = sctx;
        }
    }
}

