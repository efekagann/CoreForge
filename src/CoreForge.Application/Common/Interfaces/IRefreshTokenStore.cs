namespace CoreForge.Application.Common.Interfaces;

public interface IRefreshTokenStore
{
    Task<string> CreateRefreshTokenAsync(Guid userId, CancellationToken ct = default);
    Task<Guid?> GetUserIdByTokenAsync(string token, CancellationToken ct = default);
    Task RevokeAsync(string token, CancellationToken ct = default);
}
