using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using FastReflection.Helpers;

namespace FastReflection.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class PropertyHelperBenchmarks
{
	private readonly List<object> _results = new();

	[Benchmark]
	public void DelegateGetter()
	{
		var instance = new TestObject();
		var instanceType = typeof(TestObject);
		var propertyName = nameof(TestObject.TestProperty);

		var helper = ObjectPropertyHelper.Create(instanceType);
		var setter = helper.Setter<string>(propertyName);
		var getter = helper.Getter<string>(propertyName);

		for (int i = 0; i < 1000; i++)
		{
			setter(instance, "Test Name");

			_results.Add(getter(instance));
		}
	}

	[Benchmark]
	public void ReflectionGetter()
	{
		var instance = new TestObject();
		var instanceType = typeof(TestObject);
		var propertyName = nameof(TestObject.TestProperty);

		for (int i = 0; i < 1000; i++)
		{
			var property = instanceType.GetProperty(propertyName)!;

			property.SetValue(instance, "Test Name");

			_results.Add(property.GetValue(instance, null)!);
		}
	}
}