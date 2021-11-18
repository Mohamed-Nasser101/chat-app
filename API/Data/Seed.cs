using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            if (await userManager.Users.AnyAsync()) return;
            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);
            if (users == null) return;

            var roles = new List<AppRole>
            {
                new AppRole {Name = "Admin"},
                new AppRole {Name = "Member"},
                new AppRole {Name = "Moderator"}
            };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }

            foreach (var user in users)
            {
                // using var hmac = new HMACSHA512();

                user.UserName = user.UserName.ToLower();
                user.SecurityStamp = "fdsfsdffsdfs";
                await userManager.CreateAsync(user, "1234");
                await userManager.AddToRoleAsync(user, "Member");
                // user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("1234"));
                // user.PasswordSalt = hmac.Key;

                //context.Users.Add(user);
            }

            //await context.SaveChangesAsync();
            var admin = new AppUser
            {
                UserName = "admin",
                SecurityStamp = "ffffff"
            };
            await userManager.CreateAsync(admin, "1234");
            await userManager.AddToRolesAsync(admin, new[] {"Admin", "Moderator"});
        }
    }
}