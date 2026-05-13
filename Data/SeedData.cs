using GestionCotisations.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace GestionCotisations.Web.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Créer les rôles s'ils n'existent pas
            string[] roles = { "Admin", "Gestionnaire", "Membre" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Créer l'utilisateur Admin par défaut
            string adminEmail = "admin@cotisations.com";
            string adminPassword = "Admin123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Nom = "Super",
                    Prenom = "Admin",
                    EmailConfirmed = true,
                    EstActif = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Créer un utilisateur Gestionnaire par défaut
            string gestionnaireEmail = "gestionnaire@cotisations.com";
            string gestionnairePassword = "Gest123!";

            var gestionnaireUser = await userManager.FindByEmailAsync(gestionnaireEmail);

            if (gestionnaireUser == null)
            {
                gestionnaireUser = new ApplicationUser
                {
                    UserName = gestionnaireEmail,
                    Email = gestionnaireEmail,
                    Nom = "Gestionnaire",
                    Prenom = "Principal",
                    EmailConfirmed = true,
                    EstActif = true
                };

                var result = await userManager.CreateAsync(gestionnaireUser, gestionnairePassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(gestionnaireUser, "Gestionnaire");
                }
            }
        }
    }
}