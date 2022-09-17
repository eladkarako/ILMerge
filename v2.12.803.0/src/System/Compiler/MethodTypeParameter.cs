namespace System.Compiler
{
    using System;
    using System.Reflection;

    internal class MethodTypeParameter : TypeParameter
    {
        public MethodTypeParameter()
        {
            base.NodeType = NodeType.TypeParameter;
            base.Flags = TypeFlags.Abstract | TypeFlags.ClassSemanticsMask | TypeFlags.NestedPublic;
            base.Namespace = StandardIds.TypeParameter;
        }

        public MethodTypeParameter(InterfaceList baseInterfaces, TypeNode.NestedTypeProvider provideNestedTypes, TypeNode.TypeAttributeProvider provideAttributes, TypeNode.TypeMemberProvider provideMembers, object handle) : base(baseInterfaces, provideNestedTypes, provideAttributes, provideMembers, handle)
        {
            base.NodeType = NodeType.TypeParameter;
            base.Flags = TypeFlags.Abstract | TypeFlags.ClassSemanticsMask | TypeFlags.NestedPublic;
            base.Namespace = StandardIds.TypeParameter;
        }

        public override Type GetRuntimeType()
        {
            Method declaringMember = base.DeclaringMember as Method;
            if (declaringMember == null)
            {
                return null;
            }
            System.Reflection.MethodInfo methodInfo = declaringMember.GetMethodInfo();
            if (methodInfo == null)
            {
                return null;
            }
            Type[] genericArguments = methodInfo.GetGenericArguments();
            if (base.ParameterListIndex >= genericArguments.Length)
            {
                return null;
            }
            return genericArguments[base.ParameterListIndex];
        }

        public override bool IsStructurallyEquivalentTo(TypeNode type)
        {
            if (!object.ReferenceEquals(this, type))
            {
                ITypeParameter parameter = type as ITypeParameter;
                if (parameter == null)
                {
                    return false;
                }
                if ((base.ParameterListIndex != parameter.ParameterListIndex) || (!(type is MethodTypeParameter) && !(type is MethodClassParameter)))
                {
                    return base.IsStructurallyEquivalentTo(type as MethodTypeParameter);
                }
            }
            return true;
        }
    }
}

