namespace CoreForge.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(Guid userId, string email, IList<string> roles);
    DateTime GetAccessTokenExpiry();
}
