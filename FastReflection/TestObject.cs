namespace FastReflection;

public class TestObject
{
	public string? TestProperty { get; set; }

	public string? PrivetSetterTestProperty { get; private set; }

	public string? PrivetGetterTestProperty { private get; set; }

	public int Do()
	{
		return 1;
	}

	public string Do(string key)
	{
		return $"Done - {key}";
	}

	public string Do(string key, int serial)
	{
		return $"Done - {key}, {serial}";
	}
}