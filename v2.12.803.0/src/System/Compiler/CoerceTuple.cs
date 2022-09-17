namespace System.Compiler
{
    using System;

    internal class CoerceTuple : ConstructTuple
    {
        public Expression OriginalTuple;
        public Local Temp;

        public CoerceTuple()
        {
            base.NodeType = NodeType.CoerceTuple;
        }
    }
}

