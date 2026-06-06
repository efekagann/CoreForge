namespace CoreForge.Application.Common.Interfaces;

public interface IStorageService
{
    Task<string> UploadAsync(string fileName, Stream content, string contentType, CancellationToken ct = default);
    Task<Stream?> DownloadAsync(string fileKey, CancellationToken ct = default);
    Task DeleteAsync(string fileKey, CancellationToken ct = default);
    string GetPublicUrl(string fileKey);
}
