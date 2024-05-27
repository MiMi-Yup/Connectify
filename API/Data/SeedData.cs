using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class SeedData
    {
        public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            if (!await roleManager.Roles.AnyAsync())
            {
                var roles = new List<AppRole> {
                    new AppRole { Id = Guid.NewGuid(), Name = "Admin" },
                    new AppRole { Id = Guid.NewGuid(), Name = "Host" },
                    new AppRole { Id = Guid.NewGuid(), Name = "Member" }
                };

                foreach (var role in roles)
                {
                    await roleManager.CreateAsync(role);
                }
            }

            if (!await userManager.Users.AnyAsync())
            {
                var admin = new AppUser { UserName = "admin", DisplayName = "Administrator" };
                await userManager.CreateAsync(admin, "admin@123");
                await userManager.AddToRolesAsync(admin, new[] { "Admin", "Host", "Member" });
            }
        }
    }
}
