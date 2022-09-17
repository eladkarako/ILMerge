﻿namespace System.Compiler
{
    using System;

    internal enum NodeType
    {
        Undefined,
        Add,
        Add_Ovf,
        Add_Ovf_Un,
        And,
        Arglist,
        Box,
        Branch,
        Call,
        Calli,
        Callvirt,
        Castclass,
        Ceq,
        Cgt,
        Cgt_Un,
        Ckfinite,
        Clt,
        Clt_Un,
        Conv_I,
        Conv_I1,
        Conv_I2,
        Conv_I4,
        Conv_I8,
        Conv_Ovf_I,
        Conv_Ovf_I_Un,
        Conv_Ovf_I1,
        Conv_Ovf_I1_Un,
        Conv_Ovf_I2,
        Conv_Ovf_I2_Un,
        Conv_Ovf_I4,
        Conv_Ovf_I4_Un,
        Conv_Ovf_I8,
        Conv_Ovf_I8_Un,
        Conv_Ovf_U,
        Conv_Ovf_U_Un,
        Conv_Ovf_U1,
        Conv_Ovf_U1_Un,
        Conv_Ovf_U2,
        Conv_Ovf_U2_Un,
        Conv_Ovf_U4,
        Conv_Ovf_U4_Un,
        Conv_Ovf_U8,
        Conv_Ovf_U8_Un,
        Conv_R_Un,
        Conv_R4,
        Conv_R8,
        Conv_U,
        Conv_U1,
        Conv_U2,
        Conv_U4,
        Conv_U8,
        Cpblk,
        DebugBreak,
        Div,
        Div_Un,
        Dup,
        EndFilter,
        EndFinally,
        ExceptionHandler,
        Initblk,
        Isinst,
        Jmp,
        Ldftn,
        Ldlen,
        Ldtoken,
        Ldvirtftn,
        Localloc,
        Mkrefany,
        Mul,
        Mul_Ovf,
        Mul_Ovf_Un,
        Neg,
        Nop,
        Not,
        Or,
        Pop,
        ReadOnlyAddressOf,
        Refanytype,
        Refanyval,
        Rem,
        Rem_Un,
        Rethrow,
        Shl,
        Shr,
        Shr_Un,
        Sizeof,
        SkipCheck,
        Sub,
        Sub_Ovf,
        Sub_Ovf_Un,
        SwitchInstruction,
        Throw,
        Unbox,
        UnboxAny,
        Xor,
        AddressDereference,
        AddressOf,
        AssignmentStatement,
        Block,
        Catch,
        Construct,
        ConstructArray,
        Eq,
        ExpressionStatement,
        FaultHandler,
        Filter,
        Finally,
        Ge,
        Gt,
        Identifier,
        Indexer,
        Instruction,
        InterfaceExpression,
        Le,
        Literal,
        LogicalNot,
        Lt,
        MemberBinding,
        NamedArgument,
        Namespace,
        Ne,
        Return,
        This,
        Try,
        ArrayType,
        Assembly,
        AssemblyReference,
        Attribute,
        Class,
        ClassParameter,
        DelegateNode,
        EnumNode,
        Event,
        Field,
        FunctionPointer,
        InstanceInitializer,
        Interface,
        Local,
        Method,
        Module,
        ModuleReference,
        OptionalModifier,
        Parameter,
        Pointer,
        Property,
        Reference,
        RequiredModifier,
        SecurityAttribute,
        StaticInitializer,
        Struct,
        TypeParameter,
        Array,
        BlockReference,
        CompilationParameters,
        Document,
        EndOfRecord,
        Expression,
        Guid,
        List,
        MarshallingInformation,
        Member,
        MemberReference,
        MissingBlockReference,
        MissingExpression,
        MissingMemberReference,
        String,
        StringDictionary,
        TypeNode,
        Uri,
        XmlNode,
        AddEventHandler,
        AliasDefinition,
        AnonymousNestedFunction,
        ApplyToAll,
        ArglistArgumentExpression,
        ArglistExpression,
        ArrayTypeExpression,
        As,
        Assertion,
        AssignmentExpression,
        Assumption,
        Base,
        BlockExpression,
        BoxedTypeExpression,
        ClassExpression,
        CoerceTuple,
        CollectionEnumerator,
        Comma,
        Compilation,
        CompilationUnit,
        CompilationUnitSnippet,
        Conditional,
        ConstructDelegate,
        ConstructFlexArray,
        ConstructIterator,
        ConstructTuple,
        Continue,
        CopyReference,
        CurrentClosure,
        Decrement,
        DefaultValue,
        DoWhile,
        Exit,
        ExplicitCoercion,
        ExpressionSnippet,
        FieldInitializerBlock,
        Fixed,
        FlexArrayTypeExpression,
        For,
        ForEach,
        FunctionDeclaration,
        FunctionTypeExpression,
        Goto,
        GotoCase,
        If,
        ImplicitThis,
        Increment,
        InvariantTypeExpression,
        Is,
        LabeledStatement,
        LocalDeclaration,
        LocalDeclarationsStatement,
        Lock,
        LogicalAnd,
        LogicalOr,
        LRExpression,
        MethodCall,
        NameBinding,
        NonEmptyStreamTypeExpression,
        NonNullableTypeExpression,
        NonNullTypeExpression,
        NullableTypeExpression,
        NullCoalesingExpression,
        OutAddress,
        Parentheses,
        PointerTypeExpression,
        PostfixExpression,
        PrefixExpression,
        QualifiedIdentifer,
        RefAddress,
        ReferenceTypeExpression,
        RefTypeExpression,
        RefValueExpression,
        RemoveEventHandler,
        Repeat,
        ResourceUse,
        SetterValue,
        StackAlloc,
        StatementSnippet,
        StreamTypeExpression,
        Switch,
        SwitchCase,
        SwitchCaseBottom,
        TemplateInstance,
        TupleTypeExpression,
        TypeExpression,
        TypeIntersectionExpression,
        TypeMemberSnippet,
        Typeof,
        TypeReference,
        Typeswitch,
        TypeswitchCase,
        TypeUnionExpression,
        UnaryPlus,
        UsedNamespace,
        VariableDeclaration,
        While,
        Yield,
        ConstrainedType,
        TupleType,
        TypeAlias,
        TypeIntersection,
        TypeUnion,
        Composition,
        QueryAggregate,
        QueryAlias,
        QueryAll,
        QueryAny,
        QueryAxis,
        QueryCommit,
        QueryContext,
        QueryDelete,
        QueryDifference,
        QueryDistinct,
        QueryExists,
        QueryFilter,
        QueryGeneratedType,
        QueryGroupBy,
        QueryInsert,
        QueryIntersection,
        QueryIterator,
        QueryJoin,
        QueryLimit,
        QueryOrderBy,
        QueryOrderItem,
        QueryPosition,
        QueryProject,
        QueryQuantifiedExpression,
        QueryRollback,
        QuerySelect,
        QuerySingleton,
        QueryTransact,
        QueryTypeFilter,
        QueryUnion,
        QueryUpdate,
        QueryYielder,
        Acquire,
        Comprehension,
        ComprehensionBinding,
        Ensures,
        EnsuresExceptional,
        EnsuresNormal,
        Iff,
        Implies,
        Invariant,
        LogicalEqual,
        LogicalImply,
        Maplet,
        MethodContract,
        Modelfield,
        ModelfieldContract,
        OldExpression,
        Range,
        Read,
        Requires,
        RequiresOtherwise,
        RequiresPlain,
        RequiresValidation,
        ReturnValue,
        TypeContract,
        Write,
        OptionalModifierTypeExpression,
        RequiredModifierTypeExpression,
        Count,
        Exists,
        ExistsUnique,
        Forall,
        Max,
        Min,
        Product,
        Sum,
        Quantifier
    }
}

