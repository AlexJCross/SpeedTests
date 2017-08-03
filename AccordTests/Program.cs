using System;
using System.Linq;

using Accord.Math;
using System.Diagnostics;
using Accord.Math.Decompositions;
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
#else
            int Size = 7, N = 50;
            int Size2 = Size;
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
                double[] nVec = Vector.Random(Size);
                double[] mVec = Vector.Random(Size2);

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

                Console.WriteLine();
                Log($"Matrix-Vector (N={Size})");

                {
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
                        NbyM.MVCLoopUnrolling2(mVec, nVec);
                        NbyM.MVCLoopUnrolling4(mVec, nVec);
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