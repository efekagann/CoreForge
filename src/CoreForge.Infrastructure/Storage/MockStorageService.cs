using CoreForge.Application.Common.Interfaces;

namespace CoreForge.Infrastructure.Storage;

public class MockStorageService : IStorageService
{
    private readonly Dictionary<string, byte[]> _store = new();

    public async Task<string> UploadAsync(string fileName, Stream content, string contentType, CancellationToken ct = default)
    {
        var fileKey = $"mock_{Guid.NewGuid():N}_{fileName}";
        using var ms = new MemoryStream();
        await content.CopyToAsync(ms, ct);
        _store[fileKey] = ms.ToArray();
        return fileKey;
    }

    public Task<Stream?> DownloadAsync(string fileKey, CancellationToken ct = default)
    {
        if (!_store.TryGetValue(fileKey, out var data))
            return Task.FromResult<Stream?>(null);
        return Task.FromResult<Stream?>(new MemoryStream(data));
    }

    public Task DeleteAsync(string fileKey, CancellationToken ct = default)
    {
        _store.Remove(fileKey);
        return Task.CompletedTask;
    }

    public string GetPublicUrl(string fileKey) => $"/mock-storage/{fileKey}";
}
