using FastReflection.Delegates;

namespace FastReflection.Helpers;

public class ObjectMethodHelper
{
	private readonly Dictionary<string, MethodHelper> _methods = new();
	private readonly Type _objectType;

	public ObjectMethodHelper(Type objectType)
	{
		_objectType = objectType;
	}

	public ParameterlessMethodInvoker GetParameterlessInvoker(string methodName)
	{
		var helper = CreateMethodHelpers(methodName, Array.Empty<Type>());

		return (ParameterlessMethodInvoker)helper.Invoker;
	}

	public OneParameterMethodInvoker GetOneParameterInvoker(string methodName, Type arg1)
	{
		var helper = CreateMethodHelpers(methodName, new[] { arg1 });

		return (OneParameterMethodInvoker)helper.Invoker;
	}

	public TwoParameterMethodInvoker GetTwoParameterInvoker(string methodName, Type arg1, Type arg2)
	{
		var helper = CreateMethodHelpers(methodName, new[] { arg1, arg2 });

		return (TwoParameterMethodInvoker)helper.Invoker;
	}

	public static ObjectMethodHelper Create(Type objectType)
	{
		return new ObjectMethodHelper(objectType);
	}

	private MethodHelper CreateMethodHelpers(string methodName, Type[] parameters)
	{
		var methodKey = $"{methodName}({string.Join<Type>(", ", parameters)})";

		if (!_methods.ContainsKey(methodKey))
		{
			var helper = new MethodHelper(_objectType, methodName, parameters);

			_methods.Add(methodKey, helper);
		}

		return _methods[methodKey];
	}
}