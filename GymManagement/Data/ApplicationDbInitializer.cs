using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GymManagement.Data
{
    public static class ApplicationDbInitializer
    {
        public static async void Initialize(IServiceProvider serviceProvider,
            bool UseMigrations = true, bool SeedSampleData = true)
        {
            #region Prepare the Database
            if (UseMigrations)
            {
                using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
                {
                    try
                    {
                        //Create the database if it does not exist and apply the Migration
                        context.Database.Migrate();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.GetBaseException().Message);
                    }
                }
            }
            #endregion

            #region Seed Sample Data 
            if (SeedSampleData)
            {
                //Create Roles
                using (var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>())
                {
                    try
                    {
                        string[] roleNames = { "Admin", "Security", "Supervisor", "Staff", "Client" };

                        IdentityResult roleResult;
                        foreach (var roleName in roleNames)
                        {
                            var roleExist = await roleManager.RoleExistsAsync(roleName);
                            if (!roleExist)
                            {
                                roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.GetBaseException().Message);
                    }
                }

                //Create Users
                using (var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>())
                {
                    try
                    {
                        string defaultPassword = "Pa55w@rd";

                        //ADMIN
                        if (userManager.FindByEmailAsync("admin@outlook.com").Result == null)
                        {
                            IdentityUser user = new IdentityUser
                            {
                                UserName = "admin@outlook.com",
                                Email = "admin@outlook.com",
                                EmailConfirmed = true
                            };

                            IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

                            if (result.Succeeded)
                            {
                                userManager.AddToRoleAsync(user, "Admin").Wait();
                            }
                        }

                        //SECURITY
                        if (userManager.FindByEmailAsync("security@outlook.com").Result == null)
                        {
                            IdentityUser user = new IdentityUser
                            {
                                UserName = "security@outlook.com",
                                Email = "security@outlook.com",
                                EmailConfirmed = true
                            };

                            IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

                            if (result.Succeeded)
                            {
                                userManager.AddToRoleAsync(user, "Security").Wait();
                            }
                        }

                        //SUPERVISOR
                        if (userManager.FindByEmailAsync("supervisor@outlook.com").Result == null)
                        {
                            IdentityUser user = new IdentityUser
                            {
                                UserName = "supervisor@outlook.com",
                                Email = "supervisor@outlook.com",
                                EmailConfirmed = true
                            };

                            IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

                            if (result.Succeeded)
                            {
                                userManager.AddToRoleAsync(user, "Supervisor").Wait();
                            }
                        }

                        //STAFF
                        if (userManager.FindByEmailAsync("staff@outlook.com").Result == null)
                        {
                            IdentityUser user = new IdentityUser
                            {
                                UserName = "staff@outlook.com",
                                Email = "staff@outlook.com",
                                EmailConfirmed = true
                            };

                            IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

                            if (result.Succeeded)
                            {
                                userManager.AddToRoleAsync(user, "Staff").Wait();
                            }
                        }

                        //CLIENT
                        if (userManager.FindByEmailAsync("client@outlook.com").Result == null)
                        {
                            IdentityUser user = new IdentityUser
                            {
                                UserName = "client@outlook.com",
                                Email = "client@outlook.com",
                                EmailConfirmed = true
                            };

                            IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

                            if (result.Succeeded)
                            {
                                userManager.AddToRoleAsync(user, "Client").Wait();
                            }
                        }

                        //USER - NOT IN ANY ROLE!
                        if (userManager.FindByEmailAsync("user@outlook.com").Result == null)
                        {
                            IdentityUser user = new IdentityUser
                            {
                                UserName = "user@outlook.com",
                                Email = "user@outlook.com",
                                EmailConfirmed = true
                            };

                            IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;
                            //Not in any role
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.GetBaseException().Message);
                    }
                }
            }
            #endregion
        }
    }

}
