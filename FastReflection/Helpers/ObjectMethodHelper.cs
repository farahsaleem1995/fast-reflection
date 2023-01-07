using FastReflection.Delegates;

namespace FastReflection.Helpers;

public class ObjectMethodHelper
{
	private readonly Dictionary<string, MethodDelegate> _methodDelgates = new();
	private readonly Type _objectType;

	public ObjectMethodHelper(Type objectType)
	{
		_objectType = objectType;
	}

	public ParameterlessMethodInvoker<TResult> ParameterlessInvoker<TResult>(string methodName)
	{
		var methodDelegate = CreateMethodHelpers<TResult>(methodName, Array.Empty<Type>());

		return methodDelegate.ParameterlessInvoker<TResult>();
	}

	public OneParameterMethodInvoker<TResult> OneParameterInvoker<TArg1, TResult>(string methodName)
	{
		var method = CreateMethodHelpers<TResult>(methodName, new[] { typeof(TArg1) });

		return method.OneParameterInvoker<TResult>();
	}

	public TwoParameterMethodInvoker<TResult> TwoParameterInvoker<TArg1, TArg2, TResult>(string methodName)
	{
		var method = CreateMethodHelpers<TResult>(methodName, new[] { typeof(TArg1), typeof(TArg2) });

		return method.TwoParameterInvoker<TResult>();
	}

	public static ObjectMethodHelper Create(Type objectType)
	{
		return new ObjectMethodHelper(objectType);
	}

	private MethodDelegate CreateMethodHelpers<TResult>(string methodName, Type[] parameters)
	{
		var methodKey = $"{methodName}({string.Join<Type>(", ", parameters)})";

		if (!_methodDelgates.ContainsKey(methodKey))
		{
			var helper = new MethodHelper<TResult>(_objectType, methodName, parameters);

			_methodDelgates.Add(methodKey, new(helper.Invoker));
		}

		return _methodDelgates[methodKey];
	}
}