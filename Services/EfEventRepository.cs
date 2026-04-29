using EventRegistrationSystem.Data;
using EventRegistrationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrationSystem.Services;

public sealed class EfEventRepository : IEventRepository
{
    private readonly EventDbContext _context;

    public EfEventRepository(EventDbContext context)
    {
        _context = context;
    }

    public IReadOnlyList<EventRecord> GetPublicEvents() =>
        QueryEvents()
            .Where(record => record.Visibility == EventVisibility.Public)
            .OrderBy(record => record.StartDate)
            .ToList();

    public IReadOnlyList<EventRecord> GetManagedEvents(CurrentUser currentUser)
    {
        var baseQuery = QueryEvents().OrderBy(record => record.StartDate);

        return currentUser.Role == UserRole.SuperAdmin
            ? baseQuery.ToList()
            : baseQuery
                .Where(record => record.OrganizerEmail.ToLower() == currentUser.Email.ToLower())
                .ToList();
    }

    public IReadOnlyList<EventRecord> GetRegisteredEvents(string attendeeEmail) =>
        QueryEvents()
            .Where(record => record.Registrations.Any(registration => registration.Email.ToLower() == attendeeEmail.ToLower()))
            .OrderBy(record => record.StartDate)
            .ToList();

    public EventRecord? GetEvent(int id) =>
        QueryEvents().FirstOrDefault(record => record.Id == id);

    public EventRecord CreateEvent(CreateEventInput input, CurrentUser creator)
    {
        var createdEvent = new EventRecord
        {
            Title = input.Title,
            Category = input.Category,
            FeaturedLabel = "Freshly curated",
            Summary = input.Summary,
            Description = input.Description,
            Location = input.Location,
            TimeZone = input.TimeZone,
            Status = "Draft",
            Visibility = input.Visibility,
            Capacity = 250,
            TicketPrice = 89m,
            OrganizerName = creator.DisplayName,
            OrganizerEmail = creator.Email,
            CoverImageUrl = "https://images.unsplash.com/photo-1511578314322-379afb476865?auto=format&fit=crop&w=1200&q=80",
            StartDate = input.StartDate ?? DateOnly.FromDateTime(DateTime.Today),
            EndDate = input.StartDate ?? DateOnly.FromDateTime(DateTime.Today),
            StartTime = input.StartTime ?? new TimeOnly(9, 0),
            EndTime = input.EndTime ?? new TimeOnly(17, 0),
            UpdatedAtUtc = DateTime.UtcNow,
            MediaFolders =
            [
                new MediaFolder
                {
                    Name = "General",
                    Description = "Default library for event assets",
                    CreatedAtUtc = DateTime.UtcNow
                }
            ],
            RecentActivities =
            [
                new ActivityItem
                {
                    Icon = "edit_square",
                    Title = "Event created",
                    Description = $"{creator.DisplayName} drafted the event shell.",
                    RelativeTime = "just now"
                }
            ],
            Deadlines =
            [
                new DeadlineItem
                {
                    Title = "Publish event page",
                    DueText = "Before registration opens",
                    IsCritical = true
                }
            ]
        };

        _context.EventRecords.Add(createdEvent);
        _context.SaveChanges();
        return GetEvent(createdEvent.Id)!;
    }

    public bool RegisterAttendee(int eventId, RegistrationInput input)
    {
        var eventRecord = _context.EventRecords
            .Include(record => record.Registrations)
            .Include(record => record.RecentActivities)
            .FirstOrDefault(record => record.Id == eventId);

        if (eventRecord is null)
        {
            return false;
        }

        var alreadyRegistered = eventRecord.Registrations.Any(registration =>
            string.Equals(registration.Email, input.Email, StringComparison.OrdinalIgnoreCase));

        if (alreadyRegistered)
        {
            return false;
        }

        eventRecord.Registrations.Add(new AttendeeRegistration
        {
            FullName = input.FullName,
            Email = input.Email,
            Company = input.Company,
            TicketType = input.TicketType,
            AmountPaid = input.TicketType.Contains("VIP", StringComparison.OrdinalIgnoreCase)
                ? eventRecord.TicketPrice + 120m
                : eventRecord.TicketPrice,
            RegisteredAtUtc = DateTime.UtcNow
        });

        eventRecord.RecentActivities.Insert(0, new ActivityItem
        {
            Icon = "person_add",
            Title = "New attendee registration",
            Description = $"{input.FullName} registered for {input.TicketType}.",
            RelativeTime = "moments ago"
        });

        eventRecord.UpdatedAtUtc = DateTime.UtcNow;
        _context.SaveChanges();
        return true;
    }

    public void AddSession(int eventId, ScheduleInput input)
    {
        var eventRecord = _context.EventRecords
            .Include(record => record.Sessions)
            .Include(record => record.RecentActivities)
            .FirstOrDefault(record => record.Id == eventId);

        if (eventRecord is null)
        {
            return;
        }

        eventRecord.Sessions.Add(new EventSession
        {
            Title = input.Title,
            Date = input.Date ?? eventRecord.StartDate,
            StartTime = input.StartTime ?? eventRecord.StartTime,
            EndTime = input.EndTime ?? eventRecord.EndTime,
            Speaker = input.Speaker,
            Room = input.Room,
            Track = input.Track,
            IsLive = false
        });

        eventRecord.RecentActivities.Insert(0, new ActivityItem
        {
            Icon = "schedule",
            Title = "Schedule updated",
            Description = $"{input.Title} was added to the agenda.",
            RelativeTime = "just now"
        });

        eventRecord.UpdatedAtUtc = DateTime.UtcNow;
        _context.SaveChanges();
    }

    public void AddMediaAsset(int eventId, MediaUploadInput input)
    {
        var eventRecord = _context.EventRecords
            .Include(record => record.MediaAssets)
            .Include(record => record.MediaFolders)
            .Include(record => record.RecentActivities)
            .FirstOrDefault(record => record.Id == eventId);

        if (eventRecord is null)
        {
            return;
        }

        if (input.SetAsCover)
        {
            foreach (var asset in eventRecord.MediaAssets)
            {
                asset.IsCover = false;
            }
        }

        eventRecord.MediaAssets.Insert(0, new MediaAsset
        {
            FileName = input.FileName,
            Url = input.Url,
            FolderName = input.FolderName,
            Kind = input.Kind,
            SizeInMb = input.SizeInMb ?? 0.1m,
            IsCover = input.SetAsCover,
            UploadedAtUtc = DateTime.UtcNow,
            Description = $"{input.Kind} asset uploaded to {input.FolderName}"
        });

        if (input.SetAsCover)
        {
            eventRecord.CoverImageUrl = input.Url;
        }

        if (eventRecord.MediaFolders.All(folder => !string.Equals(folder.Name, input.FolderName, StringComparison.OrdinalIgnoreCase)))
        {
            eventRecord.MediaFolders.Add(new MediaFolder
            {
                Name = input.FolderName,
                Description = "Auto-created from upload",
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        eventRecord.RecentActivities.Insert(0, new ActivityItem
        {
            Icon = "perm_media",
            Title = "Media uploaded",
            Description = $"{input.FileName} was added to {input.FolderName}.",
            RelativeTime = "just now"
        });

        eventRecord.UpdatedAtUtc = DateTime.UtcNow;
        _context.SaveChanges();
    }

    public void AddMediaFolder(int eventId, MediaFolderInput input)
    {
        var eventRecord = _context.EventRecords
            .Include(record => record.MediaFolders)
            .Include(record => record.RecentActivities)
            .FirstOrDefault(record => record.Id == eventId);

        if (eventRecord is null)
        {
            return;
        }

        var alreadyExists = eventRecord.MediaFolders.Any(folder =>
            string.Equals(folder.Name, input.Name, StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
        {
            return;
        }

        eventRecord.MediaFolders.Add(new MediaFolder
        {
            Name = input.Name,
            Description = input.Description,
            CreatedAtUtc = DateTime.UtcNow
        });

        eventRecord.RecentActivities.Insert(0, new ActivityItem
        {
            Icon = "create_new_folder",
            Title = "New folder created",
            Description = $"{input.Name} is ready for organizing assets.",
            RelativeTime = "just now"
        });

        eventRecord.UpdatedAtUtc = DateTime.UtcNow;
        _context.SaveChanges();
    }

    public void SetCoverAsset(int eventId, int assetId)
    {
        var eventRecord = _context.EventRecords
            .Include(record => record.MediaAssets)
            .Include(record => record.RecentActivities)
            .FirstOrDefault(record => record.Id == eventId);

        if (eventRecord is null)
        {
            return;
        }

        foreach (var asset in eventRecord.MediaAssets)
        {
            asset.IsCover = asset.Id == assetId;
            if (asset.IsCover)
            {
                eventRecord.CoverImageUrl = asset.Url;
            }
        }

        eventRecord.RecentActivities.Insert(0, new ActivityItem
        {
            Icon = "image",
            Title = "Cover image updated",
            Description = "The event cover now reflects the latest selected asset.",
            RelativeTime = "just now"
        });

        eventRecord.UpdatedAtUtc = DateTime.UtcNow;
        _context.SaveChanges();
    }

    private IQueryable<EventRecord> QueryEvents() =>
        _context.EventRecords
            .Include(record => record.Sessions)
            .Include(record => record.MediaAssets)
            .Include(record => record.MediaFolders)
            .Include(record => record.Registrations)
            .Include(record => record.RecentActivities)
            .Include(record => record.Deadlines)
            .AsSplitQuery();
}
