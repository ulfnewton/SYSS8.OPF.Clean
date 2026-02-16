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
    // INFO: Issuer/Audience/Key läses från config för att hålla hemligheter utanför kodbasen.
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
        // DESIGN-VAL: Vi bygger claims explicit för att visa hur JWT-payloaden konstrueras.
        var claims = new List<Claim>
        {
            // INFO: "sub" och "email" följer JWT-standarden och gör tokenen interoperabel.
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            // INFO: ClaimTypes.Name/NameIdentifier gör att ASP.NET Core kan hitta användaren direkt.
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Email ?? "")
        };
        // TIPS: Roll-claims behövs för RequireAuthorization och [Authorize(Roles=...)]-flöden.
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        // OBS: Symmetrisk nyckel räcker i kursen och är enklare att förstå än certifikat.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
        // INFO: HMAC SHA256 är standardalgoritm för signering med symmetrisk nyckel.
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        // DESIGN-VAL: Tidsbegränsad token ger säkrare demo och tydligare AuthN-beteende.
        var token = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        // INFO: JwtSecurityTokenHandler skriver ut en kompakt string som klienten skickar vidare.
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}