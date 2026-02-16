using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using SYSS8.OPF.Clean.Infrastructure;
using SYSS8.OPF.Clean.WebApi.Auth;

using System.Security.Claims;

namespace SYSS8.OPF.Clean.WebApi.Endpoints;

// FIX: Flyttade MapAuthEndpoints till MapEndpointsExtensions.cs för att centralt hantera
// all registrering av endpoints på ett ställe, och för att hålla Auth-relaterade endpoints
// samlade i samma klass.
// DESIGN-VAL: Namnbyte AuthEndpoints → AuthenticationEndpoints för att spegla fokus (autentisering).
public static class AuthenticationEndpoints
{
    // Design-val: Valet att placera dessa record-typer inuti AuthenticationEndpoints-klassen
    // är för att de är specifika för dessa endpoints och inte används någon annanstans i
    // applikationen, vilket hjälper till att hålla koden organiserad och tydlig.
    // Design-val: Valet att placera dessa record-typer inuti AuthenticationEndpoints-klassen
    // är för att de är specifika för dessa endpoints och inte används någon annanstans i
    // applikationen, vilket hjälper till att hålla koden organiserad och tydlig.
    public sealed record RegisterRequest(string Email, string Password, string Role);
    public sealed record LoginRequest(string Email, string Password);

    // FIX: Returnera en roll. Klienten kan då fatta UI-beslut utan gissning.
    public sealed record LoginResponse(string Token, string Email, string Role);


    // RÄTT: Minimal APIs binder body automatiskt för komplexa typer (FromBody valfritt).
    // FIX: Ändrade private till public så att den kan användas i MapAuthEndpoints
    // FIX: [FromServices] onödigt — DI injicerar parametrar automatiskt här.
    public static async Task<IResult> Register(
        RegisterRequest request,
        UserManager<User> userManager,
        RoleManager<Role> roleManager)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Role))
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Registrering misslyckades",
                Detail = "Email/lösenord/roll saknas",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (!await roleManager.RoleExistsAsync(request.Role))
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Okänd roll",
                Detail = $"Rollen '{request.Role}' finns inte.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,


            // TIPS: I code-along kan vi sätta true för att slippa e-postflöde.
            EmailConfirmed = true
            // RÄTT (produktion): kräva bekräftelselänk innan inloggning.
            // EmailConfirmed = false
        };

        var create = await userManager.CreateAsync(user, request.Password);
        if (!create.Succeeded)
        {
            // TIPS: Använd alltid TypedResults.Problem för att returnera fel med korrekt
            // content-type och struktur, så att klienten kan hantera det på ett standardiserat
            // sätt.
            // FIX: Ändrade Results.BadRequest() till TypedResults.Problem() och inkluderade
            // alla felbeskrivningar i ProblemDetails, så att klienten får mer detaljerad
            // information om vad som gick fel.
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Registrering misslyckades",
                Detail = string.Join(", ", create.Errors.Select(error => error.Description)),
                Status = StatusCodes.Status400BadRequest
            });
        }

        await userManager.AddToRoleAsync(user, request.Role);
        return Results.Ok();
    }

    // FIX: Ändrade private till public så att den kan användas i MapAuthEndpoints
    public static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        UserManager<User> userManager, // [FromServices]
        IJwtTokenService tokenService)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Fel inloggning",
                Detail = "E-post eller lösenord stämmer inte",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = await tokenService.CreateAsync(user, roles);
        var role = roles.FirstOrDefault() ?? string.Empty;

        // FEL: Returnera bara första rollen → klienten måste gissa vilken som är "primär" → risk för felaktiga UI-beslut.
        //return Results.Ok(new LoginResponse(token, user.Email ?? string.Empty, roles.FirstOrDefault() ?? string.Empty));

        // RÄTT: Returnera en roll så klienten slipper gissa “primär” roll.
        return Results.Ok(new LoginResponse(token, user.Email ?? string.Empty, role));
    }

    // Design-val: En "me" endpoint som kräver autentisering, för att demonstrera hur man kan hämta information om den
    // inloggade användaren. Detta är också användbart för klienter att verifiera att de är inloggade och för att visa
    // relevant info i UI:t.
    // FIX: Ändrade private till public så att den kan användas i MapAuthEndpoints
    public static IResult Me(ClaimsPrincipal user)
    {

        // RÄTT: Name fungerar tack vare ClaimTypes.Name i JwtTokenService.
        var email = user.Identity?.Name ?? string.Empty;

        var role = user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        return Results.Ok(new { email, role });
    }
}
