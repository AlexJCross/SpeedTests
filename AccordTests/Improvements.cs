namespace SampleApp
{
    using Accord;
    using Accord.Math;
    using System;
    using System.Runtime.CompilerServices;

    public static class Improvements
    {
        #region DotAndDot

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double DotAndDotFast(this double[] rowVector, double[,] matrix, double[] columnVector)
        {
            double s2 = 0;

            for (int i = 0; i < rowVector.Length; i++)
            {
                double s = 0;
                for (int j = 0; j < columnVector.Length; j++)
                    s += matrix[i, j] * columnVector[j];

                s2 += rowVector[i] * s;
            }
            return s2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static double DotAndDotFaster(this double[] rowVector, double[,] matrix, double[] columnVector)
        {
            int cols = matrix.Columns();
            int rows = matrix.Rows();

            double result = 0;

            fixed (double* r = rowVector)
            fixed (double* a = matrix)
            fixed (double* c = columnVector)
            {
                double* pa1 = a;
                double* pa2 = a + cols;
                double* pr = r;

                // Process rows two at a time
                for (int i = 0; i < rows / 2; i++)
                {
                    double sum1 = 0, sum2 = 0;
                    double* pc = c;

                    for (int j = 0; j < cols; j++)
                    {
                        sum1 += (*pa1++) * (*pc);
                        sum2 += (*pa2++) * (*pc);
                        pc++;
                    }

                    result += (*pr++) * sum1;
                    result += (*pr++) * sum2;

                    // Now we skip a row
                    pa1 = pa2;
                    pa2 += cols;
                }

                // Process the remainder
                for (int i = 0; i < rows % 2; i++)
                {
                    double sum = 0;
                    double* pc = c;

                    for (int j = 0; j < cols; j++)
                        sum += (*pa1++) * (*pc++);

                    result += (*pr++) * sum;
                }
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double DotAndDotFastRows(this double[] rowVector, double[,] matrix, double[] columnVector)
        {
            double s2 = 0;

            for (int j = 0; j < columnVector.Length; j++)
            {
                double s = 0;
                for (int i = 0; i < rowVector.Length; i++)
                    s += rowVector[i] * matrix[i, j];

                s2 += columnVector[j] * s;
            }
            return s2;
        }

        #endregion

        #region Matrix-Vector

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MVNoCache(this double[,] matrix, double[] col, double[] res)
        {
            int cols = col.Length;
            int rows = matrix.Rows();

            for (int i = 0; i < rows; i++)
            {
                res[i] = 0;
                for (int j = 0; j < cols; j++)
                    res[i] += matrix[i, j] * col[j];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MV(this double[,] matrix, double[] col, double[] res)
        {
            int cols = col.Length;
            int rows = matrix.Rows();

            for (int i = 0; i < rows; i++)
            {
                double ytemp = 0;

                for (int j = 0; j < cols; j++)
                {
                    ytemp += matrix[i, j] * col[j];
                }

                res[i] = ytemp;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public static void MVC(this double[,] matrix, double[] col, double[] res)
        {
            int cols = col.Length;
            int rows = matrix.Rows();

            fixed (double* aBase = &matrix[0, 0])
            fixed (double* xBase = &col[0])
            {
                double* a = aBase;

                for (int i = 0; i < rows; i++)
                {
                    double ytemp = 0;
                    double* x = xBase;

                    for (int j = 0; j < cols; j++)
                        ytemp += (*a++) * (*x++);

                    res[i] = ytemp;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public static double[] MVCLoopUnrolling2(this double[,] matrix, double[] columnVector, double[] result)
        {
            int cols = matrix.Columns();
            int rows = matrix.Rows();

            fixed (double* a = matrix)
            fixed (double* x = columnVector)
            fixed (double* r = result)
            {
                double* pa1 = a;
                double* pa2 = a + cols;
                double* pr = r;

                // Process rows two at a time
                for (int i = 0; i < rows / 2; i++)
                {
                    double sum1 = 0, sum2 = 0;
                    double* px = x;

                    for (int j = 0; j < cols; j++)
                    {
                        sum1 += (double)((double)(*pa1++) * (double)(*px));
                        sum2 += (double)((double)(*pa2++) * (double)(*px));
                        px++;
                    }

                    *pr++ = (double)sum1;
                    *pr++ = (double)sum2;

                    // Now we skip a row
                    pa1 = pa2;
                    pa2 += cols;
                }

                // Process the remainder
                for (int i = 0; i < rows % 2; i++)
                {
                    double sum = 0;
                    double* px = x;

                    for (int j = 0; j < cols; j++)
                        sum += (double)((double)(*pa1++) * (double)(*px++));

                    *pr = (double)sum;
                }
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public static void MVCLoopUnrolling4(this double[,] matrix, double[] col, double[] res)
        {
            int cols = col.Length;
            int rows = matrix.Rows();

            fixed (double* aBase = &matrix[0, 0])
            fixed (double* xBase = &col[0])
            fixed (double* r = &res[0])
            {
                double* a1 = aBase + 0 * cols;
                double* a2 = aBase + 1 * cols;
                double* a3 = aBase + 2 * cols;
                double* a4 = aBase + 3 * cols;
                double* ypos = r;

                for (int i = 0; i < rows / 4; i++)
                {
                    double ytemp1 = 0;
                    double ytemp2 = 0;
                    double ytemp3 = 0;
                    double ytemp4 = 0;
                    double* x = xBase;

                    for (int j = 0; j < cols; j++)
                    {
                        ytemp1 += *x * (*a1++);
                        ytemp2 += *x * (*a2++);
                        ytemp3 += *x * (*a3++);
                        ytemp4 += *x * (*a4++);
                        x++;
                    }

                    *ypos++ = ytemp1;
                    *ypos++ = ytemp2;
                    *ypos++ = ytemp3;
                    *ypos++ = ytemp4;

                    // skip next row
                    a1 += cols;
                    a2 += cols;
                    a3 += cols;
                    a4 += cols;
                }
            }
        }

        #endregion

        #region Vector-Matrix

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void VM1(this double[,] matrix, double[] row, double[] res)
        {
            int cols = matrix.Columns();
            int rows = matrix.Rows();

            for (int j = 0; j < cols; j++)
            {
                res[j] = 0;

                for (int i = 0; i < rows; i++)
                    res[j] += row[i] * matrix[i, j];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void VM2(this double[,] matrix, double[] row, double[] res)
        {
            int cols = matrix.Columns();
            int rows = matrix.Rows();

            for (int j = 0; j < cols; j++)
            {
                double y = 0;

                for (int i = 0; i < rows; i++)
                    y += row[i] * matrix[i, j];

                res[j] = y;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public static void VM3(this double[,] matrix, double[] row, double[] res)
        {
            int cols = matrix.Columns();
            int rows = matrix.Rows();

            fixed (double* aBase = &matrix[0, 0])
            fixed (double* xBase = &row[0])
            fixed (double* rBase = &res[0])
            {
                double* r = rBase;

                for (int j = 0; j < cols; j++)
                {
                    double* a = aBase + j;
                    double* x = xBase;
                    double y = 0;

                    for (int i = 0; i < rows; i++)
                    {
                        y += (*x++) * (*a);
                        a += cols;
                    }

                    *r++ = y;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void VM4(this double[,] matrix, double[] row, double[] res)
        {
            int cols = matrix.Columns();
            int rows = matrix.Rows();

            for (int j = 0; j < cols; j++)
                res[j] = 0;

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    res[j] += row[i] * matrix[i, j];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void VM5(this double[,] matrix, double[] row, double[] res)
        {
            int cols = matrix.Columns();
            int rows = matrix.Rows();

            for (int j = 0; j < cols; j++)
                res[j] = 0;

            for (int i = 0; i < rows; i++)
            {
                double x = row[i];

                for (int j = 0; j < cols; j++)
                    res[j] += x * matrix[i, j];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public static void VM6(this double[,] matrix, double[] row, double[] res)
        {
            int cols = matrix.Columns();
            int rows = matrix.Rows();

            fixed (double* A = &matrix[0, 0])
            fixed (double* X = &row[0])
            fixed (double* R = &res[0])
            {
                double* pa = A;
                double* px = X;
                double* pr = R;

                for (int j = 0; j < cols; j++)
                    *pr++ = 0;

                for (int i = 0; i < rows; i++)
                {
                    pr = R;
                    double xval = *px++;
                    for (int j = 0; j < cols; j++)
                        *pr++ += xval * (*pa++);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void VM7(this double[,] matrix, double[] rowVector, double[] result)
        {
            int cols = matrix.Columns();
            int rows = matrix.Rows();

            fixed (double* a = matrix)
            fixed (double* x = rowVector)
            fixed (double* r = result)
            {
                double* pa1 = a;
                double* pa2 = a + cols;

                double* px = x;
                double* pr = r;

                for (int j = 0; j < cols; j++)
                    *pr++ = 0;

                // Process rows two at a time
                for (int i = 0; i < rows / 2; i++)
                {
                    pr = r;
                    double x1 = *px++;
                    double x2 = *px++;
                    for (int j = 0; j < cols; j++)
                    {
                        *pr += x1 * (*pa1++);
                        *pr += x2 * (*pa2++);
                        pr++;
                    }

                    // Now we skip a row
                    pa1 = pa2;
                    pa2 += cols;
                }

                // Process the remainder
                for (int i = 0; i < rows % 2; i++)
                {
                    pr = r;
                    double x1 = *px++;
                    for (int j = 0; j < cols; j++)
                        *pr++ += x1 * (*pa1++);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public static void VM8(this double[,] matrix, double[] row, double[] res)
        {
            int cols = matrix.Columns();
            int rows = matrix.Rows();

            fixed (double* aBase = &matrix[0, 0])
            fixed (double* xBase = &row[0])
            fixed (double* rBase = &res[0])
            {
                double* a1 = aBase + 0 * cols;
                double* a2 = aBase + 1 * cols;
                double* a3 = aBase + 2 * cols;
                double* a4 = aBase + 3 * cols;

                double* x = xBase;
                double* r = rBase;

                for (int j = 0; j < cols; j++)
                    *r++ = 0;

                for (int i = 0; i < rows / 4; i++)
                {
                    r = rBase;
                    double x1 = *x++;
                    double x2 = *x++;
                    double x3 = *x++;
                    double x4 = *x++;

                    for (int j = 0; j < cols; j++)
                    {
                        *r++ += x1 * (*a1++)
                              + x2 * (*a2++)
                              + x3 * (*a3++)
                              + x4 * (*a4++);
                    }

                    // skip next row
                    a1 += 3 * cols;
                    a2 += 3 * cols;
                    a3 += 3 * cols;
                    a4 += 3 * cols;
                }

                for (int i = 0; i < rows % 4; i++)
                {
                    r = rBase;
                    double xval = *x++;
                    for (int j = 0; j < cols; j++)
                        *r++ += xval * (*a1++);
                }
            }
        }

        #endregion

        #region Outer

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static double[,] OuterNew(this double[] a, double[] b, double[,] result)
        {
            fixed (double* A = a)
            fixed (double* B = b)
            fixed (double* R = result)
            {
                double* pa = A;
                double* pr = R;

                for (int i = 0; i < a.Length; i++)
                {
                    double x = *pa++;
                    double* pb = B;

                    for (int j = 0; j < b.Length; j++)
                        *pr++ = x * (*pb++);
                }
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static double[,] OuterNew2(this double[] a, double[] b, double[,] result)
        {
            fixed (double* R = result)
            {
                double* pr = R;
                for (int i = 0; i < a.Length; i++)
                {
                    double x = a[i];
                    for (int j = 0; j < b.Length; j++)
                        *pr++ = x * b[j];
                }
            }
            return result;
        }

        #endregion

        #region Kronecker

        public static unsafe double[][] Kronecker(this double[][] a, double[,] b, double[][] result)
        {
            int arows = a.Rows();
            int acols = a.Columns();
            int brows = b.Rows();
            int bcols = b.Columns();

            fixed (double* B = b)
                for (int i = 0; i < arows; i++)
                    for (int j = 0; j < acols; j++)
                    {
                        double aval = (double)a[i][j];
                        double* pb = B;

                        for (int k = 0; k < brows; k++)
                            for (int l = 0; l < bcols; l++)
                                result[i * brows + k][j * bcols + l] = (double)(aval * (*pb++));
                    }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static double[] KronNew2(this double[] a, double[] b, double[] result)
        {
            fixed (double* R = result)
            {
                double* pr = R;
                for (int i = 0; i < a.Length; i++)
                {
                    double x = a[i];
                    for (int j = 0; j < b.Length; j++)
                        *pr++ = (double)((double)x * (double)b[j]);
                }
            }
            return result;
        }

        #endregion

        public static double[][] DotWithTransposedNew(this double[][] a, double[][] b, double[][] result)
        {
            int n = a.Columns();
            int m = a.Rows();
            int p = b.Rows();

            for (int i = 0; i < m; i++)
            {
                double[] arow = a[i];
                for (int j = 0; j < p; j++)
                {
                    double sum = 0;
                    double[] brow = b[j];
                    for (int k = 0; k < arow.Length; k++)
                        sum += arow[k] * brow[k];
                    result[i][j] = sum;
                }
            }

            return result;
        }


        public static double[][] DotWithTransposedNew1(this double[][] a, double[][] b, double[][] result)
        {
            for (int i = 0; i < a.Length; i++)
            {
                double[] arow = a[i];
                for (int j = 0; j < b.Length; j++)
                {
                    double sum = 0;
                    double[] brow = b[j];
                    for (int k = 0; k < arow.Length; k++)
                        sum += arow[k] * brow[k];
                    result[i][j] = sum;
                }
            }

            return result;
        }


        public static unsafe double[][] DotWithTransposedNew1(this double[][] a, double[,] b, double[][] result)
        {
            int n = b.Rows();

            fixed (double* B = b)
            for (int i = 0; i < a.Length; i++)
            {
                double* pb = B;
                double[] arow = a[i];
                for (int j = 0; j < n; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < arow.Length; k++)
                        sum += arow[k] * (*pb++);
                    result[i][j] = sum;
                }
            }

            return result;
        }


        public static unsafe double[][] DotWithTransposedNew1(this double[,] a, double[][] b, double[][] result)
        {
            int n = a.Rows();

            fixed (double* A = a) 
            for (int j = 0; j < b.Length; j++)
            {
                double* pa = A;
                for (int i = 0; i < n; i++)
                {
                    double sum = 0;
                    double[] brow = b[j];
                    for (int k = 0; k < brow.Length; k++)
                        sum += (*pa++) * brow[k];
                    result[i][j] = sum;
                }
            }

            return result;
        }




        public static unsafe double[][] DotWithTransposedNew2(this double[][] a, double[][] b, double[][] result)
        {
            int n = a.Columns();
            int m = a.Rows();
            int p = b.Rows();

            for (int i = 0; i < m; i++)
            {
                fixed (double* AR = a[i])
                    for (int j = 0; j < p; j++)
                    {
                        double sum = 0;
                        double* pa = AR;
                        double[] brow = b[j];

                        for (int k = 0; k < brow.Length; k++)
                            sum += (*pa++) * brow[k];

                        result[i][j] = sum;
                    }
            }

            return result;
        }

#if NET45 || NET46 || NET462 || NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static unsafe double[][] DotNew(this double[,] a, double[][] b, double[][] result)
        {
            int N = result.Length;
            int K = a.Columns();
            int M = result.Columns();

            var t = new double[K];

            fixed (double* A = a)
                for (int j = 0; j < M; j++)
                {
                    for (int k = 0; k < t.Length; k++)
                        t[k] = b[k][j];

                    double* pa = A;
                    for (int i = 0; i < N; i++)
                    {
                        double s = (double)0;
                        for (int k = 0; k < t.Length; k++)
                            s += (double)((double)(*pa++) * (double)t[k]);
                        result[i][j] = (double)s;
                    }
                }

            return result;
        }


        public static double[] DotNew(this double[] rowVector, double[][] matrix, double[] result)
        {
            for (int j = 0; j < result.Length; j++)
            {
                double s = 0;
                for (int k = 0; k < rowVector.Length; k++)
                    s += rowVector[k] * matrix[k][j];
                result[j] = s;
            }

            return result;
        }

        #region Transpose and Dot

        public static unsafe double[,] TransposeAndDotNew(this double[,] a, double[,] b, double[,] result)
        {
            int n = a.Rows();
            int m = a.Columns();
            int p = b.Columns();

#if DEBUG
            if (n != b.Rows() || result.Rows() > m || result.Columns() > p)
                throw new DimensionMismatchException();
            var C = a.Transpose().To<double[,]>().Dot(b.To<double[,]>());
#endif

            fixed (double* R = result)
            fixed (double* B = b)
            fixed (double* ptemp = new double[p])
            {
                double* pr = R;

                for (int i = 0; i < m; i++)
                {
                    double* pt = ptemp;
                    double* pb = B;

                    for (int k = 0; k < n; k++)
                    {
                        double aval = a[k, i];
                        for (int j = 0; j < p; j++)
                            *pt++ += aval * (*pb++);
                        pt = ptemp;
                    }

                    // Update the results row and clear the cache
                    for (int j = 0; j < p; j++)
                    {
                        *pr++ = *pt;
                        *pt++ = 0;
                    }
                }
            }

#if DEBUG
            if (!C.IsEqual(result.To<double[,]>(), 1e-4))
                throw new Exception();
#endif

            return result;
        }

        #endregion

        public static unsafe double[][] Dot(this long[,] a, double[][] b, double[][] result)
        {
            int N = result.Length;
            int K = a.Columns();
            int M = result.Columns();

            var t = new double[K];
            fixed(long* A = a)
            for (int j = 0; j < M; j++)
            {
                for (int k = 0; k < t.Length; k++)
                    t[k] = b[k][j];

                long* pa = A;
                for (int i = 0; i < N; i++)
                {
                    double s = (double)0;
                    for (int k = 0; k < t.Length; k++)
                        s += (double)((double)(*pa++) * (double)t[k]);
                    result[i][j] = (double)s;
                }
            }
            return result;
        }
    }
}