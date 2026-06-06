using CoreForge.Application.Auth.DTOs;
using MediatR;

namespace CoreForge.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string Token) : IRequest<AuthResponseDto>;
