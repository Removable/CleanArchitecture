using Ardalis.Result;
using CleanArchitecture.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Infrastructure.Identity;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
    IAuthorizationService authorizationService)
    : IIdentityService
{
    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);

        return user?.UserName;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new ApplicationUser { UserName = userName, Email = userName };

        var result = await userManager.CreateAsync(user, password).ConfigureAwait(false);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);

        return user != null && await userManager.IsInRoleAsync(user, role).ConfigureAwait(false);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);

        if (user == null)
        {
            return false;
        }

        var principal = await userClaimsPrincipalFactory.CreateAsync(user).ConfigureAwait(false);

        var result = await authorizationService.AuthorizeAsync(principal, policyName).ConfigureAwait(false);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);

        return user != null ? await DeleteUserAsync(user).ConfigureAwait(false) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await userManager.DeleteAsync(user).ConfigureAwait(false);

        return result.ToApplicationResult();
    }
}
