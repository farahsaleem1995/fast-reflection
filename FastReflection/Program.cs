using FastReflection;
using FastReflection.Helpers;

var instance = new TestObject();
var instanceType = typeof(TestObject);
var propertyName = nameof(TestObject.TestProperty);

var setterDelegate = ObjectPropertyHelper.Create(instanceType).Setter<string>(propertyName);

var getterDelegate = ObjectPropertyHelper.Create(instanceType).Getter<string>(propertyName);

setterDelegate(instance, "Test Name");

Console.WriteLine(getterDelegate(instance));

var doDelegate = ObjectMethodHelper.Create(instanceType).ParameterlessInvoker<int>("Do");

Console.WriteLine(doDelegate(instance));

var do1Delegate = ObjectMethodHelper.Create(instanceType).OneParameterInvoker<string, string>("Do");

Console.WriteLine(do1Delegate(instance, "key"));

var do2Delegate = ObjectMethodHelper.Create(instanceType).TwoParameterInvoker<string, int, string>("Do");

Console.WriteLine(do2Delegate(instance, "key", 1));

var method = instanceType.GetMethod("Do", Array.Empty<Type>())!;

Console.WriteLine(method.Invoke(instance, null));