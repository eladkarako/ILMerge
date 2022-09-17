namespace ILMergeGui
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class DynaInvoke
    {
        private static Dictionary<string, Assembly> AssemblyReferences = new Dictionary<string, Assembly>();
        private static Dictionary<string, DynaClassInfo> ClassReferences = new Dictionary<string, DynaClassInfo>();

        private static T CallMethod<T>(DynaClassInfo ci, string MethodName, object[] mArgs) => 
            ((T) ci.type.InvokeMember(MethodName, BindingFlags.InvokeMethod, null, ci.ClassObject, mArgs));

        public static void CallMethod(string AssemblyName, string ClassName, string MethodName, object[] mArgs, object[] cArgs = null)
        {
            CallMethod<object>(GetClassReference(AssemblyName, ClassName, cArgs), MethodName, mArgs);
        }

        public static T CallMethod<T>(string AssemblyName, string ClassName, string MethodName, object[] mArgs, object[] cArgs = null) => 
            CallMethod<T>(GetClassReference(AssemblyName, ClassName, cArgs), MethodName, mArgs);

        internal static DynaClassInfo GetClassReference(string AssemblyName, string ClassName, object[] cArgs = null)
        {
            if (PreLoadAssembly(AssemblyName, ClassName, cArgs))
            {
                return ClassReferences[AssemblyName];
            }
            return null;
        }

        private static T GetProperty<T>(DynaClassInfo ci, string PropName)
        {
            object obj2 = ci.type.GetProperty(PropName).GetValue(ci.ClassObject, new object[0]);
            if (obj2 != null)
            {
                return (T) obj2;
            }
            return default(T);
        }

        public static T GetProperty<T>(string AssemblyName, string ClassName, string PropName, object[] cArgs = null) => 
            GetProperty<T>(GetClassReference(AssemblyName, ClassName, cArgs), PropName);

        public static bool PreLoadAssembly(string AssemblyName, string ClassName, object[] cArgs = null)
        {
            if (!ClassReferences.ContainsKey(AssemblyName))
            {
                Assembly assembly;
                if (!AssemblyReferences.ContainsKey(AssemblyName))
                {
                    AssemblyReferences.Add(AssemblyName, assembly = Assembly.LoadFrom(AssemblyName));
                }
                else
                {
                    assembly = AssemblyReferences[AssemblyName];
                }
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsClass && type.FullName.EndsWith("." + ClassName))
                    {
                        try
                        {
                            DynaClassInfo info = new DynaClassInfo(type, Activator.CreateInstance(type, cArgs));
                            ClassReferences.Add(AssemblyName, info);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            return ClassReferences.ContainsKey(AssemblyName);
        }

        private static T SetProperty<T>(DynaClassInfo ci, string PropName, T arg)
        {
            PropertyInfo property = ci.type.GetProperty(PropName);
            Type propertyType = property.PropertyType;
            if ((propertyType.IsGenericType && (propertyType.GetGenericArguments().Length != 0)) && propertyType.GetGenericArguments()[0].IsEnum)
            {
                TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);
                string text1 = arg.ToString();
                if (text1 == null)
                {
                }
                property.SetValue(ci.ClassObject, converter.ConvertFrom(""), new object[0]);
                TypeConverter converter2 = TypeDescriptor.GetConverter(typeof(T));
                string text2 = ((int) property.GetValue(ci.ClassObject, new object[0])).ToString();
                if (text2 == null)
                {
                }
                return (T) converter2.ConvertFrom("");
            }
            property.SetValue(ci.ClassObject, arg, new object[0]);
            return (T) property.GetValue(ci.ClassObject, new object[0]);
        }

        public static T SetProperty<T>(string AssemblyName, string ClassName, string PropName, T pArg, object[] cArgs = null) => 
            SetProperty<T>(GetClassReference(AssemblyName, ClassName, cArgs), PropName, pArg);

        internal class DynaClassInfo
        {
            public object ClassObject;
            public Type type;

            public DynaClassInfo()
            {
            }

            public DynaClassInfo(Type t, object c)
            {
                this.type = t;
                this.ClassObject = c;
            }
        }
    }
}

