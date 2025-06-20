using System.Runtime.InteropServices;

namespace KKdBaseLib
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Quat
    {
        [FieldOffset(0x00)] public float X;
        [FieldOffset(0x04)] public float Y;
        [FieldOffset(0x08)] public float Z;
        [FieldOffset(0x0C)] public float W;

        public static Quat Identity => new Quat(0.0f, 0.0f, 0.0f, 1.0f);

        public Quat(float x, float y, float z)
        {
            x *= 0.5f;
            y *= 0.5f;
            z *= 0.5f;

            var c1 = (float)System.Math.Cos(x);
            var c2 = (float)System.Math.Cos(y);
            var c3 = (float)System.Math.Cos(z);
            var s1 = (float)System.Math.Sin(x);
            var s2 = (float)System.Math.Sin(y);
            var s3 = (float)System.Math.Sin(z);

            X = s1 * c2 * c3 + c1 * s2 * s3;
            Y = c1 * s2 * c3 - s1 * c2 * s3;
            Z = c1 * c2 * s3 + s1 * s2 * c3;
            W = c1 * c2 * c3 - s1 * s2 * s3;
        }

        public Quat(Vec3 vec) : this(vec.X, vec.Y, vec.Z)
        { }

        public Quat(float x, float y, float z, float w)
        { X = x; Y = y; Z = z; W = w; }

        public Quat(Vec3 vec, float w)
        { X = vec.X; Y = vec.Y; Z = vec.Z; W = w; }

        public Quat(Vec4 vec)
        { X = vec.X; Y = vec.Y; Z = vec.Z; W = vec.W; }

        public Vec3 XYZ { get => new Vec3(X, Y, Z); set { X = value.X; Y = value.Y; Z = value.Z; } }

        public readonly float Length        => (X * X + Y * Y + Z * Z + W * W).Sqrt();
        public readonly float LengthSquared =>  X * X + Y * Y + Z * Z + W * W;
        public readonly Quat Normalized => this * (Length == 0.0f ? 1.0f : (1.0f / Length));

        public static Quat operator +(Quat left, Quat right)
        { left.X += right.X; left.Y += right.Y; left.Z += right.Z; left.W += right.W; return left; }
        public static Quat operator -(Quat left, Quat right)
        { left.X -= right.X; left.Y -= right.Y; left.Z -= right.Z; left.W -= right.W; return left; }
        public static Quat operator -(Quat quat) =>
            new Quat(-quat.X, -quat.Y, -quat.Z, -quat.W);
        public static Quat operator *( Quat  quat, float scale)
        { quat.X *= scale  ; quat.Y *= scale  ; quat.Z *= scale  ; quat.W *= scale  ; return quat; }
        public static Quat operator *(float scale,  Quat  quat)
        { quat.X *= scale  ; quat.Y *= scale  ; quat.Z *= scale  ; quat.W *= scale  ; return quat; }
        public static Quat operator *( Quat  quat,  Vec4 scale)
        { quat.X *= scale.X; quat.Y *= scale.Y; quat.Z *= scale.Z; quat.W *= scale.W; return quat; }
        public static Quat operator *(  Quat left,  Quat right) =>
            new Quat(right.W * left.XYZ + left.W * right.XYZ + Vec3.Cross(left.XYZ, right.XYZ),
                left.W * right.W - Vec3.Dot(left.XYZ, right.XYZ)).Normalized;
        public static Quat operator /(  Quat left,  Quat right) =>
            left * -right;
        public static bool operator ==(Quat A, Quat B) =>  A.Equals(B);
        public static bool operator !=(Quat A, Quat B) => !A.Equals(B);

        public static float Distance       (Quat left, Quat right) =>
            ((right.X - left.X) * (right.X - left.X) + (right.Y - left.Y) * (right.Y - left.Y) +
             (right.Z - left.Z) * (right.Z - left.Z) + (right.W - left.W) * (right.W - left.W)).Sqrt();
        public static float DistanceSquared(Quat left, Quat right) =>
             (right.X - left.X) * (right.X - left.X) + (right.Y - left.Y) * (right.Y - left.Y) +
             (right.Z - left.Z) * (right.Z - left.Z) + (right.W - left.W) * (right.W - left.W);
        public static float Dot(Quat left, Quat right) =>
            left.X * right.X + left.Y * right.Y + left.Z * right.Z + left.W * right.W;
        public Quat Round(     ) =>
            new Quat { X = X.Round( ), Y = Y.Round( ), Z = Z.Round( ), W = W.Round( ) };
        public Quat Round(int d) =>
            new Quat { X = X.Round(d), Y = Y.Round(d), Z = Z.Round(d), W = W.Round(d) };

        public static Quat Lerp(Quat left, Quat right, float blend) {
            Quat x_t;
            Quat y_t;
            x_t = left;
            y_t = right;

            if (Dot(x_t, y_t) < 0.0f)
                x_t = -x_t;

            return (x_t * (1.0f - blend) + y_t * blend).Normalized;
        }

        public static Quat Slerp(Quat left, Quat right, float blend)
        {
            Quat x_t;
            Quat y_t;
            x_t = left;
            y_t = right;

            float dot = Dot(x_t, y_t);
            if (dot < 0.0f)
            {
                dot = -dot;
                y_t = -y_t;
            }

            if (1.0 - dot <= 0.08f)
                return Lerp(x_t, y_t, blend);

            dot = System.Math.Min(dot, 1.0f);

            float theta = (float)System.Math.Acos(dot);
            if (theta == 0.0f)
                return x_t;

            float st = 1.0f / (float)System.Math.Sin(theta);
            float s0 = (float)System.Math.Sin((1.0f - blend) * theta) * st;
            float s1 = (float)System.Math.Sin(theta * blend) * st;
            return (x_t * s0 + y_t * s1).Normalized;
        }

        public static explicit operator Vec4(Quat quat) =>
            new Vec4(quat.XYZ, quat.W);
        public static explicit operator Quat(Vec4  vec) =>
            new Quat( vec.XYZ,  vec.W);

        public bool Equals(Quat other) =>
            X == other.X && Y == other.Y && Z == other.Z && W == other.W;

        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();
        public override string ToString() => $"({X}; {Y}; {Z}; {W})";
        public string ToString(int d) => $"({X.Round(d)}; {Y.Round(d)}; {Z.Round(d)}; {W.Round(d)})";
    }
}
