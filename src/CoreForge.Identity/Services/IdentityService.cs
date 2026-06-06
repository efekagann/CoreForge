using CoreForge.Application.Common.Interfaces;
using CoreForge.Domain.Common;
using CoreForge.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace CoreForge.Identity.Services;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager
) : IIdentityService
{
    public async Task<(Result Result, Guid UserId)> CreateUserAsync(
        string email, string password, string? firstName, string? lastName)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await userManager.CreateAsync(user, password);

        return result.Succeeded
            ? (Result.Success(), user.Id)
            : (Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description))), Guid.Empty);
    }

    public async Task<(bool Success, Guid UserId, IList<string> Roles)> ValidateCredentialsAsync(
        string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !user.IsActive)
            return (false, Guid.Empty, []);

        var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return (false, Guid.Empty, []);

        var roles = await userManager.GetRolesAsync(user);
        return (true, user.Id, roles);
    }

    public async Task<(string Email, IList<string> Roles)?> GetUserDetailsAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return null;

        var roles = await userManager.GetRolesAsync(user);
        return (user.Email!, roles);
    }

    public async Task<Result> AssignRoleAsync(Guid userId, string role)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result.Failure("User not found.");

        var result = await userManager.AddToRoleAsync(user, role);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<bool> UserExistsAsync(string email)
        => await userManager.FindByEmailAsync(email) is not null;
}
