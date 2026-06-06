using CoreForge.Application.Auth.DTOs;
using MediatR;

namespace CoreForge.Application.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string? FirstName,
    string? LastName
) : IRequest<AuthResponseDto>;
