using CoreForge.Application.Auth.DTOs;
using CoreForge.Application.Common.Exceptions;
using CoreForge.Application.Common.Interfaces;
using CoreForge.Application.Common.Localization;
using MediatR;

namespace CoreForge.Application.Auth.Commands.Login;

public class LoginCommandHandler(
    IIdentityService identityService,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenStore refreshTokenStore
) : IRequestHandler<LoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (success, userId, roles) = await identityService.ValidateCredentialsAsync(request.Email, request.Password);

        if (!success)
            throw new UnauthorizedException(ResourceKeys.Auth.InvalidCredentials);

        var accessToken = jwtTokenGenerator.GenerateAccessToken(userId, request.Email, roles);
        var refreshToken = await refreshTokenStore.CreateRefreshTokenAsync(userId, cancellationToken);

        return new AuthResponseDto(accessToken, refreshToken, jwtTokenGenerator.GetAccessTokenExpiry(), request.Email, roles);
    }
}
