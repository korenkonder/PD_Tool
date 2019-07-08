using KKdMainLib.IO;

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
        {   get =>    Count > 0 ? Entries[index] : default;
            set { if (Count > 0)  Entries[index] =   value; } }

        public override string ToString() => Count < 1 ? "No Entries" :
            Count == 1 ? Entries[0].ToString() : "Count: " + Count;
    }

    public static class PointerExt
    {
        public static Pointer<T> ReadPointer<T>(this Stream IO) =>
            new Pointer<T> { Offset = IO.ReadInt32() };

        public static Pointer<string> ReadPointerString(this Stream IO)
        {
            Pointer<string> val = IO.ReadPointer<string>();
            val.Value = IO.ReadStringAtOffset(val.Offset); return val;
        }

        public static CountPointer<T> ReadCountPointer<T>(this Stream IO) =>
            new CountPointer<T> { Count = IO.ReadInt32(), Offset = IO.ReadInt32() };
    }
}