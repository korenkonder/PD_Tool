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

        public float Length        => (X * X + Y * Y + Z * Z + W * W).Sqrt();
        public float LengthSquared =>  X * X + Y * Y + Z * Z + W * W;
        public Quat Normalized => new Quat(X, Y, Z, W) * (Length == 0.0f ? 1.0f : (1.0f / Length));
        public Vec3 Euler { get
            {
                if (LengthSquared == 0) return default;

                const float SINGULARITY_THRESHOLD = 0.4999995f;
                const float HalfPI = (float)(System.Math.PI * 0.5);

                float sqx = X * X, sqy = Y * Y, sqz = Z * Z, sqw = W * W;
                float unit = sqx + sqy + sqz + sqw;
                float singularityTest = X * Z + Y * W;

                     if (singularityTest >  SINGULARITY_THRESHOLD * unit)
                    return new Vec3(0.0f,  HalfPI,  (float)(System.Math.Atan2(X, W) * 2.0));
                else if (singularityTest < -SINGULARITY_THRESHOLD * unit)
                    return new Vec3(0.0f, -HalfPI, -(float)(System.Math.Atan2(X, W) * 2.0));
                else
                    return new Vec3((float)System.Math.Atan2(2 * (X * W - Y * Z), sqw - sqx - sqy + sqz),
                                    (float)System.Math.Asin (2 * singularityTest / unit),
                                    (float)System.Math.Atan2(2 * (Z * W - X * Y), sqw + sqx - sqy - sqz));
            }
        }
        public Vec3 EulerDeg { get
            {
                if (LengthSquared == 0) return default;

                const float SINGULARITY_THRESHOLD = 0.4999995f;
                const float HalfPI = (float)(System.Math.PI * 0.5);

                float sqx = X * X, sqy = Y * Y, sqz = Z * Z, sqw = W * W;
                float unit = sqx + sqy + sqz + sqw;
                float singularityTest = X * Z + Y * W;

                     if (singularityTest >  SINGULARITY_THRESHOLD * unit)
                    return new Vec3(0.0f,  HalfPI,  (float)(System.Math.Atan2(X, W) * 2.0));
                else if (singularityTest < -SINGULARITY_THRESHOLD * unit)
                    return new Vec3(0.0f, -HalfPI, -(float)(System.Math.Atan2(X, W) * 2.0));
                else
                    return new Vec3((float)System.Math.Atan2(2 * (X * W - Y * Z), sqw - sqx - sqy + sqz),
                                    (float)System.Math.Asin (2 * singularityTest / unit),
                                    (float)System.Math.Atan2(2 * (Z * W - X * Y), sqw + sqx - sqy - sqz))
                        * (float)(180.0 / System.Math.PI);
            }
        }

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

        public static Quat Slerp(Quat q1, Quat q2, float blend)
        {
            q1 = q1.Normalized;
            q2 = q2.Normalized;

            float dot = Dot(q1, q2);
            if (dot < 0.0f)
            {
                q2 = -q2;
                dot = -dot;
            }

            Quat result;
            const float DOT_THRESHOLD = 0.9995f;
            if (dot <= DOT_THRESHOLD)
            {
                float theta_0 = (float)System.Math.Acos(dot);
                float theta = theta_0 * blend;
                float sin_theta = (float)System.Math.Sin(theta);
                float sin_theta_0 = (float)System.Math.Sin(theta_0);

                float s0 = (float)System.Math.Cos(theta) - dot * sin_theta / sin_theta_0;
                float s1 = sin_theta / sin_theta_0;
                result = s0 * q1 + s1 * q2;
            }
            else
                result = (1.0f - blend) * q1 + blend * q2;
            return result.Normalized;
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
