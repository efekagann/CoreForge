using CoreForge.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace CoreForge.Infrastructure.Storage;

public class LocalStorageService(IOptions<StorageSettings> options) : IStorageService
{
    private readonly string _basePath = Path.Combine(
        AppContext.BaseDirectory, options.Value.Local.BasePath);

    public async Task<string> UploadAsync(string fileName, Stream content, string contentType, CancellationToken ct = default)
    {
        Directory.CreateDirectory(_basePath);
        var fileKey = $"{Guid.NewGuid():N}_{fileName}";
        var fullPath = Path.Combine(_basePath, fileKey);

        using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, ct);

        return fileKey;
    }

    public Task<Stream?> DownloadAsync(string fileKey, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, fileKey);
        if (!File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);

        return Task.FromResult<Stream?>(File.OpenRead(fullPath));
    }

    public Task DeleteAsync(string fileKey, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, fileKey);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public string GetPublicUrl(string fileKey) => $"/uploads/{fileKey}";
}
