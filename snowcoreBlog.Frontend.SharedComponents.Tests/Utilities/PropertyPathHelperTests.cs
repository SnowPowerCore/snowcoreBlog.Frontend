using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using snowcoreBlog.Frontend.SharedComponents.Utilities;
using System.Reflection;

namespace snowcoreBlog.Frontend.SharedComponents.Tests.Utilities;

public class PropertyPathHelperTests
{
    private class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public NestedModel Nested { get; set; } = new();
        public List<ItemModel> Items { get; set; } = new();
    }

    private class NestedModel
    {
        public string Description { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    private class ItemModel
    {
        public string Title { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    [Fact]
    public void ToFluentPropertyPath_WithSimpleProperty_ShouldReturnPropertyName()
    {
        // Arrange
        var model = new TestModel { Name = "Test" };
        var editContext = new EditContext(model);
        var fieldIdentifier = new FieldIdentifier(model, nameof(TestModel.Name));

        // Act
        var result = PropertyPathHelperAccessor.ToFluentPropertyPath(editContext, fieldIdentifier);

        // Assert
        result.Should().Be("Name");
    }

    [Fact]
    public void ToFluentPropertyPath_WithNestedProperty_ShouldReturnDottedPath()
    {
        // Arrange
        var model = new TestModel();
        var editContext = new EditContext(model);
        var fieldIdentifier = new FieldIdentifier(model.Nested, nameof(NestedModel.Description));

        // Act
        var result = PropertyPathHelperAccessor.ToFluentPropertyPath(editContext, fieldIdentifier);

        // Assert
        result.Should().Be("Nested.Description");
    }

    [Fact]
    public void ToFluentPropertyPath_WithArrayProperty_ShouldReturnIndexedPath()
    {
        // Arrange
        var model = new TestModel
        {
            Items = new List<ItemModel>
            {
                new() { Title = "First" },
                new() { Title = "Second" }
            }
        };
        var editContext = new EditContext(model);
        var fieldIdentifier = new FieldIdentifier(model.Items[1], nameof(ItemModel.Title));

        // Act
        var result = PropertyPathHelperAccessor.ToFluentPropertyPath(editContext, fieldIdentifier);

        // Assert
        result.Should().Be("Items[1].Title");
    }

    [Fact]
    public void ToFluentPropertyPath_WithFirstArrayItem_ShouldReturnZeroIndex()
    {
        // Arrange
        var model = new TestModel
        {
            Items = new List<ItemModel>
            {
                new() { Title = "First" }
            }
        };
        var editContext = new EditContext(model);
        var fieldIdentifier = new FieldIdentifier(model.Items[0], nameof(ItemModel.Title));

        // Act
        var result = PropertyPathHelperAccessor.ToFluentPropertyPath(editContext, fieldIdentifier);

        // Assert
        result.Should().Be("Items[0].Title");
    }
}

// Helper to access internal static class
internal static class PropertyPathHelperAccessor
{
    private static readonly Type HelperType;
    private static readonly MethodInfo ToFluentPropertyPathMethod;

    static PropertyPathHelperAccessor()
    {
        var assembly = typeof(snowcoreBlog.Frontend.SharedComponents.Validation.FormFluentValidationValidator).Assembly;
        HelperType = assembly.GetType("snowcoreBlog.Frontend.SharedComponents.Utilities.PropertyPathHelper")!;
        ToFluentPropertyPathMethod = HelperType.GetMethod("ToFluentPropertyPath", BindingFlags.Public | BindingFlags.Static)!;
    }

    public static string ToFluentPropertyPath(EditContext editContext, FieldIdentifier fieldIdentifier)
    {
        return (string)ToFluentPropertyPathMethod.Invoke(null, new object[] { editContext, fieldIdentifier })!;
    }
}
