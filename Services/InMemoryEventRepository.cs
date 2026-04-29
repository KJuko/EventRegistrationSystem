using EventRegistrationSystem.Models;

namespace EventRegistrationSystem.Services;

public sealed class InMemoryEventRepository : IEventRepository
{
    private readonly List<EventRecord> _events;
    private int _nextEventId = 10;
    private int _nextSessionId = 100;
    private int _nextMediaId = 200;
    private int _nextFolderId = 300;
    private int _nextRegistrationId = 400;

    public InMemoryEventRepository()
    {
        _events = DemoEventData.CreateSeedEvents();
    }

    public IReadOnlyList<EventRecord> GetPublicEvents() =>
        _events
            .Where(eventRecord => eventRecord.Visibility == EventVisibility.Public)
            .OrderBy(eventRecord => eventRecord.StartDate)
            .ToList();

    public IReadOnlyList<EventRecord> GetManagedEvents(CurrentUser currentUser) =>
        (currentUser.Role == UserRole.SuperAdmin
            ? _events
            : _events.Where(eventRecord => string.Equals(eventRecord.OrganizerEmail, currentUser.Email, StringComparison.OrdinalIgnoreCase)))
        .OrderBy(eventRecord => eventRecord.StartDate)
        .ToList();

    public IReadOnlyList<EventRecord> GetRegisteredEvents(string attendeeEmail) =>
        _events
            .Where(eventRecord => eventRecord.Registrations.Any(registration =>
                string.Equals(registration.Email, attendeeEmail, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(eventRecord => eventRecord.StartDate)
            .ToList();

    public EventRecord? GetEvent(int id) => _events.FirstOrDefault(eventRecord => eventRecord.Id == id);

    public EventRecord CreateEvent(CreateEventInput input, CurrentUser creator)
    {
        var createdEvent = new EventRecord
        {
            Id = _nextEventId++,
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
            UpdatedAtUtc = DateTime.UtcNow
        };

        createdEvent.MediaFolders.Add(new MediaFolder
        {
            Id = _nextFolderId++,
            Name = "General",
            Description = "Default library for event assets",
            CreatedAtUtc = DateTime.UtcNow
        });

        createdEvent.RecentActivities.Add(new ActivityItem
        {
            Icon = "edit_square",
            Title = "Event created",
            Description = $"{creator.DisplayName} drafted the event shell.",
            RelativeTime = "just now"
        });

        createdEvent.Deadlines.Add(new DeadlineItem
        {
            Title = "Publish event page",
            DueText = "Before registration opens",
            IsCritical = true
        });

        _events.Add(createdEvent);
        return createdEvent;
    }

    public bool RegisterAttendee(int eventId, RegistrationInput input)
    {
        var eventRecord = GetEvent(eventId);
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
            Id = _nextRegistrationId++,
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
        return true;
    }

    public void AddSession(int eventId, ScheduleInput input)
    {
        var eventRecord = GetEvent(eventId);
        if (eventRecord is null)
        {
            return;
        }

        eventRecord.Sessions.Add(new EventSession
        {
            Id = _nextSessionId++,
            Title = input.Title,
            Date = input.Date ?? eventRecord.StartDate,
            StartTime = input.StartTime ?? eventRecord.StartTime,
            EndTime = input.EndTime ?? eventRecord.EndTime,
            Speaker = input.Speaker,
            Room = input.Room,
            Track = input.Track,
            IsLive = false
        });

        eventRecord.Sessions.Sort((left, right) =>
        {
            var dateComparison = left.Date.CompareTo(right.Date);
            return dateComparison != 0 ? dateComparison : left.StartTime.CompareTo(right.StartTime);
        });

        eventRecord.RecentActivities.Insert(0, new ActivityItem
        {
            Icon = "schedule",
            Title = "Schedule updated",
            Description = $"{input.Title} was added to the agenda.",
            RelativeTime = "just now"
        });

        eventRecord.UpdatedAtUtc = DateTime.UtcNow;
    }

    public void AddMediaAsset(int eventId, MediaUploadInput input)
    {
        var eventRecord = GetEvent(eventId);
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
            Id = _nextMediaId++,
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
                Id = _nextFolderId++,
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
    }

    public void AddMediaFolder(int eventId, MediaFolderInput input)
    {
        var eventRecord = GetEvent(eventId);
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
            Id = _nextFolderId++,
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
    }

    public void SetCoverAsset(int eventId, int assetId)
    {
        var eventRecord = GetEvent(eventId);
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
    }
}
