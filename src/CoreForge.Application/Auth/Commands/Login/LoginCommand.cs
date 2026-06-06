using CoreForge.Application.Auth.DTOs;
using MediatR;

namespace CoreForge.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;
