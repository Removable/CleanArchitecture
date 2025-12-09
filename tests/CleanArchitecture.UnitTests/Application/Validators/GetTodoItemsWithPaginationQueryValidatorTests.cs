using CleanArchitecture.Application.Features.TodoItems.Queries.GetTodoItemsWithPagination;
using FluentValidation.TestHelper;

namespace CleanArchitecture.UnitTests.Application.Validators;

public class GetTodoItemsWithPaginationQueryValidatorTests
{
    private readonly GetTodoItemsWithPaginationQueryValidator _validator;

    public GetTodoItemsWithPaginationQueryValidatorTests()
    {
        _validator = new GetTodoItemsWithPaginationQueryValidator();
    }

    [Fact]
    public async Task Validate_WithValidQuery_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var query = new GetTodoItemsWithPaginationQuery
        {
            ListId = Guid.NewGuid(),
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithEmptyListId_ShouldHaveValidationError()
    {
        // Arrange
        var query = new GetTodoItemsWithPaginationQuery
        {
            ListId = Guid.Empty,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ListId)
            .WithErrorMessage("ListId is required.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validate_WithInvalidPageNumber_ShouldHaveValidationError(int pageNumber)
    {
        // Arrange
        var query = new GetTodoItemsWithPaginationQuery
        {
            ListId = Guid.NewGuid(),
            PageNumber = pageNumber,
            PageSize = 10
        };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageNumber)
            .WithErrorMessage("PageNumber at least greater than or equal to 1.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validate_WithInvalidPageSize_ShouldHaveValidationError(int pageSize)
    {
        // Arrange
        var query = new GetTodoItemsWithPaginationQuery
        {
            ListId = Guid.NewGuid(),
            PageNumber = 1,
            PageSize = pageSize
        };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("PageSize at least greater than or equal to 1.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task Validate_WithValidPageNumber_ShouldNotHaveValidationError(int pageNumber)
    {
        // Arrange
        var query = new GetTodoItemsWithPaginationQuery
        {
            ListId = Guid.NewGuid(),
            PageNumber = pageNumber,
            PageSize = 10
        };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PageNumber);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task Validate_WithValidPageSize_ShouldNotHaveValidationError(int pageSize)
    {
        // Arrange
        var query = new GetTodoItemsWithPaginationQuery
        {
            ListId = Guid.NewGuid(),
            PageNumber = 1,
            PageSize = pageSize
        };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }
}
