using System.Runtime.InteropServices;

namespace KKdBaseLib
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Mat2
    {
        [FieldOffset(0x00)] public Vec2 Row0;
        [FieldOffset(0x08)] public Vec2 Row1;

        public Vec2 Column0 { get => new Vec2(Row0.X, Row1.X);
            set { Row0.X = value.X; Row1.X = value.Y; } }
        public Vec2 Column1 { get => new Vec2(Row0.Y, Row1.Y);
            set { Row0.Y = value.X; Row1.Y = value.Y; } }

        public static Mat2 Identity => new Mat2(new Vec2(1.0f, 0.0f),
                                                new Vec2(0.0f, 1.0f));

        public Mat2(Vec2 row0, Vec2 row1)
        { Row0 = row0; Row1 = row1; }

        public Mat2 Invert() => -this;

        public static Mat2 operator +(Mat2 left, Mat2 right)
        { left.Row0 += right.Row0; left.Row1 += right.Row1; return left; }

        public static Mat2 operator -(Mat2 left, Mat2 right)
        { left.Row0 -= right.Row0; left.Row1 -= right.Row1; return left; }

        public static Mat2 operator -(Mat2 mat)
        {
            int[]   colIdx = {  0,  0 };
            int[]   rowIdx = {  0,  0 };
            int[] pivotIdx = { -1, -1 };

            Mat2 result = mat;
            float[,] inverse =
            {
                { mat.Row0.X, mat.Row0.Y },
                { mat.Row1.X, mat.Row1.Y }
            };
            int icol = 0;
            int irow = 0;
            for (int i = 0; i < 2; i++)
            {
                float maxPivot = 0.0f;
                for (int j = 0; j < 2; j++)
                {
                    if (pivotIdx[j] == 0) continue;

                    for (int k = 0; k < 2; k++)
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
                    for (int k = 0; k < 2; k++)
                    { float t = inverse[irow, k]; inverse[irow, k] = inverse[icol, k]; inverse[icol, k] = t; }

                rowIdx[i] = irow;
                colIdx[i] = icol;

                float pivot = inverse[icol, icol];

                float oneOverPivot = 1.0f / pivot;
                inverse[icol, icol] = 1.0f;
                for (int k = 0; k < 2; k++)
                    inverse[icol, k] *= oneOverPivot;

                for (int j = 0; j < 2; j++)
                {
                    if (icol == j) continue;

                    float f = inverse[j, icol];
                    inverse[j, icol] = 0.0f;
                    for (int k = 0; k < 3; k++)
                        inverse[j, k] -= inverse[icol, k] * f;
                }
            }

            for (int j = 1; j >= 0; j--)
            {
                int ir = rowIdx[j];
                int ic = colIdx[j];
                for (int k = 0; k < 2; k++)
                { float t = inverse[k, ic]; inverse[k, ic] = inverse[k, ir]; inverse[k, ir] = t; }
            }

            result.Row0.X = inverse[0, 0]; result.Row0.Y = inverse[0, 1];
            result.Row1.X = inverse[1, 0]; result.Row1.Y = inverse[1, 1];
            return result;
        }

        public static Mat2 operator *(Mat2 left, Mat2 right)
        {
            Mat2 result = default;
            result.Row0 = left.Row0.X * right.Row0 + left.Row0.Y * right.Row1;
            result.Row1 = left.Row1.X * right.Row0 + left.Row1.Y * right.Row1;
            return result;
        }

        public static Mat2 operator *(Mat2 mat, float scale) =>
            new Mat2(mat.Row0 * scale, mat.Row1 * scale);

        public static bool operator ==(Mat2 A, Mat2 B) =>  A.Equals(B);
        public static bool operator !=(Mat2 A, Mat2 B) => !A.Equals(B);

        public bool Equals(Mat2 other) =>
            Row0 == other.Row0 && Row1 == other.Row1;

        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();
        public override string ToString() => $"({Row0}; {Row1})";
        public string ToString(int d) => $"({Row0.ToString(d)}; {Row1.ToString(d)})";
    }
}
