namespace FastReflection.Delegates;

public class PropertyDelegate
{
	public const string InvalidDelegateErrorMessage = "Property '{0}' cannot be activated for value of '{1}' type.";

	private readonly Delegate _propertyDelegate;

	public PropertyDelegate(Delegate propertyDelegate)
	{
		_propertyDelegate = propertyDelegate;
	}

	public PropertySetter<TValue> Setter<TValue>()
	{
		return (PropertySetter<TValue>)_propertyDelegate;
	}

	public PropertyGetter<TValue> Getter<TValue>()
	{
		return (PropertyGetter<TValue>)_propertyDelegate;
	}

	private TValue CastDelegate<TValue>() where TValue : Delegate
	{
		try
		{
			return (TValue)_propertyDelegate;
		}
		catch (InvalidCastException)
		{
			throw new InvalidOperationException(
				string.Format(InvalidDelegateErrorMessage, _propertyDelegate, typeof(TValue)));
		}
	}
}

public delegate TValue PropertyGetter<TValue>(object instance);

public delegate void PropertySetter<TValue>(object instance, TValue value);