using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using SYSS8.OPF.Clean.Infrastructure;

namespace SYSS8.OPF.Clean.WebApi.Identity;

public static class IdentitySeeder
{
    // INFO: Seeding skapar demo-data så att studenter kan testa flöden direkt.
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AuthorDbContext>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

        if (ctx.Database.IsRelational())
            await ctx.Database.MigrateAsync(ct);

        // DESIGN-VAL: Rollerna speglar vanliga kursroller och används i policy-exempel.
        var roles = new[] { "Admin", "Lärare", "Student" };
        foreach (var r in roles)
            if (!await roleMgr.RoleExistsAsync(r))
                await roleMgr.CreateAsync(new Role { Name = r });

        // OBS: Enkla lösenord underlättar labbflöden (byt i produktion).
        await EnsureUserAsync("admin@example.com", "Admin", "Password1!", userMgr, roleMgr);
        await EnsureUserAsync("teacher@example.com", "Lärare", "Password1!", userMgr, roleMgr);
        await EnsureUserAsync("student@example.com", "Student", "Password1!", userMgr, roleMgr);
    }

    private static async Task EnsureUserAsync(
        string email, string role, string password,
        UserManager<User> userMgr, RoleManager<Role> roleMgr)
    {
        var user = await userMgr.FindByEmailAsync(email);
        if (user is null)
        {
            // TIPS: EmailConfirmed = true gör att vi slipper e-postflöde i kursmiljön.
            // I produktion måste du antingen implementera e-postflöde eller sätta det till false
            // och använda "Glömt lösenordet" för att logga in första gången.
            user = new User { UserName = email, Email = email, EmailConfirmed = true };

            // OBS: Använd två-parameter-overloaden så att du sätter lösenordet i samma steg som
            // användaren skapas. Annars måste du anropa UpdateAsync efteråt, vilket kräver
            // ytterligare databasoperationer.
            var create = await userMgr.CreateAsync(user, password);
            if (!create.Succeeded)
                throw new InvalidOperationException($"Kunde inte skapa användare {email}: " +
                    string.Join(", ", create.Errors.Select(e => e.Description)));
        }
        if (!await userMgr.IsInRoleAsync(user, role))
            await userMgr.AddToRoleAsync(user, role);
    }
}