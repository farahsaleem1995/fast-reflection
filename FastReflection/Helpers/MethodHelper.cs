using FastReflection.Delegates;
using System.Reflection;

namespace FastReflection.Helpers;

public class MethodHelper
{
	public const string InvalidDelegateErrorMessage = "Method '{0}' cannot be activated for value of '{1}' type.";

	private const string InvalidMethodErrorMessage =
		"Method '{0}' of type'{1}' could not be found or it's nor public or declared method.";

	private static readonly MethodInfo _noParameCallInvokerOpenGenericMethod =
		typeof(MethodHelper).GetTypeInfo().GetDeclaredMethod(nameof(ParameterlessCallInvoker))!;

	private static readonly MethodInfo _oneParameCallInvokerOpenGenericMethod =
		typeof(MethodHelper).GetTypeInfo().GetDeclaredMethod(nameof(OneParamCallInvoker))!;

	private static readonly MethodInfo _twoParameCallInvokerOpenGenericMethod =
		typeof(MethodHelper).GetTypeInfo().GetDeclaredMethod(nameof(TwoParamCallInvoker))!;

	private readonly Type _declaringType;
	private readonly string _methodName;
	private readonly Type[] _parameters;
	private Delegate? _methodDelegate;

	public MethodHelper(Type declaringType, string methodName, Type[] parameters)
	{
		_declaringType = declaringType;
		_methodName = methodName;
		_parameters = parameters;
	}

	public Delegate Invoker
	{
		get
		{
			if (_methodDelegate == null)
			{
				var methodInfo = GetMethodInfo(_declaringType, _methodName, _parameters);

				_methodDelegate = MakeFastMethodInvoker(methodInfo);
			}

			return _methodDelegate;
		}
	}

	private static MethodInfo GetMethodInfo(Type declaringType, string methodName, Type[] parameters)
	{
		var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

		var methodInfo = declaringType.GetMethod(methodName, bindingFlags, parameters);

		if (methodInfo == null)
		{
			var signature = string.Format("{0}({1})", methodName, string.Join<Type>(", ", parameters));

			throw new InvalidOperationException(
				string.Format(InvalidMethodErrorMessage, signature, declaringType));
		}

		return methodInfo;
	}

	private static Delegate MakeFastMethodInvoker(MethodInfo methodInfo)
	{
		return methodInfo.GetParameters().Length switch
		{
			0 => MakeFastParameterlessMethodInvoker(methodInfo),
			1 => MakeFastOneParameterMethodInvoker(methodInfo),
			2 => MakeFastTwoParameterMethodInvoker(methodInfo),
			_ => throw new InvalidOperationException(),
		};
	}

	private static ParameterlessMethodInvoker MakeFastParameterlessMethodInvoker(MethodInfo methodInfo)
	{
		return TryMakeFastInvoker(methodInfo, () => TryMakeFastParameterlessMethodInvoker(methodInfo));
	}

	private static ParameterlessMethodInvoker TryMakeFastParameterlessMethodInvoker(MethodInfo methodInfo)
	{
		var typeInput = methodInfo.DeclaringType!;
		var typeOutput = methodInfo.ReturnType;

		var invokerDelegateType = typeof(Func<,>).MakeGenericType(typeInput, typeOutput);
		var invokerDelegate = methodInfo.CreateDelegate(invokerDelegateType);

		var callInvokerClosedGenericMethod = _noParameCallInvokerOpenGenericMethod
			.MakeGenericMethod(typeInput, typeOutput);

		var callInvokerDelegate = callInvokerClosedGenericMethod.CreateDelegate(
			typeof(ParameterlessMethodInvoker), invokerDelegate);

		return (ParameterlessMethodInvoker)callInvokerDelegate;
	}

	private static object ParameterlessCallInvoker<TDecalringType, TReturnType>(
		Func<TDecalringType, TReturnType> deleg, object target)
	{
		return deleg((TDecalringType)target)!;
	}

	private static OneParameterMethodInvoker MakeFastOneParameterMethodInvoker(MethodInfo methodInfo)
	{
		return TryMakeFastInvoker(methodInfo, () => TryMakeFastOneParameterMethodInvoker(methodInfo));
	}

	private static OneParameterMethodInvoker TryMakeFastOneParameterMethodInvoker(MethodInfo methodInfo)
	{
		var typeInput1 = methodInfo.DeclaringType!;
		var typeInput2 = methodInfo.GetParameters()[0].ParameterType!;
		var typeOutput = methodInfo.ReturnType;

		var invokerDelegateType = typeof(Func<,,>).MakeGenericType(typeInput1, typeInput2, typeOutput);
		var invokerDelegate = methodInfo.CreateDelegate(invokerDelegateType);

		var callInvokerClosedGenericMethod = _oneParameCallInvokerOpenGenericMethod
			.MakeGenericMethod(typeInput1, typeInput2, typeOutput);

		var callInvokerDelegate = callInvokerClosedGenericMethod.CreateDelegate(
			typeof(OneParameterMethodInvoker), invokerDelegate);

		return (OneParameterMethodInvoker)callInvokerDelegate;
	}

	private static object OneParamCallInvoker<TDecalringType, TArg1, TReturnType>(
		Func<TDecalringType, TArg1, TReturnType> deleg, object target, object arg1)
	{
		return deleg((TDecalringType)target, (TArg1)arg1)!;
	}

	private static TwoParameterMethodInvoker MakeFastTwoParameterMethodInvoker(MethodInfo methodInfo)
	{
		return TryMakeFastInvoker(methodInfo, () => TryMakeFastTwoParameterMethodInvoker(methodInfo));
	}

	private static TwoParameterMethodInvoker TryMakeFastTwoParameterMethodInvoker(MethodInfo methodInfo)
	{
		var typeInput1 = methodInfo.DeclaringType!;
		var typeInput2 = methodInfo.GetParameters()[0].ParameterType!;
		var typeInput3 = methodInfo.GetParameters()[1].ParameterType!;
		var typeOutput = methodInfo.ReturnType;

		var invokerDelegateType = typeof(Func<,,,>).MakeGenericType(typeInput1, typeInput2, typeInput3, typeOutput);
		var invokerDelegate = methodInfo.CreateDelegate(invokerDelegateType);

		var callInvokerClosedGenericMethod = _twoParameCallInvokerOpenGenericMethod
			.MakeGenericMethod(typeInput1, typeInput2, typeInput3, typeOutput);

		var callInvokerDelegate = callInvokerClosedGenericMethod.CreateDelegate(
			typeof(TwoParameterMethodInvoker), invokerDelegate);

		return (TwoParameterMethodInvoker)callInvokerDelegate;
	}

	private static object TwoParamCallInvoker<TDecalringType, TArg1, TArg2, TReturnType>(
		Func<TDecalringType, TArg1, TArg2, TReturnType> deleg, object target, object arg1, object arg2)
	{
		return deleg((TDecalringType)target, (TArg1)arg1, (TArg2)arg2)!;
	}

	private static TInvoker TryMakeFastInvoker<TInvoker>(MethodInfo methodInfo, Func<TInvoker> func)
	{
		try
		{
			return func();
		}
		catch (Exception e)
		{
			throw new InvalidOperationException(
				string.Format(InvalidDelegateErrorMessage, methodInfo.Name, typeof(TInvoker)), e);
		}
	}
}