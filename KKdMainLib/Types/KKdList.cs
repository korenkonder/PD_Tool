using System.Collections;

namespace KKdMainLib.Types
{
    public struct KKdList<T> : IEnumerator, IEnumerable
    {
        public static KKdList<T> Null => new KKdList<T>();
        public static KKdList<T> New => new KKdList<T>() { Capacity = 0 };
        public static KKdList<T> NewReserve(int Capacity) => new KKdList<T>() { Capacity = Capacity };

        private int Index;
        private int ArrayLength;
        private T[] Array;

        public bool  IsNull => Array == null;
        public bool NotNull => Array != null;

        public int Count { get; private set; }

        public int Capacity { get => ArrayLength;
            set
            {
                ArrayLength = value;
                if (Array != null) System.Array.Resize(ref Array, ArrayLength);
                else Array = new T[ArrayLength];
            }
        }


        public KKdList(T[] Array)
        {
            Index = 0;
            Count = Array.Length;
            ArrayLength = Array.Length;
            this.Array = Array;
        }

        public T Current => Index < Count ? Array[Index] : default(T);

        object IEnumerator.Current => Current;

        public T this[int index]
        {   get { if (Array != null) return Array[index]; return default(T); }
            set { if (Array != null)        Array[index] = value; } }

        public bool MoveNext()
        { if (Index == (Count - 1)) { Index = 0; return false; }
          else                           { Index++  ; return  true; } }

        public IEnumerator GetEnumerator() => this;

        public void Dispose() { Array = null; ArrayLength = 0; Count = 0; Index = 0; }

        public void Reset() => Index = 0;

        public void Add(T item)
        {
            if (IsNull) return;

            Count++;
            if (ArrayLength < Count)
                System.Array.Resize(ref Array, ArrayLength = Count);
            Array[Count - 1] = item;
        }

        public void RemoveAt(int Index)
        {
            if (IsNull) return;

            for (int i = Index; i < Count; i++)
                Array[i] = Array[i + 1];
        }

        public void RemoveRange(int IndexStart, int IndexEnd)
        {
            if (IndexEnd - IndexStart < 1) return;
            if (IsNull) return;

            for (int i = IndexStart; i < Count; i++)
                Array[i] = Array[i + IndexEnd - IndexStart];
        }

        public T[] ToArray() => Array;

        public bool Contains(T val)
        {
            if (IsNull) return false;
            for (int i = 0; i < Count; i++)
                if (Array[i].Equals(val)) return true;
            return false;
        }

        public int IndexOf(T val)
        {
            if (IsNull) return -1;
            for (int i = 0; i < Count; i++)
                if (Array[i].Equals(val)) return i;
            return -1;
        }
    }
}
