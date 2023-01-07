using FastReflection.Delegates;
using System.Reflection;

namespace FastReflection.Helpers;

public class PropertyHelper<TValue>
{
	private static readonly MethodInfo _callPropertyGetterOpenGenericMethod =
		typeof(PropertyHelper<TValue>).GetTypeInfo().GetDeclaredMethod(nameof(CallPropertyGetter))!;

	private static readonly MethodInfo _callPropertySetterOpenGenericMethod =
		typeof(PropertyHelper<TValue>).GetTypeInfo().GetDeclaredMethod(nameof(CallPropertySetter))!;

	private readonly Type _declaringType;
	private readonly string _propertyName;

	public PropertyHelper(Type declaringType, string propertyName)
	{
		_declaringType = declaringType;
		_propertyName = propertyName;
	}

	public PropertyGetter<TValue> Getter
	{
		get
		{
			var propertyInfo = GetPropertyInfo(_declaringType, _propertyName);

			if (propertyInfo.GetMethod?.IsPublic == false)
			{
				throw new InvalidOperationException();
			}

			return MakeFastPropertyGetter(propertyInfo);
		}
	}

	public PropertySetter<TValue> Setter
	{
		get
		{
			var propertyInfo = GetPropertyInfo(_declaringType, _propertyName);

			if (propertyInfo.SetMethod?.IsPublic == false)
			{
				throw new InvalidOperationException();
			}

			return MakeFastPropertySetter(propertyInfo);
		}
	}

	private static PropertyInfo GetPropertyInfo(Type declaringType, string propertyMethod)
	{
		var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

		var propertyInfo = declaringType.GetProperty(propertyMethod, bindingFlags);

		return propertyInfo ?? throw new InvalidOperationException();
	}

	private static PropertyGetter<TValue> MakeFastPropertyGetter(PropertyInfo propertyInfo)
	{
		return TryMakeFastProperty(propertyInfo, () => TryMakeFastPropertyGetter(propertyInfo));
	}

	private static PropertyGetter<TValue> TryMakeFastPropertyGetter(PropertyInfo propertyInfo)
	{
		var propertyGetMethod = propertyInfo.GetMethod!;

		var typeInput = propertyGetMethod.DeclaringType!;
		var typeOutput = propertyGetMethod.ReturnType;

		var delegateType = typeof(Func<,>).MakeGenericType(typeInput, typeOutput);
		var propertyGetterDelegate = propertyGetMethod.CreateDelegate(delegateType);

		var wrapperDelegateMethod = _callPropertyGetterOpenGenericMethod.MakeGenericMethod(typeInput);

		var accessorDelegate = wrapperDelegateMethod.CreateDelegate(typeof(PropertyGetter<TValue>),
			propertyGetterDelegate);

		return (PropertyGetter<TValue>)accessorDelegate;
	}

	private static TValue CallPropertyGetter<TDecalringType>(
		Func<TDecalringType, TValue> deleg, object target)
	{
		return deleg((TDecalringType)target)!;
	}

	private static PropertySetter<TValue> MakeFastPropertySetter(PropertyInfo propertyInfo)
	{
		return TryMakeFastProperty(propertyInfo, () => TryMakeFastPropertySetter(propertyInfo));
	}

	private static PropertySetter<TValue> TryMakeFastPropertySetter(PropertyInfo propertyInfo)
	{
		var setMethod = propertyInfo.SetMethod!;
		var parameters = setMethod.GetParameters();

		var typeInput = setMethod.DeclaringType!;
		var parameterType = parameters[0].ParameterType;

		var propertySetterAsAction = setMethod.CreateDelegate(typeof(Action<,>)
			.MakeGenericType(typeInput, parameterType));

		var callPropertySetterClosedGenericMethod = _callPropertySetterOpenGenericMethod
			.MakeGenericMethod(typeInput);

		var callPropertySetterDelegate = callPropertySetterClosedGenericMethod.CreateDelegate(
			typeof(PropertySetter<TValue>), propertySetterAsAction);

		return (PropertySetter<TValue>)callPropertySetterDelegate;
	}

	private static void CallPropertySetter<TDecalringType>(
		Action<TDecalringType, TValue> setter, object target, object value)
	{
		setter((TDecalringType)target, (TValue)value);
	}

	private static TProperty TryMakeFastProperty<TProperty>(PropertyInfo propertyInfo, Func<TProperty> func)
	{
		try
		{
			return func();
		}
		catch (Exception e)
		{
			throw new InvalidOperationException(string.Format(
				PropertyDelegate.InvalidDelegateErrorMessage, propertyInfo.Name, typeof(TProperty)), e);
		}
	}
}