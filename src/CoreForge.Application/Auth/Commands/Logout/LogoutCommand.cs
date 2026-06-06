using MediatR;

namespace CoreForge.Application.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest;
