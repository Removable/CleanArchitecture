using CleanArchitecture.Application.Features.TodoItems.Commands.UpdateTodoItem;
using FluentValidation.TestHelper;

namespace CleanArchitecture.UnitTests.Application.Validators;

public class UpdateTodoItemCommandValidatorTests
{
    private readonly UpdateTodoItemCommandValidator _validator;

    public UpdateTodoItemCommandValidatorTests()
    {
        _validator = new UpdateTodoItemCommandValidator();
    }

    [Fact]
    public async Task Validate_WithValidTitle_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateTodoItemCommand
        {
            TodoListId = Guid.NewGuid(),
            TodoItemId = Guid.NewGuid(),
            Title = "Valid Title",
            Done = false
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
        var command = new UpdateTodoItemCommand
        {
            TodoListId = Guid.NewGuid(),
            TodoItemId = Guid.NewGuid(),
            Title = string.Empty,
            Done = false
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
        var command = new UpdateTodoItemCommand
        {
            TodoListId = Guid.NewGuid(),
            TodoItemId = Guid.NewGuid(),
            Title = null!,
            Done = false
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
        var command = new UpdateTodoItemCommand
        {
            TodoListId = Guid.NewGuid(),
            TodoItemId = Guid.NewGuid(),
            Title = new string('a', 201),
            Done = false
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
        var command = new UpdateTodoItemCommand
        {
            TodoListId = Guid.NewGuid(),
            TodoItemId = Guid.NewGuid(),
            Title = new string('a', 200),
            Done = false
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }
}
