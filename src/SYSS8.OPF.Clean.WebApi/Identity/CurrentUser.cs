using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SYSS8.OPF.Clean.Application;

namespace SYSS8.OPF.Clean.WebApi.Identity;

// Vi använder oss av IHttpContextAccessor för att få tillgång till JWT
// och därmed kan vi koppla current user med Identity
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;

    public CurrentUser(IHttpContextAccessor http) => _http = http;

    public Guid? UserId => Guid.TryParse(
        _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier),
        out var id) ? id : null;

    public string? Email => _http.HttpContext?.User.FindFirstValue(ClaimTypes.Email);

    public IReadOnlyList<string> Roles =>
        _http.HttpContext?.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
        ?? new List<string>();
}