﻿using IoTDeviceManager.server.Models.Auth;
using Microsoft.AspNetCore.Identity;

namespace IoTDeviceManager.server.Data
{
    public class DatabaseSeeding
    {
        async Task SeedDatabaseAsync(IServiceProvider services)
        {
            RoleManager<IdentityRole> roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            UserManager<ApplicationUser> userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // Seed roles
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
