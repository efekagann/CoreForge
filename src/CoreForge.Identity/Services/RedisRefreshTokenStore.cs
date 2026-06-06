using CoreForge.Application.Common.Interfaces;
using CoreForge.Identity.Settings;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace CoreForge.Identity.Services;

public class RedisRefreshTokenStore(IDistributedCache cache, IOptions<JwtSettings> options)
    : IRefreshTokenStore
{
    private readonly JwtSettings _settings = options.Value;
    private static string Key(string token) => $"rt:{token}";

    public async Task<string> CreateRefreshTokenAsync(Guid userId, CancellationToken ct = default)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        await cache.SetStringAsync(
            Key(token),
            userId.ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(_settings.RefreshTokenExpirationDays)
            },
            ct);

        return token;
    }

    public async Task<Guid?> GetUserIdByTokenAsync(string token, CancellationToken ct = default)
    {
        var value = await cache.GetStringAsync(Key(token), ct);
        return value is not null && Guid.TryParse(value, out var id) ? id : null;
    }

    public async Task RevokeAsync(string token, CancellationToken ct = default)
        => await cache.RemoveAsync(Key(token), ct);
}
