using System.Runtime.InteropServices;

namespace KKdBaseLib
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Vec2
    {
        [FieldOffset(0x00)] public float X;
        [FieldOffset(0x04)] public float Y;

        public Vec2(float val)
        { X = val; Y = val; }

        public Vec2(float X, float Y)
        { this.X = X; this.Y = Y; }

        public Vec2(Vec4 vec)
        { X = vec.X; Y = vec.Y; }

        public float Length        => (X * X + Y * Y).Sqrt();
        public float LengthSquared =>  X * X + Y * Y;
        public Vec2 Normalized => this = Length == 0.0f ? new Vec2() : this * (1.0f / Length);

        public static Vec2 operator +(Vec2 left, Vec2 right)
        { left.X += right.X; left.Y += right.Y; return left; }
        public static Vec2 operator -(Vec2 left, Vec2 right)
        { left.X -= right.X; left.Y -= right.Y; return left; }
        public static Vec2 operator -(Vec2 vec)
        { vec.X = -vec.X; vec.Y = -vec.Y; return vec; }
        public static Vec2 operator *(Vec2   vec,   float scale)
        { vec.X *= scale  ; vec.Y *= scale  ; return vec; }
        public static Vec2 operator *(  float scale, Vec2   vec)
        { vec.X *= scale  ; vec.Y *= scale  ; return vec; }
        public static Vec2 operator *(Vec2   vec, Vec2 scale)
        { vec.X *= scale.X; vec.Y *= scale.Y; return vec; }
        public static Vec2 operator /(Vec2   vec,   float scale)
        { vec.X /= scale  ; vec.Y /= scale  ; return vec; }
        public static Vec2 operator /(Vec2   vec, Vec2 scale)
        { vec.X /= scale.X; vec.Y /= scale.Y; return vec; }
        public static bool operator ==(Vec2 A, Vec2 B) => A.X == B.X && A.Y == B.Y;
        public static bool operator !=(Vec2 A, Vec2 B) => A.X != B.X || A.Y != B.Y;

        public static Vec2 operator *(Vec2 vec, Mat2 mat) =>
            new Vec2(vec.X * mat.Row0.X + vec.Y * mat.Row1.X,
                     vec.X * mat.Row0.Y + vec.Y * mat.Row1.Y);

        public static float Distance       (Vec2 left, Vec2 right) =>
            (right.X - left.X) * (right.X - left.X) + (right.Y - left.Y) * (right.Y - left.Y).Sqrt();
        public static float DistanceSquared(Vec2 left, Vec2 right) =>
            (right.X - left.X) * (right.X - left.X) + (right.Y - left.Y) * (right.Y - left.Y);
        public static float Dot(Vec2 left, Vec2 right) =>
             left.X * right.X + left.Y * right.Y;
        public static Vec2 Lerp(Vec2 a, Vec2 b,   float blend) =>
            new Vec2 { X = blend   * (b.X - a.X) + a.X, Y = blend   * (b.Y - a.Y) + a.Y };
        public static Vec2 Lerp(Vec2 a, Vec2 b, Vec2 blend) =>
            new Vec2 { X = blend.X * (b.X - a.X) + a.X, Y = blend.Y * (b.Y - a.Y) + a.Y };
        public Vec2 Round(     ) =>
            new Vec2 { X = X.Round( ), Y = Y.Round( ) };
        public Vec2 Round(int d) =>
            new Vec2 { X = X.Round(d), Y = Y.Round(d) };

        public bool Equals(Vec2 other) =>
            X == other.X && Y == other.Y;

        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();
        public override string ToString() => $"({X}; {Y})";
        public string ToString(int d) => $"({X.Round(d)}; {Y.Round(d)})";
    }
}
