namespace KKdMainLib.Types
{
    public struct Pointer<T>
    {
        public int Offset;
        public T Value;

        public override string ToString() => Main.ToString(Value);
    }
    
    public struct CountPointer<T>
    {
        public int Count { get => Entries != null ? Entries.Length : 0;
                           set => Entries  =  new T[value]; }
        public int Offset;
        public T[] Entries;
        
        public T this[int index]
        {
            get { if (Count > 0) return Entries[index]; return default(T); }
            set { if (Count > 0) { Entries[index] = value; } }
        }

        public override string ToString() => Count < 1 ? "No Entries" :
            Count == 1 ? Entries[0].ToString() : "Count: " + Count;
    }
}