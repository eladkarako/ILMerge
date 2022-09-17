namespace System.Compiler
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    internal class SecurityAttribute : Node
    {
        private SecurityAction action;
        private AttributeList permissionAttributes;
        protected PermissionSet permissions;
        protected string serializedPermissions;

        public SecurityAttribute() : base(NodeType.SecurityAttribute)
        {
        }

        private IPermission CreatePermission(System.Security.Permissions.SecurityAttribute secAttr)
        {
            new PermissionSet(PermissionState.None).PermitOnly();
            try
            {
                return secAttr.CreatePermission();
            }
            catch
            {
            }
            return null;
        }

        protected object GetPermissionOrSetOfPermissionsFromAttribute(AttributeNode attr)
        {
            if (attr == null)
            {
                return null;
            }
            System.Security.Permissions.SecurityAttribute runtimeAttribute = attr.GetRuntimeAttribute() as System.Security.Permissions.SecurityAttribute;
            if (runtimeAttribute == null)
            {
                return null;
            }
            PermissionSetAttribute attribute2 = runtimeAttribute as PermissionSetAttribute;
            if (attribute2 != null)
            {
                return attribute2.CreatePermissionSet();
            }
            return this.CreatePermission(runtimeAttribute);
        }

        protected SecurityElement GetSecurityElement() => 
            SecurityElement.FromString(this.serializedPermissions);

        protected PermissionSet InstantiatePermissionAttributes()
        {
            PermissionSet set = new PermissionSet(PermissionState.None);
            AttributeList permissionAttributes = this.PermissionAttributes;
            int num = 0;
            int num2 = (permissionAttributes == null) ? 0 : permissionAttributes.Count;
            while (num < num2)
            {
                object permissionOrSetOfPermissionsFromAttribute = this.GetPermissionOrSetOfPermissionsFromAttribute(permissionAttributes[num]);
                if (permissionOrSetOfPermissionsFromAttribute != null)
                {
                    if (permissionOrSetOfPermissionsFromAttribute is PermissionSet)
                    {
                        set = set.Union((PermissionSet) permissionOrSetOfPermissionsFromAttribute);
                    }
                    else
                    {
                        IPermission perm = permissionOrSetOfPermissionsFromAttribute as IPermission;
                        if (perm != null)
                        {
                            set.AddPermission(perm);
                        }
                    }
                }
                num++;
            }
            return set;
        }

        public SecurityAction Action
        {
            get => 
                this.action;
            set
            {
                this.action = value;
            }
        }

        public AttributeList PermissionAttributes
        {
            get => 
                this.permissionAttributes;
            set
            {
                this.permissionAttributes = value;
            }
        }

        public PermissionSet Permissions
        {
            get
            {
                if (this.permissions == null)
                {
                    lock (this)
                    {
                        if (this.permissions != null)
                        {
                            return this.permissions;
                        }
                        PermissionSet set2 = null;
                        if (this.PermissionAttributes != null)
                        {
                            set2 = this.InstantiatePermissionAttributes();
                        }
                        else if (this.serializedPermissions != null)
                        {
                            set2 = new PermissionSet(PermissionState.None);
                            set2.FromXml(this.GetSecurityElement());
                        }
                        this.permissions = set2;
                    }
                }
                return this.permissions;
            }
            set
            {
                this.permissions = value;
            }
        }

        public string SerializedPermissions
        {
            get
            {
                if ((this.serializedPermissions == null) && (this.PermissionAttributes != null))
                {
                    lock (this)
                    {
                        if (this.serializedPermissions != null)
                        {
                            return this.serializedPermissions;
                        }
                        PermissionSet permissions = this.Permissions;
                        if (permissions == null)
                        {
                            return null;
                        }
                        SecurityElement element = permissions.ToXml();
                        if (element == null)
                        {
                            return null;
                        }
                        this.serializedPermissions = element.ToString();
                    }
                }
                return this.serializedPermissions;
            }
            set
            {
                this.serializedPermissions = value;
            }
        }
    }
}

