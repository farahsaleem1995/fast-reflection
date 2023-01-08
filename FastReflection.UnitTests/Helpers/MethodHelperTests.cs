using FastReflection.Delegates;
using FastReflection.Helpers;
using FluentAssertions;

namespace FastReflection.UnitTests.Helpers;

public class MethodHelperTests
{
	[Theory]
	[InlineData("InvalidMethodName", new Type[] { })]
	[InlineData("InvalidMethodName", new Type[] { typeof(string) })]
	[InlineData("InvalidMethodName", new Type[] { typeof(string), typeof(int) })]
	[InlineData(nameof(TestObject.Do), new Type[] { typeof(int) })]
	[InlineData(nameof(TestObject.Do), new Type[] { typeof(char) })]
	[InlineData(nameof(TestObject.Do), new Type[] { typeof(int), typeof(string) })]
	[InlineData(nameof(TestObject.Do), new Type[] { typeof(string), typeof(int), typeof(object) })]
	public void Can_detect_invalid_method_definition(string methodName, Type[] parameters)
	{
		// Arrange
		var declaringType = typeof(TestObject);
		var methodSignature = $"{methodName}({string.Join<Type>(", ", parameters)})";

		// Act
		var act = () =>
		{
			var helper = new MethodHelper(declaringType, methodName, parameters);
			_ = helper.Invoker;
		};

		// Assert
		var message = $"Method '{methodSignature}' of type '{declaringType}' could not be found, or it's not public or declared.";
		act.Should().Throw<InvalidOperationException>()
			.WithMessage(message);
	}

	[Theory]
	[InlineData(nameof(TestObject.Do), new Type[] { }, typeof(ParameterlessMethodInvoker),
		new object[] { }, 1)]
	[InlineData(nameof(TestObject.Do), new Type[] { typeof(string) }, typeof(OneParameterMethodInvoker),
		new object[] { "key" }, "Done - key")]
	[InlineData(nameof(TestObject.Do), new Type[] { typeof(string), typeof(int) }, typeof(TwoParameterMethodInvoker),
		new object[] { "key", 1 }, "Done - key, 1")]
	public void Make_invoker_using_valid_method_definition(
		string methodName, Type[] parameters, Type invokerType, object[] invokerArgs, object invokerResult)
	{
		// Arrange
		var declaringType = typeof(TestObject);
		var args = new List<object>() { new TestObject() };
		foreach (var arg in invokerArgs)
		{
			args.Add(arg);
		}

		// Act
		var helper = new MethodHelper(declaringType, methodName, parameters);
		var invoker = helper.Invoker;
		var result = invoker.DynamicInvoke(args.ToArray());

		// Assert
		invoker.Should().BeOfType(invokerType);
		result.Should().Be(invokerResult);
	}
}