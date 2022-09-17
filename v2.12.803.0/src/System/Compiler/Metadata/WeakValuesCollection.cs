namespace System.Compiler.Metadata
{
    using System;
    using System.Collections;

    internal class WeakValuesCollection : ICollection, IEnumerable
    {
        private ICollection collection;

        internal WeakValuesCollection(ICollection collection)
        {
            this.collection = collection;
        }

        public void CopyTo(Array array, int index)
        {
            IEnumerator enumerator = this.GetEnumerator();
            for (int i = 0; enumerator.MoveNext(); i++)
            {
                array.SetValue(enumerator.Current, (int) (index + i));
            }
        }

        public IEnumerator GetEnumerator() => 
            new WeakValuesEnumerator(this.collection.GetEnumerator());

        public int Count =>
            this.collection.Count;

        public bool IsSynchronized =>
            this.collection.IsSynchronized;

        public object SyncRoot =>
            this.collection.SyncRoot;
    }
}

