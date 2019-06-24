namespace KKdMainLib.Types
{
    public struct Pointer<T>
    {
        public int Offset;
        public T Value;
    }

    public struct CountPointer<T>
    {
        public int Count { get => Entries != null ? Entries.Length : 0;
                           set => Entries  =  new T[value]; }
        public int Offset;

        public T[] Entries;
    }
}