using CoreForge.Application.Auth.DTOs;
using CoreForge.Application.Common.Exceptions;
using CoreForge.Application.Common.Interfaces;
using CoreForge.Application.Common.Localization;
using CoreForge.Domain.Common;
using MediatR;

namespace CoreForge.Application.Auth.Commands.Register;

public class RegisterCommandHandler(
    IIdentityService identityService,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenStore refreshTokenStore
) : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var (result, userId) = await identityService.CreateUserAsync(
            request.Email, request.Password, request.FirstName, request.LastName);

        if (!result.IsSuccess)
            throw new ConflictException(ResourceKeys.Auth.UserAlreadyExists);

        await identityService.AssignRoleAsync(userId, Roles.User);

        var roles = new List<string> { Roles.User };
        var accessToken = jwtTokenGenerator.GenerateAccessToken(userId, request.Email, roles);
        var refreshToken = await refreshTokenStore.CreateRefreshTokenAsync(userId, cancellationToken);

        return new AuthResponseDto(accessToken, refreshToken, jwtTokenGenerator.GetAccessTokenExpiry(), request.Email, roles);
    }
}
