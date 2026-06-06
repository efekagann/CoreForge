using CoreForge.Application.Common.Interfaces;
using MediatR;

namespace CoreForge.Application.Auth.Commands.Logout;

public class LogoutCommandHandler(IRefreshTokenStore refreshTokenStore)
    : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
        => await refreshTokenStore.RevokeAsync(request.RefreshToken, cancellationToken);
}
