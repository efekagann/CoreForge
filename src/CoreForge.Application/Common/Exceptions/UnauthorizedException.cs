namespace CoreForge.Application.Common.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string resourceKey) : base(resourceKey, 401) { }
}
