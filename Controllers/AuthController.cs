using EventRegistrationSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrationSystem.Controllers;

public sealed class AuthController : AppController
{
    private readonly UserManager<EventUser> _userManager;
    private readonly SignInManager<EventUser> _signInManager;
    private readonly IWebHostEnvironment _environment;

    public AuthController(
        UserManager<EventUser> userManager,
        SignInManager<EventUser> signInManager,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _environment = environment;
    }

    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login(string? next = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectAfterSignIn(CurrentUser.Role);
        }

        return View(new LoginPageViewModel
        {
            CurrentUser = CurrentUser,
            Next = next,
            Form = new LoginInput()
        });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginPageViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectAfterSignIn(CurrentUser.Role);
        }

        if (!ModelState.IsValid)
        {
            model.CurrentUser = CurrentUser;
            return View(model);
        }

        var account = await _userManager.FindByEmailAsync(model.Form.Email.Trim());
        if (account is null || !account.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            model.CurrentUser = CurrentUser;
            return View(model);
        }

        var signInResult = await _signInManager.PasswordSignInAsync(
            account.UserName ?? account.Email ?? model.Form.Email.Trim(),
            model.Form.Password,
            model.Form.RememberMe,
            lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "This account is temporarily locked.");
            model.CurrentUser = CurrentUser;
            return View(model);
        }

        if (!signInResult.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            model.CurrentUser = CurrentUser;
            return View(model);
        }

        TempData["FlashMessage"] = "Welcome back. Your workspace is ready.";

        if (!string.IsNullOrWhiteSpace(model.Next) && Url.IsLocalUrl(model.Next))
        {
            return Redirect(model.Next);
        }

        return RedirectAfterSignIn(await GetPrimaryRoleFromUserAsync(account));
    }

    [AllowAnonymous]
    [HttpGet("register")]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectAfterSignIn(CurrentUser.Role);
        }

        return View(new RegisterPageViewModel
        {
            CurrentUser = CurrentUser,
            Form = new RegisterInput()
        });
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterPageViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectAfterSignIn(CurrentUser.Role);
        }

        if (!ModelState.IsValid)
        {
            model.CurrentUser = CurrentUser;
            return View(model);
        }

        var user = new EventUser
        {
            DisplayName = model.Form.DisplayName.Trim(),
            Email = model.Form.Email.Trim(),
            UserName = model.Form.Email.Trim(),
            Role = RoleNames.Attendee,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            LockoutEnabled = true
        };

        var createResult = await _userManager.CreateAsync(user, model.Form.Password);
        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);
            model.CurrentUser = CurrentUser;
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, RoleNames.Attendee);
        await _signInManager.SignInAsync(user, isPersistent: true);

        TempData["FlashMessage"] = "Your account has been created.";
        return RedirectToAction("Dashboard", "Events");
    }

    [Authorize]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["FlashMessage"] = "You have been signed out.";
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet("password-reset")]
    public IActionResult PasswordReset() => View(new PasswordResetInput());

    [AllowAnonymous]
    [HttpPost("password-reset")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PasswordReset(PasswordResetInput form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        string? resetLink = null;
        var account = await _userManager.FindByEmailAsync(form.Email.Trim());
        if (account is not null && account.IsActive)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(account);
            if (_environment.IsDevelopment())
            {
                resetLink = Url.Action(nameof(ResetPassword), "Auth", new
                {
                    email = account.Email,
                    token
                }, Request.Scheme);
            }
        }

        TempData["ResetLink"] = resetLink;
        TempData["FlashMessage"] = "Reset instructions have been prepared.";
        return RedirectToAction(nameof(PasswordResetConfirmation), new { email = form.Email });
    }

    [AllowAnonymous]
    [HttpGet("password-reset/confirmation")]
    public IActionResult PasswordResetConfirmation(string email)
    {
        return View(new PasswordResetConfirmationViewModel
        {
            CurrentUser = CurrentUser,
            Email = email,
            ResetLink = TempData["ResetLink"] as string
        });
    }

    [AllowAnonymous]
    [HttpGet("reset-password")]
    public IActionResult ResetPassword(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            TempData["FlashError"] = "The password reset link is invalid.";
            return RedirectToAction(nameof(Login));
        }

        return View(new ResetPasswordPageViewModel
        {
            CurrentUser = CurrentUser,
            Form = new ResetPasswordInput
            {
                Email = email,
                Token = token
            }
        });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordPageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.CurrentUser = CurrentUser;
            return View(model);
        }

        var account = await _userManager.FindByEmailAsync(model.Form.Email.Trim());
        if (account is null || !account.IsActive)
        {
            ModelState.AddModelError(string.Empty, "The reset request is no longer valid.");
            model.CurrentUser = CurrentUser;
            return View(model);
        }

        var resetResult = await _userManager.ResetPasswordAsync(account, model.Form.Token, model.Form.Password);
        if (!resetResult.Succeeded)
        {
            AddIdentityErrors(resetResult);
            model.CurrentUser = CurrentUser;
            return View(model);
        }

        TempData["FlashMessage"] = "Your password has been reset.";
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpGet("account")]
    public async Task<IActionResult> Manage()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        return View(await BuildManageAccountViewModelAsync(user));
    }

    [Authorize]
    [HttpPost("account/profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile([Bind(Prefix = "ProfileForm")] ManageProfileInput form)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        if (!ModelState.IsValid)
        {
            return View("Manage", await BuildManageAccountViewModelAsync(user, form, new ChangePasswordInput()));
        }

        user.DisplayName = form.DisplayName.Trim();
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            AddIdentityErrors(updateResult);
            return View("Manage", await BuildManageAccountViewModelAsync(user, form, new ChangePasswordInput()));
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["FlashMessage"] = "Your profile has been updated.";
        return RedirectToAction(nameof(Manage));
    }

    [Authorize]
    [HttpPost("account/password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword([Bind(Prefix = "PasswordForm")] ChangePasswordInput form)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        if (!ModelState.IsValid)
        {
            return View("Manage", await BuildManageAccountViewModelAsync(user, new ManageProfileInput
            {
                DisplayName = user.DisplayName,
                Email = user.Email ?? string.Empty
            }, form));
        }

        var changeResult = await _userManager.ChangePasswordAsync(user, form.CurrentPassword, form.NewPassword);
        if (!changeResult.Succeeded)
        {
            AddIdentityErrors(changeResult);
            return View("Manage", await BuildManageAccountViewModelAsync(user, new ManageProfileInput
            {
                DisplayName = user.DisplayName,
                Email = user.Email ?? string.Empty
            }, form));
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["FlashMessage"] = "Your password has been changed.";
        return RedirectToAction(nameof(Manage));
    }

    [HttpGet("access-denied")]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task<ManageAccountViewModel> BuildManageAccountViewModelAsync(
        EventUser user,
        ManageProfileInput? profileForm = null,
        ChangePasswordInput? passwordForm = null)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new ManageAccountViewModel
        {
            CurrentUser = CurrentUser,
            Roles = roles.ToArray(),
            ProfileForm = profileForm ?? new ManageProfileInput
            {
                DisplayName = user.DisplayName,
                Email = user.Email ?? string.Empty
            },
            PasswordForm = passwordForm ?? new ChangePasswordInput()
        };
    }

    private IActionResult RedirectAfterSignIn(UserRole role)
    {
        return role is UserRole.Organizer or UserRole.SuperAdmin
            ? RedirectToAction("Dashboard", "Organizer")
            : RedirectToAction("Dashboard", "Events");
    }

    private async Task<UserRole> GetPrimaryRoleFromUserAsync(EventUser user)
    {
        if (await _userManager.IsInRoleAsync(user, RoleNames.SuperAdmin))
        {
            return UserRole.SuperAdmin;
        }

        if (await _userManager.IsInRoleAsync(user, RoleNames.Organizer))
        {
            return UserRole.Organizer;
        }

        return UserRole.Attendee;
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
