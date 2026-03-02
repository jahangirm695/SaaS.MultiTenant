using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.DTOs;
using SaaS.Application.DTOs.Auth;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace SaaS.Application.Commands.Auth;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<LoginResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IAuthService authService,
        ILogger<RegisterCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _logger = logger;
    }

    public async Task<ApiResponse<LoginResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var req = request.Request;

            // Check if email already exists
            var existingUser = await _unitOfWork.Users.FindAsync(u => u.Email == req.Email, cancellationToken);
            if (existingUser.Any())
            {
                return ApiResponse<LoginResponse>.FailureResponse(
                    "Email already registered",
                    new List<string> { "A user with this email already exists" });
            }

            // Check subdomain availability directly using repository
            var existingTenant = await _unitOfWork.Tenants.FindAsync(
                t => t.Subdomain.ToLower() == req.Subdomain.ToLower(),
                cancellationToken);

            if (existingTenant.Any())
            {
                return ApiResponse<LoginResponse>.FailureResponse(
                    "Subdomain not available",
                    new List<string> { "This subdomain is already taken" });
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Create tenant
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = req.TenantName,
                Subdomain = req.Subdomain.ToLower(),
                IsActive = true,
                ContactEmail = req.Email,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Tenants.AddAsync(tenant, cancellationToken);

            // Create admin user
            var user = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Email = req.Email.ToLower(),
                PasswordHash = _authService.HashPassword(req.Password),
                FirstName = req.FirstName,
                LastName = req.LastName,
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user, cancellationToken);

            // Assign Free plan by default
            var freePlan = (await _unitOfWork.SubscriptionPlans.FindAsync(
                p => p.PlanType == PlanType.Free && p.IsActive, cancellationToken)).FirstOrDefault();

            if (freePlan != null)
            {
                var subscription = new TenantSubscription
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    PlanId = freePlan.Id,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10), // Free plan doesn't expire
                    IsActive = true,
                    AutoRenew = true,
                    AmountPaid = 0,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.TenantSubscriptions.AddAsync(subscription, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("New tenant registered: {TenantName} ({Subdomain})", tenant.Name, tenant.Subdomain);

            // Generate tokens
            var token = _authService.GenerateJwtToken(user);
            var refreshToken = _authService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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

            return ApiResponse<LoginResponse>.SuccessResponse(response, "Registration successful");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error during registration");
            return ApiResponse<LoginResponse>.FailureResponse(
                "Registration failed",
                new List<string> { ex.Message });
        }
    }
}