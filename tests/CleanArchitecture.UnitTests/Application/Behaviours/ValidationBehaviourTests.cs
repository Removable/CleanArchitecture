using CleanArchitecture.Application.Common.Behaviours;
using FluentValidation;
using FluentValidation.Results;
using Mediator;
using Moq;
using AppValidationException = CleanArchitecture.Application.Common.Exceptions.ValidationException;

namespace CleanArchitecture.UnitTests.Application.Behaviours;

public class ValidationBehaviourTests
{
    public record TestRequest : IMessage
    {
        public string Name { get; init; } = string.Empty;
    }

    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallNext()
    {
        // Arrange
        var behaviour = new ValidationBehaviour<TestRequest, string>(null);
        var request = new TestRequest { Name = "Test" };
        var nextCalled = false;

        ValueTask<string> Next(TestRequest req, CancellationToken ct)
        {
            nextCalled = true;
            return new ValueTask<string>("Success");
        }

        // Act
        var result = await behaviour.Handle(request, Next, CancellationToken.None);

        // Assert
        nextCalled.ShouldBeTrue();
        result.ShouldBe("Success");
    }

    [Fact]
    public async Task Handle_WithEmptyValidators_ShouldCallNext()
    {
        // Arrange
        var validators = Array.Empty<IValidator<TestRequest>>();
        var behaviour = new ValidationBehaviour<TestRequest, string>(validators);
        var request = new TestRequest { Name = "Test" };
        var nextCalled = false;

        ValueTask<string> Next(TestRequest req, CancellationToken ct)
        {
            nextCalled = true;
            return new ValueTask<string>("Success");
        }

        // Act
        var result = await behaviour.Handle(request, Next, CancellationToken.None);

        // Assert
        nextCalled.ShouldBeTrue();
        result.ShouldBe("Success");
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCallNext()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var validators = new[] { validatorMock.Object };
        var behaviour = new ValidationBehaviour<TestRequest, string>(validators);
        var request = new TestRequest { Name = "Test" };
        var nextCalled = false;

        ValueTask<string> Next(TestRequest req, CancellationToken ct)
        {
            nextCalled = true;
            return new ValueTask<string>("Success");
        }

        // Act
        var result = await behaviour.Handle(request, Next, CancellationToken.None);

        // Assert
        nextCalled.ShouldBeTrue();
        result.ShouldBe("Success");
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<TestRequest>>();
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required")
        };
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        var validators = new[] { validatorMock.Object };
        var behaviour = new ValidationBehaviour<TestRequest, string>(validators);
        var request = new TestRequest { Name = string.Empty };

        ValueTask<string> Next(TestRequest req, CancellationToken ct)
        {
            return new ValueTask<string>("Success");
        }

        // Act & Assert
        await Should.ThrowAsync<AppValidationException>(
            async () => await behaviour.Handle(request, Next, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_ShouldAggregateFailures()
    {
        // Arrange
        var validator1Mock = new Mock<IValidator<TestRequest>>();
        validator1Mock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Error 1") }));

        var validator2Mock = new Mock<IValidator<TestRequest>>();
        validator2Mock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Error 2") }));

        var validators = new[] { validator1Mock.Object, validator2Mock.Object };
        var behaviour = new ValidationBehaviour<TestRequest, string>(validators);
        var request = new TestRequest { Name = string.Empty };

        ValueTask<string> Next(TestRequest req, CancellationToken ct)
        {
            return new ValueTask<string>("Success");
        }

        // Act & Assert
        var exception = await Should.ThrowAsync<AppValidationException>(
            async () => await behaviour.Handle(request, Next, CancellationToken.None));

        exception.Errors.ShouldNotBeNull();
        exception.Errors.Count.ShouldBe(1); // Grouped by property name
        exception.Errors["Name"].Length.ShouldBe(2);
    }
}
