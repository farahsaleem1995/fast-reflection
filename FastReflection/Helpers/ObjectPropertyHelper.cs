using FastReflection.Delegates;

namespace FastReflection.Helpers;

public class ObjectPropertyHelper
{
	private readonly Type _objectType;
	private readonly Dictionary<string, PropertyDelegate> _propertyDelegates = new();

	private ObjectPropertyHelper(Type objectType)
	{
		_objectType = objectType;
	}

	public static ObjectPropertyHelper Create(Type objectType)
	{
		return new ObjectPropertyHelper(objectType);
	}

	public PropertyGetter<TValue> Getter<TValue>(string propertyName)
	{
		var cacheKey = $"Getter_{propertyName}";

		if (!_propertyDelegates.ContainsKey(cacheKey))
		{
			var helper = new PropertyHelper<TValue>(_objectType, propertyName);

			_propertyDelegates.Add(cacheKey, new(helper.Getter));
		}

		return _propertyDelegates[cacheKey].Getter<TValue>();
	}

	public PropertySetter<TValue> Setter<TValue>(string propertyName)
	{
		var cacheKey = $"Setter_{propertyName}";

		if (!_propertyDelegates.ContainsKey(cacheKey))
		{
			var helper = new PropertyHelper<TValue>(_objectType, propertyName);

			_propertyDelegates.Add(cacheKey, new(helper.Setter));
		}

		return _propertyDelegates[cacheKey].Setter<TValue>();
	}
}