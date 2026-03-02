using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.DTOs;
using SaaS.Application.DTOs.Auth;
using SaaS.Application.Interfaces;

namespace SaaS.Application.Commands.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IAuthService authService,
        ILogger<LoginCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _logger = logger;
    }

    public async Task<ApiResponse<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var req = request.Request;

            var users = await _unitOfWork.Users.FindAsync(u => u.Email == req.Email, cancellationToken);
            var user = users.FirstOrDefault();

            if (user == null || !_authService.VerifyPassword(req.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for email: {Email}", req.Email);
                return ApiResponse<LoginResponse>.FailureResponse(
                    "Invalid credentials",
                    new List<string> { "Email or password is incorrect" });
            }

            if (!user.IsActive)
            {
                return ApiResponse<LoginResponse>.FailureResponse(
                    "Account inactive",
                    new List<string> { "Your account has been deactivated" });
            }

            // Check tenant status
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(user.TenantId, cancellationToken);
            if (tenant == null || !tenant.IsActive)
            {
                return ApiResponse<LoginResponse>.FailureResponse(
                    "Account inactive",
                    new List<string> { "Your organization account is inactive" });
            }

            // Generate tokens
            var token = _authService.GenerateJwtToken(user);
            var refreshToken = _authService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            user.LastLoginAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User logged in: {Email}", user.Email);

            var response = new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Role = user.Role,
                    TenantId = user.TenantId,
                    TenantName = tenant.Name
                }
            };

            return ApiResponse<LoginResponse>.SuccessResponse(response, "Login successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return ApiResponse<LoginResponse>.FailureResponse(
                "Login failed",
                new List<string> { ex.Message });
        }
    }
}
