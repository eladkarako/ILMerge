namespace System.Compiler
{
    using System;

    internal interface ITypeParameter
    {
        Member DeclaringMember { get; set; }

        Module DeclaringModule { get; }

        TypeNode DeclaringType { get; }

        TypeFlags Flags { get; }

        bool IsUnmanaged { get; }

        Identifier Name { get; }

        int ParameterListIndex { get; set; }

        System.Compiler.SourceContext SourceContext { get; }

        System.Compiler.TypeParameterFlags TypeParameterFlags { get; set; }

        int UniqueKey { get; }
    }
}

