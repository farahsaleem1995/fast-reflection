using FastReflection.Delegates;
using System.Reflection;

namespace FastReflection.Helpers;

public class PropertyHelper
{
	public const string InvalidDelegateErrorMessage = "Property '{0}' cannot be activated for value of '{1}' type.";

	private const string InvalidPropertyErrorMessage =
		"Property '{0}' of type'{1}' could not be found, or it's not public or declared, or has no public {2}.";

	private static readonly MethodInfo _callPropertyGetterOpenGenericMethod =
		typeof(PropertyHelper).GetTypeInfo().GetDeclaredMethod(nameof(CallPropertyGetter))!;

	private static readonly MethodInfo _callPropertySetterOpenGenericMethod =
		typeof(PropertyHelper).GetTypeInfo().GetDeclaredMethod(nameof(CallPropertySetter))!;

	private readonly Type _declaringType;
	private readonly string _propertyName;
	private PropertyGetter? _getterDelegate;
	private PropertySetter? _setterDelegate;

	public PropertyHelper(Type declaringType, string propertyName)
	{
		_declaringType = declaringType;
		_propertyName = propertyName;
	}

	public PropertyGetter Getter
	{
		get
		{
			if (_getterDelegate == null)
			{
				var propertyInfo = GetPropertyInfo(_declaringType, _propertyName, PropertyType.Getter);

				_getterDelegate = MakeFastPropertyGetter(propertyInfo);
			}

			return _getterDelegate;
		}
	}

	public PropertySetter Setter
	{
		get
		{
			if (_setterDelegate == null)
			{
				var propertyInfo = GetPropertyInfo(_declaringType, _propertyName, PropertyType.Setter);

				_setterDelegate = MakeFastPropertySetter(propertyInfo);
			}

			return _setterDelegate;
		}
	}

	private static PropertyInfo GetPropertyInfo(Type declaringType, string propertyName, PropertyType propertyType)
	{
		var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

		var propertyInfo = declaringType.GetProperty(propertyName, bindingFlags);

		if (propertyInfo == null || !IsPropertyHasPublicAccessor(propertyInfo, propertyType))
		{
			throw new InvalidOperationException(
				string.Format(InvalidPropertyErrorMessage, propertyName, declaringType, propertyType));
		}

		return propertyInfo;
	}

	private static bool IsPropertyHasPublicAccessor(PropertyInfo propertyInfo, PropertyType propertyType)
	{
		return propertyType switch
		{
			PropertyType.Getter => propertyInfo.GetMethod?.IsPublic == true,
			PropertyType.Setter => propertyInfo.SetMethod?.IsPublic == true,
			_ => throw new NotImplementedException(),
		};
	}

	private static PropertyGetter MakeFastPropertyGetter(PropertyInfo propertyInfo)
	{
		return TryMakeFastProperty(propertyInfo, () => TryMakeFastPropertyGetter(propertyInfo));
	}

	private static PropertyGetter TryMakeFastPropertyGetter(PropertyInfo propertyInfo)
	{
		var propertyGetMethod = propertyInfo.GetMethod!;

		var typeInput = propertyGetMethod.DeclaringType!;
		var typeOutput = propertyGetMethod.ReturnType;

		var delegateType = typeof(Func<,>).MakeGenericType(typeInput, typeOutput);
		var propertyGetterDelegate = propertyGetMethod.CreateDelegate(delegateType);

		var wrapperDelegateMethod = _callPropertyGetterOpenGenericMethod
			.MakeGenericMethod(typeInput, typeOutput);

		var accessorDelegate = wrapperDelegateMethod.CreateDelegate(typeof(PropertyGetter),
			propertyGetterDelegate);

		return (PropertyGetter)accessorDelegate;
	}

	private static object CallPropertyGetter<TDecalringType, TValue>(
		Func<TDecalringType, TValue> deleg, object target)
	{
		return deleg((TDecalringType)target)!;
	}

	private static PropertySetter MakeFastPropertySetter(PropertyInfo propertyInfo)
	{
		return TryMakeFastProperty(propertyInfo, () => TryMakeFastPropertySetter(propertyInfo));
	}

	private static PropertySetter TryMakeFastPropertySetter(PropertyInfo propertyInfo)
	{
		var setMethod = propertyInfo.SetMethod!;
		var parameters = setMethod.GetParameters();

		var typeInput = setMethod.DeclaringType!;
		var parameterType = parameters[0].ParameterType;

		var propertySetterAsAction = setMethod.CreateDelegate(typeof(Action<,>)
			.MakeGenericType(typeInput, parameterType));

		var callPropertySetterClosedGenericMethod = _callPropertySetterOpenGenericMethod
			.MakeGenericMethod(typeInput, parameterType);

		var callPropertySetterDelegate = callPropertySetterClosedGenericMethod.CreateDelegate(
			typeof(PropertySetter), propertySetterAsAction);

		return (PropertySetter)callPropertySetterDelegate;
	}

	private static void CallPropertySetter<TDecalringType, TValue>(
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
			throw new InvalidOperationException(
				string.Format(InvalidDelegateErrorMessage, propertyInfo.Name, typeof(TProperty)), e);
		}
	}

	private enum PropertyType
	{
		Getter,
		Setter,
	}
}