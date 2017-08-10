using System;

using Accord.Math;
using System.Diagnostics;
using System.Text;

namespace SampleApp
{
    public class Program
    {


        static void Main(string[] args)
        {
#if !DEBUG
            int Size = 8, N = 1 << 22;
            int Size2 = Size;
            int Size3 = Size;
#else
            int Size = 7, N = 50;
            int Size2 = Size + 1;
            int Size3 = Size + 2;
#endif

            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();

            string headers = "|Size|Trials|Accord (ms)|Proposed (ms)|Multiplier|";
            string banner = @"|----------|:-------------:|------:|------:|------:|";
            sb1.AppendLine(headers);
            sb1.AppendLine(banner);
            sb2.AppendLine(headers);
            sb2.AppendLine(banner);

            for (int p = 0; p < 10; p++)
            {

                var NbyM = Matrix.Random(Size, Size2);
                var MbyP = Matrix.Random(Size2, Size3);
                var NbyP = Matrix.Random(Size, Size3);
                var MbyN = NbyM.Transpose();

                double[] nVec = Vector.Random(Size);
                double[] mVec = Vector.Random(Size2);

                {
                    #region Jit the code

                    // JIT the code
                    for (int i = 0; i < 2; i++)
                    {
                        var check = MbyN.TransposeAndDot(MbyP);
                        MbyN.TransposeAndDotNew(MbyP, NbyP);

                        if (!check.IsEqual(NbyP, 1e-10))
                        {
                            throw new Exception();
                        }
                    }

                    #endregion


                    Log($"Transpose and Dot Matrix-Matrix (N={Size})");

                    long time;
                    var sw = Stopwatch.StartNew();

                    // First let's see how fast Accord is
                    sw.Restart();
                    for (int i = 0; i < N; i++)
                        MbyN.TransposeAndDot(MbyP, NbyP);

                    long timeA = sw.ElapsedMilliseconds;
                    Console.WriteLine($"*** Accord *** {timeA:n0}ms");
                    Console.WriteLine();


                    // Now let's see if we can speed it up :)
                    sw.Restart();
                    for (int i = 0; i < N; i++)
                        MbyN.TransposeAndDotNew(MbyP, NbyP);

                    time = sw.ElapsedMilliseconds;
                    Console.WriteLine($"Time taken pointer {time:n0}ms");
                }

                {
                    var NbyM2 = NbyM.ToJagged();

                    #region Jit the code

                    // JIT the code
                    for (int i = 0; i < 10; i++)
                    {
                        // Accord
                        nVec.Dot(NbyM2, mVec);
                        nVec.DotNew(NbyM2, mVec);
                    }

                    #endregion


                    Log($"Jagged row-Matrix (N={Size})");

                    long time;
                    var sw = Stopwatch.StartNew();

                    // First let's see how fast Accord is
                    sw.Restart();
                    for (int i = 0; i < N; i++)
                        nVec.Dot(NbyM2, mVec);

                    long timeA = sw.ElapsedMilliseconds;
                    Console.WriteLine($"*** Accord *** {timeA:n0}ms");
                    Console.WriteLine();


                    // Now let's see if we can speed it up :)
                    sw.Restart();
                    for (int i = 0; i < N; i++)
                        nVec.DotNew(NbyM2, mVec);

                    time = sw.ElapsedMilliseconds;
                    Console.WriteLine($"Time taken pointer {time:n0}ms");
                }

                {
                    #region Jit the code

                    // JIT the code
                    for (int i = 0; i < 10; i++)
                    {
                        // Accord
                        var res1 = nVec.Outer(mVec, NbyM);
                        var res2 = nVec.OuterNew(mVec, NbyM);

                        if (!res1.IsEqual(res2, 1e-10))
                        {
                            throw new Exception();
                        }
                    }

                    #endregion

                    Log($"Outer (N={Size})");

                    long time;
                    var sw = Stopwatch.StartNew();

                    // First let's see how fast Accord is
                    sw.Restart();
                    for (int i = 0; i < N; i++)
                        nVec.Outer(mVec, NbyM);

                    long timeA = sw.ElapsedMilliseconds;
                    Console.WriteLine($"*** Accord *** {timeA:n0}ms");
                    Console.WriteLine();


                    // Now let's see if we can speed it up :)
                    sw.Restart();
                    for (int i = 0; i < N; i++)
                        nVec.OuterNew(mVec, NbyM);

                    time = sw.ElapsedMilliseconds;
                    Console.WriteLine($"Time taken pointer {time:n0}ms");

                    sw.Restart();
                    for (int i = 0; i < N; i++)
                        nVec.OuterNew2(mVec, NbyM);

                    time = sw.ElapsedMilliseconds;
                    Console.WriteLine($"Time taken less pointers {time:n0}ms");
                }

                {
                    #region Jit the code

                    // JIT the code
                    for (int i = 0; i < 10; i++)
                    {
                        // Accord
                        var res1 = nVec.DotAndDot(NbyM, mVec);

                        // Variations to test speed improvement
                        var res2 = nVec.DotAndDotFast(NbyM, mVec);
                        var res3 = nVec.DotAndDotFaster(NbyM, mVec);
                    }

                    #endregion

                    Log($"Vector-Matrix-Vector (N={Size})");

                    long time;
                    var sw = Stopwatch.StartNew();

                    // First let's see how fast Accord is
                    sw.Restart();
                    for (int i = 0; i < N; i++)
                        nVec.DotAndDot(NbyM, mVec);

                    long timeA = sw.ElapsedMilliseconds;
                    Console.WriteLine($"*** Accord *** {timeA:n0}ms");
                    Console.WriteLine();


                    // Now let's see if we can speed it up :)
                    sw.Restart();
                    for (int i = 0; i < N; i++)
                        nVec.DotAndDotFast(NbyM, mVec);

                    time = sw.ElapsedMilliseconds;
                    Console.WriteLine($"Time taken no intermediate {time:n0}ms");


                    sw.Restart();
                    for (int i = 0; i < N; i++)
                        nVec.DotAndDotFaster(NbyM, mVec);

                    long timeP = sw.ElapsedMilliseconds;
                    Console.WriteLine($"Time taken no intermediate pointer arithmetic loop unroll x2 {timeP:n0}ms");
                }

                {
                    #region Jit the code

                    // JIT the code
                    for (int i = 0; i < 10; i++)
                    {
                        // Accord
                        nVec.Dot(NbyM, mVec);

                        // Variations to test speed improvement
                        NbyM.VM1(nVec, mVec);
                        NbyM.VM2(nVec, mVec);
                        NbyM.VM3(nVec, mVec);
                        NbyM.VM4(nVec, mVec);
                        NbyM.VM5(nVec, mVec);
                        NbyM.VM6(nVec, mVec);
                        NbyM.VM7(nVec, mVec);
                        NbyM.VM8(nVec, mVec);
                    }

                    #endregion

                    Log($"Vector-Matrix (N={Size})");

                    long time;
                    var sw = Stopwatch.StartNew();

                    // First let's see how fast Accord is
                    sw.Restart();
                    for (int i = 0; i < N; i++)
                        nVec.Dot(NbyM, mVec);

                    long timeA = sw.ElapsedMilliseconds;
                    Console.WriteLine($"*** Accord *** {timeA:n0}ms");
                    Console.WriteLine();


                    // Now let's see if we can speed it up :)
                    sw.Restart();
                    for (int i = 0; i < N; i++)
                        NbyM.VM6(nVec, mVec);

                    time = sw.ElapsedMilliseconds;
                    Console.WriteLine($"Time taken temp cache aware pointer arithmetic {time:n0}ms");


                    sw.Restart();
                    for (int i = 0; i < N; i++)
                        NbyM.VM7(nVec, mVec);

                    long timeP = sw.ElapsedMilliseconds;
                    Console.WriteLine($"Time taken temp cache aware pointer arithmetic loop unroll x2 {timeP:n0}ms");


                    sw.Restart();
                    for (int i = 0; i < N; i++)
                        NbyM.VM8(nVec, mVec);

                    time = sw.ElapsedMilliseconds;
                    Console.WriteLine($"Time taken temp cache aware pointer arithmetic loop unroll x4 {time:n0}ms");

                    double mul = Math.Round(timeA / (double)timeP, 1);
                    sb1.AppendLine($"|{Size}|{N}|{timeA}|{timeP}|x{mul}|");
                }

                {
                    Console.WriteLine();
                    Log($"Matrix-Vector (N={Size})");

                    #region Jit the code

                    // JIT the code
                    for (int i = 0; i < 10; i++)
                    {
                        // Accord
                        NbyM.Dot(mVec, nVec);

                        // Variations to test speed improvement
                        NbyM.MVNoCache(mVec, nVec);
                        NbyM.MV(mVec, nVec);
                        NbyM.MVC(mVec, nVec);
                        nVec.Clear();

                        NbyM.MVCLoopUnrolling2(mVec, nVec);
                        nVec.Clear();

                        NbyM.MVCLoopUnrolling4(mVec, nVec);
                        nVec.Clear();
                    }

                    #endregion

                    long time;
                    var sw = Stopwatch.StartNew();


                    // First let's see how fast Accord is
                    sw.Restart();
                    for (int i = 0; i < N; i++)
                        NbyM.Dot(mVec, nVec);

                    long timeA = sw.ElapsedMilliseconds;
                    Console.WriteLine($"*** Accord *** {timeA:n0}ms");
                    Console.WriteLine();


                    // Now let's see if we can speed it up :)
                    sw.Restart();
                    for (int i = 0; i < N; i++)
                        NbyM.MVC(mVec, nVec);

                    time = sw.ElapsedMilliseconds;
                    Console.WriteLine($"Time taken pointer arithmetic is {time:n0}ms");

                    sw.Restart();
                    for (int i = 0; i < N; i++)
                        NbyM.MVCLoopUnrolling2(mVec, nVec);

                    long timeP = sw.ElapsedMilliseconds;
                    Console.WriteLine($"Time taken pointer arithmetic x2 loop unrolling is {timeP:n0}ms");

                    sw.Restart();
                    for (int i = 0; i < N; i++)
                        NbyM.MVCLoopUnrolling4(mVec, nVec);

                    time = sw.ElapsedMilliseconds;
                    Console.WriteLine($"Time taken pointer arithmetic x4 loop unrolling is {time:n0}ms");
                    Console.WriteLine();

                    double mul = Math.Round(timeA / (double)timeP, 1);
                    sb2.AppendLine($"|{Size}|{N}|{timeA}|{timeP}|x{mul}|");
                }

                Size *= 2;
                Size2 = Size;
                Size3 = Size;
                N /= 4;
            }

            System.IO.File.WriteAllText(@"Benchmarking.txt", sb1.ToString() + Environment.NewLine + sb2.ToString());
        }

        public static void Log(string label = null)
        {
            var banner = new string('=', 50);
            Console.WriteLine(banner);

            if (label != null)
            {
                Console.WriteLine(label);
                Console.WriteLine(banner);
            }
        }
    }
}