using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SYSS8.OPF.Clean.Domain;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYSS8.OPF.Clean.Infrastructure.Identity
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthorDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("IdentitySeeder");

            if (context.Database.IsRelational())
            {
                await context.Database.MigrateAsync();
            }

            var roles = new[] { "Admin", "Lärare", "Student" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var res = await roleManager.CreateAsync(new Role { Name = role });
                    if (res?.Succeeded != true)
                    {
                        logger.LogError("Kunde inte skapa roll {Role}: {Errors}",
                            role, string.Join(", ", res!.Errors.Select(e => e.Description)));
                    }
                    else
                    {
                        logger.LogInformation("Skapade roll {Role}", role);
                    }
                }
            }

            await EnsureUserAsync("admin@example.com", "Admin", "Pass123!", userManager, roleManager);
            await EnsureUserAsync("teacher@example.com", "Lärare", "Pass123!", userManager, roleManager);
            await EnsureUserAsync("student@example.com", "Student", "Pass123!", userManager, roleManager);

            var users = await userManager.Users.ToListAsync(cancellationToken);

            foreach (var user in users)
            {
                var userRoles = await userManager.GetRolesAsync(user);
                logger.LogInformation("Användare {Email} har roller: {Roles}",
                    user.Email, string.Join(", ", userRoles));
            }
        }

        private static async Task EnsureUserAsync(
            string email,
            string role,
            string password,
            UserManager<User> userManager,
            RoleManager<Role> roleManager)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                };

                // FIX: Ändrade från CreateAsync(user) till CreateAsync(user, password) för
                // att sätta lösenordet direkt.
                var create = await userManager.CreateAsync(user, password);
                if (!create!.Succeeded)
                {
                    throw new InvalidOperationException($"Kunde inte skapa användare {email}: "
                        + string.Join(", ", create.Errors.Select(error => error.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
