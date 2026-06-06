namespace CoreForge.Application.Common.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string resourceKey) : base(resourceKey, 404) { }
}
