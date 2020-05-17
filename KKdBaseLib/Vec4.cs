using System.Runtime.InteropServices;

namespace KKdBaseLib
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Vec4
    {
        [FieldOffset(0x00)] public float X;
        [FieldOffset(0x04)] public float Y;
        [FieldOffset(0x08)] public float Z;
        [FieldOffset(0x0C)] public float W;

        public Vec4(float val)
        { X = val; Y = val; Z = val; W = 0.0f; }

        public Vec4(float X, float Y)
        { this.X = X; this.Y = Y; Z = 0.0f; W = 0.0f; }

        public Vec4(float X, float Y, float Z)
        { this.X = X; this.Y = Y; this.Z = Z; W = 0.0f; }

        public Vec4(float X, float Y, float Z, float W)
        { this.X = X; this.Y = Y; this.Z = Z; this.W = W; }

        public Vec4(Vec2 vec)
        { X = vec.X; Y = vec.Y; Z = 0.0f; W = 0.0f; }

        public Vec4(Vec2 vec, float Z)
        { X = vec.X; Y = vec.Y; this.Z = Z; W = 0.0f; }

        public Vec4(Vec2 vec, float Z, float W)
        { X = vec.X; Y = vec.Y; this.Z = Z; this.W = W; }

        public Vec4(Vec3 vec)
        { X = vec.X; Y = vec.Y; Z = vec.Z; W = 0.0f; }

        public Vec4(Vec3 vec, float W)
        { X = vec.X; Y = vec.Y; Z = vec.Z; this.W = W; }

        public Vec2 XY  { get => new Vec2(X, Y   ); set { X = value.X; Y = value.Y; } }
        public Vec3 XYZ { get => new Vec3(X, Y, Z); set { X = value.X; Y = value.Y; Z = value.Z; } }

        public float Length        => (X * X + Y * Y + Z * Z + W * W).Sqrt();
        public float LengthSquared =>  X * X + Y * Y + Z * Z + W * W;
        public Vec4 Normalized => new Vec4(X, Y, Z, W) * (Length == 0.0f ? 1.0f : (1.0f / Length));

        public static Vec4 operator +(Vec4 left, Vec4 right)
        { left.X += right.X; left.Y += right.Y; left.Z += right.Z; left.W += right.W; return left; }
        public static Vec4 operator -(Vec4 left, Vec4 right)
        { left.X -= right.X; left.Y -= right.Y; left.Z -= right.Z; left.W -= right.W; return left; }
        public static Vec4 operator -(Vec4 vec)
        { vec.X = -vec.X; vec.Y = -vec.Y; vec.Z = -vec.Z; vec.W = -vec.W; return vec; }
        public static Vec4 operator *(  Vec4  vec, float scale)
        { vec.X *= scale  ; vec.Y *= scale  ; vec.Z *= scale  ; vec.W *= scale  ; return vec; }
        public static Vec4 operator *(float scale,  Vec4   vec)
        { vec.X *= scale  ; vec.Y *= scale  ; vec.Z *= scale  ; vec.W *= scale  ; return vec; }
        public static Vec4 operator *(  Vec4  vec,  Vec4 scale)
        { vec.X *= scale.X; vec.Y *= scale.Y; vec.Z *= scale.Z; vec.W *= scale.W; return vec; }
        public static Vec4 operator /(  Vec4  vec, float scale)
        { vec.X /= scale  ; vec.Y /= scale  ; vec.Z /= scale  ; vec.W /= scale  ; return vec; }
        public static Vec4 operator /(  Vec4  vec,  Vec4 scale)
        { vec.X /= scale.X; vec.Y /= scale.Y; vec.Z /= scale.Z; vec.W /= scale.W; return vec; }
        public static bool operator ==(Vec4 A, Vec4 B) =>  A.Equals(B);
        public static bool operator !=(Vec4 A, Vec4 B) => !A.Equals(B);

        public static Vec4 operator +(Vec4 left, Vec3 right)
        { left.X += right.X; left.Y += right.Y; left.Z += right.Z; return left; }
        public static Vec4 operator -(Vec4 left, Vec3 right)
        { left.X -= right.X; left.Y -= right.Y; left.Z -= right.Z; return left; }
        public static Vec4 operator *(  Vec4  vec,  Vec3 scale)
        { vec.X *= scale.X; vec.Y *= scale.Y; vec.Z *= scale.Z; return vec; }
        public static Vec4 operator /(  Vec4  vec,  Vec3 scale)
        { vec.X /= scale.X; vec.Y /= scale.Y; vec.Z /= scale.Z; return vec; }

        public static Vec4 operator *(Vec4 vec, Mat4 mat) =>
            new Vec4(vec.X * mat.Row0.X + vec.Y * mat.Row1.X + vec.Z * mat.Row2.X + vec.W * mat.Row3.X,
                     vec.X * mat.Row0.Y + vec.Y * mat.Row1.Y + vec.Z * mat.Row2.Y + vec.W * mat.Row3.Y,
                     vec.X * mat.Row0.Z + vec.Y * mat.Row1.Z + vec.Z * mat.Row2.Z + vec.W * mat.Row3.Z,
                     vec.X * mat.Row0.W + vec.Y * mat.Row1.W + vec.Z * mat.Row2.W + vec.W * mat.Row3.W);

        public static float Distance       (Vec4 left, Vec4 right) =>
            ((right.X - left.X) * (right.X - left.X) + (right.Y - left.Y) * (right.Y - left.Y) +
             (right.Z - left.Z) * (right.Z - left.Z) + (right.W - left.W) * (right.W - left.W)).Sqrt();
        public static float DistanceSquared(Vec4 left, Vec4 right) =>
             (right.X - left.X) * (right.X - left.X) + (right.Y - left.Y) * (right.Y - left.Y) +
             (right.Z - left.Z) * (right.Z - left.Z) + (right.W - left.W) * (right.W - left.W);
        public static float Dot(Vec4 left, Vec4 right) =>
            left.X * right.X + left.Y * right.Y + left.Z * right.Z + left.W * right.W;
        public static Vec4 Lerp(Vec4 a, Vec4 b,   float blend) =>
            new Vec4 { X = blend   * (b.X - a.X) + a.X, Y = blend   * (b.Y - a.Y) + a.Y,
                       Z = blend   * (b.Z - a.Z) + a.Z, W = blend   * (b.W - a.W) + a.W };
        public static Vec4 Lerp(Vec4 a, Vec4 b, Vec4 blend) =>
            new Vec4 { X = blend.X * (b.X - a.X) + a.X, Y = blend.Y * (b.Y - a.Y) + a.Y,
                       Z = blend.Z * (b.Z - a.Z) + a.Z, W = blend.W * (b.W - a.W) + a.W };
        public Vec4 Round(     ) =>
            new Vec4 { X = X.Round( ), Y = Y.Round( ), Z = Z.Round( ), W = W.Round( ) };
        public Vec4 Round(int d) =>
            new Vec4 { X = X.Round(d), Y = Y.Round(d), Z = Z.Round(d), W = W.Round(d) };

        public bool Equals(Vec4 other) =>
            X == other.X && Y == other.Y && Z == other.Z && W == other.W;

        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();
        public override string ToString() => $"({X}; {Y}; {Z}; {W})";
        public string ToString(int d) => $"({X.Round(d)}; {Y.Round(d)}; {Z.Round(d)}; {W.Round(d)})";
    }
}
