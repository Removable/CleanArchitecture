using CleanArchitecture.Application.Common.Behaviours;
using Mediator;
using Microsoft.Extensions.Logging;
using Moq;

namespace CleanArchitecture.UnitTests.Application.Behaviours;

public class UnhandledExceptionBehaviourTests
{
    public record TestRequest : IMessage
    {
        public string Name { get; init; } = string.Empty;
    }

    [Fact]
    public async Task Handle_WithNoException_ShouldReturnResult()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TestRequest>>();
        var behaviour = new UnhandledExceptionBehaviour<TestRequest, string>(loggerMock.Object);
        var request = new TestRequest { Name = "Test" };

        ValueTask<string> Next(TestRequest req, CancellationToken ct)
        {
            return new ValueTask<string>("Success");
        }

        // Act
        var result = await behaviour.Handle(request, Next, CancellationToken.None);

        // Assert
        result.ShouldBe("Success");
    }

    [Fact]
    public async Task Handle_WithException_ShouldLogAndRethrow()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TestRequest>>();
        var behaviour = new UnhandledExceptionBehaviour<TestRequest, string>(loggerMock.Object);
        var request = new TestRequest { Name = "Test" };
        var expectedException = new InvalidOperationException("Test exception");

        ValueTask<string> Next(TestRequest req, CancellationToken ct)
        {
            throw expectedException;
        }

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await behaviour.Handle(request, Next, CancellationToken.None));

        exception.ShouldBe(expectedException);

        // Verify logging was called
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithCustomException_ShouldPreserveExceptionType()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TestRequest>>();
        var behaviour = new UnhandledExceptionBehaviour<TestRequest, string>(loggerMock.Object);
        var request = new TestRequest { Name = "Test" };

        ValueTask<string> Next(TestRequest req, CancellationToken ct)
        {
            throw new ArgumentNullException("testParam", "Argument was null");
        }

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await behaviour.Handle(request, Next, CancellationToken.None));

        exception.ParamName.ShouldBe("testParam");
    }
}
