using EventRegistrationSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrationSystem.Data;

public sealed class EventDbContext : IdentityDbContext<EventUser, IdentityRole<int>, int>
{
    public EventDbContext(DbContextOptions<EventDbContext> options)
        : base(options)
    {
    }

    public DbSet<EventUser> EventUsers => Set<EventUser>();
    public DbSet<EventRecord> EventRecords => Set<EventRecord>();
    public DbSet<EventSession> EventSessions => Set<EventSession>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<MediaFolder> MediaFolders => Set<MediaFolder>();
    public DbSet<AttendeeRegistration> AttendeeRegistrations => Set<AttendeeRegistration>();
    public DbSet<ActivityItem> ActivityItems => Set<ActivityItem>();
    public DbSet<DeadlineItem> DeadlineItems => Set<DeadlineItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EventUser>(entity =>
        {
            entity.ToTable("event_users");
            entity.Property(user => user.DisplayName).HasMaxLength(120).IsRequired();
            entity.Property(user => user.Role).HasMaxLength(32).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(255);
            entity.Property(user => user.UserName).HasMaxLength(255);
            entity.Property(user => user.NormalizedEmail).HasMaxLength(255);
            entity.Property(user => user.NormalizedUserName).HasMaxLength(255);
            entity.HasIndex(user => user.Email).IsUnique();
        });

        modelBuilder.Entity<IdentityRole<int>>(entity =>
        {
            entity.ToTable("identity_roles");
        });

        modelBuilder.Entity<IdentityUserRole<int>>(entity =>
        {
            entity.ToTable("identity_user_roles");
        });

        modelBuilder.Entity<IdentityUserClaim<int>>(entity =>
        {
            entity.ToTable("identity_user_claims");
        });

        modelBuilder.Entity<IdentityUserLogin<int>>(entity =>
        {
            entity.ToTable("identity_user_logins");
        });

        modelBuilder.Entity<IdentityUserToken<int>>(entity =>
        {
            entity.ToTable("identity_user_tokens");
        });

        modelBuilder.Entity<IdentityRoleClaim<int>>(entity =>
        {
            entity.ToTable("identity_role_claims");
        });

        modelBuilder.Entity<EventRecord>(entity =>
        {
            entity.ToTable("event_records");
            entity.Property(record => record.Title).HasMaxLength(120).IsRequired();
            entity.Property(record => record.Category).HasMaxLength(60).IsRequired();
            entity.Property(record => record.FeaturedLabel).HasMaxLength(80);
            entity.Property(record => record.Summary).HasMaxLength(160).IsRequired();
            entity.Property(record => record.Description).HasMaxLength(2000).IsRequired();
            entity.Property(record => record.Location).HasMaxLength(160).IsRequired();
            entity.Property(record => record.TimeZone).HasMaxLength(64).IsRequired();
            entity.Property(record => record.Status).HasMaxLength(40).IsRequired();
            entity.Property(record => record.Visibility).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(record => record.OrganizerName).HasMaxLength(120).IsRequired();
            entity.Property(record => record.OrganizerEmail).HasMaxLength(255).IsRequired();
            entity.Property(record => record.CoverImageUrl).HasMaxLength(2048);
            entity.Property(record => record.TicketPrice).HasPrecision(12, 2);
            entity.HasIndex(record => record.OrganizerEmail);
            entity.HasIndex(record => new { record.Visibility, record.StartDate });
        });

        modelBuilder.Entity<EventSession>(entity =>
        {
            entity.ToTable("event_sessions");
            entity.Property(session => session.Title).HasMaxLength(200).IsRequired();
            entity.Property(session => session.Track).HasMaxLength(60).IsRequired();
            entity.Property(session => session.Speaker).HasMaxLength(120);
            entity.Property(session => session.Room).HasMaxLength(80);
            entity.HasOne(session => session.Event)
                .WithMany(record => record.Sessions)
                .HasForeignKey(session => session.EventRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MediaAsset>(entity =>
        {
            entity.ToTable("media_assets");
            entity.Property(asset => asset.FileName).HasMaxLength(255).IsRequired();
            entity.Property(asset => asset.Kind).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(asset => asset.Url).HasMaxLength(2048).IsRequired();
            entity.Property(asset => asset.FolderName).HasMaxLength(120).IsRequired();
            entity.Property(asset => asset.Description).HasMaxLength(255);
            entity.Property(asset => asset.SizeInMb).HasPrecision(12, 2);
            entity.HasOne(asset => asset.Event)
                .WithMany(record => record.MediaAssets)
                .HasForeignKey(asset => asset.EventRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MediaFolder>(entity =>
        {
            entity.ToTable("media_folders");
            entity.Property(folder => folder.Name).HasMaxLength(120).IsRequired();
            entity.Property(folder => folder.Description).HasMaxLength(255);
            entity.HasOne(folder => folder.Event)
                .WithMany(record => record.MediaFolders)
                .HasForeignKey(folder => folder.EventRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AttendeeRegistration>(entity =>
        {
            entity.ToTable("attendee_registrations");
            entity.Property(registration => registration.FullName).HasMaxLength(80).IsRequired();
            entity.Property(registration => registration.Email).HasMaxLength(255).IsRequired();
            entity.Property(registration => registration.Company).HasMaxLength(120);
            entity.Property(registration => registration.TicketType).HasMaxLength(60).IsRequired();
            entity.Property(registration => registration.AmountPaid).HasPrecision(12, 2);
            entity.HasIndex(registration => new { registration.EventRecordId, registration.Email }).IsUnique();
            entity.HasOne(registration => registration.Event)
                .WithMany(record => record.Registrations)
                .HasForeignKey(registration => registration.EventRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ActivityItem>(entity =>
        {
            entity.ToTable("activity_items");
            entity.Property(activity => activity.Title).HasMaxLength(120).IsRequired();
            entity.Property(activity => activity.Description).HasMaxLength(255).IsRequired();
            entity.Property(activity => activity.RelativeTime).HasMaxLength(40).IsRequired();
            entity.Property(activity => activity.Icon).HasMaxLength(40).IsRequired();
            entity.HasOne(activity => activity.Event)
                .WithMany(record => record.RecentActivities)
                .HasForeignKey(activity => activity.EventRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DeadlineItem>(entity =>
        {
            entity.ToTable("deadline_items");
            entity.Property(deadline => deadline.Title).HasMaxLength(120).IsRequired();
            entity.Property(deadline => deadline.DueText).HasMaxLength(80).IsRequired();
            entity.HasOne(deadline => deadline.Event)
                .WithMany(record => record.Deadlines)
                .HasForeignKey(deadline => deadline.EventRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
