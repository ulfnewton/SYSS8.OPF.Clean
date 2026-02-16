using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SYSS8.OPF.Clean.Infrastructure;

namespace SYSS8.OPF.Clean.WebApi.Auth;

public static class AuthEndpoints
{
    // DESIGN-VAL: Request/response-records ligger nära endpoints för att göra undervisningen tydlig.
    public sealed record RegisterRequest(string Email, string Password, string Role);
    public sealed record LoginRequest(string Email, string Password);
    // INFO: Login returnerar Token + Email + Role så klienten kan visa användare och aktivera UI-regler.
    public sealed record LoginResponse(string Token, string Email, string Role);

    public static IEndpointRouteBuilder MapAuth(this IEndpointRouteBuilder app)
    {
        // DESIGN-VAL: Gruppar auth-endpoints för tydlig routing och enklare Swagger-navigering.
        var group = app.MapGroup("/auth").WithTags("Auth");
        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
        // TIPS: "me"-endpointen verifierar att JWT valideras och låter klienten visa användarinfo.
        group.MapGet("/me", Me).RequireAuthorization();
        return app;
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterRequest req,
        UserManager<User> userMgr,
        RoleManager<Role> roleMgr)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return Results.BadRequest(new ProblemDetails { Title = "Ogiltiga fält", Detail = "Email/lösenord saknas", Status = 400 });

        if (!await roleMgr.RoleExistsAsync(req.Role))
            return Results.BadRequest(new ProblemDetails { Title = "Okänd roll", Detail = req.Role, Status = 400 });

        var user = new User { UserName = req.Email, Email = req.Email, EmailConfirmed = true };
        var create = await userMgr.CreateAsync(user, req.Password);
        if (!create.Succeeded)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Registrering misslyckades",
                Detail = string.Join(", ", create.Errors.Select(e => e.Description)),
                Status = 400
            });
        }
        await userMgr.AddToRoleAsync(user, req.Role);
        return Results.Ok();
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequest req,
        UserManager<User> userMgr,
        IJwtTokenService tokens)
    {
        // INFO: Minimal API + Identity ger ett rakt loginflöde med tydliga 401-responser.
        var user = await userMgr.FindByEmailAsync(req.Email);
        if (user is null || !await userMgr.CheckPasswordAsync(user, req.Password))
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Fel inloggning", 
                Detail = "E‑post eller lösenord stämmer inte", 
                Status = 401
            });
        }
        var roles = await userMgr.GetRolesAsync(user);
        var token = await tokens.CreateAsync(user, roles);
        // OBS: Vi returnerar en roll för att UI ska spegla serverns verkliga behörighet.
        return Results.Ok(new LoginResponse(token, user.Email ?? "", roles.FirstOrDefault() ?? ""));
    }

    private static IResult Me(ClaimsPrincipal user)
    {
        // TIPS: Identity sätter ClaimTypes.Name när token skapas, vilket gör Name tillgängligt här.
        var email = user.Identity?.Name ?? "";
        // INFO: ClaimTypes.Role används av ASP.NET Core för rollbaserad auktorisering.
        var role = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "";
        return Results.Ok(new { email, role });
    }
}