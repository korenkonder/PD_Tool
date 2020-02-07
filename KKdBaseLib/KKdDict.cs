namespace KKdBaseLib
{
    public struct KKdDict<TKey, TValue> : System.IDisposable, System.Collections.IEnumerator, INull
    {
        public static KKdDict<TKey, TValue> Null => new KKdDict<TKey, TValue>();
        public static KKdDict<TKey, TValue> New  => new KKdDict<TKey, TValue>() { count = 0, Capacity = 0 };
        public static KKdDict<TKey, TValue> NewReserve(int Capacity) =>
            new KKdDict<TKey, TValue>() { count = 0, Capacity = Capacity };

        private int count;
        private int index;
        private TKey  [] keyArray;
        private TValue[] valArray;

        public int Count => count;

        public bool  IsNull => keyArray == null || valArray == null;
        public bool NotNull => keyArray != null && valArray != null;

        public int Capacity { get => keyArray != null && valArray != null ? valArray.Length : -1;
            set { if (keyArray !=  null) System.Array.Resize(ref keyArray, value); else keyArray = new TKey  [value];
                  if (valArray !=  null) System.Array.Resize(ref valArray, value); else valArray = new TValue[value];
                  if (count    >= value) count = value; } }


        public KKdDict(TKey[] keyArray, TValue[] valArray)
        {
            this.keyArray = null; this.valArray = null; count = 0; index = 0;
            if (keyArray == null || valArray == null || keyArray.Length != valArray.Length) return;
            count = valArray.Length; this.keyArray = keyArray; this.valArray = valArray; }

        public KeyValuePair<TKey, TValue> Current => index < count ?
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
        {   get =>    valArray != null && valArray.Length > 0 && CK(key, out int index) ? valArray[index] : default;
            set { if (valArray != null && valArray.Length > 0) if (CK(key, out int index)) valArray[index] = value; else Add(key, value); } }

        public bool MoveNext()
        { if (index == count - 1) { index = 0; return false; }
          else                    { index++  ; return  true; } }

        public System.Collections.IEnumerator GetEnumerator() => this;

        public void Dispose() { keyArray = null;  valArray = null; count = 0; index = 0; }

        public void Reset() => index = 0;

        public void Add(KeyValuePair<TKey, TValue> pair)
        {
            if (IsNull) return;

            count++;
            if (keyArray.Length < count) System.Array.Resize(ref keyArray, count);
            if (valArray.Length < count) System.Array.Resize(ref valArray, count);
            keyArray[count - 1] = pair.Key;
            valArray[count - 1] = pair.Value;
        }

        public void Add(TKey key, TValue val)
        {
            if (IsNull) return;

            count++;
            if (keyArray.Length < count) System.Array.Resize(ref keyArray, count);
            if (valArray.Length < count) System.Array.Resize(ref valArray, count);
            keyArray[count - 1] = key;
            valArray[count - 1] = val;
        }

        public bool RemoveKey(TKey key)
        {
            if (IsNull) return false;

            int index = IndexOf(key);
            if (index == -1) return false;

            if (index + 1 < count)
            { System.Array.Copy(keyArray, index + 1, keyArray, index, count - index);
              System.Array.Copy(valArray, index + 1, valArray, index, count - index); }
            count--;
            return true;
        }

        public bool RemoveValue(TValue val)
        {
            if (IsNull) return false;

            int index = IndexOf(val);
            if (index == -1) return false;

            if (index + 1 < count)
            { System.Array.Copy(keyArray, index + 1, keyArray, index, count - index);
              System.Array.Copy(valArray, index + 1, valArray, index, count - index); }
            count--;
            return true;
        }

        public void RemoveAt(int index)
        {
            if (IsNull || index < 0 || index >= count) return;

            if (index + 1 < count)
            { System.Array.Copy(keyArray, index + 1, keyArray, index, count - index);
              System.Array.Copy(valArray, index + 1, valArray, index, count - index); }
            count--;
        }

        public void RemoveRange(int indexStart, int indexEnd)
        {
            int indexcount = indexEnd - indexStart;
            if (IsNull || indexcount < 1 || indexStart < 0 || indexEnd > count) return;

            if ((indexEnd + indexcount) < count)
            { System.Array.Copy(keyArray, indexEnd, keyArray, indexStart, indexcount);
              System.Array.Copy(valArray, indexEnd, valArray, indexStart, indexcount); }
            count -= indexcount;
        }

        public TValue[] ToArray() => valArray;

        public bool ContainsKey  (TKey   key) => CK(key, out int index);
        public bool ContainsValue(TValue val) => CV(val, out int index);
        public bool Contains     (KeyValuePair<TKey, TValue> pair) => C(pair, out int index);

        public bool ContainsKey  (TKey   key, out int index) => CK(key, out index);
        public bool ContainsValue(TValue val, out int index) => CV(val, out index);
        public bool Contains     (KeyValuePair<TKey, TValue> pair, out int index) => C(pair, out index);

        private bool CK(TKey key, out int index)
        {
            index = -1;
            if (IsNull) return false;
            for (int i = 0; i < count; i++)
                     if (keyArray[i] == null && key == null) { index = i; return true; }
                else if (keyArray[i] == null || key == null) continue;
                else if (keyArray[i].Equals(key)) { index = i; return true; }
            return false;
        }

        private bool CV(TValue val, out int index)
        {
            index = -1;
            if (IsNull) return false;
            for (int i = 0; i < count; i++)
                     if (valArray[i] == null && val == null) { index = i; return true; }
                else if (valArray[i] == null || val == null) continue;
                else if (valArray[i].Equals(val)) { index = i; return true; }
            return false;
        }

        private bool C(KeyValuePair<TKey, TValue> pair, out int index)
        {
            index = -1;
            if (IsNull) return false;
            for (int i = 0; i < count; i++)
                     if  (keyArray[i] == null && pair.Key   == null &&
                          valArray[i] == null && pair.Value == null) { index = i; return true; }
                else if ((keyArray[i] == null || pair.Key   == null) &&
                         (valArray[i] == null || pair.Value == null)) continue;
                else if  (keyArray[i].Equals(pair.Key  ) &&
                          valArray[i].Equals(pair.Value)) { index = i; return true; }
            return false;
        }

        public TKey GetKey(TValue val)
        {
            if (IsNull) return default;
            for (int i = 0; i < count; i++)
                     if (keyArray[i] == null && val == null) return keyArray[i];
                else if (keyArray[i] == null || val == null) continue;
                else if (keyArray[i].Equals(val)) return keyArray[i];
            return default;
        }

        private int IndexOf(TKey key)
        {
            if (IsNull) return -1;
            for (int i = 0; i < count; i++)
                     if (keyArray[i] == null && key == null) return i;
                else if (keyArray[i] == null || key == null) continue;
                else if (keyArray[i].Equals(key)) return i;
            return -1;
        }

        private int IndexOf(TValue val)
        {
            if (IsNull) return -1;
            for (int i = 0; i < count; i++)
                     if (valArray[i] == null && val == null) return i;
                else if (valArray[i] == null || val == null) continue;
                else if (valArray[i].Equals(val)) return i;
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
