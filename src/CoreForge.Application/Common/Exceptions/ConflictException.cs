namespace CoreForge.Application.Common.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string resourceKey) : base(resourceKey, 409) { }
}
