namespace System.Compiler
{
    using System;

    internal sealed class MarshallingInformation
    {
        private string @class;
        private string cookie;
        private int elementSize;
        private System.Compiler.NativeType elementType;
        private System.Compiler.NativeType nativeType;
        private int numberOfElements;
        private int paramIndex;
        private int size;

        public MarshallingInformation Clone() => 
            ((MarshallingInformation) base.MemberwiseClone());

        public string Class
        {
            get => 
                this.@class;
            set
            {
                this.@class = value;
            }
        }

        public string Cookie
        {
            get => 
                this.cookie;
            set
            {
                this.cookie = value;
            }
        }

        public int ElementSize
        {
            get => 
                this.elementSize;
            set
            {
                this.elementSize = value;
            }
        }

        public System.Compiler.NativeType ElementType
        {
            get => 
                this.elementType;
            set
            {
                this.elementType = value;
            }
        }

        public System.Compiler.NativeType NativeType
        {
            get => 
                this.nativeType;
            set
            {
                this.nativeType = value;
            }
        }

        public int NumberOfElements
        {
            get => 
                this.numberOfElements;
            set
            {
                this.numberOfElements = value;
            }
        }

        public int ParamIndex
        {
            get => 
                this.paramIndex;
            set
            {
                this.paramIndex = value;
            }
        }

        public int Size
        {
            get => 
                this.size;
            set
            {
                this.size = value;
            }
        }
    }
}

