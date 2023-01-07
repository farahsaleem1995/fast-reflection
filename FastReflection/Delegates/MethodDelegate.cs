namespace FastReflection.Delegates;

public class MethodDelegate
{
	public const string InvalidDelegateErrorMessage = "Method '{0}' cannot be activated for value of '{1}' type.";

	private readonly Delegate _methodDelegate;

	public MethodDelegate(Delegate methodDelegate)
	{
		_methodDelegate = methodDelegate;
	}

	public ParameterlessMethodInvoker<TResult> ParameterlessInvoker<TResult>()
	{
		return CastDelegate<ParameterlessMethodInvoker<TResult>>();
	}

	public OneParameterMethodInvoker<TResult> OneParameterInvoker<TResult>()
	{
		return CastDelegate<OneParameterMethodInvoker<TResult>>();
	}

	public TwoParameterMethodInvoker<TResult> TwoParameterInvoker<TResult>()
	{
		return CastDelegate<TwoParameterMethodInvoker<TResult>>();
	}

	private TResult CastDelegate<TResult>() where TResult : Delegate
	{
		try
		{
			return (TResult)_methodDelegate;
		}
		catch (InvalidCastException)
		{
			throw new InvalidOperationException(
				string.Format(InvalidDelegateErrorMessage, _methodDelegate, typeof(TResult)));
		}
	}
}

public delegate T ParameterlessMethodInvoker<T>(object instance);

public delegate T OneParameterMethodInvoker<T>(object instance, object arg1);

public delegate T TwoParameterMethodInvoker<T>(object instance, object arg1, object arg2);