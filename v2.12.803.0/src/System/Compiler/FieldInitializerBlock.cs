namespace System.Compiler
{
    using System;

    internal class FieldInitializerBlock : Block
    {
        public bool IsStatic;
        public TypeNode Type;

        public FieldInitializerBlock()
        {
            base.NodeType = NodeType.FieldInitializerBlock;
        }

        public FieldInitializerBlock(TypeNode type, bool isStatic)
        {
            base.NodeType = NodeType.FieldInitializerBlock;
            this.Type = type;
            this.IsStatic = isStatic;
        }
    }
}

