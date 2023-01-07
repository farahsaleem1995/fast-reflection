using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using FastReflection.Helpers;

namespace FastReflection.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class MethodHelperBenchmarks
{
	[Benchmark]
	public void DelegateInvoker()
	{
		var instance = new TestObject();
		var instanceType = typeof(TestObject);

		var invoker = ObjectMethodHelper.Create(instanceType).GetParameterlessInvoker("Do");

		for (int i = 0; i < 1000; i++)
		{
			_ = invoker(instance);
		}
	}

	[Benchmark]
	public void ReflectionInvoker()
	{
		var instance = new TestObject();
		var instanceType = typeof(TestObject);

		for (int i = 0; i < 1000; i++)
		{
			var method = instanceType.GetMethod("Do", Array.Empty<Type>())!;
			_ = method.Invoke(instance, null);
		}
	}
}