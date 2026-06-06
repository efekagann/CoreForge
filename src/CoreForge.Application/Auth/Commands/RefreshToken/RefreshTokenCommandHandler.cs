using CoreForge.Application.Auth.DTOs;
using CoreForge.Application.Common.Exceptions;
using CoreForge.Application.Common.Interfaces;
using CoreForge.Application.Common.Localization;
using MediatR;

namespace CoreForge.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    IRefreshTokenStore refreshTokenStore,
    IIdentityService identityService,
    IJwtTokenGenerator jwtTokenGenerator
) : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var userId = await refreshTokenStore.GetUserIdByTokenAsync(request.Token, cancellationToken);

        if (userId is null)
            throw new UnauthorizedException(ResourceKeys.Auth.InvalidToken);

        var details = await identityService.GetUserDetailsAsync(userId.Value);

        if (details is null)
            throw new NotFoundException(ResourceKeys.Auth.UserNotFound);

        await refreshTokenStore.RevokeAsync(request.Token, cancellationToken);

        var (email, roles) = details.Value;
        var accessToken = jwtTokenGenerator.GenerateAccessToken(userId.Value, email, roles);
        var newRefreshToken = await refreshTokenStore.CreateRefreshTokenAsync(userId.Value, cancellationToken);

        return new AuthResponseDto(accessToken, newRefreshToken, jwtTokenGenerator.GetAccessTokenExpiry(), email, roles);
    }
}
