using FastReflection.Delegates;

namespace FastReflection.Helpers;

public class ObjectPropertyHelper
{
	private readonly Type _objectType;
	private readonly Dictionary<string, PropertyHelper> _properties = new();

	private ObjectPropertyHelper(Type objectType)
	{
		_objectType = objectType;
	}

	public static ObjectPropertyHelper Create(Type objectType)
	{
		return new ObjectPropertyHelper(objectType);
	}

	public PropertyGetter Getter(string propertyName)
	{
		var helper = GetPropertyHelper(propertyName);

		return helper.Getter;
	}

	public PropertySetter Setter(string propertyName)
	{
		var helper = GetPropertyHelper(propertyName);

		return helper.Setter;
	}

	private PropertyHelper GetPropertyHelper(string propertyName)
	{
		if (!_properties.ContainsKey(propertyName))
		{
			var helper = new PropertyHelper(_objectType, propertyName);

			_properties.Add(propertyName, helper);
		}

		return _properties[propertyName];
	}
}