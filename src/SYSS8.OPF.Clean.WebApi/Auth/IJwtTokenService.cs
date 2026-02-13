using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SYSS8.OPF.Clean.Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SYSS8.OPF.Clean.WebApi.Auth
{
    public interface IJwtTokenService
    {
        // Skapa ett Jwt-token
        Task<string> CreateAsync(User user, IEnumerable<string> roles, CancellationToken ct = default);
    }
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _options;
        private readonly UserManager<User> _userManager;

        public JwtTokenService(IOptions<JwtOptions> options, UserManager<User> userManager)
        {
            _options = options.Value;
            _userManager = userManager;
        }

        public async Task<String> CreateAsync(User user, IEnumerable<string> roles, CancellationToken ct = default)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Name, user.Email ?? "")
            };
            claims.AddRange(roles.Select(
                role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
