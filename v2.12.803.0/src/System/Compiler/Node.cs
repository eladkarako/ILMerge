namespace System.Compiler
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Threading;

    internal abstract class Node : IUniqueKey
    {
        public bool IsErroneous;
        private System.Compiler.NodeType nodeType;
        public System.Compiler.SourceContext SourceContext;
        private int uniqueKey;
        private static int uniqueKeyCounter;
        private static Hashtable VisitorTypeFor;

        protected Node(System.Compiler.NodeType nodeType)
        {
            this.NodeType = nodeType;
        }

        public virtual Node Clone()
        {
            Node node = (Node) base.MemberwiseClone();
            node.uniqueKey = 0;
            return node;
        }

        public virtual object GetVisitorFor(object callingVisitor, string visitorClassName)
        {
            if ((callingVisitor != null) && (visitorClassName != null))
            {
                return GetVisitorFor(base.GetType(), callingVisitor, visitorClassName);
            }
            return null;
        }

        private static object GetVisitorFor(Type nodeType, object callingVisitor, string visitorClassName)
        {
            if (((nodeType != null) && (callingVisitor != null)) && (visitorClassName != null))
            {
                if (VisitorTypeFor == null)
                {
                    VisitorTypeFor = new Hashtable();
                }
                string str = visitorClassName;
                if (visitorClassName.IndexOf('.') < 0)
                {
                    str = nodeType.Namespace + "." + visitorClassName;
                }
                if (str == callingVisitor.GetType().FullName)
                {
                    return null;
                }
                System.Reflection.AssemblyName assemblyRef = null;
                Assembly assembly = null;
                WeakReference reference = (WeakReference) VisitorTypeFor[str];
                Type target = (reference == null) ? null : ((Type) reference.Target);
                if (target == typeof(object))
                {
                    return null;
                }
                string directoryName = null;
                if (target == null)
                {
                    assembly = nodeType.Assembly;
                    if (assembly == null)
                    {
                        return null;
                    }
                    target = assembly.GetType(str, false);
                }
                if (target == null)
                {
                    if (assembly.Location == null)
                    {
                        return null;
                    }
                    directoryName = Path.GetDirectoryName(assembly.Location);
                    assemblyRef = new System.Reflection.AssemblyName {
                        Name = "Visitors",
                        CodeBase = "file:///" + Path.Combine(directoryName, "Visitors.dll")
                    };
                    try
                    {
                        assembly = Assembly.Load(assemblyRef);
                    }
                    catch
                    {
                    }
                    if (assembly != null)
                    {
                        target = assembly.GetType(str, false);
                    }
                    if (target == null)
                    {
                        assemblyRef.Name = str;
                        assemblyRef.CodeBase = "file:///" + Path.Combine(directoryName, str + ".dll");
                        try
                        {
                            assembly = Assembly.Load(assemblyRef);
                        }
                        catch
                        {
                        }
                        if (assembly != null)
                        {
                            target = assembly.GetType(str, false);
                        }
                    }
                }
                if (target == null)
                {
                    target = typeof(object);
                    assembly = nodeType.Assembly;
                }
                if (assembly != null)
                {
                    lock (VisitorTypeFor)
                    {
                        VisitorTypeFor[str] = new WeakReference(target);
                    }
                }
                if (target != typeof(object))
                {
                    try
                    {
                        return Activator.CreateInstance(target, new object[] { callingVisitor });
                    }
                    catch
                    {
                    }
                }
            }
            return null;
        }

        public System.Compiler.NodeType NodeType
        {
            get => 
                this.nodeType;
            set
            {
                this.nodeType = value;
            }
        }

        int IUniqueKey.UniqueId =>
            this.UniqueKey;

        public virtual int UniqueKey
        {
            get
            {
                if (this.uniqueKey == 0)
                {
                    int uniqueKeyCounter;
                    int num2;
                    do
                    {
                        uniqueKeyCounter = Node.uniqueKeyCounter;
                        num2 = uniqueKeyCounter + 0x11;
                        if (num2 <= 0)
                        {
                            num2 = 0xf4240;
                        }
                    }
                    while (Interlocked.CompareExchange(ref Node.uniqueKeyCounter, num2, uniqueKeyCounter) != uniqueKeyCounter);
                    this.uniqueKey = num2;
                }
                return this.uniqueKey;
            }
        }
    }
}

