using CleanArchitecture.Application.Common.Behaviours;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Security;
using Mediator;
using Microsoft.Extensions.Logging;
using Moq;

namespace CleanArchitecture.UnitTests.Application.Behaviours;

public class AuthorizationBehaviourTests
{
    private readonly Mock<IUser> _userMock;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<ILogger<AuthorizationBehaviour<TestAuthorizedRequest, string>>> _loggerMock;

    public AuthorizationBehaviourTests()
    {
        _userMock = new Mock<IUser>();
        _identityServiceMock = new Mock<IIdentityService>();
        _loggerMock = new Mock<ILogger<AuthorizationBehaviour<TestAuthorizedRequest, string>>>();
    }

    public record TestUnauthorizedRequest : IMessage
    {
        public string Name { get; init; } = string.Empty;
    }

    [Authorize]
    public record TestAuthorizedRequest : IMessage
    {
        public string Name { get; init; } = string.Empty;
    }

    [Authorize(Roles = new[] { "Admin" })]
    public record TestRoleAuthorizedRequest : IMessage
    {
        public string Name { get; init; } = string.Empty;
    }

    [Authorize(Policy = "TestPolicy")]
    public record TestPolicyAuthorizedRequest : IMessage
    {
        public string Name { get; init; } = string.Empty;
    }

    [Fact]
    public async Task Handle_WithoutAuthorizeAttribute_ShouldCallNext()
    {
        // Arrange
        var unauthorizedLoggerMock = new Mock<ILogger<AuthorizationBehaviour<TestUnauthorizedRequest, string>>>();
        var behaviour = new AuthorizationBehaviour<TestUnauthorizedRequest, string>(
            _userMock.Object,
            _identityServiceMock.Object,
            unauthorizedLoggerMock.Object);

        var request = new TestUnauthorizedRequest { Name = "Test" };
        var nextCalled = false;

        ValueTask<string> Next(TestUnauthorizedRequest req, CancellationToken ct)
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
    public async Task Handle_WithAuthorizeAttribute_AndNoUserId_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns((string?)null);

        var behaviour = new AuthorizationBehaviour<TestAuthorizedRequest, string>(
            _userMock.Object,
            _identityServiceMock.Object,
            _loggerMock.Object);

        var request = new TestAuthorizedRequest { Name = "Test" };

        ValueTask<string> Next(TestAuthorizedRequest req, CancellationToken ct)
        {
            return new ValueTask<string>("Success");
        }

        // Act & Assert
        await Should.ThrowAsync<UnauthorizedAccessException>(
            async () => await behaviour.Handle(request, Next, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithAuthorizeAttribute_AndValidUser_ShouldCallNext()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns("test-user-id");

        var behaviour = new AuthorizationBehaviour<TestAuthorizedRequest, string>(
            _userMock.Object,
            _identityServiceMock.Object,
            _loggerMock.Object);

        var request = new TestAuthorizedRequest { Name = "Test" };
        var nextCalled = false;

        ValueTask<string> Next(TestAuthorizedRequest req, CancellationToken ct)
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
    public async Task Handle_WithRoleRequirement_AndUserHasRole_ShouldCallNext()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns("test-user-id");
        _userMock.Setup(u => u.Roles).Returns(new[] { "Admin" });

        var roleLoggerMock = new Mock<ILogger<AuthorizationBehaviour<TestRoleAuthorizedRequest, string>>>();
        var behaviour = new AuthorizationBehaviour<TestRoleAuthorizedRequest, string>(
            _userMock.Object,
            _identityServiceMock.Object,
            roleLoggerMock.Object);

        var request = new TestRoleAuthorizedRequest { Name = "Test" };
        var nextCalled = false;

        ValueTask<string> Next(TestRoleAuthorizedRequest req, CancellationToken ct)
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
    public async Task Handle_WithRoleRequirement_AndUserDoesNotHaveRole_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns("test-user-id");
        _userMock.Setup(u => u.Roles).Returns(new[] { "User" });

        var roleLoggerMock = new Mock<ILogger<AuthorizationBehaviour<TestRoleAuthorizedRequest, string>>>();
        var behaviour = new AuthorizationBehaviour<TestRoleAuthorizedRequest, string>(
            _userMock.Object,
            _identityServiceMock.Object,
            roleLoggerMock.Object);

        var request = new TestRoleAuthorizedRequest { Name = "Test" };

        ValueTask<string> Next(TestRoleAuthorizedRequest req, CancellationToken ct)
        {
            return new ValueTask<string>("Success");
        }

        // Act & Assert
        await Should.ThrowAsync<ForbiddenAccessException>(
            async () => await behaviour.Handle(request, Next, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithPolicyRequirement_AndPolicyPasses_ShouldCallNext()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns("test-user-id");
        _identityServiceMock.Setup(s => s.AuthorizeAsync("test-user-id", "TestPolicy"))
            .ReturnsAsync(true);

        var policyLoggerMock = new Mock<ILogger<AuthorizationBehaviour<TestPolicyAuthorizedRequest, string>>>();
        var behaviour = new AuthorizationBehaviour<TestPolicyAuthorizedRequest, string>(
            _userMock.Object,
            _identityServiceMock.Object,
            policyLoggerMock.Object);

        var request = new TestPolicyAuthorizedRequest { Name = "Test" };
        var nextCalled = false;

        ValueTask<string> Next(TestPolicyAuthorizedRequest req, CancellationToken ct)
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
    public async Task Handle_WithPolicyRequirement_AndPolicyFails_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns("test-user-id");
        _identityServiceMock.Setup(s => s.AuthorizeAsync("test-user-id", "TestPolicy"))
            .ReturnsAsync(false);

        var policyLoggerMock = new Mock<ILogger<AuthorizationBehaviour<TestPolicyAuthorizedRequest, string>>>();
        var behaviour = new AuthorizationBehaviour<TestPolicyAuthorizedRequest, string>(
            _userMock.Object,
            _identityServiceMock.Object,
            policyLoggerMock.Object);

        var request = new TestPolicyAuthorizedRequest { Name = "Test" };

        ValueTask<string> Next(TestPolicyAuthorizedRequest req, CancellationToken ct)
        {
            return new ValueTask<string>("Success");
        }

        // Act & Assert
        await Should.ThrowAsync<ForbiddenAccessException>(
            async () => await behaviour.Handle(request, Next, CancellationToken.None));
    }
}
