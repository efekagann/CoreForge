using CoreForge.Domain.Common;

namespace CoreForge.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(Result Result, Guid UserId)> CreateUserAsync(string email, string password, string? firstName, string? lastName);
    Task<(bool Success, Guid UserId, IList<string> Roles)> ValidateCredentialsAsync(string email, string password);
    Task<(string Email, IList<string> Roles)?> GetUserDetailsAsync(Guid userId);
    Task<Result> AssignRoleAsync(Guid userId, string role);
    Task<bool> UserExistsAsync(string email);
}
