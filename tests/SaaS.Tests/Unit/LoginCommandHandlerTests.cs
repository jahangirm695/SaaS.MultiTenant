using FluentAssertions;
using Moq;
using SaaS.Application.Commands.Auth;
using SaaS.Application.DTOs.Auth;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Xunit;

namespace SaaS.Tests.Unit.Application.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _authServiceMock = new Mock<IAuthService>();
        _handler = new LoginCommandHandler(
            _unitOfWorkMock.Object,
            _authServiceMock.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<LoginCommandHandler>>());
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccessResponse()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hashedpassword",
            FirstName = "John",
            LastName = "Doe",
            Role = UserRole.Admin,
            IsActive = true,
            TenantId = Guid.NewGuid()
        };

        var tenant = new Tenant
        {
            Id = user.TenantId,
            Name = "Test Company",
            IsActive = true
        };

        var mockRepo = new Mock<IRepository<User>>();
        mockRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), default))
            .ReturnsAsync(new[] { user });

        _unitOfWorkMock.Setup(u => u.Users).Returns(mockRepo.Object);
        _unitOfWorkMock.Setup(u => u.Tenants.GetByIdAsync(user.TenantId, default))
            .ReturnsAsync(tenant);

        _authServiceMock.Setup(a => a.VerifyPassword(password, user.PasswordHash))
            .Returns(true);
        _authServiceMock.Setup(a => a.GenerateJwtToken(user))
            .Returns("fake-jwt-token");
        _authServiceMock.Setup(a => a.GenerateRefreshToken())
            .Returns("fake-refresh-token");

        var command = new LoginCommand(new LoginRequest
        {
            Email = email,
            Password = password
        });

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().NotBeNullOrEmpty();
        result.Data.User.Email.Should().Be(email);
    }

    [Fact]
    public async Task Handle_InvalidCredentials_ReturnsFailureResponse()
    {
        // Arrange
        var mockRepo = new Mock<IRepository<User>>();
        mockRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), default))
            .ReturnsAsync(Array.Empty<User>());

        _unitOfWorkMock.Setup(u => u.Users).Returns(mockRepo.Object);

        var command = new LoginCommand(new LoginRequest
        {
            Email = "invalid@example.com",
            Password = "wrongpassword"
        });

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
    }
}
