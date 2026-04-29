using EventRegistrationSystem.Services;
using EventRegistrationSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrationSystem.Data;

public static class EventDbSeeder
{
    public static async Task SeedDemoDataAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<EventDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = services.GetRequiredService<UserManager<EventUser>>();

        await IdentitySchemaBootstrapper.EnsureIdentitySchemaAsync(context);
        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);
        await SeedEventsAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager)
    {
        foreach (var roleName in RoleNames.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            await roleManager.CreateAsync(new IdentityRole<int>(roleName));
        }
    }

    private static async Task SeedUsersAsync(UserManager<EventUser> userManager)
    {
        foreach (var definition in DemoUserData.GetSeedUsers())
        {
            var existingUser = await userManager.FindByEmailAsync(definition.Email);

            if (existingUser is null)
            {
                var createdUser = new EventUser
                {
                    DisplayName = definition.DisplayName,
                    Email = definition.Email,
                    UserName = definition.Email,
                    Role = RoleNames.GetPrimaryRoleName(definition.Roles),
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    LockoutEnabled = true
                };

                var createResult = await userManager.CreateAsync(createdUser, definition.Password);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException($"Unable to seed user {definition.Email}: {string.Join(", ", createResult.Errors.Select(error => error.Description))}");
                }

                foreach (var roleName in definition.Roles)
                {
                    await userManager.AddToRoleAsync(createdUser, roleName);
                }

                continue;
            }

            existingUser.DisplayName = definition.DisplayName;
            existingUser.Email = definition.Email;
            existingUser.UserName = definition.Email;
            existingUser.Role = RoleNames.GetPrimaryRoleName(definition.Roles);
            existingUser.EmailConfirmed = true;
            existingUser.IsActive = true;
            existingUser.LockoutEnabled = true;

            var updateResult = await userManager.UpdateAsync(existingUser);
            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException($"Unable to update seeded user {definition.Email}: {string.Join(", ", updateResult.Errors.Select(error => error.Description))}");
            }

            var resetPasswordToken = await userManager.GeneratePasswordResetTokenAsync(existingUser);
            var resetPasswordResult = await userManager.ResetPasswordAsync(existingUser, resetPasswordToken, definition.Password);
            if (!resetPasswordResult.Succeeded)
            {
                throw new InvalidOperationException($"Unable to reset seeded user password for {definition.Email}: {string.Join(", ", resetPasswordResult.Errors.Select(error => error.Description))}");
            }

            var existingRoles = await userManager.GetRolesAsync(existingUser);
            var rolesToRemove = existingRoles.Except(definition.Roles, StringComparer.OrdinalIgnoreCase).ToArray();
            if (rolesToRemove.Length > 0)
            {
                await userManager.RemoveFromRolesAsync(existingUser, rolesToRemove);
            }

            var rolesToAdd = definition.Roles.Except(existingRoles, StringComparer.OrdinalIgnoreCase).ToArray();
            if (rolesToAdd.Length > 0)
            {
                await userManager.AddToRolesAsync(existingUser, rolesToAdd);
            }
        }
    }

    private static async Task SeedEventsAsync(EventDbContext context)
    {
        if (await context.EventRecords.AnyAsync())
        {
            return;
        }

        var seedEvents = DemoEventData.CreateSeedEvents();

        foreach (var eventRecord in seedEvents)
        {
            eventRecord.Id = 0;

            foreach (var session in eventRecord.Sessions)
            {
                session.Id = 0;
            }

            foreach (var asset in eventRecord.MediaAssets)
            {
                asset.Id = 0;
            }

            foreach (var folder in eventRecord.MediaFolders)
            {
                folder.Id = 0;
            }

            foreach (var registration in eventRecord.Registrations)
            {
                registration.Id = 0;
            }

            foreach (var activity in eventRecord.RecentActivities)
            {
                activity.Id = 0;
            }

            foreach (var deadline in eventRecord.Deadlines)
            {
                deadline.Id = 0;
            }
        }

        context.EventRecords.AddRange(seedEvents);
        await context.SaveChangesAsync();
    }
}
