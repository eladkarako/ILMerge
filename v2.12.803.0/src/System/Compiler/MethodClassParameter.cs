namespace System.Compiler
{
    using System;
    using System.Reflection;

    internal class MethodClassParameter : ClassParameter
    {
        public MethodClassParameter()
        {
            base.NodeType = NodeType.ClassParameter;
            base.baseClass = CoreSystemTypes.Object;
            base.Flags = TypeFlags.Abstract | TypeFlags.NestedPublic;
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
                if ((base.ParameterListIndex != parameter.ParameterListIndex) || (!(type is MethodClassParameter) && !(type is MethodTypeParameter)))
                {
                    return base.IsStructurallyEquivalentTo(type as MethodClassParameter);
                }
            }
            return true;
        }
    }
}

