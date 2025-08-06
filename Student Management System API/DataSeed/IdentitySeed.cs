using Microsoft.AspNetCore.Identity;

namespace Student_Management_System_API.DataSeed
{
    public static class IdentitySeed
    {
        public static async Task SeedAsync(IServiceProvider service)
        {
            var userManager = service.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Admin", "Student", "Teacher" };

            foreach(var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var adminEmail = "ranakhaeled@gmail.com";
            var adminPassword = "Admin@123";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if(adminUser == null)
            {
                var newAdmin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(newAdmin, adminPassword);
                if (result.Succeeded)  await userManager.AddToRoleAsync(newAdmin, "Admin");
            }
        }
    }
}
