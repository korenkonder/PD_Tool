using System.Runtime.InteropServices;

namespace KKdBaseLib
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Mat4
    {
        [FieldOffset(0x00)] public Vec4 Row0;
        [FieldOffset(0x10)] public Vec4 Row1;
        [FieldOffset(0x20)] public Vec4 Row2;
        [FieldOffset(0x30)] public Vec4 Row3;

        public Vec4 Column0 { get => new Vec4(Row0.X, Row1.X, Row2.X, Row3.X);
            set { Row0.X = value.X; Row1.X = value.Y; Row2.X = value.Z; Row3.X = value.W; } }
        public Vec4 Column1 { get => new Vec4(Row0.Y, Row1.X, Row2.Y, Row3.Y);
            set { Row0.Y = value.X; Row1.Y = value.Y; Row2.Y = value.Z; Row3.Y = value.W; } }
        public Vec4 Column2 { get => new Vec4(Row0.Z, Row1.Z, Row2.Z, Row3.Z);
            set { Row0.Z = value.X; Row1.Z = value.Y; Row2.Z = value.Z; Row3.Z = value.W; } }
        public Vec4 Column3 { get => new Vec4(Row0.W, Row1.W, Row2.W, Row3.W);
            set { Row0.W = value.X; Row1.W = value.Y; Row2.W = value.Z; Row3.W = value.W; } }

        public static Mat4 Identity => new Mat4(new Vec4(1.0f, 0.0f, 0.0f, 0.0f),
                                                new Vec4(0.0f, 1.0f, 0.0f, 0.0f),
                                                new Vec4(0.0f, 0.0f, 1.0f, 0.0f),
                                                new Vec4(0.0f, 0.0f, 0.0f, 1.0f));

        public Mat4(float m00, float m01, float m02, float m03, float m10, float m11, float m12, float m13,
                    float m20, float m21, float m22, float m23, float m30, float m31, float m32, float m33)
        { Row0 = new Vec4(m00, m01, m02, m03); Row1 = new Vec4(m10, m11, m12, m13);
          Row2 = new Vec4(m20, m21, m22, m23); Row3 = new Vec4(m30, m31, m32, m33); }

        public Mat4(Vec4 row0, Vec4 row1, Vec4 row2, Vec4 row3)
        { Row0 = row0; Row1 = row1; Row2 = row2; Row3 = row3; }

        public Mat4(Mat3 mat)
        { Row0 = new Vec4(mat.Row0); Row1 = new Vec4(mat.Row1);
          Row2 = new Vec4(mat.Row2); Row3 = new Vec4(0.0f, 0.0f, 0.0f, 1.0f); }

        public Mat4(Quat q)
        { Row0 = Row1 = Row2 = Row3 = default; FromQuat(q); }

        public Mat4(Quat q, Vec3 t)
        { Row0 = Row1 = Row2 = Row3 = default; FromQuat(q); Row3 = new Vec4(t, 1.0f); }

        public Mat4(Quat q, Vec4 t)
        { Row0 = Row1 = Row2 = Row3 = default; FromQuat(q); Row3 = t; }

        public Mat4(QuatTrans qt)
        { Row0 = Row1 = Row2 = Row3 = default; FromQuat(qt.Quat); Row3 = new Vec4(qt.Trans, 1.0f); }

        private void FromQuat(Quat q)
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

            Row0 = new Vec4(1.0f - (qz + m),    l -  qw * k ,   qx + qw * h , 0.0f);
            Row1 = new Vec4(   l +  qw * k , 1.0f - (qz + a),   qy - qw * g , 0.0f);
            Row2 = new Vec4(  qx -  qw * h ,   qy +  qw * g , 1.0f - (a + m), 0.0f);
            Row3 = new Vec4(0.0f           , 0.0f           , 0.0f          , 1.0f);
        }

        public Quat ToQuat()
        {
            Vec4 row0 = Row0;
            Vec4 row1 = Row1;
            Vec4 row2 = Row2;

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

        public Mat4 Invert() => -this;

        public Mat4 Translate(Vec3 vec)
        { Row3 = new Vec4(vec, 1.0f) * this; return this; }

        public Mat4 Translate(Vec4 vec)
        { Row3 = vec * this; return this; }

        public static Mat4 operator +(Mat4 left, Mat4 right)
        { left.Row0 += right.Row0; left.Row1 += right.Row1;
          left.Row2 += right.Row2; left.Row3 += right.Row3; return left; }

        public static Mat4 operator -(Mat4 left, Mat4 right)
        { left.Row0 -= right.Row0; left.Row1 -= right.Row1;
          left.Row2 -= right.Row2; left.Row3 -= right.Row3; return left; }

        public static Mat4 operator -(Mat4 mat)
        {
            int[]   colIdx = {  0,  0,  0,  0 };
            int[]   rowIdx = {  0,  0,  0,  0 };
            int[] pivotIdx = { -1, -1, -1, -1 };

            Mat4 result = mat;
            float[,] inverse =
            {
                { mat.Row0.X, mat.Row0.Y, mat.Row0.Z, mat.Row0.W },
                { mat.Row1.X, mat.Row1.Y, mat.Row1.Z, mat.Row1.W },
                { mat.Row2.X, mat.Row2.Y, mat.Row2.Z, mat.Row2.W },
                { mat.Row3.X, mat.Row3.Y, mat.Row3.Z, mat.Row3.W }
            };
            int icol = 0;
            int irow = 0;
            for (int i = 0; i < 4; i++)
            {
                float maxPivot = 0.0f;
                for (int j = 0; j < 4; j++)
                {
                    if (pivotIdx[j] == 0) continue;

                    for (int k = 0; k < 4; k++)
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
                    for (int k = 0; k < 4; k++)
                        (inverse[irow, k], inverse[icol, k]) = (inverse[icol, k], inverse[irow, k]);

                rowIdx[i] = irow;
                colIdx[i] = icol;

                float pivot = inverse[icol, icol];

                float oneOverPivot = 1.0f / pivot;
                inverse[icol, icol] = 1.0f;
                for (int k = 0; k < 4; k++)
                    inverse[icol, k] *= oneOverPivot;

                for (int j = 0; j < 4; j++)
                {
                    if (icol == j) continue;

                    float f = inverse[j, icol];
                    inverse[j, icol] = 0.0f;
                    for (int k = 0; k < 4; k++)
                        inverse[j, k] -= inverse[icol, k] * f;
                }
            }

            for (int j = 3; j >= 0; j--)
            {
                int ir = rowIdx[j];
                int ic = colIdx[j];
                for (int k = 0; k < 4; k++)
                {
                    float f = inverse[k, ir];
                    inverse[k, ir] = inverse[k, ic];
                    inverse[k, ic] = f;
                }
            }

            result.Row0.X = inverse[0, 0]; result.Row0.Y = inverse[0, 1];
            result.Row0.Z = inverse[0, 2]; result.Row0.W = inverse[0, 3];
            result.Row1.X = inverse[1, 0]; result.Row1.Y = inverse[1, 1];
            result.Row1.Z = inverse[1, 2]; result.Row1.W = inverse[1, 3];
            result.Row2.X = inverse[2, 0]; result.Row2.Y = inverse[2, 1];
            result.Row2.Z = inverse[2, 2]; result.Row2.W = inverse[2, 3];
            result.Row3.X = inverse[3, 0]; result.Row3.Y = inverse[3, 1];
            result.Row3.Z = inverse[3, 2]; result.Row3.W = inverse[3, 3];
            return result;
        }

        public static Mat4 operator *(Mat4 left, Mat4 right)
        {
            Mat4 result = default;
            result.Row0 = left.Row0.X * right.Row0 + left.Row0.Y * right.Row1
                + left.Row0.Z * right.Row2 + left.Row0.W * right.Row3;
            result.Row1 = left.Row1.X * right.Row0 + left.Row1.Y * right.Row1
                + left.Row1.Z * right.Row2 + left.Row1.W * right.Row3;
            result.Row2 = left.Row2.X * right.Row0 + left.Row2.Y * right.Row1
                + left.Row2.Z * right.Row2 + left.Row2.W * right.Row3;
            result.Row3 = left.Row3.X * right.Row0 + left.Row3.Y * right.Row1
                + left.Row3.Z * right.Row2 + left.Row3.W * right.Row3;
            return result;
        }

        public static Mat4 operator *(Mat4 mat, float scale) =>
            new Mat4(mat.Row0 * scale, mat.Row1 * scale, mat.Row2 * scale, mat.Row3 * scale);

        public static bool operator ==(Mat4 A, Mat4 B) =>  A.Equals(B);
        public static bool operator !=(Mat4 A, Mat4 B) => !A.Equals(B);

        public bool Equals(Mat4 other) =>
            Row0 == other.Row0 && Row1 == other.Row1 && Row2 == other.Row2 && Row3 == other.Row3;

        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();
        public override string ToString() => $"({Row0}; {Row1}; {Row2}; {Row3})";
        public string ToString(int d) => $"({Row0.ToString(d)}; {Row1.ToString(d)}; {Row2.ToString(d)}; {Row3.ToString(d)})";
    }
}
