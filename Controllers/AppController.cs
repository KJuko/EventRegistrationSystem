using System.Security.Claims;
using EventRegistrationSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrationSystem.Controllers;

public abstract class AppController : Controller
{
    protected CurrentUser CurrentUser => GetCurrentUser();

    protected CurrentUser GetCurrentUser()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return CurrentUser.Guest;
        }

        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? CurrentUser.Guest.Email;
        var displayName = User.FindFirstValue("display_name")
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? email.Split('@')[0];
        var role = User.IsInRole(RoleNames.SuperAdmin)
            ? UserRole.SuperAdmin
            : User.IsInRole(RoleNames.Organizer)
                ? UserRole.Organizer
                : UserRole.Attendee;

        return new CurrentUser
        {
            DisplayName = displayName,
            Email = email,
            Role = role
        };
    }

    protected IActionResult? RequireOrganizer()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Challenge();
        }

        if (CurrentUser.CanManageEvents)
        {
            return null;
        }

        return Forbid();
    }
}
