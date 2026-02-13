using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SYSS8.OPF.Clean.Infrastructure;
using SYSS8.OPF.Clean.WebApi.Auth;
using System.Security.Claims;

namespace SYSS8.OPF.Clean.WebApi.Endpoints;

public static class AuthEndpoints
{
    public sealed record RegisterRequest(string Email, string Password, string Role);
    public sealed record LoginRequest(string Email, string Password);
    public sealed record LoginResponse(string Token, string Email, string Role);

    // hör hemma i MapEndpointsExtensions.cs
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");
        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
        group.MapGet("/me", Me).RequireAuthorization();
        return app;
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterRequest request,
        [FromServices] UserManager<User> userManager,
        [FromServices] RoleManager<Role> roleManager)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Registrering misslyckades",
                Detail = "Email/lösenord saknas",
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
            EmailConfirmed = true,
        };

        var create = await userManager.CreateAsync(user, request.Password);
        if (!create.Succeeded)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Registrering misslyckades",
                Detail = string.Join(", ", create.Errors.Select(error => error.Description)),
                Status = StatusCodes.Status400BadRequest
            });
        }

        await userManager.AddToRoleAsync(user, request.Role);
        return Results.Ok();
    }

    private static async Task<IResult> Login(
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
        return Results.Ok(new LoginResponse(token, user.Email ?? string.Empty, roles.FirstOrDefault() ?? string.Empty));
    }

    private static IResult Me(ClaimsPrincipal user)
    {
        var email = user.Identity?.Name ?? string.Empty;
        var role = user.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)?.Value 
            ?? string.Empty;
        return Results.Ok(new { email, role });
    }
}
