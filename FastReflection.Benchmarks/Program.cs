using BenchmarkDotNet.Running;
using FastReflection.Benchmarks;

var summary1 = BenchmarkRunner.Run<PropertyHelperBenchmarks>();
var summary2 = BenchmarkRunner.Run<MethodHelperBenchmarks>();