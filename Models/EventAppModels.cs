using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EventRegistrationSystem.Models;

public enum UserRole
{
    Attendee,
    Organizer,
    SuperAdmin
}

public static class RoleNames
{
    public const string Attendee = nameof(UserRole.Attendee);
    public const string Organizer = nameof(UserRole.Organizer);
    public const string SuperAdmin = nameof(UserRole.SuperAdmin);

    public static readonly string[] All =
    [
        Attendee,
        Organizer,
        SuperAdmin
    ];

    public static string GetPrimaryRoleName(IEnumerable<string> roles)
    {
        var roleSet = new HashSet<string>(roles, StringComparer.OrdinalIgnoreCase);

        if (roleSet.Contains(SuperAdmin))
        {
            return SuperAdmin;
        }

        if (roleSet.Contains(Organizer))
        {
            return Organizer;
        }

        return Attendee;
    }
}

public enum EventVisibility
{
    Public,
    Private
}

public enum MediaAssetKind
{
    Image,
    Video,
    Document
}

public sealed class CurrentUser
{
    public string DisplayName { get; set; } = "Guest Explorer";
    public string Email { get; set; } = "guest@lumina.local";
    public UserRole Role { get; set; } = UserRole.Attendee;
    public bool IsGuest => string.Equals(Email, "guest@lumina.local", StringComparison.OrdinalIgnoreCase);
    public bool CanManageEvents => Role is UserRole.Organizer or UserRole.SuperAdmin;

    public static CurrentUser Guest => new();
}

public sealed class EventUser : IdentityUser<int>
{
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = RoleNames.Attendee;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class EventRecord
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string FeaturedLabel { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string TimeZone { get; set; } = "UTC+07:00";
    public string Status { get; set; } = "Draft";
    public EventVisibility Visibility { get; set; } = EventVisibility.Public;
    public int Capacity { get; set; }
    public decimal TicketPrice { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public string OrganizerEmail { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<EventSession> Sessions { get; set; } = [];
    public List<MediaAsset> MediaAssets { get; set; } = [];
    public List<MediaFolder> MediaFolders { get; set; } = [];
    public List<AttendeeRegistration> Registrations { get; set; } = [];
    public List<ActivityItem> RecentActivities { get; set; } = [];
    public List<DeadlineItem> Deadlines { get; set; } = [];

    public int TicketsSoldPercentage =>
        Capacity == 0 ? 0 : (int)Math.Round(Registrations.Count * 100m / Capacity, MidpointRounding.AwayFromZero);

    public decimal Revenue => Registrations.Sum(registration => registration.AmountPaid);

    public MediaAsset? CoverAsset => MediaAssets.FirstOrDefault(asset => asset.IsCover);

    public string DateRangeText =>
        StartDate == EndDate
            ? StartDate.ToString("MMM dd, yyyy")
            : $"{StartDate:MMM dd} - {EndDate:MMM dd, yyyy}";
}

public sealed class EventSession
{
    public int Id { get; set; }
    public int EventRecordId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Track { get; set; } = string.Empty;
    public string Speaker { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public bool IsLive { get; set; }
    public EventRecord? Event { get; set; }
}

public sealed class MediaAsset
{
    public int Id { get; set; }
    public int EventRecordId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public MediaAssetKind Kind { get; set; }
    public string Url { get; set; } = string.Empty;
    public string FolderName { get; set; } = string.Empty;
    public decimal SizeInMb { get; set; }
    public bool IsCover { get; set; }
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public EventRecord? Event { get; set; }
}

public sealed class MediaFolder
{
    public int Id { get; set; }
    public int EventRecordId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public EventRecord? Event { get; set; }
}

public sealed class AttendeeRegistration
{
    public int Id { get; set; }
    public int EventRecordId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string TicketType { get; set; } = "General Admission";
    public decimal AmountPaid { get; set; }
    public DateTime RegisteredAtUtc { get; set; } = DateTime.UtcNow;
    public EventRecord? Event { get; set; }
}

public sealed class ActivityItem
{
    public int Id { get; set; }
    public int EventRecordId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RelativeTime { get; set; } = string.Empty;
    public string Icon { get; set; } = "event";
    public EventRecord? Event { get; set; }
}

public sealed class DeadlineItem
{
    public int Id { get; set; }
    public int EventRecordId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DueText { get; set; } = string.Empty;
    public bool IsCritical { get; set; }
    public EventRecord? Event { get; set; }
}

public abstract class AppPageViewModel
{
    public CurrentUser CurrentUser { get; set; } = CurrentUser.Guest;
}

public sealed class HomePageViewModel : AppPageViewModel
{
    public IReadOnlyList<EventRecord> FeaturedEvents { get; set; } = [];
}

public sealed class UserDashboardViewModel : AppPageViewModel
{
    public IReadOnlyList<EventRecord> RegisteredEvents { get; set; } = [];
    public IReadOnlyList<EventRecord> RecommendedEvents { get; set; } = [];
}

public sealed class EventDetailsViewModel : AppPageViewModel
{
    public EventRecord Event { get; set; } = new();
    public RegistrationInput RegistrationForm { get; set; } = new();
    public bool AlreadyRegistered { get; set; }
}

public sealed class OrganizerDashboardViewModel : AppPageViewModel
{
    public IReadOnlyList<EventRecord> ManagedEvents { get; set; } = [];
    public EventRecord? FeaturedEvent { get; set; }
}

public sealed class ManageEventViewModel : AppPageViewModel
{
    public EventRecord Event { get; set; } = new();
}

public sealed class CreateEventPageViewModel : AppPageViewModel
{
    public CreateEventInput Form { get; set; } = new();
    public IReadOnlyList<string> Categories { get; set; } = [];
}

public sealed class SchedulePageViewModel : AppPageViewModel
{
    public EventRecord Event { get; set; } = new();
    public ScheduleInput Form { get; set; } = new();
}

public sealed class MediaPageViewModel : AppPageViewModel
{
    public EventRecord Event { get; set; } = new();
    public MediaUploadInput Form { get; set; } = new();
    public MediaFolderInput FolderForm { get; set; } = new();
}

public sealed class AssetLibraryViewModel : AppPageViewModel
{
    public EventRecord Event { get; set; } = new();
}

public sealed class PasswordResetConfirmationViewModel : AppPageViewModel
{
    public string Email { get; set; } = string.Empty;
    public string? ResetLink { get; set; }
}

public sealed class LoginPageViewModel : AppPageViewModel
{
    public LoginInput Form { get; set; } = new();
    public string? Next { get; set; }
}

public sealed class LoginInput
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; } = true;
}

public sealed class PasswordResetInput
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public sealed class RegistrationInput
{
    [Required]
    [StringLength(80)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [StringLength(120)]
    public string Company { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Ticket type")]
    public string TicketType { get; set; } = "General Admission";
}

public sealed class CreateEventInput
{
    [Required]
    [StringLength(120)]
    [Display(Name = "Event name")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(60)]
    public string Category { get; set; } = "Conference";

    [Required]
    [Display(Name = "Event date")]
    public DateOnly? StartDate { get; set; }

    [Required]
    [Display(Name = "Start time")]
    public TimeOnly? StartTime { get; set; }

    [Required]
    [Display(Name = "End time")]
    public TimeOnly? EndTime { get; set; }

    [Required]
    [StringLength(160)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [StringLength(160, MinimumLength = 40)]
    public string Summary { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 120)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Time zone")]
    public string TimeZone { get; set; } = "UTC+07:00";

    public EventVisibility Visibility { get; set; } = EventVisibility.Public;
}

public sealed class ScheduleInput
{
    [Required]
    [Display(Name = "Session title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateOnly? Date { get; set; }

    [Required]
    [Display(Name = "Start time")]
    public TimeOnly? StartTime { get; set; }

    [Required]
    [Display(Name = "End time")]
    public TimeOnly? EndTime { get; set; }

    [StringLength(120)]
    public string Speaker { get; set; } = string.Empty;

    [StringLength(80)]
    public string Room { get; set; } = string.Empty;

    [StringLength(60)]
    public string Track { get; set; } = "Main Track";

    public bool IsRecurring { get; set; }

    [Display(Name = "Recurrence")]
    public string RecurrencePattern { get; set; } = "Weekly";

    [Display(Name = "Ends on")]
    public DateOnly? RecurrenceEndDate { get; set; }

    public int? Occurrences { get; set; } = 1;
}

public sealed class MediaUploadInput : IValidatableObject
{
    [StringLength(255)]
    [Display(Name = "Asset name")]
    public string FileName { get; set; } = string.Empty;

    [Url]
    [Display(Name = "Asset URL")]
    public string Url { get; set; } = string.Empty;

    [Display(Name = "Upload file")]
    public IFormFile? UploadedFile { get; set; }

    [Required]
    public MediaAssetKind Kind { get; set; } = MediaAssetKind.Image;

    [Required]
    [Display(Name = "Folder")]
    public string FolderName { get; set; } = "General";

    [Range(0.1, 5000)]
    [Display(Name = "Size (MB)")]
    public decimal? SizeInMb { get; set; }

    [Display(Name = "Set as cover")]
    public bool SetAsCover { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var hasUploadedFile = UploadedFile is { Length: > 0 };
        var hasExternalUrl = !string.IsNullOrWhiteSpace(Url);

        if (!hasUploadedFile && !hasExternalUrl)
        {
            yield return new ValidationResult(
                "Choose a local file or provide an asset URL.",
                [nameof(UploadedFile), nameof(Url)]);
        }
    }
}

public sealed class MediaFolderInput
{
    [Required]
    [Display(Name = "Folder name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;
}
