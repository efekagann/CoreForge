namespace CoreForge.Application.Common.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(string resourceKey) : base(resourceKey, 403) { }
}
