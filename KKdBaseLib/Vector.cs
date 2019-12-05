namespace KKdBaseLib
{
    public struct Vector2<T> : INull
    {
        public T X;
        public T Y;

        public Vector2(T X, T Y)
        { this.X = X; this.Y = Y; }

        public bool  IsNull => X == null && Y == null;
        public bool NotNull => X != null || Y != null;

        public override string ToString() => $"({X}; {Y})";
    }

    public struct Vector3<T> : INull
    {
        public T X;
        public T Y;
        public T Z;

        public Vector3(T X, T Y, T Z)
        { this.X = X; this.Y = Y; this.Z = Z; }

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

        public Vector4(T X, T Y, T Z, T W)
        { this.X = X; this.Y = Y; this.Z = Z; this.W = W; }

        public bool  IsNull => X == null && Y == null && Z == null && W == null;
        public bool NotNull => X != null || Y != null || Z != null || W != null;

        public override string ToString() => $"({X}; {Y}; {Z}; {W})";
    }
}
