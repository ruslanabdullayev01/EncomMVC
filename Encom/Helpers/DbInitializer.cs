using Encom.Constants;
using Encom.DAL;
using Encom.Models;
using Microsoft.AspNetCore.Identity;
using System;

namespace Encom.Helpers
{
    public class DbInitializer
    {
        public async static Task SeedAsync(RoleManager<IdentityRole> roleManager, UserManager<User> userManager, AppDbContext db)
        {
            foreach (var item in Enum.GetValues(typeof(UserRoles)))
            {
                if (!await roleManager.RoleExistsAsync(item.ToString()!))
                {
                    await roleManager.CreateAsync(new IdentityRole
                    {
                        Name = item.ToString(),
                    });
                }
            }

            if (await userManager.FindByNameAsync("SuperAdmin") == null)
            {
                var user = new User
                {
                    FullName = "SuperAdmin",
                    UserName = "SuperAdmin",
                    Email = "superadmin@gmail.com"
                };

                var result = await userManager.CreateAsync(user, "SuperAdmin@#Encom2024");

                if (!result.Succeeded)
                {
                    foreach (var item in result.Errors)
                    {
                        throw new Exception(item.Description);
                    }
                }

                await userManager.AddToRoleAsync(user, UserRoles.SuperAdmin.ToString());
            }

            //string azLanguageName = "AZ";
            //Language? azLanguage = db.Languages.FirstOrDefault(l => l.Name == azLanguageName);

            //if (azLanguage == null)
            //{
            //    azLanguage = new Language
            //    {
            //        Name = azLanguageName,
            //        Culture = "az-Latn-AZ",
            //    };

            //    db.Languages.Add(azLanguage);
            //    await db.SaveChangesAsync();
            //}

            //string enLanguageName = "EN";
            //Language? enLanguage = db.Languages.FirstOrDefault(l => l.Name == enLanguageName);

            //if (enLanguage == null)
            //{
            //    enLanguage = new Language
            //    {
            //        Name = enLanguageName,
            //        Culture = "en-US",
            //    };

            //    db.Languages.Add(enLanguage);
            //    await db.SaveChangesAsync();
            //}

        }
    }
}
