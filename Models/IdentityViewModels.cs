using System.ComponentModel.DataAnnotations;

namespace EventRegistrationSystem.Models;

public sealed class RegisterPageViewModel : AppPageViewModel
{
    public RegisterInput Form { get; set; } = new();
}

public sealed class RegisterInput
{
    [Required]
    [StringLength(120)]
    [Display(Name = "Display name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class ResetPasswordPageViewModel : AppPageViewModel
{
    public ResetPasswordInput Form { get; set; } = new();
}

public sealed class ResetPasswordInput
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class ManageAccountViewModel : AppPageViewModel
{
    public ManageProfileInput ProfileForm { get; set; } = new();
    public ChangePasswordInput PasswordForm { get; set; } = new();
    public IReadOnlyList<string> Roles { get; set; } = [];
}

public sealed class ManageProfileInput
{
    [Required]
    [StringLength(120)]
    [Display(Name = "Display name")]
    public string DisplayName { get; set; } = string.Empty;

    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}

public sealed class ChangePasswordInput
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Current password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword))]
    [Display(Name = "Confirm new password")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public sealed class RoleSelectionItem
{
    public string Name { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

public sealed class AdminUserListItemViewModel
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
}

public sealed class AdminUsersPageViewModel : AppPageViewModel
{
    public string Query { get; set; } = string.Empty;
    public IReadOnlyList<AdminUserListItemViewModel> Users { get; set; } = [];
}

public sealed class AdminCreateUserPageViewModel : AppPageViewModel
{
    public AdminCreateUserInput Form { get; set; } = new();
    public IReadOnlyList<RoleSelectionItem> RoleOptions { get; set; } = [];
}

public sealed class AdminCreateUserInput
{
    [Required]
    [StringLength(120)]
    [Display(Name = "Display name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Active account")]
    public bool IsActive { get; set; } = true;

    public string[] Roles { get; set; } = [];
}

public sealed class AdminEditUserPageViewModel : AppPageViewModel
{
    public AdminEditUserInput Form { get; set; } = new();
    public AdminPasswordResetInput PasswordResetForm { get; set; } = new();
    public IReadOnlyList<RoleSelectionItem> RoleOptions { get; set; } = [];
    public bool IsSelf { get; set; }
}

public sealed class AdminEditUserInput
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    [Display(Name = "Display name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Active account")]
    public bool IsActive { get; set; } = true;

    public string[] Roles { get; set; } = [];
}

public sealed class AdminPasswordResetInput
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Display(Name = "Confirm new password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
