namespace KKdMainLib.Types
{
    public struct Vector2<T>
    {
        public T X;
        public T Y;

        public Vector2(T X, T Y)
        { this.X = X; this.Y = Y; }

        public override string ToString() => "X: " + X + "; Y: " + Y;
    }
    
    public struct Vector3<T>
    {
        public T X;
        public T Y;
        public T Z;

        public Vector3(T X, T Y, T Z)
        { this.X = X; this.Y = Y; this.Z = Z; }

        public override string ToString() => "X: " + X + "; Y: " + Y + "; Z: " + Z;
    }
}
