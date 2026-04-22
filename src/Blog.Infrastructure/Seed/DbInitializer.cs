using Blog.Infrastructure.Data;
using Blog.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Blog.Infrastructure.Seed;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider serviceProvider,
        string dbJsonPath,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var importer = scope.ServiceProvider.GetRequiredService<DbJsonImporter>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        foreach (var roleName in new[] { "admin", "author", "reader" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new AppRole { Name = roleName });
            }
        }

        var adminEmail = "admin@local.test";
        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin is null)
        {
            admin = new AppUser
            {
                UserName = "admin",
                Email = adminEmail,
                EmailConfirmed = true,
                DisplayName = "System Admin",
                Slug = "admin"
            };

            var createResult = await userManager.CreateAsync(admin, "Admin123!@#");
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", createResult.Errors.Select(x => x.Description)));
            }

            await userManager.AddToRoleAsync(admin, "admin");
        }

        await importer.ImportAsync(dbJsonPath, cancellationToken);
    }
}