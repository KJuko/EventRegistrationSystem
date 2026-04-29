using EventRegistrationSystem.Models;

namespace EventRegistrationSystem.Services;

public static class DemoUserData
{
    public const string SuperAdminEmail = "superadmin@semistash.io";
    public const string SuperAdminPassword = "Admin@123";
    public const string OrganizerEmail = "organizer@semistash.io";
    public const string OrganizerPassword = "Organizer@123";
    public const string AttendeeEmail = "attendee@semistash.io";
    public const string AttendeePassword = "Attendee@123";

    public static IReadOnlyList<DemoUserSeed> GetSeedUsers() =>
    [
        new DemoUserSeed(
            "Platform Super Admin",
            SuperAdminEmail,
            SuperAdminPassword,
            [RoleNames.SuperAdmin]),
        new DemoUserSeed(
            "Semistash Organizer",
            OrganizerEmail,
            OrganizerPassword,
            [RoleNames.Organizer]),
        new DemoUserSeed(
            "Taylor Rivers",
            AttendeeEmail,
            AttendeePassword,
            [RoleNames.Attendee])
    ];
}

public sealed record DemoUserSeed(
    string DisplayName,
    string Email,
    string Password,
    IReadOnlyList<string> Roles);
