namespace System.Compiler
{
    using System;

    internal class Instruction : Node
    {
        private int offset;
        private System.Compiler.OpCode opCode;
        private object value;

        public Instruction() : base(NodeType.Instruction)
        {
        }

        public Instruction(System.Compiler.OpCode opCode, int offset) : this(opCode, offset, null)
        {
        }

        public Instruction(System.Compiler.OpCode opCode, int offset, object value) : base(NodeType.Instruction)
        {
            this.opCode = opCode;
            this.offset = offset;
            this.value = value;
        }

        public int Offset
        {
            get => 
                this.offset;
            set
            {
                this.offset = value;
            }
        }

        public System.Compiler.OpCode OpCode
        {
            get => 
                this.opCode;
            set
            {
                this.opCode = value;
            }
        }

        public object Value
        {
            get => 
                this.value;
            set
            {
                this.value = value;
            }
        }
    }
}

