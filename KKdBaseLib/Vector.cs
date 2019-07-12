namespace KKdBaseLib
{
    public struct Vector2<T>
    {
        public T X;
        public T Y;

        public Vector2(T X, T Y)
        { this.X = X; this.Y = Y; }

        public bool NotNull => X != null && Y != null;

        public override string ToString() => "X: " + X + "; Y: " + Y;
    }
    
    public struct Vector3<T>
    {
        public T X;
        public T Y;
        public T Z;

        public Vector3(T X, T Y, T Z)
        { this.X = X; this.Y = Y; this.Z = Z; }

        public bool NotNull => X != null && Y != null && Z != null;

        public override string ToString() => "X: " + X + "; Y: " + Y + "; Z: " + Z;
    }

    public struct Vector4<T>
    {
        public T X;
        public T Y;
        public T Z;
        public T W;

        public Vector4(T X, T Y, T Z, T W)
        { this.X = X; this.Y = Y; this.Z = Z; this.W = W; }

        public bool NotNull => X != null && Y != null && Z != null && W != null;

        public override string ToString() => "X: " + X + "; Y: " + Y + "; Z: " + Z + "; W: " + W;
    }
}
