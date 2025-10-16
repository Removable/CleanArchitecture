using System.Security.Claims;
using CleanArchitecture.Application.Common.Interfaces;

namespace CleanArchitecture.Web.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
    public string? Id => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public string[]? Roles =>
        httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray();
}
