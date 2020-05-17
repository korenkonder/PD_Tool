using System.Runtime.InteropServices;

namespace KKdBaseLib
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Vec3
    {
        [FieldOffset(0x00)] public float X;
        [FieldOffset(0x04)] public float Y;
        [FieldOffset(0x08)] public float Z;

        public Vec3(float val)
        { X = val; Y = val; Z = val; }

        public Vec3(float X, float Y)
        { this.X = X; this.Y = Y; Z = 0.0f; }

        public Vec3(float X, float Y, float Z)
        { this.X = X; this.Y = Y; this.Z = Z; }

        public Vec3(Vec2 vec)
        { X = vec.X; Y = vec.Y; Z = 0.0f; }

        public Vec3(Vec2 vec, float Z)
        { X = vec.X; Y = vec.Y; this.Z = Z; }

        public Vec3(Vec4 vec)
        { X = vec.X; Y = vec.Y; Z = vec.Z; }

        public Vec2 XY => new Vec2(X, Y);

        public float Length        => (X * X + Y * Y + Z * Z).Sqrt();
        public float LengthSquared =>  X * X + Y * Y + Z * Z;
        public Vec3 Normalized => this = Length == 0.0f ? new Vec3() : this * (1.0f / Length);

        public static Vec3 operator +(Vec3 left, Vec3 right)
        { left.X += right.X; left.Y += right.Y; left.Z += right.Z; return left; }
        public static Vec3 operator -(Vec3 left, Vec3 right)
        { left.X -= right.X; left.Y -= right.Y; left.Z -= right.Z; return left; }
        public static Vec3 operator -(Vec3 vec)
        { vec.X = -vec.X; vec.Y = -vec.Y; vec.Z = -vec.Z; return vec; }
        public static Vec3 operator *( Vec3  vec, float scale)
        { vec.X *= scale  ; vec.Y *= scale  ; vec.Z *= scale  ; return vec; }
        public static Vec3 operator *(float scale, Vec3   vec)
        { vec.X *= scale  ; vec.Y *= scale  ; vec.Z *= scale  ; return vec; }
        public static Vec3 operator *( Vec3  vec,  Vec3 scale)
        { vec.X *= scale.X; vec.Y *= scale.Y; vec.Z *= scale.Z; return vec; }
        public static Vec3 operator /( Vec3  vec, float scale)
        { vec.X /= scale  ; vec.Y /= scale  ; vec.Z /= scale  ; return vec; }
        public static Vec3 operator /( Vec3  vec,  Vec3 scale)
        { vec.X /= scale.X; vec.Y /= scale.Y; vec.Z /= scale.Z; return vec; }
        public static bool operator ==(Vec3 A, Vec3 B) => A.X == B.X && A.Y == B.Y && A.Z == B.Z;
        public static bool operator !=(Vec3 A, Vec3 B) => A.X != B.X || A.Y != B.Y || A.Z != B.Z;

        public static Vec3 operator *(Vec3 vec, Mat3 mat) =>
            new Vec3(vec.X * mat.Row0.X + vec.Y * mat.Row1.X + vec.Z * mat.Row2.X,
                     vec.X * mat.Row0.Y + vec.Y * mat.Row1.Y + vec.Z * mat.Row2.Y,
                     vec.X * mat.Row0.Z + vec.Y * mat.Row1.Z + vec.Z * mat.Row2.Z);

        public static float Distance       (Vec3 left, Vec3 right) =>
            ((right.X - left.X) * (right.X - left.X) + (right.Y - left.Y) *
             (right.Y - left.Y) + (right.Z - left.Z) * (right.Z - left.Z)).Sqrt();
        public static float DistanceSquared(Vec3 left, Vec3 right) =>
             (right.X - left.X) * (right.X - left.X) + (right.Y - left.Y) *
             (right.Y - left.Y) + (right.Z - left.Z) * (right.Z - left.Z);
        public static float Dot(Vec3 left, Vec3 right) =>
             left.X * right.X + left.Y * right.Y + left.Z * right.Z;
        public static Vec3 Cross(Vec3 left, Vec3 right) =>
            new Vec3 { X = left.Y * right.Z - left.Z * right.Y,
                       Y = left.Z * right.X - left.X * right.Z,
                       Z = left.X * right.Y - left.Y * right.X, };
        public static Vec3 Lerp(Vec3 a, Vec3 b,   float blend) =>
            new Vec3 { X = blend   * (b.X - a.X) + a.X,
                       Y = blend   * (b.Y - a.Y) + a.Y,
                       Z = blend   * (b.Z - a.Z) + a.Z };
        public static Vec3 Lerp(Vec3 a, Vec3 b, Vec3 blend) =>
            new Vec3 { X = blend.X * (b.X - a.X) + a.X,
                       Y = blend.Y * (b.Y - a.Y) + a.Y,
                       Z = blend.Z * (b.Z - a.Z) + a.Z };
        public Vec3 Round(     ) =>
            new Vec3 { X = X.Round( ), Y = Y.Round( ), Z = Z.Round( ) };
        public Vec3 Round(int d) =>
            new Vec3 { X = X.Round(d), Y = Y.Round(d), Z = Z.Round(d) };

        public bool Equals(Vec3 other) =>
            X == other.X && Y == other.Y && Z == other.Z;

        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();
        public override string ToString() => $"({X}; {Y}; {Z})";
        public string ToString(int d) => $"({X.Round(d)}; {Y.Round(d)}; {Z.Round(d)})";
    }
}
