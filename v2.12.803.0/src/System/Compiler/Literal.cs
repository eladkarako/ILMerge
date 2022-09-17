namespace System.Compiler
{
    using System;

    internal class Literal : Expression
    {
        public static Literal DoubleOne;
        public static Literal False;
        public static Literal Int32MinusOne;
        public static Literal Int32One;
        public static Literal Int32Sixteen;
        public static Literal Int32Two;
        public static Literal Int32Zero;
        public static Literal Int64One;
        public static Literal Int64Zero;
        public static Literal Null;
        public static Literal SingleOne;
        public Expression SourceExpression;
        public static Literal True;
        public bool TypeWasExplicitlySpecifiedInSource;
        private object value;

        public Literal() : base(NodeType.Literal)
        {
        }

        public Literal(object Value) : base(NodeType.Literal)
        {
            this.value = Value;
        }

        public Literal(object value, TypeNode type) : base(NodeType.Literal)
        {
            this.value = value;
            this.Type = type;
        }

        public Literal(object value, TypeNode type, SourceContext sourceContext) : base(NodeType.Literal)
        {
            this.value = value;
            base.SourceContext = sourceContext;
            this.Type = type;
        }

        public static void ClearStatics()
        {
            DoubleOne = null;
            False = null;
            Int32MinusOne = null;
            Int32Zero = null;
            Int32One = null;
            Int32Two = null;
            Int32Sixteen = null;
            Int64Zero = null;
            Int64One = null;
            Null = null;
            SingleOne = null;
            True = null;
        }

        public static void Initialize()
        {
            DoubleOne = new Literal(1.0, CoreSystemTypes.Double);
            False = new Literal(false, CoreSystemTypes.Boolean);
            Int32MinusOne = new Literal(-1, CoreSystemTypes.Int32);
            Int32Zero = new Literal(0, CoreSystemTypes.Int32);
            Int32One = new Literal(1, CoreSystemTypes.Int32);
            Int32Two = new Literal(2, CoreSystemTypes.Int32);
            Int32Sixteen = new Literal(0x10, CoreSystemTypes.Int32);
            Int64Zero = new Literal(0L, CoreSystemTypes.Int64);
            Int64One = new Literal(1L, CoreSystemTypes.Int64);
            Null = new Literal(null, CoreSystemTypes.Object);
            SingleOne = new Literal(1f, CoreSystemTypes.Single);
            True = new Literal(true, CoreSystemTypes.Boolean);
        }

        public static bool IsNullLiteral(Expression expr)
        {
            Literal literal = expr as Literal;
            return ((literal?.Type == CoreSystemTypes.Object) && (literal.Value == null));
        }

        public override string ToString() => 
            this.Value?.ToString();

        public object Value =>
            this.value;
    }
}

