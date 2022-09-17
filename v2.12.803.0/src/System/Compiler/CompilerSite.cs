namespace System.Compiler
{
    using System;

    internal class CompilerSite
    {
        public virtual void OutputMessage(string message)
        {
        }

        public virtual bool ShouldCancel =>
            false;
    }
}

