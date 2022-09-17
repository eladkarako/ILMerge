namespace System.Compiler
{
    using System;

    internal class This : Parameter
    {
        public This()
        {
            base.NodeType = NodeType.This;
            base.Name = StandardIds.This;
        }

        public This(TypeNode type)
        {
            base.NodeType = NodeType.This;
            base.Name = StandardIds.This;
            this.Type = type;
        }
    }
}

