namespace KKdBaseLib
{
    public struct KKdDict<TKey, TValue> : System.IDisposable, System.Collections.IEnumerator, INull
    {
        public static KKdDict<TKey, TValue> Null => new KKdDict<TKey, TValue>();
        public static KKdDict<TKey, TValue> New  => new KKdDict<TKey, TValue>() { Count = 0, Capacity = 0 };
        public static KKdDict<TKey, TValue> NewReserve(int Capacity) =>
            new KKdDict<TKey, TValue>() { Count = 0, Capacity = Capacity };

        private int index;
        private TKey  [] keyArray;
        private TValue[] valArray;

        public int Count { get; private set; }

        public bool  IsNull => keyArray == null || valArray == null;
        public bool NotNull => keyArray != null && valArray != null;

        public int Capacity { get => keyArray != null && valArray != null ? valArray.Length : -1;
            set { if (keyArray != null) System.Array.Resize(ref keyArray, value); else keyArray = new TKey  [value];
                  if (valArray != null) System.Array.Resize(ref valArray, value); else valArray = new TValue[value];
                  if (Count >= value) Count = value; } }


        public KKdDict(TKey[] keyArray, TValue[] valArray)
        {
            this.keyArray = null; this.valArray = null; Count = 0; index = 0;
            if (keyArray == null || valArray == null || keyArray.Length != valArray.Length) return;
            Count = valArray.Length; this.keyArray = keyArray; this.valArray = valArray; }

        public KeyValuePair<TKey, TValue> Current => index < Count ?
            new KeyValuePair<TKey, TValue>(keyArray[index], valArray[index]) : default;

        object System.Collections.IEnumerator.Current => Current;

        public TKey  [] Keys   => keyArray;
        public TValue[] Values => valArray;

        public KeyValuePair<TKey, TValue> this[ int index, bool list]
        {   get =>    keyArray != null && valArray != null && index > -1 && index < keyArray.Length &&
                index < valArray.Length ? new KeyValuePair<TKey, TValue>(keyArray[index], valArray[index]) : default;
            set { if (keyArray != null && valArray != null && index > -1 && index < keyArray.Length &&
                    index < valArray.Length) { keyArray[index] = value.Key; valArray[index] = value.Value; } } }

        public KeyValuePair<TKey, TValue> this[uint index, bool list]
        {   get =>    keyArray != null && valArray != null && index < keyArray.Length &&
                index < valArray.Length ? new KeyValuePair<TKey, TValue>(keyArray[index], valArray[index]) : default;
            set { if (keyArray != null && valArray != null && index < keyArray.Length &&
                    index < valArray.Length) { keyArray[index] = value.Key; valArray[index] = value.Value; } } }

        public TValue this[TKey key]
        {   get =>    valArray != null && valArray.Length > 0 && ContainsKey(key) ? valArray[IndexOf(key)] : default;
            set { if (valArray != null && valArray.Length > 0 && ContainsKey(key))  valArray[IndexOf(key)] =   value; } }

        public bool MoveNext()
        { if (index == Count - 1) { index = 0; return false; }
          else                    { index++  ; return  true; } }


        public System.Collections.IEnumerator GetEnumerator() => this;

        public void Dispose() { valArray = null; Count = 0; index = 0; }

        public void Reset() => index = 0;

        public void Add(KeyValuePair<TKey, TValue> pair)
        {
            if (IsNull) return;

            Count++;
            if (keyArray.Length < Count) System.Array.Resize(ref keyArray, Count);
            if (valArray.Length < Count) System.Array.Resize(ref valArray, Count);
            keyArray[Count - 1] = pair.Key;
            valArray[Count - 1] = pair.Value;
        }

        public void Add(TKey key, TValue val)
        {
            if (IsNull) return;

            Count++;
            if (keyArray.Length < Count) System.Array.Resize(ref keyArray, Count);
            if (valArray.Length < Count) System.Array.Resize(ref valArray, Count);
            keyArray[Count - 1] = key;
            valArray[Count - 1] = val;
        }

        public bool RemoveKey(TKey key)
        {
            if (IsNull) return false;

            int index = IndexOf(key);
            if (index == -1) return false;

            for (int i = index + 1; i < Count; i++)
            {
                keyArray[i - 1] = keyArray[i];
                valArray[i - 1] = valArray[i];
            }
            Count--;
            return true;
        }

        public bool RemoveValue(TValue val)
        {
            if (IsNull) return false;

            int index = IndexOf(val);
            if (index == -1) return false;

            for (int i = index + 1; i < Count; i++)
            {
                keyArray[i - 1] = keyArray[i];
                valArray[i - 1] = valArray[i];

            }
            Count--;
            return true;
        }

        public void RemoveAt(int index)
        {
            if (IsNull) return;

            for (int i = index + 1; i < Count; i++)
            {
                keyArray[i - 1] = keyArray[i];
                valArray[i - 1] = valArray[i];
            }
            Count--;
        }

        public void RemoveRange(int IndexStart, int IndexEnd)
        {
            if (IsNull) return;
            if (IndexEnd - IndexStart < 1) return;

            if (IndexEnd - IndexStart != Count)
                for (int i = IndexStart; i < Count; i++)
                {
                    keyArray[i] = keyArray[i + IndexEnd - IndexStart];
                    valArray[i] = valArray[i + IndexEnd - IndexStart];
                }
            Count -= IndexEnd - IndexStart;
        }

        public TValue[] ToArray() => valArray;

        public bool ContainsKey(TKey key)
        {
            if (IsNull) return false;
            for (int i = 0; i < Count; i++)
                     if (keyArray[i] == null && key == null) return true;
                else if (keyArray[i] == null || key == null)    continue;
                else if (keyArray[i]    .Equals(key)       ) return true;
            return false;
        }

        public bool ContainsValue(TValue val)
        {
            if (IsNull) return false;
            for (int i = 0; i < Count; i++)
                     if (valArray[i] == null && val == null) return true;
                else if (valArray[i] == null || val == null)    continue;
                else if (valArray[i]    .Equals(val)       ) return true;
            return false;
        }

        public bool Contains(KeyValuePair<TKey, TValue> pair)
        {
            if (IsNull) return false;
            for (int i = 0; i < Count; i++)
                     if ( keyArray[i] == null && pair.Key == null  &&
                     valArray[i] == null && pair.Value == null ) return true;
                else if ((keyArray[i] == null || pair.Key == null) &&
                    (valArray[i] == null || pair.Value == null))    continue;
                else if ( keyArray[i]    .Equals(pair.Key)         &&
                     valArray[i]    .Equals(pair.Value)        ) return true;
            return false;
        }

        private int IndexOf(TKey key)
        {
            if (IsNull) return -1;
            for (int i = 0; i < Count; i++)
                     if (keyArray[i] == null && key == null) return i;
                else if (keyArray[i] == null || key == null) continue;
                else if (keyArray[i]    .Equals(key)       ) return i;
            return -1;
        }

        private int IndexOf(TValue val)
        {
            if (IsNull) return -1;
            for (int i = 0; i < Count; i++)
                     if (keyArray[i] == null && val == null) return i;
                else if (keyArray[i] == null || val == null) continue;
                else if (keyArray[i]    .Equals(val)       ) return i;
            return -1;
        }
    }

    public struct KeyValuePair<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;

        public KeyValuePair(TKey key, TValue value)
        { Key = key; Value = value; }
    }
}
