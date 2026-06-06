namespace CoreForge.Application.Common.Exceptions;

public abstract class AppException : Exception
{
    public string ResourceKey { get; }
    public int StatusCode { get; }

    protected AppException(string resourceKey, int statusCode)
        : base(resourceKey)
    {
        ResourceKey = resourceKey;
        StatusCode = statusCode;
    }
}
