namespace KKdBaseLib
{
    public struct Vector2<T> : INull
    {
        public T X;
        public T Y;

        public Vector2(T x, T y)
        { X = x; Y = y; }

        public bool  IsNull => X == null && Y == null;
        public bool NotNull => X != null || Y != null;

        public override string ToString() => $"({X}; {Y})";
    }

    public struct Vector3<T> : INull
    {
        public T X;
        public T Y;
        public T Z;

        public Vector3(T x, T y, T z)
        { X = x; Y = y; Z = z; }

        public bool  IsNull => X == null && Y == null && Z == null;
        public bool NotNull => X != null || Y != null || Z != null;

        public override string ToString() => $"({X}; {Y}; {Z})";
    }

    public struct Vector4<T> : INull
    {
        public T X;
        public T Y;
        public T Z;
        public T W;

        public Vector4(T x, T y, T z, T w)
        { X = x; Y = y; Z = z; W = w; }

        public bool  IsNull => X == null && Y == null && Z == null && W == null;
        public bool NotNull => X != null || Y != null || Z != null || W != null;

        public override string ToString() => $"({X}; {Y}; {Z}; {W})";
    }
}
