using EventRegistrationSystem.Models;

namespace EventRegistrationSystem.Services;

public static class DemoEventData
{
    public static List<EventRecord> CreateSeedEvents()
    {
        var nextSessionId = 100;
        var nextMediaId = 200;
        var nextFolderId = 300;
        var nextRegistrationId = 400;

        var summit = new EventRecord
        {
            Id = 1,
            Title = "Global Tech Summit 2026",
            Category = "Conference",
            FeaturedLabel = "Annual flagship event",
            Summary = "Three days of architecture, AI, cloud, and product leadership sessions.",
            Description = "Global Tech Summit 2026 is a high-energy gathering for technology leaders, architects, and curious builders. The program blends keynote sessions, hands-on workshops, and curated networking so attendees can leave with practical ideas and meaningful partnerships.",
            Location = "Phnom Penh, Sen Sok",
            TimeZone = "UTC+07:00",
            Status = "Live",
            Visibility = EventVisibility.Public,
            Capacity = 1500,
            TicketPrice = 249m,
            OrganizerName = "Semistash Semio",
            OrganizerEmail = "organizer@semistash.io",
            CoverImageUrl = "https://images.unsplash.com/photo-1511578314322-379afb476865?auto=format&fit=crop&w=1200&q=80",
            StartDate = new DateOnly(2026, 10, 24),
            EndDate = new DateOnly(2026, 10, 26),
            StartTime = new TimeOnly(8, 30),
            EndTime = new TimeOnly(18, 0),
            UpdatedAtUtc = DateTime.UtcNow.AddHours(-2)
        };

        summit.Sessions.AddRange(
        [
            new EventSession
            {
                Id = nextSessionId++,
                Date = new DateOnly(2026, 10, 24),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                Title = "Opening Keynote: Building the Resilient Stack",
                Track = "Keynote",
                Speaker = "Marcus Thorne",
                Room = "Grand Ballroom A"
            },
            new EventSession
            {
                Id = nextSessionId++,
                Date = new DateOnly(2026, 10, 24),
                StartTime = new TimeOnly(10, 30),
                EndTime = new TimeOnly(11, 15),
                Title = "Sustainable Compute: Powering the Next Decade",
                Track = "Panel Discussion",
                Speaker = "Sarah Chen and Elias Volt",
                Room = "Stage 04"
            },
            new EventSession
            {
                Id = nextSessionId++,
                Date = new DateOnly(2026, 10, 24),
                StartTime = new TimeOnly(12, 0),
                EndTime = new TimeOnly(13, 30),
                Title = "Cloud Native Security Masterclass",
                Track = "Interactive Workshop",
                Speaker = "David Kim",
                Room = "Workshop Room 12",
                IsLive = true
            }
        ]);

        summit.MediaFolders.AddRange(
        [
            new MediaFolder { Id = nextFolderId++, Name = "Hero", Description = "Cover and landing visuals", CreatedAtUtc = DateTime.UtcNow.AddDays(-7) },
            new MediaFolder { Id = nextFolderId++, Name = "Social Media Kit", Description = "Promo assets for partners", CreatedAtUtc = DateTime.UtcNow.AddDays(-5) },
            new MediaFolder { Id = nextFolderId++, Name = "Stage Design", Description = "Production visuals", CreatedAtUtc = DateTime.UtcNow.AddDays(-4) }
        ]);

        summit.MediaAssets.AddRange(
        [
            new MediaAsset
            {
                Id = nextMediaId++,
                FileName = "Stage_Design_Concept.jpg",
                Kind = MediaAssetKind.Image,
                Url = "https://images.unsplash.com/photo-1505373877841-8d25f7d46678?auto=format&fit=crop&w=1200&q=80",
                FolderName = "Hero",
                SizeInMb = 4.8m,
                IsCover = true,
                UploadedAtUtc = DateTime.UtcNow.AddHours(-5),
                Description = "Primary event cover"
            },
            new MediaAsset
            {
                Id = nextMediaId++,
                FileName = "Keynote_Opening_Final.mp4",
                Kind = MediaAssetKind.Video,
                Url = "https://images.unsplash.com/photo-1492684223066-81342ee5ff30?auto=format&fit=crop&w=1200&q=80",
                FolderName = "Stage Design",
                SizeInMb = 142.5m,
                UploadedAtUtc = DateTime.UtcNow.AddHours(-2),
                Description = "Opening cinematic teaser"
            },
            new MediaAsset
            {
                Id = nextMediaId++,
                FileName = "Gala_Table_Setup.png",
                Kind = MediaAssetKind.Image,
                Url = "https://images.unsplash.com/photo-1517457373958-b7bdd4587205?auto=format&fit=crop&w=1200&q=80",
                FolderName = "Social Media Kit",
                SizeInMb = 8.2m,
                UploadedAtUtc = DateTime.UtcNow.AddHours(-1),
                Description = "Social promo still"
            }
        ]);

        summit.RecentActivities.AddRange(
        [
            new ActivityItem
            {
                Title = "New attendee registration",
                Description = "Sarah Jenkins registered for Full Pass.",
                RelativeTime = "4m ago",
                Icon = "person_add"
            },
            new ActivityItem
            {
                Title = "Payout successful",
                Description = "Transfer of $12,450 to bank account completed.",
                RelativeTime = "2h ago",
                Icon = "payments"
            },
            new ActivityItem
            {
                Title = "Speaker details updated",
                Description = "Dr. Aris Thorne updated the keynote abstract.",
                RelativeTime = "5h ago",
                Icon = "edit_square"
            }
        ]);

        summit.Deadlines.AddRange(
        [
            new DeadlineItem { Title = "Finalize catering contract", DueText = "Due in 4 hours", IsCritical = true },
            new DeadlineItem { Title = "Speaker orientation video", DueText = "Due tomorrow, 10:00 AM", IsCritical = false },
            new DeadlineItem { Title = "Early bird ticket closing", DueText = "Due Oct 12, 2026", IsCritical = false }
        ]);

        summit.Registrations.AddRange(
        [
            new AttendeeRegistration
            {
                Id = nextRegistrationId++,
                FullName = "Taylor Rivers",
                Email = DemoUserData.AttendeeEmail,
                Company = "Northstar Labs",
                TicketType = "Full Pass",
                AmountPaid = 249m,
                RegisteredAtUtc = DateTime.UtcNow.AddDays(-7)
            },
            new AttendeeRegistration
            {
                Id = nextRegistrationId++,
                FullName = "Rina Sok",
                Email = "rina@futuregrid.io",
                Company = "FutureGrid",
                TicketType = "VIP Pass",
                AmountPaid = 369m,
                RegisteredAtUtc = DateTime.UtcNow.AddDays(-6)
            }
        ]);

        var designForum = new EventRecord
        {
            Id = 2,
            Title = "Design Futures Forum",
            Category = "Workshop",
            FeaturedLabel = "Curated roundtable",
            Summary = "A single-day experience for brand, product, and service designers shaping immersive events.",
            Description = "Design Futures Forum explores the crossover between brand strategy, spatial storytelling, and attendee experience. It is built for small-group conversation and practical takeaways.",
            Location = "Siem Reap, Heritage District",
            TimeZone = "UTC+07:00",
            Status = "Registration Open",
            Visibility = EventVisibility.Public,
            Capacity = 220,
            TicketPrice = 95m,
            OrganizerName = "Semistash Semio",
            OrganizerEmail = "organizer@semistash.io",
            CoverImageUrl = "https://images.unsplash.com/photo-1528605248644-14dd04022da1?auto=format&fit=crop&w=1200&q=80",
            StartDate = new DateOnly(2026, 6, 18),
            EndDate = new DateOnly(2026, 6, 18),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(16, 30),
            UpdatedAtUtc = DateTime.UtcNow.AddDays(-1)
        };

        designForum.Sessions.Add(new EventSession
        {
            Id = nextSessionId++,
            Date = designForum.StartDate,
            StartTime = new TimeOnly(10, 30),
            EndTime = new TimeOnly(11, 45),
            Title = "Editorial Storytelling in Event Interfaces",
            Track = "Workshop",
            Speaker = "Nadia Lor",
            Room = "Studio Hall"
        });

        designForum.MediaFolders.Add(new MediaFolder
        {
            Id = nextFolderId++,
            Name = "Workshop Deck",
            Description = "Slides and reference imagery",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-2)
        });

        designForum.MediaAssets.Add(new MediaAsset
        {
            Id = nextMediaId++,
            FileName = "Workshop_Board.jpg",
            Kind = MediaAssetKind.Image,
            Url = "https://images.unsplash.com/photo-1497366754035-f200968a6e72?auto=format&fit=crop&w=1200&q=80",
            FolderName = "Workshop Deck",
            SizeInMb = 3.1m,
            IsCover = true,
            UploadedAtUtc = DateTime.UtcNow.AddDays(-2),
            Description = "Promo board"
        });

        designForum.RecentActivities.Add(new ActivityItem
        {
            Title = "Agenda finalized",
            Description = "The workshop run-of-show was published to the team.",
            RelativeTime = "yesterday",
            Icon = "draft_orders"
        });

        designForum.Deadlines.Add(new DeadlineItem
        {
            Title = "Confirm printed materials",
            DueText = "Due in 2 days",
            IsCritical = false
        });

        var sustainability = new EventRecord
        {
            Id = 3,
            Title = "Sustainability Leaders Breakfast",
            Category = "Networking",
            FeaturedLabel = "Invite-only salon",
            Summary = "An executive breakfast pairing sustainability operators with sponsors and venue partners.",
            Description = "This breakfast series creates a quiet environment for senior operators to compare reporting practices, vendor frameworks, and sponsorship models tied to measurable sustainability goals.",
            Location = "Phnom Penh, Riverside Pavilion",
            TimeZone = "UTC+07:00",
            Status = "Invite Only",
            Visibility = EventVisibility.Private,
            Capacity = 80,
            TicketPrice = 0m,
            OrganizerName = "Semistash Semio",
            OrganizerEmail = "organizer@semistash.io",
            CoverImageUrl = "https://images.unsplash.com/photo-1511795409834-ef04bbd61622?auto=format&fit=crop&w=1200&q=80",
            StartDate = new DateOnly(2026, 7, 12),
            EndDate = new DateOnly(2026, 7, 12),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(10, 30),
            UpdatedAtUtc = DateTime.UtcNow.AddHours(-8)
        };

        sustainability.Sessions.Add(new EventSession
        {
            Id = nextSessionId++,
            Date = sustainability.StartDate,
            StartTime = new TimeOnly(8, 30),
            EndTime = new TimeOnly(9, 15),
            Title = "Circular procurement and live event operations",
            Track = "Roundtable",
            Speaker = "Maya Chen",
            Room = "Riverside Pavilion"
        });

        sustainability.Deadlines.Add(new DeadlineItem
        {
            Title = "Finalize guest list",
            DueText = "Due next week",
            IsCritical = true
        });

        return [summit, designForum, sustainability];
    }
}
