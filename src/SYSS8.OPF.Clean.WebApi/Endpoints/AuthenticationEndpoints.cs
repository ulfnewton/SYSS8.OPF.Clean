using System.Security.Claims;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using SYSS8.OPF.Clean.Infrastructure;
using SYSS8.OPF.Clean.WebApi.Auth;

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
    // INFO: Login returnerar Token + Email + Role så klienten kan visa användare och aktivera UI-regler.
    public sealed record LoginResponse(string Token, string Email, string Role);

    // FIX: Ändrade returtyp från IResult till Results<Ok, ProblemHttpResult> för att tydligare specificera
    // möjliga utfall och underlätta felsökning i kursmiljön. Detta gör att vi kan använda TypedResults.Problem
    // för att returnera detaljerade felmeddelanden samt standardiserade problem-details-responser.
    public static async Task<Results<Ok, ProblemHttpResult>> Register(
        [FromBody] RegisterRequest req,
        UserManager<User> userMgr,
        RoleManager<Role> roleMgr)
    {
        // FEL: Returnerar detaljerade felmeddelanden från Identity för att underlätta felsökning i kursmiljön.
        //return Results.BadRequest(new ProblemDetails {...});

        // RÄTT: TypedResults.Problem används för att returnera en standardiserad problem-details-respons med
        // relevant information om varför registreringen misslyckades.
        // return TypedResults.Problem(new ProblemDetails {...});

        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return TypedResults.Problem(new ProblemDetails { Title = "Ogiltiga fält", Detail = "Email/lösenord saknas", Status = StatusCodes.Status400BadRequest });

        if (!await roleMgr.RoleExistsAsync(req.Role))
            return TypedResults.Problem(new ProblemDetails { Title = "Okänd roll", Detail = req.Role, Status = StatusCodes.Status400BadRequest });

        // TIPS: I code-along och Development kan vi sätta EmailConfirmed = true för att slippa e-postflöde.
        // RÄTT (produktion): kräva bekräftelselänk innan inloggning.
        var user = new User { UserName = req.Email, Email = req.Email, EmailConfirmed = true };

        // OBS: Använd två-parameter-overloaden så att du sätter lösenordet i samma steg som användaren skapas.
        // Annars måste du anropa UpdateAsync efteråt, vilket kräver ytterligare databasoperationer.
        var create = await userMgr.CreateAsync(user, req.Password);
        if (!create.Succeeded)
        {
            // FEL: Returnerar detaljerade felmeddelanden från Identity för att underlätta felsökning i kursmiljön.
            //return Results.BadRequest(new ProblemDetails

            // RÄTT: TypedResults.Problem används för att returnera en standardiserad problem-details-respons med
            // relevant information om varför registreringen misslyckades.
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Registrering misslyckades",
                Detail = string.Join(", ", create.Errors.Select(e => e.Description)),
                Status = 400
            });
        }

        await userMgr.AddToRoleAsync(user, req.Role);
        return TypedResults.Ok();
    }

    public static async Task<Results<Ok<LoginResponse>, ProblemHttpResult>> Login(
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
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var roles = await userMgr.GetRolesAsync(user);
        var token = await tokens.CreateAsync(user, roles);

        // OBS: Vi returnerar en roll för att UI ska spegla serverns verkliga behörighet. 
        // I produktion kan det vara bättre att returnera alla roller och låta klienten hantera det,
        // eller att inte returnera roller alls och istället ha en separat endpoint för att hämta
        // användarinfo.
        return TypedResults.Ok(new LoginResponse(token, user.Email ?? "", roles.FirstOrDefault() ?? ""));
    }

    // Design-val: En "me" endpoint som kräver autentisering, för att demonstrera hur man kan hämta information om den
    // inloggade användaren. Detta är också användbart för klienter att verifiera att de är inloggade och för att visa
    // relevant info i UI:t.
    // FIX: Ändrade private till public så att den kan användas i MapAuthEndpoints
    public static IResult Me(ClaimsPrincipal user)
    {
        // TIPS: Identity sätter ClaimTypes.Name när token skapas, vilket gör Name tillgängligt här.
        var email = user.Identity?.Name ?? "";
        // INFO: ClaimTypes.Role används av ASP.NET Core för rollbaserad auktorisering.
        var role = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "";
        return Results.Ok(new { email, role });
    }
}
