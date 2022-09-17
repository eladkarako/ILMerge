namespace System.Compiler
{
    using System;

    internal class ModuleReference : Node
    {
        private System.Compiler.Module module;
        private string name;

        public ModuleReference() : base(NodeType.ModuleReference)
        {
        }

        public ModuleReference(string name, System.Compiler.Module module) : base(NodeType.ModuleReference)
        {
            this.name = name;
            this.module = module;
        }

        public System.Compiler.Module Module
        {
            get => 
                this.module;
            set
            {
                this.module = value;
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
    }
}

