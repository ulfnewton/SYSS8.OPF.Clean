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
                // 2. Dessa två claims använder JWT‑standardens namn.
                //    Andra system som läser tokenen förväntar sig just "sub" och "email".
                //    'sub' = användarens unika ID.
                //    'email' = användarens e‑postadress.
                new(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                
                // 3. Dessa två claims använder ASP.NET Cores egna namn.
                //    ClaimTypes.NameIdentifier och ClaimTypes.Name gör att
                //    User.Identity.Name och liknande funktioner fungerar automatiskt.
                //    Det gör livet enklare när vi ska läsa ut info om användaren i API:t.
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name,           user.Email ?? "")
            };

            // 4. Lägg till alla roller som claims.
            //    Varje roll blir en egen claim med typen ClaimTypes.Role.
            //    Detta gör att ASP.NET Core kan hantera [Authorize(Roles="Admin")] osv.
            claims.AddRange(roles.Select(
                role => new Claim(ClaimTypes.Role, role)));

            // 5. Skapa en säkerhetsnyckel baserat på vår hemliga nyckel i konfigurationen.
            //    Den används för att signera tokenen så att ingen kan ändra den i efterhand.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

            // 6. Bestäm vilken algoritm vi signerar tokenen med (HMAC SHA256 är standard).
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 7. Bygg själva JWT‑tokenen.
            //    - issuer: vem som skapat tokenen
            //    - audience: vem tokenen är avsedd för
            //    - claims: all info vi stoppade in ovan
            //    - notBefore: tokenen gäller från och med nu
            //    - expires: tokenen slutar gälla om 8 timmar
            //    - signingCredentials: hur tokenen signeras
            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

            // 8. Konvertera token‑objektet till en kompakt sträng (den som klienten får).
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
