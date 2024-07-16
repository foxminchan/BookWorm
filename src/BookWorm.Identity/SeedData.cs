using System.Security.Claims;
using BookWorm.Identity.Data;
using BookWorm.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Identity;

public sealed class SeedData(
    ILogger<SeedData> logger,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager) : IDbSeeder<ApplicationDbContext>
{
    public async Task SeedAsync(ApplicationDbContext context)
    {
        var admin = new IdentityRole("Admin");

        if (await roleManager.Roles.AllAsync(r => r.Name != admin.Name))
        {
            await roleManager.CreateAsync(admin);
        }

        var user = new IdentityRole("User");

        if (await roleManager.Roles.AllAsync(r => r.Name != user.Name))
        {
            await roleManager.CreateAsync(user);
        }

        var nhan = await userManager.FindByEmailAsync("nguyenxuannhan407@gmail.com");

        if (nhan is null)
        {
            nhan = new()
            {
                UserName = "nhan",
                Email = "nguyenxuannhan407@gmail.com",
                FirstName = "Xuan Nhan",
                LastName = "Nguyen",
                EmailConfirmed = true,
                PhoneNumber = "1234567890",
                PhoneNumberConfirmed = true
            };

            var result = userManager.CreateAsync(nhan, "P@ssw0rd").Result;

            if (!result.Succeeded)
            {
                throw new(result.Errors.First().Description);
            }

            await userManager.AddToRoleAsync(nhan, admin.Name!);

            await userManager.AddClaimsAsync(nhan,
            [
                new(ClaimTypes.Name, $"{nhan.FirstName} {nhan.LastName}"),
                new(ClaimTypes.GivenName, nhan.FirstName),
                new(ClaimTypes.Surname, nhan.LastName),
                new(ClaimTypes.Email, nhan.Email),
                new(ClaimTypes.MobilePhone, nhan.PhoneNumber)
            ]);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("nhan created");
            }
        }
        else
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("nhan already exists");
            }
        }

        var smith = await userManager.FindByEmailAsync("smith@gmail.com");

        if (smith is null)
        {
            smith = new()
            {
                UserName = "smith", 
                Email = "smith@gmail.com",
                FirstName = "John",
                LastName = "Smith",
                EmailConfirmed = true,
                PhoneNumber = "0987654321",
                PhoneNumberConfirmed = true
            };

            var result = userManager.CreateAsync(smith, "P@ssw0rd").Result;

            if (!result.Succeeded)
            {
                throw new(result.Errors.First().Description);
            }

            await userManager.AddToRoleAsync(smith, user.Name!);

            await userManager.AddClaimsAsync(smith,
            [
                new(ClaimTypes.Name, $"{smith.FirstName} {smith.LastName}"),
                new(ClaimTypes.GivenName, smith.FirstName),
                new(ClaimTypes.Surname, smith.LastName),
                new(ClaimTypes.Email, smith.Email),
                new(ClaimTypes.MobilePhone, smith.PhoneNumber)
            ]);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("smith created");
            }
        }
        else
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("smith already exists");
            }
        }
    }
}
