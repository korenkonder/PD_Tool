using System.Runtime.InteropServices;

namespace KKdBaseLib
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Mat3
    {
        [FieldOffset(0x00)] public Vec3 Row0;
        [FieldOffset(0x0C)] public Vec3 Row1;
        [FieldOffset(0x18)] public Vec3 Row2;

        public Vec3 Column0 { get => new Vec3(Row0.X, Row1.X, Row2.X);
            set { Row0.X = value.X; Row1.X = value.Y; Row2.X = value.Z; } }
        public Vec3 Column1 { get => new Vec3(Row0.Y, Row1.X, Row2.Y);
            set { Row0.Y = value.X; Row1.Y = value.Y; Row2.Y = value.Z; } }
        public Vec3 Column2 { get => new Vec3(Row0.Z, Row1.Z, Row2.Z);
            set { Row0.Z = value.X; Row1.Z = value.Y; Row2.Z = value.Z;} }

        public static Mat3 Identity => new Mat3(new Vec3(1.0f, 0.0f, 0.0f),
                                                new Vec3(0.0f, 1.0f, 0.0f),
                                                new Vec3(0.0f, 0.0f, 1.0f));

        public Mat3(float m00, float m01, float m02, float m10, float m11, float m12, float m20, float m21, float m22)
        { Row0 = new Vec3(m00, m01, m02); Row1 = new Vec3(m10, m11, m12); Row2 = new Vec3(m20, m21, m22);}

        public Mat3(Vec3 row0, Vec3 row1, Vec3 row2)
        { Row0 = row0; Row1 = row1; Row2 = row2; }

        public Mat3(Mat4 mat)
        { Row0 = new Vec3(mat.Row0); Row1 = new Vec3(mat.Row1); Row2 = new Vec3(mat.Row2); }

        public Mat3(Quat q)
        {
            float qx = q.X;
            float qy = q.Y;
            float qz = q.Z;
            float qw = q.W;
            float g = qx + qx;
            float h = qy + qy;
            float k = qz + qz;
            float a = qx * g;
            float l = qx * h;
            float m = qy * h;
            qx *= k;
            qy *= k;
            qz *= k;

            Row0 = new Vec3(1.0f - qz - m,    l - qw * k,   qx + qw * h);
            Row1 = new Vec3(   l + qw * k, 1.0f - qz - a,   qy - qw * g);
            Row2 = new Vec3(  qx - qw * h,   qy + qw * g, 1.0f -  m - a);
        }

        public Quat ToQuat()
        {
            Vec3 row0 = Row0;
            Vec3 row1 = Row1;
            Vec3 row2 = Row2;

            Quat q = default;
            var trace = 0.25 * (row0.X + row1.Y + row2.Z+ 1.0);

            if (trace > 0)
            {
                double sq = System.Math.Sqrt(trace);

                q.W = (float)sq;
                sq = 1.0 / (4.0 * sq);
                q.X = (float)((row1.Z - row2.Y) * sq);
                q.Y = (float)((row2.X - row0.Z) * sq);
                q.Z = (float)((row0.Y - row1.X) * sq);
            }
            else if (row0.X > row1.Y && row0.X > row2.Z)
            {
                double sq = 2.0 * System.Math.Sqrt(1.0 + row0.X - row1.Y - row2.Z);

                q.X = (float)(0.25 * sq);
                sq = 1.0 / sq;
                q.W = (float)((row2.Y - row1.Z) * sq);
                q.Y = (float)((row1.X + row0.Y) * sq);
                q.Z = (float)((row2.X + row0.Z) * sq);
            }
            else if (row1.Y > row2.Z)
            {
                double sq = 2.0 * System.Math.Sqrt(1.0 + row1.Y - row0.X - row2.Z);

                q.Y = (float)(0.25 * sq);
                sq = 1.0 / sq;
                q.W = (float)((row2.X - row0.Z) * sq);
                q.X = (float)((row1.X + row0.Y) * sq);
                q.Z = (float)((row2.Y + row1.Z) * sq);
            }
            else
            {
                double sq = 2.0 * System.Math.Sqrt(1.0 + row2.Z - row0.X - row1.Y);

                q.Z = (float)(0.25 * sq);
                sq = 1.0 / sq;
                q.W = (float)((row1.X - row0.Y) * sq);
                q.X = (float)((row2.X + row0.Z) * sq);
                q.Y = (float)((row2.Y + row1.Z) * sq);
            }

            return q.Normalized;
        }

        public Mat3 Invert() => -this;

        public static Mat3 operator +(Mat3 left, Mat3 right)
        { left.Row0 += right.Row0; left.Row1 += right.Row1; left.Row2 += right.Row2; return left; }

        public static Mat3 operator -(Mat3 left, Mat3 right)
        { left.Row0 -= right.Row0; left.Row1 -= right.Row1; left.Row2 -= right.Row2; return left; }

        public static Mat3 operator -(Mat3 mat)
        {
            int[]   colIdx = {  0,  0,  0 };
            int[]   rowIdx = {  0,  0,  0 };
            int[] pivotIdx = { -1, -1, -1 };

            Mat3 result = mat;
            float[,] inverse =
            {
                { mat.Row0.X, mat.Row0.Y, mat.Row0.Z },
                { mat.Row1.X, mat.Row1.Y, mat.Row1.Z },
                { mat.Row2.X, mat.Row2.Y, mat.Row2.Z }
            };
            int icol = 0;
            int irow = 0;
            for (int i = 0; i < 3; i++)
            {
                float maxPivot = 0.0f;
                for (int j = 0; j < 3; j++)
                {
                    if (pivotIdx[j] == 0) continue;

                    for (int k = 0; k < 3; k++)
                    {
                        if (pivotIdx[k] == -1)
                        {
                            float absVal = System.Math.Abs(inverse[j, k]);
                            if (absVal > maxPivot)
                            {
                                maxPivot = absVal;
                                irow = j;
                                icol = k;
                            }
                        }
                        else if (pivotIdx[k] > 0)
                            return result;
                    }
                }

                pivotIdx[icol]++;

                if (irow != icol)
                    for (int k = 0; k < 3; k++)
                    { float t = inverse[irow, k]; inverse[irow, k] = inverse[icol, k]; inverse[icol, k] = t; }

                rowIdx[i] = irow;
                colIdx[i] = icol;

                float pivot = inverse[icol, icol];

                float oneOverPivot = 1.0f / pivot;
                inverse[icol, icol] = 1.0f;
                for (int k = 0; k < 3; k++)
                    inverse[icol, k] *= oneOverPivot;

                for (int j = 0; j < 3; j++)
                {
                    if (icol == j) continue;

                    float f = inverse[j, icol];
                    inverse[j, icol] = 0.0f;
                    for (int k = 0; k < 3; k++)
                        inverse[j, k] -= inverse[icol, k] * f;
                }
            }

            for (int j = 2; j >= 0; j--)
            {
                int ir = rowIdx[j];
                int ic = colIdx[j];
                for (int k = 0; k < 3; k++)
                { float t = inverse[k, ic]; inverse[k, ic] = inverse[k, ir]; inverse[k, ir] = t; }
            }

            result.Row0.X = inverse[0, 0]; result.Row0.Y = inverse[0, 1]; result.Row0.Z = inverse[0, 2];
            result.Row1.X = inverse[1, 0]; result.Row1.Y = inverse[1, 1]; result.Row1.Z = inverse[1, 2];
            result.Row2.X = inverse[2, 0]; result.Row2.Y = inverse[2, 1]; result.Row2.Z = inverse[2, 2];
            return result;
        }

        public static Mat3 operator *(Mat3 left, Mat3 right)
        {
            Mat3 result = default;
            result.Row0 = left.Row0.X * right.Row0 + left.Row0.Y * right.Row1 + left.Row0.Z * right.Row2;
            result.Row1 = left.Row1.X * right.Row0 + left.Row1.Y * right.Row1 + left.Row1.Z * right.Row2;
            result.Row2 = left.Row2.X * right.Row0 + left.Row2.Y * right.Row1 + left.Row2.Z * right.Row2;
            return result;
        }

        public static Mat3 operator *(Mat3 mat, float scale) =>
            new Mat3(mat.Row0 * scale, mat.Row1 * scale, mat.Row2 * scale);

        public static bool operator ==(Mat3 A, Mat3 B) =>  A.Equals(B);
        public static bool operator !=(Mat3 A, Mat3 B) => !A.Equals(B);

        public bool Equals(Mat3 other) =>
            Row0 == other.Row0 && Row1 == other.Row1 && Row2 == other.Row2;

        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();
        public override string ToString() => $"({Row0}; {Row1}; {Row2})";
        public string ToString(int d) => $"({Row0.ToString(d)}; {Row1.ToString(d)}; {Row2.ToString(d)})";
    }
}
