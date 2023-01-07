using FastReflection.Delegates;
using System.Reflection;

namespace FastReflection.Helpers;

public class MethodHelper<TResult>
{
	private static readonly MethodInfo _noParameCallInvokerOpenGenericMethod =
		typeof(MethodHelper<TResult>).GetTypeInfo().GetDeclaredMethod(nameof(NoParamCallInvoker))!;

	private static readonly MethodInfo _oneParameCallInvokerOpenGenericMethod =
		typeof(MethodHelper<TResult>).GetTypeInfo().GetDeclaredMethod(nameof(OneParamCallInvoker))!;

	private static readonly MethodInfo _twoParameCallInvokerOpenGenericMethod =
		typeof(MethodHelper<TResult>).GetTypeInfo().GetDeclaredMethod(nameof(TwoParamCallInvoker))!;

	private readonly Delegate? _methodDelegate;

	public MethodHelper(Type declaringType, string methodName, Type[] parameters)
	{
		var methodInfo = GetMethodInfo(declaringType, methodName, parameters);

		_methodDelegate = methodInfo.GetParameters().Length switch
		{
			0 => MakeFastNoParamMethodInvoker(methodInfo),
			1 => MakeFastOneParamMethodInvoker(methodInfo),
			2 => MakeFastTwoParamMethodInvoker(methodInfo),
			_ => throw new InvalidOperationException(),
		};
	}

	public Delegate Invoker => _methodDelegate ?? throw new InvalidOperationException();

	private static MethodInfo GetMethodInfo(Type declaringType, string propertyMethod, Type[] parameters)
	{
		var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

		var methodInfo = declaringType.GetMethod(propertyMethod, bindingFlags, parameters);

		return methodInfo ?? throw new InvalidOperationException();
	}

	private static ParameterlessMethodInvoker<TResult> MakeFastNoParamMethodInvoker(MethodInfo methodInfo)
	{
		return TryMakeFastInvoker(methodInfo, () => TryMakeFastNoParamMethodInvoker(methodInfo));
	}

	private static ParameterlessMethodInvoker<TResult> TryMakeFastNoParamMethodInvoker(MethodInfo methodInfo)
	{
		var typeInput = methodInfo.DeclaringType!;
		var typeOutput = methodInfo.ReturnType;

		var invokerDelegateType = typeof(Func<,>).MakeGenericType(typeInput, typeOutput);
		var invokerDelegate = methodInfo.CreateDelegate(invokerDelegateType);

		var callInvokerClosedGenericMethod = _noParameCallInvokerOpenGenericMethod
			.MakeGenericMethod(typeInput);

		var callInvokerDelegate = callInvokerClosedGenericMethod.CreateDelegate(
			typeof(ParameterlessMethodInvoker<TResult>), invokerDelegate);

		return (ParameterlessMethodInvoker<TResult>)callInvokerDelegate;
	}

	private static TResult NoParamCallInvoker<TDecalringType>(
		Func<TDecalringType, TResult> deleg, object target)
	{
		return deleg((TDecalringType)target)!;
	}

	private static OneParameterMethodInvoker<TResult> MakeFastOneParamMethodInvoker(MethodInfo methodInfo)
	{
		return TryMakeFastInvoker(methodInfo, () => TryMakeFastOneParamMethodInvoker(methodInfo));
	}

	private static OneParameterMethodInvoker<TResult> TryMakeFastOneParamMethodInvoker(MethodInfo methodInfo)
	{
		var typeInput1 = methodInfo.DeclaringType!;
		var typeInput2 = methodInfo.GetParameters()[0].ParameterType!;
		var typeOutput = methodInfo.ReturnType;

		var invokerDelegateType = typeof(Func<,,>).MakeGenericType(typeInput1, typeInput2, typeOutput);
		var invokerDelegate = methodInfo.CreateDelegate(invokerDelegateType);

		var callInvokerClosedGenericMethod = _oneParameCallInvokerOpenGenericMethod
			.MakeGenericMethod(typeInput1, typeInput2);

		var callInvokerDelegate = callInvokerClosedGenericMethod.CreateDelegate(
			typeof(OneParameterMethodInvoker<TResult>), invokerDelegate);

		return (OneParameterMethodInvoker<TResult>)callInvokerDelegate;
	}

	private static TResult OneParamCallInvoker<TDecalringType, TArg1>(
		Func<TDecalringType, TArg1, TResult> deleg, object target, object arg1)
	{
		return deleg((TDecalringType)target, (TArg1)arg1)!;
	}

	private static TwoParameterMethodInvoker<TResult> MakeFastTwoParamMethodInvoker(MethodInfo methodInfo)
	{
		return TryMakeFastInvoker(methodInfo, () => TryMakeFastTwoParamMethodInvoker(methodInfo));
	}

	private static TwoParameterMethodInvoker<TResult> TryMakeFastTwoParamMethodInvoker(MethodInfo methodInfo)
	{
		var typeInput1 = methodInfo.DeclaringType!;
		var typeInput2 = methodInfo.GetParameters()[0].ParameterType!;
		var typeInput3 = methodInfo.GetParameters()[1].ParameterType!;
		var typeOutput = methodInfo.ReturnType;

		var invokerDelegateType = typeof(Func<,,,>).MakeGenericType(typeInput1, typeInput2, typeInput3, typeOutput);
		var invokerDelegate = methodInfo.CreateDelegate(invokerDelegateType);

		var callInvokerClosedGenericMethod = _twoParameCallInvokerOpenGenericMethod
			.MakeGenericMethod(typeInput1, typeInput2, typeInput3);

		var callInvokerDelegate = callInvokerClosedGenericMethod.CreateDelegate(
			typeof(TwoParameterMethodInvoker<TResult>), invokerDelegate);

		return (TwoParameterMethodInvoker<TResult>)callInvokerDelegate;
	}

	private static TResult TwoParamCallInvoker<TDecalringType, TArg1, TArg2>(
		Func<TDecalringType, TArg1, TArg2, TResult> deleg, object target, object arg1, object arg2)
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
			throw new InvalidOperationException(string.Format(
				MethodDelegate.InvalidDelegateErrorMessage, methodInfo.Name, typeof(TInvoker)), e);
		}
	}
}