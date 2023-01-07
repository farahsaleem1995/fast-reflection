using FastReflection;
using FastReflection.Helpers;

var instance = new TestObject();
var instanceType = typeof(TestObject);
var propertyName = nameof(TestObject.TestProperty);

var propertyHelper = ObjectPropertyHelper.Create(instanceType);

var setterDelegate = propertyHelper.Setter(propertyName);

var getterDelegate = propertyHelper.Getter(propertyName);

setterDelegate(instance, "Test Name");

Console.WriteLine($"-------------- Getter : {getterDelegate(instance)}");

var methodHelper = ObjectMethodHelper.Create(instanceType);

var doDelegate = methodHelper.GetParameterlessInvoker("Do");

Console.WriteLine($"Parameterless Invoker : {doDelegate(instance)}");

var do1Delegate = methodHelper.GetOneParameterInvoker("Do", typeof(string));

Console.WriteLine($"One Parameter Invoker : {do1Delegate(instance, "key")}");

var do2Delegate = methodHelper.GetTwoParameterInvoker("Do", typeof(string), typeof(int));

Console.WriteLine($"Two Parameter Invoker : {do2Delegate(instance, "key", 1)}");