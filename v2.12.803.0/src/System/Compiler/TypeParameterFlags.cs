namespace System.Compiler
{
    using System;

    [Flags]
    internal enum TypeParameterFlags
    {
        Contravariant = 2,
        Covariant = 1,
        DefaultConstructorConstraint = 0x10,
        NonVariant = 0,
        NoSpecialConstraint = 0,
        ReferenceTypeConstraint = 4,
        SpecialConstraintMask = 0x1c,
        ValueTypeConstraint = 8,
        VarianceMask = 3
    }
}

