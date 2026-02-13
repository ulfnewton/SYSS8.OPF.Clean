using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

            if (context.Database.IsRelational())
            {
                await context.Database.MigrateAsync();
            }

            var roles = new[] { "Admin", "Lärare", "Student" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new Role { Name = role });
                }
            }

            await EnsureUserAsync("admin@example.com", "Admin", "Pass123!", userManager, roleManager);
            await EnsureUserAsync("teacher@example.com", "Lärare", "Pass123!", userManager, roleManager);
            await EnsureUserAsync("student@example.com", "Student", "Pass123!", userManager, roleManager);

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
                var create = await userManager.CreateAsync(user);
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
