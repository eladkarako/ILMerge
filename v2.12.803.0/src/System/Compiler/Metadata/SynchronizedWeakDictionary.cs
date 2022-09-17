namespace System.Compiler.Metadata
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal class SynchronizedWeakDictionary : IDictionary, ICollection, IEnumerable
    {
        private System.Collections.Hashtable Hashtable = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());

        internal SynchronizedWeakDictionary()
        {
        }

        public void Add(object key, object value)
        {
            this.Hashtable.Add(key, new WeakReference(value));
        }

        public void Clear()
        {
            this.Hashtable.Clear();
        }

        public bool Contains(object key) => 
            this.Hashtable.Contains(key);

        public void CopyTo(Array array, int index)
        {
            IEnumerator enumerator = this.GetEnumerator();
            for (int i = 0; enumerator.MoveNext(); i++)
            {
                array.SetValue(enumerator.Current, (int) (index + i));
            }
        }

        public IDictionaryEnumerator GetEnumerator() => 
            this.Hashtable.GetEnumerator();

        public void Remove(object key)
        {
            this.Hashtable.Remove(key);
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            new WeakValuesEnumerator(this.Hashtable.GetEnumerator());

        public int Count =>
            this.Hashtable.Count;

        public bool IsFixedSize =>
            false;

        public bool IsReadOnly =>
            false;

        public bool IsSynchronized =>
            false;

        public object this[object key]
        {
            get
            {
                WeakReference reference = (WeakReference) this.Hashtable[key];
                return reference?.Target;
            }
            set
            {
                this.Hashtable[key] = new WeakReference(value);
            }
        }

        public ICollection Keys =>
            this.Hashtable.Keys;

        public object SyncRoot =>
            this.Hashtable.SyncRoot;

        public ICollection Values =>
            new WeakValuesCollection(this.Hashtable.Values);
    }
}

