using CleanArchitecture.Application.Features.TodoItems.Commands.CreateTodoItem;
using FluentValidation.TestHelper;

namespace CleanArchitecture.UnitTests.Application.Validators;

public class CreateTodoItemCommandValidatorTests
{
    private readonly CreateTodoItemCommandValidator _validator;

    public CreateTodoItemCommandValidatorTests()
    {
        _validator = new CreateTodoItemCommandValidator();
    }

    [Fact]
    public async Task Validate_WithValidTitle_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateTodoItemCommand
        {
            ListId = Guid.NewGuid(),
            Title = "Valid Title"
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task Validate_WithEmptyTitle_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateTodoItemCommand
        {
            ListId = Guid.NewGuid(),
            Title = string.Empty
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task Validate_WithNullTitle_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateTodoItemCommand
        {
            ListId = Guid.NewGuid(),
            Title = null
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task Validate_WithTitleExceeding200Characters_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateTodoItemCommand
        {
            ListId = Guid.NewGuid(),
            Title = new string('a', 201)
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task Validate_WithTitleExactly200Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateTodoItemCommand
        {
            ListId = Guid.NewGuid(),
            Title = new string('a', 200)
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }
}
