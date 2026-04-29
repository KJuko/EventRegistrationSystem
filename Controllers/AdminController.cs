using EventRegistrationSystem.Data;
using EventRegistrationSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrationSystem.Controllers;

[Authorize(Policy = "AdminOnly")]
public sealed class AdminController : AppController
{
    private readonly EventDbContext _context;
    private readonly UserManager<EventUser> _userManager;

    public AdminController(EventDbContext context, UserManager<EventUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("admin")]
    public IActionResult Index() => RedirectToAction(nameof(Users));

    [HttpGet("admin/users")]
    public async Task<IActionResult> Users(string? q = null)
    {
        var query = _context.EventUsers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(user =>
                user.Email != null && user.Email.Contains(term) ||
                user.DisplayName.Contains(term));
        }

        var users = await query
            .OrderBy(user => user.CreatedAtUtc)
            .ToListAsync();

        var userIds = users.Select(user => user.Id).ToArray();
        var rolePairs = await (
            from userRole in _context.UserRoles
            join role in _context.Roles on userRole.RoleId equals role.Id
            where userIds.Contains(userRole.UserId)
            select new
            {
                userRole.UserId,
                RoleName = role.Name ?? string.Empty
            })
            .ToListAsync();

        var model = new AdminUsersPageViewModel
        {
            CurrentUser = CurrentUser,
            Query = q ?? string.Empty,
            Users = users.Select(user => new AdminUserListItemViewModel
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email ?? string.Empty,
                IsActive = user.IsActive,
                IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
                CreatedAtUtc = user.CreatedAtUtc,
                Roles = rolePairs
                    .Where(pair => pair.UserId == user.Id)
                    .Select(pair => pair.RoleName)
                    .OrderBy(role => role)
                    .ToArray()
            }).ToList()
        };

        return View(model);
    }

    [HttpGet("admin/users/create")]
    public IActionResult CreateUser()
    {
        return View(new AdminCreateUserPageViewModel
        {
            CurrentUser = CurrentUser,
            Form = new AdminCreateUserInput
            {
                Roles = [RoleNames.Attendee]
            },
            RoleOptions = BuildRoleOptions([RoleNames.Attendee])
        });
    }

    [HttpPost("admin/users/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(AdminCreateUserPageViewModel model)
    {
        ValidateRoleSelection(model.Form.Roles);

        if (!ModelState.IsValid)
        {
            model.CurrentUser = CurrentUser;
            model.RoleOptions = BuildRoleOptions(model.Form.Roles);
            return View(model);
        }

        var user = new EventUser
        {
            DisplayName = model.Form.DisplayName.Trim(),
            Email = model.Form.Email.Trim(),
            UserName = model.Form.Email.Trim(),
            Role = RoleNames.GetPrimaryRoleName(model.Form.Roles),
            EmailConfirmed = true,
            IsActive = model.Form.IsActive,
            CreatedAtUtc = DateTime.UtcNow,
            LockoutEnabled = true
        };

        var createResult = await _userManager.CreateAsync(user, model.Form.Password);
        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);
            model.CurrentUser = CurrentUser;
            model.RoleOptions = BuildRoleOptions(model.Form.Roles);
            return View(model);
        }

        var addRolesResult = await _userManager.AddToRolesAsync(user, model.Form.Roles.Distinct(StringComparer.OrdinalIgnoreCase));
        if (!addRolesResult.Succeeded)
        {
            AddIdentityErrors(addRolesResult);
            model.CurrentUser = CurrentUser;
            model.RoleOptions = BuildRoleOptions(model.Form.Roles);
            return View(model);
        }

        await _userManager.UpdateSecurityStampAsync(user);
        TempData["FlashMessage"] = $"{user.DisplayName} has been created.";
        return RedirectToAction(nameof(Users));
    }

    [HttpGet("admin/users/{id:int}")]
    public async Task<IActionResult> EditUser(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return NotFound();
        }

        return View(await BuildEditUserViewModelAsync(user));
    }

    [HttpPost("admin/users/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(int id, [Bind(Prefix = "Form")] AdminEditUserInput form)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return NotFound();
        }

        ValidateRoleSelection(form.Roles);
        var currentAdminId = _userManager.GetUserId(User);
        if (currentAdminId == id.ToString() && !form.IsActive)
        {
            ModelState.AddModelError(string.Empty, "You cannot deactivate your own account.");
        }

        if (currentAdminId == id.ToString() && !form.Roles.Contains(RoleNames.SuperAdmin, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "You cannot remove your own super admin role.");
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildEditUserViewModelAsync(user, form, new AdminPasswordResetInput { Id = id }));
        }

        user.DisplayName = form.DisplayName.Trim();
        user.Email = form.Email.Trim();
        user.UserName = form.Email.Trim();
        user.Role = RoleNames.GetPrimaryRoleName(form.Roles);
        user.IsActive = form.IsActive;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            AddIdentityErrors(updateResult);
            return View(await BuildEditUserViewModelAsync(user, form, new AdminPasswordResetInput { Id = id }));
        }

        var existingRoles = await _userManager.GetRolesAsync(user);
        var rolesToRemove = existingRoles.Except(form.Roles, StringComparer.OrdinalIgnoreCase).ToArray();
        if (rolesToRemove.Length > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                AddIdentityErrors(removeResult);
                return View(await BuildEditUserViewModelAsync(user, form, new AdminPasswordResetInput { Id = id }));
            }
        }

        var rolesToAdd = form.Roles.Except(existingRoles, StringComparer.OrdinalIgnoreCase).ToArray();
        if (rolesToAdd.Length > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                AddIdentityErrors(addResult);
                return View(await BuildEditUserViewModelAsync(user, form, new AdminPasswordResetInput { Id = id }));
            }
        }

        await _userManager.UpdateSecurityStampAsync(user);
        TempData["FlashMessage"] = $"{user.DisplayName} has been updated.";
        return RedirectToAction(nameof(EditUser), new { id });
    }

    [HttpPost("admin/users/{id:int}/password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id, [Bind(Prefix = "PasswordResetForm")] AdminPasswordResetInput form)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View("EditUser", await BuildEditUserViewModelAsync(user, passwordResetForm: form));
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, form.Password);
        if (!resetResult.Succeeded)
        {
            AddIdentityErrors(resetResult);
            return View("EditUser", await BuildEditUserViewModelAsync(user, passwordResetForm: form));
        }

        await _userManager.UpdateSecurityStampAsync(user);
        TempData["FlashMessage"] = $"Password reset completed for {user.DisplayName}.";
        return RedirectToAction(nameof(EditUser), new { id });
    }

    [HttpPost("admin/users/{id:int}/lock")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LockUser(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return NotFound();
        }

        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        await _userManager.UpdateSecurityStampAsync(user);

        TempData["FlashMessage"] = $"{user.DisplayName} has been locked.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost("admin/users/{id:int}/unlock")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnlockUser(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return NotFound();
        }

        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.UpdateSecurityStampAsync(user);

        TempData["FlashMessage"] = $"{user.DisplayName} has been unlocked.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost("admin/users/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return NotFound();
        }

        if (_userManager.GetUserId(User) == id.ToString())
        {
            TempData["FlashError"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Users));
        }

        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            AddIdentityErrors(deleteResult);
            TempData["FlashError"] = string.Join(" ", deleteResult.Errors.Select(error => error.Description));
            return RedirectToAction(nameof(Users));
        }

        TempData["FlashMessage"] = $"{user.DisplayName} has been deleted.";
        return RedirectToAction(nameof(Users));
    }

    private async Task<AdminEditUserPageViewModel> BuildEditUserViewModelAsync(
        EventUser user,
        AdminEditUserInput? form = null,
        AdminPasswordResetInput? passwordResetForm = null)
    {
        var selectedRoles = form?.Roles ?? (await _userManager.GetRolesAsync(user)).ToArray();

        return new AdminEditUserPageViewModel
        {
            CurrentUser = CurrentUser,
            IsSelf = _userManager.GetUserId(User) == user.Id.ToString(),
            Form = form ?? new AdminEditUserInput
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email ?? string.Empty,
                IsActive = user.IsActive,
                Roles = selectedRoles.ToArray()
            },
            PasswordResetForm = passwordResetForm ?? new AdminPasswordResetInput
            {
                Id = user.Id
            },
            RoleOptions = BuildRoleOptions(selectedRoles)
        };
    }

    private static IReadOnlyList<RoleSelectionItem> BuildRoleOptions(IEnumerable<string> selectedRoles)
    {
        var selection = new HashSet<string>(selectedRoles, StringComparer.OrdinalIgnoreCase);
        return RoleNames.All
            .Select(roleName => new RoleSelectionItem
            {
                Name = roleName,
                IsSelected = selection.Contains(roleName)
            })
            .ToList();
    }

    private void ValidateRoleSelection(IReadOnlyCollection<string> selectedRoles)
    {
        if (selectedRoles.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Select at least one role.");
            return;
        }

        var invalidRole = selectedRoles.FirstOrDefault(role => !RoleNames.All.Contains(role, StringComparer.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(invalidRole))
        {
            ModelState.AddModelError(string.Empty, $"Role '{invalidRole}' is not allowed.");
        }
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
