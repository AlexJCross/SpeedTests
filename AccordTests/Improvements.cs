// Accord.NET Sample Applications
// http://accord-framework.net
//
// Copyright © 2009-2017, César Souza
// All rights reserved. 3-BSD License:
//
//   Redistribution and use in source and binary forms, with or without
//   modification, are permitted provided that the following conditions are met:
//
//      * Redistributions of source code must retain the above copyright
//        notice, this list of conditions and the following disclaimer.
//
//      * Redistributions in binary form must reproduce the above copyright
//        notice, this list of conditions and the following disclaimer in the
//        documentation and/or other materials provided with the distribution.
//
//      * Neither the name of the Accord.NET Framework authors nor the
//        names of its contributors may be used to endorse or promote products
//        derived from this software without specific prior written permission.
// 
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//  DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
//  DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//  LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//  ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 


using Accord.Math;
using System.Runtime.CompilerServices;

namespace SampleApp
{
    public static class Blah
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
        unsafe public static void MVCLoopUnrolling2(this double[,] matrix, double[] col, double[] res)
        {
            int cols = matrix.Columns();
            int rows = matrix.Rows();

            fixed (double* aBase = &matrix[0, 0])
            fixed (double* xBase = &col[0])
            fixed (double* r = &res[0])
            {
                double* a1 = aBase;
                double* a2 = aBase + cols;
                double* ypos = r;

                for (int i = 0; i < rows / 2; i++)
                {
                    double ytemp1 = 0;
                    double ytemp2 = 0;
                    double* x = xBase;

                    for (int j = 0; j < cols; j++)
                    {
                        ytemp1 += *x * (*a1++);
                        ytemp2 += *x * (*a2++);
                        x++;
                    }

                    *ypos = ytemp1;
                    ypos++;

                    *ypos = ytemp2;
                    ypos++;

                    // skip next row
                    a1 += cols;
                    a2 += cols;
                }
            }
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
        unsafe public static void VM7(this double[,] a, double[] x, double[] result)
        {
            int cols = a.Columns();
            int rows = a.Rows();

            fixed (double* A = &a[0, 0])
            fixed (double* X = &x[0])
            fixed (double* R = &result[0])
            {
                double* pa1 = A;
                double* pa2 = A + cols;

                double* px = X;
                double* pr = R;

                for (int j = 0; j < cols; j++)
                    *pr++ = 0;

                for (int i = 0; i < rows / 2; i++)
                {
                    pr = R;
                    double x1 = *px++;
                    double x2 = *px++;
                    for (int j = 0; j < cols; j++)
                    {
                        *pr += x1 * (*pa1++);
                        *pr += x2 * (*pa2++);
                        pr++;
                    }

                    // skip next row
                    pa1 = pa2;
                    pa2 += cols;
                }

                for (int i = 0; i < rows % 2; i++)
                {
                    pr = R;
                    double xval = *px++;
                    for (int j = 0; j < cols; j++)
                        *pr++ += xval * (*pa1++);
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
                double* a1 = aBase + 0*cols;
                double* a2 = aBase + 1*cols;
                double* a3 = aBase + 2*cols;
                double* a4 = aBase + 3*cols;

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
    }
}