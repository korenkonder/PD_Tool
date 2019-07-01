using System.Collections;

namespace KKdMainLib.Types
{
    public struct KKdList<T> : IEnumerator, IEnumerable
    {
        public static KKdList<T> Null => new KKdList<T>();
        public static KKdList<T> New => new KKdList<T>() { Capacity = 0 };
        public static KKdList<T> NewReserve(int Capacity) => new KKdList<T>() { Capacity = Capacity };

        private int index;
        private T[] array;

        public int Count { get; private set; }

        public bool  IsNull => array == null;
        public bool NotNull => array != null;

        public int Capacity { get => array != null ? array.Length : -1;
            set
            {
                if (array != null) System.Array.Resize(ref array, value);
                else array = new T[value];
            }
        }


        public KKdList(T[] Array)
        {
            index = 0;
            Count = Array.Length;
            this.array = Array;
        }

        public T Current => index < Count ? array[index] : default(T);

        object IEnumerator.Current => Current;

        public T this[int index]
        {   get { if (array != null) return array[index]; return default(T); }
            set { if (array != null)        array[index] = value; } }

        public bool MoveNext()
        { if (index == (Count - 1)) { index = 0; return false; }
          else                      { index++  ; return  true; } }

        public IEnumerator GetEnumerator() => this;

        public void Dispose() { array = null; Count = 0; index = 0; }

        public void Reset() => index = 0;

        public void Add(T item)
        {
            if (IsNull) return;

            Count++;
            if (array.Length < Count)
                System.Array.Resize(ref array, Count);
            array[Count - 1] = item;
        }

        public void RemoveAt(int index)
        {
            if (IsNull) return;

            for (int i = index; i < Count; i++)
                array[i] = array[i + 1];
        }

        public void RemoveRange(int IndexStart, int IndexEnd)
        {
            if (IndexEnd - IndexStart < 1) return;
            if (IsNull) return;

            for (int i = IndexStart; i < Count; i++)
                array[i] = array[i + IndexEnd - IndexStart];
        }

        public T[] ToArray() => array;

        public bool Contains(T val)
        {
            if (IsNull) return false;
            for (int i = 0; i < Count; i++)
                if (array[i].Equals(val)) return true;
            return false;
        }

        public int IndexOf(T val)
        {
            if (IsNull) return -1;
            for (int i = 0; i < Count; i++)
                if (array[i].Equals(val)) return i;
            return -1;
        }
    }
}
