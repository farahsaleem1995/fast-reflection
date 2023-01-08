using FastReflection.Delegates;
using FastReflection.Helpers;
using FluentAssertions;

namespace FastReflection.UnitTests.Helpers;

public class PropertyHelperTests
{
	[Theory]
	[InlineData("InvalidPropertyName")]
	[InlineData(nameof(TestObject.PrivetGetterTestProperty))]
	public void Can_detect_invalid_property_getter(string propertyName)
	{
		// Arrange
		var declaringType = typeof(TestObject);

		// Act
		var act = () =>
		{
			var helper = new PropertyHelper(declaringType, propertyName);
			_ = helper.Getter;
		};

		// Assert
		var message = $"Property '{propertyName}' of type'{declaringType}' could not be found, or it's not public or declared, or has no public Getter.";
		act.Should().Throw<InvalidOperationException>()
			.WithMessage(message);
	}

	[Theory]
	[InlineData("InvalidPropertyName")]
	[InlineData(nameof(TestObject.PrivetSetterTestProperty))]
	public void Can_detect_invalid_property_setter(string propertyName)
	{
		// Arrange
		var declaringType = typeof(TestObject);

		// Act
		var act = () =>
		{
			var helper = new PropertyHelper(declaringType, propertyName);
			_ = helper.Setter;
		};

		// Assert
		var message = $"Property '{propertyName}' of type'{declaringType}' could not be found, or it's not public or declared, or has no public Setter.";
		act.Should().Throw<InvalidOperationException>()
			.WithMessage(message);
	}

	[Fact]
	public void Make_valid_property_getter()
	{
		// Arrange
		var declaringType = typeof(TestObject);
		var propertyName = nameof(TestObject.TestProperty);
		var propertyValue = "Valid Value";
		var instance = new TestObject
		{
			TestProperty = propertyValue
		};

		// Act
		var helper = new PropertyHelper(declaringType, propertyName);
		var getter = helper.Getter;
		var getResult = getter(instance);

		// Assert
		getResult.Should().Be(propertyValue);
	}

	[Fact]
	public void Make_valid_property_setter()
	{
		// Arrange
		var declaringType = typeof(TestObject);
		var propertyName = nameof(TestObject.TestProperty);
		var propertyValue = "Valid Value";
		var instance = new TestObject();

		// Act
		var helper = new PropertyHelper(declaringType, propertyName);
		var setter = helper.Setter;
		setter(instance, propertyValue);

		// Assert
		instance.TestProperty.Should().Be(propertyValue);
	}
}