using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SYSS8.OPF.Clean.Infrastructure;

namespace SYSS8.OPF.Clean.WebApi.Auth;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string Key { get; set; } = "";
}

public interface IJwtTokenService
{
    Task<string> CreateAsync(User user, IEnumerable<string> roles, CancellationToken ct = default);
}

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _opt;
    private readonly UserManager<User> _userManager;

    public JwtTokenService(IOptions<JwtOptions> opt, UserManager<User> userManager)
    {
        _opt = opt.Value;
        _userManager = userManager;
    }

    public async Task<string> CreateAsync(User user, IEnumerable<string> roles, CancellationToken ct = default)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Email ?? "")
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}