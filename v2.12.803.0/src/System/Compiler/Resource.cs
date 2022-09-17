namespace System.Compiler
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Resource
    {
        private bool isPublic;
        private string name;
        private Module definingModule;
        private byte[] data;
        public bool IsPublic
        {
            get => 
                this.isPublic;
            set
            {
                this.isPublic = value;
            }
        }
        public string Name
        {
            get => 
                this.name;
            set
            {
                this.name = value;
            }
        }
        public Module DefiningModule
        {
            get => 
                this.definingModule;
            set
            {
                this.definingModule = value;
            }
        }
        public byte[] Data
        {
            get => 
                this.data;
            set
            {
                this.data = value;
            }
        }
    }
}

