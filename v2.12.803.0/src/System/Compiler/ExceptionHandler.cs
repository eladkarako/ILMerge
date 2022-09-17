namespace System.Compiler
{
    using System;

    internal class ExceptionHandler : Node
    {
        private Block blockAfterHandlerEnd;
        private Block blockAfterTryEnd;
        private Block filterExpression;
        private TypeNode filterType;
        private Block handlerStartBlock;
        private NodeType handlerType;
        private Block tryStartBlock;

        public ExceptionHandler() : base(NodeType.ExceptionHandler)
        {
        }

        public Block BlockAfterHandlerEnd
        {
            get => 
                this.blockAfterHandlerEnd;
            set
            {
                this.blockAfterHandlerEnd = value;
            }
        }

        public Block BlockAfterTryEnd
        {
            get => 
                this.blockAfterTryEnd;
            set
            {
                this.blockAfterTryEnd = value;
            }
        }

        public Block FilterExpression
        {
            get => 
                this.filterExpression;
            set
            {
                this.filterExpression = value;
            }
        }

        public TypeNode FilterType
        {
            get => 
                this.filterType;
            set
            {
                this.filterType = value;
            }
        }

        public Block HandlerStartBlock
        {
            get => 
                this.handlerStartBlock;
            set
            {
                this.handlerStartBlock = value;
            }
        }

        public NodeType HandlerType
        {
            get => 
                this.handlerType;
            set
            {
                this.handlerType = value;
            }
        }

        public Block TryStartBlock
        {
            get => 
                this.tryStartBlock;
            set
            {
                this.tryStartBlock = value;
            }
        }
    }
}

