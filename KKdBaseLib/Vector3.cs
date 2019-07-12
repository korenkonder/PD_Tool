namespace KKdBaseLib
{
    public struct Vector3
    {
        public double X;
        public double Y;
        public double Z;

        public Vector3(double X, double Y, double Z)
        { this.X = X; this.Y = Y; this.Z = Z; }

        public double Length => (X * X + Y * Y + Z * Z).Sqrt();
        public Vector3 Normalized => this = new Vector3()
        { X = X * (1 / Length), Y = Y * (1 / Length), Z = Z * (1 / Length) };

        public static Vector3 operator +(Vector3 left, Vector3 right)
        { left.X += right.X; left.Y += right.Y; left.Z += right.Z; return left; }
        public static Vector3 operator -(Vector3 left, Vector3 right)
        { left.X -= right.X; left.Y -= right.Y; left.Z -= right.Z; return left; }
        public static Vector3 operator -(Vector3 vec)
        { vec.X = -vec.X; vec.Y = -vec.Y; vec.Z = -vec.Z; return vec; }
        public static Vector3 operator *(Vector3   vec,  double scale)
        { vec.X *= scale  ; vec.Y *= scale  ; vec.Z *= scale  ; return vec; }
        public static Vector3 operator *( double scale, Vector3   vec)
        { vec.X *= scale  ; vec.Y *= scale  ; vec.Z *= scale  ; return vec; }
        public static Vector3 operator *(Vector3   vec, Vector3 scale)
        { vec.X *= scale.X; vec.Y *= scale.Y; vec.Z *= scale.Z; return vec; }
        public static Vector3 operator /(Vector3   vec,  double scale)
        { vec.X /= scale  ; vec.Y /= scale  ; vec.Z /= scale  ; return vec; }
        public static Vector3 operator /(Vector3   vec, Vector3 scale)
        { vec.X /= scale.X; vec.Y /= scale.Y; vec.Z /= scale.Z; return vec; }
        public static bool operator ==(Vector3 A, Vector3 B) =>  A.Equals(B);
        public static bool operator !=(Vector3 A, Vector3 B) => !A.Equals(B);

        public static double Distance       (Vector3 left, Vector3 right) =>
            (right.X - left.X) * (right.X - left.X) + (right.Y - left.Y) *
            (right.Y - left.Y) + (right.Z - left.Z) * (right.Z - left.Z).Sqrt();
        public static double DistanceSquared(Vector3 left, Vector3 right) =>
            (right.X - left.X) * (right.X - left.X) + (right.Y - left.Y) *
            (right.Y - left.Y) + (right.Z - left.Z) * (right.Z - left.Z);
        public static double  Dot  (Vector3 left, Vector3 right) =>
            (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z);
        public static Vector3 Cross(Vector3 left, Vector3 right) =>
            new Vector3 { X = (left.Y * right.Z) - (left.Z * right.Y),
                          Y = (left.Z * right.X) - (left.X * right.Z),
                          Z = (left.X * right.Y) - (left.Y * right.X), };
        public static Vector3 Lerp (Vector3    a, Vector3     b,  double blend) =>
            new Vector3 { X = blend   * (b.X - a.X) + a.X,
                          Y = blend   * (b.Y - a.Y) + a.Y,
                          Z = blend   * (b.Z - a.Z) + a.Z };
        public static Vector3 Lerp (Vector3    a, Vector3     b, Vector3 blend) =>
            new Vector3 { X = blend.X * (b.X - a.X) + a.X,
                          Y = blend.Y * (b.Y - a.Y) + a.Y,
                          Z = blend.Z * (b.Z - a.Z) + a.Z };
        public Vector3 Round(     ) =>
            new Vector3 { X = X.Round( ), Y = Y.Round( ), Z = Z.Round( ) };
        public Vector3 Round(int d) =>
            new Vector3 { X = X.Round(d), Y = Y.Round(d), Z = Z.Round(d) };

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }
        public override bool Equals(object obj) =>
            obj is Vector3 vec ? Equals(vec) : false;
        public bool Equals(Vector3 other) =>
            X == other.X && Y == other.Y && Z == other.Z;

        public override string ToString() => "X: " + X + "; Y: " + Y + "; Z: " + Z;
    }
}
