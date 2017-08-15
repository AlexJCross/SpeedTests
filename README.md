# SpeedTests

_A note on precision: No method has been implemented such that the precision has been degraded by early casting. It is possible to speed up a number of methods if we allow that to happen, but I will submit that code in a separate commit. However, it is the case that some of the numbers will change ever so slightly as a result of floating point arithmetic; the order in which numbers are being summed has changed in some instances and, in general, (a + b) + c does not equal (a + c) + b when floating point numbers are involved._

#### Cross-Product
Signature: `double[] Cross(this double[] a, double[] b, double[] result)`
Unit test: `Accord.Tests.Math.MatrixTest.CrossProductTest()`
Numerical impact: None
Comments: Addressing issue [GH-755](https://github.com/accord-net/framework/issues/755). Unit test confirms correctness of cross-product and ensures that `result` vector is assigned correctly.

#### Matrix-Matrix
Signature: `double[][] Dot(this double[,] a, double[][] b, double[][] result)`
Unit test: `Accord.Tests.Math.MatrixTest.MultiplyTwoMatrices3()`
Performance impact:

|Size|Trials|Accord (ms)|Proposed (ms)|Multiplier|
|----------|:-------------:|------:|------:|------:|
|8|8388608|14062|6577|x2.1|
|16|1048576|12768|5324|x2.4|
|32|131072|11840|4677|x2.5|
|64|16384|11809|4988|x2.4|
|128|2048|11462|4762|x2.4|
|256|256|11455|4637|x2.5|
|512|32|11365|4632|x2.5|
|1024|4|12315|5024|x2.5|

****************************************

Signature: `double[,] TransposeAndDot(this double[,] a, double[,] b, double[,] result)`
Unit test 1: `Accord.Tests.Math.MatrixTest.TransposeAndMultiplyTest()`
Unit test 2: `Accord.Tests.Math.MatrixTest.TransposeAndDotBatchTest()`
Performance impact:

|Size|Trials|Accord (ms)|Proposed (ms)|Multiplier|
|----------|:-------------:|------:|------:|------:|
|8|8388608|14733|7469|x2|
|16|1048576|13332|5736|x2.3|
|32|131072|12424|4742|x2.6|
|64|16384|13087|4861|x2.7|
|128|2048|13490|4544|x3|
|256|256|17989|4473|x4|
|512|32|18799|4698|x4|
|1024|4|51634|4714|x11|

****************************************

Signature: `double[][] DotWithTransposed(this double[][] a, double[][] b, double[][] result)`
Unit test 1: `Accord.Tests.Math.MatrixTest.DotWithTransposeTest_Jagged()`
Unit test 2: `Accord.Tests.Math.MatrixTest.DotWithTransposeBatchTest_Jagged()`
Performance impact: 

|Size|Trials|Accord (ms)|Proposed (ms)|Multiplier|
|----------|:-------------:|------:|------:|------:|
|8|8388608|9222|4388|x2.1|
|16|1048576|6203|3868|x1.6|
|32|131072|5795|3815|x1.5|
|64|16384|5175|4200|x1.2|
|128|2048|4850|4297|x1.1|
|256|256|4615|4109|x1.1|
|512|32|4803|4347|x1.1|
|1024|4|5050|4485|x1.1|

****************************************

Signature: `double[][] DotWithTransposed(this double[][] a, double[,] b, double[][] result)`
Unit test 1: `Accord.Tests.Math.MatrixTest.DotWithTransposeTest_Jagged1()`
Unit test 2: `Accord.Tests.Math.MatrixTest.DotWithTransposeBatchTest_Jagged1()`
Performance impact:

|Size|Trials|Accord (ms)|Proposed (ms)|Multiplier|
|----------|:-------------:|------:|------:|------:|
|8|8388608|11248|4348|x2.6|
|16|1048576|7600|4134|x1.8|
|32|131072|6239|4006|x1.6|
|64|16384|5821|3954|x1.5|
|128|2048|5242|4077|x1.3|
|256|256|5066|4121|x1.2|
|512|32|4973|4117|x1.2|
|1024|4|5308|4310|x1.2|

****************************************

Signature: `double[][] DotWithTransposed(this double[,] a, double[][] b, double[][] result)`
Unit test 1: `Accord.Tests.Math.MatrixTest.DotWithTransposeTest_Jagged2()`
Unit test 2: `Accord.Tests.Math.MatrixTest.DotWithTransposeBatchTest_Jagged2()`
Performance impact:

|Size|Trials|Accord (ms)|Proposed (ms)|Multiplier|
|----------|:-------------:|------:|------:|------:|
|8|8388608|15405|4317|x3.6|
|16|1048576|12709|3877|x3.3|
|32|131072|11675|4320|x2.7|
|64|16384|11839|4186|x2.8|
|128|2048|11389|4162|x2.7|
|256|256|11339|4293|x2.6|
|512|32|11458|4247|x2.7|
|1024|4|11700|4600|x2.5|

#### Matrix-Vector
Signature: `double[] Dot(this double[,] matrix, double[] columnVector, double[] result)`
Unit test 1: `Accord.Tests.Math.MatrixTest.MultiplyMatrixVectorTest()`
Unit test 2: `Accord.Tests.Math.MatrixTest.MultiplyMatrixVectorBatchTest()`
Performance impact:

|Size|Trials|Accord (ms)|Proposed (ms)|Multiplier|
|----------|:-------------:|------:|------:|------:|
|8|8388608|1437|821|x1.8|
|16|2097152|1393|630|x2.2|
|32|524288|1399|560|x2.5|
|64|131072|1436|564|x2.5|
|128|32768|1427|554|x2.6|
|256|8192|1418|543|x2.6|
|512|2048|1405|573|x2.5|
|1024|512|1396|575|x2.4|



#### Vector-Matrix-Vector
Signature: `double DotAndDot(this double[] rowVector, double[,] matrix, double[] columnVector)`
Unit test: `Accord.Tests.Math.MatrixTest.DotAndDotTest()`
Performance impact:

|Size|Trials|Accord (ms)|Proposed (ms)|Multiplier|
|----------|:-------------:|------:|------:|------:|
|8|8388608|1917|679|x2.8|
|16|2097152|1663|491|x3.4|
|32|524288|1570|461|x3.4|
|64|131072|1605|439|x3.7|
|128|32768|1774|436|x4.1|
|256|8192|2388|442|x5.4|
|512|2048|2806|467|x6|
|1024|512|8393|497|x16.9|


#### Outer
Signature: `double[,] Outer(this double[] a, double[] b, double[,] result)`
Unit test 1: `Accord.Tests.Math.MatrixTest.OuterProductTest()`
Unit test 2: `Accord.Tests.Math.MatrixTest.OuterProductTestDifferentOverloads()`
Performance impact:

|Size|Trials|Accord (ms)|Proposed (ms)|Multiplier|
|----------|:-------------:|------:|------:|------:|
|8|8388608|1227|621|x2|
|16|2097152|1414|606|x2.3|
|32|524288|1302|571|x2.3|
|64|131072|1237|510|x2.4|
|128|32768|1251|430|x2.9|
|256|8192|1188|386|x3.1|
|512|2048|1231|407|x3|
|1024|512|1179|388|x3|

#### Kronecker (Vectors)
Signature: `double[] Kronecker(this double[] a, double[] b, double[] result)`
Unit test 1: `Accord.Tests.Math.MatrixTest.KroneckerVectorTest()`
Unit test 2: `Accord.Tests.Math.MatrixTest.KroneckerVectorBatchTest()`
Performance impact:

|Size|Trials|Accord (ms)|Proposed (ms)|Multiplier|
|----------|:-------------:|------:|------:|------:|
|8|8388608|1179|649|x1.8|
|16|2097152|995|580|x1.7|
|32|524288|1082|593|x1.8|
|64|131072|995|600|x1.7|
|128|32768|1000|697|x1.4|
|256|8192|1047|737|x1.4|
|512|2048|1051|735|x1.4|
|1024|512|1006|753|x1.3|


#### Kronecker (Matrices)
Signature1: `double[][] Kronecker(this double[,] a, double[][] b, double[][] result)`
Signature2: `double[][] Kronecker(this double[][] a, double[,] b, double[][] result)`
Signature3: `double[][] Kronecker(this double[][] a, double[][] b, double[][] result)`
Unit test 1: `Accord.Tests.Math.MatrixTest.KroneckerTest()`
Unit test 2: `Accord.Tests.Math.MatrixTest.KroneckerBatchTest()`
Comments: These methods threw `NotImplementedException` and have now been implemented and tested.
Performance impact: N/A

