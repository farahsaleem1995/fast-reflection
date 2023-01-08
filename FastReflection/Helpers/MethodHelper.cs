using FastReflection.Delegates;
using System.Reflection;

namespace FastReflection.Helpers;

public class MethodHelper
{
	private const string InvalidDelegateErrorMessage = "Method '{0}' cannot be activated for value of '{1}' type.";

	private const string InvalidMethodErrorMessage =
		"Method '{0}' of type '{1}' could not be found or it's nor public or declared method.";

	private const string UnsupportedParameterCountErrorMessage =
		"Method with '{0}' parameters is unsupported";

	private static readonly MethodInfo _parameterlessCallInvokerOpenGenericMethod =
		typeof(MethodHelper).GetTypeInfo().GetDeclaredMethod(nameof(ParameterlessCallInvoker))!;

	private static readonly MethodInfo _oneParameterCallInvokerOpenGenericMethod =
		typeof(MethodHelper).GetTypeInfo().GetDeclaredMethod(nameof(OneParamCallInvoker))!;

	private static readonly MethodInfo _twoParameterCallInvokerOpenGenericMethod =
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

				_methodDelegate = MakeFastMethodInvoker(methodInfo, _parameters);
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

	private static Delegate MakeFastMethodInvoker(MethodInfo methodInfo, Type[] parameters)
	{
		var settings = Settings.GetByParameterCount(parameters.Length);

		try
		{
			return TryMakeFastMethodInvoker(methodInfo, settings);
		}
		catch (Exception e)
		{
			throw new InvalidOperationException(
				string.Format(InvalidDelegateErrorMessage, methodInfo.Name, settings.DelegateType), e);
		}
	}

	private static Delegate TryMakeFastMethodInvoker(MethodInfo methodInfo, Settings settings)
	{
		var invokerDelegateGenericTypeArgs = GetMethodDelegateGenericTypeArguments(methodInfo);
		var invokerDelegateType = settings.DelegateType.MakeGenericType(invokerDelegateGenericTypeArgs);
		var invokerDelegate = methodInfo.CreateDelegate(invokerDelegateType);

		var callInvokerClosedGenericMethod = settings.CallMethod
			.MakeGenericMethod(invokerDelegateGenericTypeArgs);

		var callInvokerDelegate = callInvokerClosedGenericMethod
			.CreateDelegate(settings.CallDelegateType, invokerDelegate);

		return callInvokerDelegate;
	}

	private static Type[] GetMethodDelegateGenericTypeArguments(MethodInfo methodInfo)
	{
		var declaringType = methodInfo.DeclaringType!;
		var returnType = methodInfo.ReturnType;
		var parameters = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();

		var genericArgs = new List<Type>() { declaringType };
		genericArgs.AddRange(parameters);
		genericArgs.Add(returnType);

		return genericArgs.ToArray();
	}

	private static object ParameterlessCallInvoker<TDecalringType, TReturnType>(
		Func<TDecalringType, TReturnType> deleg, object target)
	{
		return deleg((TDecalringType)target)!;
	}

	private static object OneParamCallInvoker<TDecalringType, TArg1, TReturnType>(
		Func<TDecalringType, TArg1, TReturnType> deleg, object target, object arg1)
	{
		return deleg((TDecalringType)target, (TArg1)arg1)!;
	}

	private static object TwoParamCallInvoker<TDecalringType, TArg1, TArg2, TReturnType>(
		Func<TDecalringType, TArg1, TArg2, TReturnType> deleg, object target, object arg1, object arg2)
	{
		return deleg((TDecalringType)target, (TArg1)arg1, (TArg2)arg2)!;
	}

	private class Settings
	{
		private Settings(Type delegateType, MethodInfo callMethod, Type callDelegateType)
		{
			DelegateType = delegateType;
			CallMethod = callMethod;
			CallDelegateType = callDelegateType;
		}

		public static Settings GetByParameterCount(int parameterCount)
		{
			return parameterCount switch
			{
				0 => new Settings(typeof(Func<,>), _parameterlessCallInvokerOpenGenericMethod,
					typeof(ParameterlessMethodInvoker)),

				1 => new Settings(typeof(Func<,,>), _oneParameterCallInvokerOpenGenericMethod,
					typeof(OneParameterMethodInvoker)),

				2 => new Settings(typeof(Func<,,,>), _twoParameterCallInvokerOpenGenericMethod,
					typeof(TwoParameterMethodInvoker)),

				_ => throw new InvalidOperationException(
					string.Format(UnsupportedParameterCountErrorMessage, parameterCount)),
			};
		}

		public Type DelegateType { get; }

		public MethodInfo CallMethod { get; }

		public Type CallDelegateType { get; }
	}
}