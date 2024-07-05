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
            #region User
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
            #endregion

            #region Languages
            string azLanguageName = "AZ";
            Language? azLanguage = db.Languages.FirstOrDefault(l => l.Name == azLanguageName);

            if (azLanguage == null)
            {
                azLanguage = new Language
                {
                    Name = azLanguageName,
                    Culture = "az-Latn-AZ",
                };

                db.Languages.Add(azLanguage);
                await db.SaveChangesAsync();
            }

            string enLanguageName = "EN";
            Language? enLanguage = db.Languages.FirstOrDefault(l => l.Name == enLanguageName);

            if (enLanguage == null)
            {
                enLanguage = new Language
                {
                    Name = enLanguageName,
                    Culture = "en-US",
                };

                db.Languages.Add(enLanguage);
                await db.SaveChangesAsync();
            }

            string ruLanguageName = "RU";
            Language? ruLanguage = db.Languages.FirstOrDefault(l => l.Name == ruLanguageName);

            if (ruLanguage == null)
            {
                ruLanguage = new Language
                {
                    Name = ruLanguageName,
                    Culture = "ru-RU",
                };

                db.Languages.Add(ruLanguage);
                await db.SaveChangesAsync();
            }

            string trLanguageName = "TR";
            Language? trLanguage = db.Languages.FirstOrDefault(l => l.Name == trLanguageName);

            if (trLanguage == null)
            {
                trLanguage = new Language
                {
                    Name = trLanguageName,
                    Culture = "tr-TR",
                };

                db.Languages.Add(trLanguage);
                await db.SaveChangesAsync();
            }
            #endregion

            #region Settings
            string homeHeroDescriptionKey = "Home Page Hero Description";
            string homeHeroDescriptionValue = "Hər bir layihədə üstün keyfiyyətlə işləyərək yaşayış yerlərinizə dəyər qatırıq. Peşəkar komandamız və yenilikçi yanaşmamızla sizə təhlükəsiz və estetik strukturlar təqdim etməyi öhdəliyik.";
            Setting? homeHeroDescription = db.Settings.FirstOrDefault(l => l.Key == homeHeroDescriptionKey);

            if (homeHeroDescription is null)
            {
                for (int languageId = 1; languageId <= 4; languageId++)
                {
                    homeHeroDescription = new Setting
                    {
                        Key = homeHeroDescriptionKey,
                        Value = homeHeroDescriptionValue,
                        LanguageId = languageId,
                        LanguageGroup = 1,
                    };

                    db.Settings.Add(homeHeroDescription);
                }
            }
            await db.SaveChangesAsync();


            string newsHeroDescriptionKey = "News Page Hero Description";
            string newsHeroDescriptionValue = "İnşaat sektoru daim inkişaf edir və dəyişir. Burada şirkətimizlə bağlı ən son xəbərləri, layihələrimizi və sektorda baş verən yenilikləri tapa bilərsiniz.";
            Setting? newsHeroDescription = db.Settings.FirstOrDefault(l => l.Key == newsHeroDescriptionKey);

            if (newsHeroDescription is null)
            {
                for (int languageId = 1; languageId <= 4; languageId++)
                {
                    newsHeroDescription = new Setting
                    {
                        Key = newsHeroDescriptionKey,
                        Value = newsHeroDescriptionValue,
                        LanguageId = languageId,
                        LanguageGroup = 2,
                    };

                    db.Settings.Add(newsHeroDescription);
                }
            }
            await db.SaveChangesAsync();


            string formInContactUsPageKey = "Form In Contact Us Page";
            string formInContactUsValue = "Ətraflı məlumat və ehtiyaclarınızı necə qarşılaya biləcəyimiz üçün aşağıdakı formanı doldurun və komandamızdan kimsə əlaqə saxlasın.";
            Setting? formInContactUs = db.Settings.FirstOrDefault(l => l.Key == formInContactUsPageKey);

            if (formInContactUs is null)
            {
                for (int languageId = 1; languageId <= 4; languageId++)
                {
                    formInContactUs = new Setting
                    {
                        Key = formInContactUsPageKey,
                        Value = formInContactUsValue,
                        LanguageId = languageId,
                        LanguageGroup = 3,
                    };

                    db.Settings.Add(formInContactUs);
                }
            }
            await db.SaveChangesAsync();


            #endregion

        }
    }
}
