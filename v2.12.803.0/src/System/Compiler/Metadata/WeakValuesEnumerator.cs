namespace System.Compiler.Metadata
{
    using System;
    using System.Collections;

    internal class WeakValuesEnumerator : IEnumerator
    {
        private IEnumerator enumerator;

        internal WeakValuesEnumerator(IEnumerator enumerator)
        {
            this.enumerator = enumerator;
        }

        public bool MoveNext() => 
            this.enumerator.MoveNext();

        public void Reset()
        {
            this.enumerator.Reset();
        }

        public object Current
        {
            get
            {
                object current = this.enumerator.Current;
                if (current is DictionaryEntry)
                {
                    DictionaryEntry entry = (DictionaryEntry) current;
                    current = entry.Value;
                }
                WeakReference reference = current as WeakReference;
                if (reference != null)
                {
                    return reference.Target;
                }
                return null;
            }
        }
    }
}

