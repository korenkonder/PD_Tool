using System.Collections;
using System.Collections.Generic;

namespace KKdBaseLib
{
    public struct KKdList<T> : System.IDisposable, IEnumerable<T>, IEnumerable, IEnumerator, INull
    {
        public static KKdList<T> Null => new KKdList<T>();
        public static KKdList<T> New  => new KKdList<T>() { count = 0, Capacity = 0 };
        public static KKdList<T> NewReserve(int capacity) => new KKdList<T>() { count = 0, Capacity = capacity };

        private int count;
        private int index;
        private T[] array;

        private Enumerator enumerator;

        public int Count => count;

        public bool  IsNull => array == null;
        public bool NotNull => array != null;

        public int Capacity { get => array != null ? array.Length : -1;
            set { if (array !=  null) System.Array.Resize(ref array, value); else array = new T[value];
                  if (Count >= value) count = value; } }


        public KKdList(T[] array)
        {
            index = -1;
            if (array != null)
            {
                count = array.Length;
                this.array = new T[count];
                System.Array.Copy(array, this.array, count);
            }
            else
            {
                count = 0;
                this.array = null;
            }
            enumerator = new Enumerator(this.array);
        }

        public T Current => index > -1 && index < Count ? array[index] : default;

        object IEnumerator.Current => enumerator.Current;

        public bool MoveNext() => enumerator.MoveNext();
        public void Reset() => enumerator.Reset();

        public T this[ int index]
        {   get =>    array != null && index > -1 && index < array.Length ? array[index] : default;
            set { if (array != null && index > -1 && index < array.Length)  array[index] =   value; } }

        public T this[uint index]
        {   get =>    array != null && index < array.Length ? array[index] : default;
            set { if (array != null && index < array.Length)  array[index] =   value; } }

        public T this[long index]
        {   get =>    array != null && index < array.Length ? array[index] : default;
            set { if (array != null && index < array.Length)  array[index] =   value; } }

        public IEnumerator GetEnumerator() => enumerator = new Enumerator(array);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => enumerator = new Enumerator(array);

        IEnumerator IEnumerable.GetEnumerator() => enumerator = new Enumerator(array);

        public void Dispose() { array = null; count = 0; index = -1; }

        public void Add(T item)
        {
            if (IsNull) return;

            count++;
            if (array.Length < count)
                System.Array.Resize(ref array, array.Length * 2 + 1);
            array[count - 1] = item;
        }

        public void RemoveAt(int index)
        {
            if (IsNull || index < 0 || index >= count) return;

            if (index + 1 < count)
                System.Array.Copy(array, index + 1, array, index, count - index - 1);
            count--;
        }

        public void RemoveRange(int indexStart, int indexEnd)
        {
            int indexCount = indexEnd - indexStart;
            if (IsNull || indexCount < 1 || indexStart < 0 || indexEnd > count) return;

            if ((indexEnd + indexCount) < count)
                System.Array.Copy(array, indexEnd, array, indexStart, indexCount);
            count -= indexCount;
        }

        public T[] ToArray() => array;

        public bool Contains(T val)
        {
            if (IsNull) return false;
            for (int i = 0; i < count; i++)
                     if (array[i] == null && val == null) return true;
                else if (array[i] == null || val == null)    continue;
                else if (array[i].Equals(val)) return true;
            return false;
        }

        public int IndexOf(T val)
        {
            if (IsNull) return -1;
            for (int i = 0; i < count; i++)
                     if (array[i] == null && val == null) return i;
                else if (array[i] == null || val == null) continue;
                else if (array[i].Equals(val)) return i;
            return -1;
        }

        public void Sort()
        { Capacity = Count; List<T> List = (List<T>)this; List.Sort();
          array = List.ToArray(); count = List.Count; }

        public static explicit operator KKdList<T>(   List<T> list) =>
            new KKdList<T> { array = list.ToArray(), count = list.Count };

        public static explicit operator    List<T>(KKdList<T> list)
        { list.Capacity = list.Count; List<T> outList = new List<T>();
          for (int i = 0; i < list.Count; i++) outList.Add(list[i]); return outList; }

        public override string ToString() =>
            $"(Count: {Count}; Capacity: {Capacity}; Current: {Current})";

        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private T[] array;
            private int index;
            private int count;
            private T current;

            internal Enumerator(T[] array)
            { this.array = array; count = index = 0;   current = default;
              if (array != null && array.Length > 0) { current = array[0]; count = array.Length; } }

            public void Dispose()
            { array = null; count = index = 0; current = default; }

            public bool MoveNext()
            {
                if (index < count) { current = array[index]; index++;           return  true; }
                else               { current =      default; index = count + 1; return false; }
            }

            public T Current => current;

            object IEnumerator.Current =>
                (index == 0 || index == array.Length + 1) ? default : current;

            void IEnumerator.Reset() => Reset();

            public void Reset() { index = 0; current = default; }
        }
    }
}
