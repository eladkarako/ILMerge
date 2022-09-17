namespace System.Compiler
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Win32Resource
    {
        private string typeName;
        private int typeId;
        private string name;
        private int id;
        private int languageId;
        private int codePage;
        private byte[] data;
        public string TypeName
        {
            get => 
                this.typeName;
            set
            {
                this.typeName = value;
            }
        }
        public int TypeId
        {
            get => 
                this.typeId;
            set
            {
                this.typeId = value;
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
        public int Id
        {
            get => 
                this.id;
            set
            {
                this.id = value;
            }
        }
        public int LanguageId
        {
            get => 
                this.languageId;
            set
            {
                this.languageId = value;
            }
        }
        public int CodePage
        {
            get => 
                this.codePage;
            set
            {
                this.codePage = value;
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

