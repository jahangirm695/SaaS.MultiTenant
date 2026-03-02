using MediatR;
using SaaS.Application.DTOs;
using SaaS.Application.DTOs.Auth;

namespace SaaS.Application.Commands.Auth;

public record RegisterCommand(RegisterRequest Request) : IRequest<ApiResponse<LoginResponse>>;
