using EventRegistrationSystem.Models;
using EventRegistrationSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace EventRegistrationSystem.Controllers;

public sealed class OrganizerController : AppController
{
    private static readonly string[] Categories =
    [
        "Conference",
        "Workshop",
        "Networking",
        "Training",
        "Launch Event",
        "Fundraiser"
    ];

    private readonly IEventRepository _repository;
    private readonly IAssetStorageService _assetStorageService;

    public OrganizerController(IEventRepository repository, IAssetStorageService assetStorageService)
    {
        _repository = repository;
        _assetStorageService = assetStorageService;
    }

    [HttpGet("organizer")]
    public IActionResult Dashboard()
    {
        var guard = RequireOrganizer();
        if (guard is not null)
        {
            return guard;
        }

        var managedEvents = _repository.GetManagedEvents(CurrentUser);
        return View(new OrganizerDashboardViewModel
        {
            CurrentUser = CurrentUser,
            ManagedEvents = managedEvents,
            FeaturedEvent = managedEvents.FirstOrDefault()
        });
    }

    [HttpGet("organizer/events/create")]
    public IActionResult Create()
    {
        var guard = RequireOrganizer();
        if (guard is not null)
        {
            return guard;
        }

        return View(BuildCreateModel(new CreateEventInput
        {
            StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            TimeZone = "UTC+07:00"
        }));
    }

    [HttpPost("organizer/events/create")]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateEventPageViewModel model)
    {
        var guard = RequireOrganizer();
        if (guard is not null)
        {
            return guard;
        }

        if (!ModelState.IsValid)
        {
            model.CurrentUser = CurrentUser;
            model.Categories = Categories;
            return View(model);
        }

        var createdEvent = _repository.CreateEvent(model.Form, CurrentUser);
        TempData["FlashMessage"] = $"\"{createdEvent.Title}\" has been created and saved as a draft.";
        return RedirectToAction(nameof(Manage), new { id = createdEvent.Id });
    }

    [HttpGet("organizer/events/{id:int}")]
    public IActionResult Manage(int id)
    {
        var guard = RequireOrganizer();
        if (guard is not null)
        {
            return guard;
        }

        var eventRecord = _repository.GetEvent(id);
        if (eventRecord is null)
        {
            return NotFound();
        }

        return View(new ManageEventViewModel
        {
            CurrentUser = CurrentUser,
            Event = eventRecord
        });
    }

    [HttpGet("organizer/events/{id:int}/schedule")]
    public IActionResult Schedule(int id)
    {
        var guard = RequireOrganizer();
        if (guard is not null)
        {
            return guard;
        }

        var eventRecord = _repository.GetEvent(id);
        if (eventRecord is null)
        {
            return NotFound();
        }

        return View(BuildScheduleModel(eventRecord, new ScheduleInput
        {
            Date = eventRecord.StartDate,
            StartTime = eventRecord.StartTime,
            EndTime = eventRecord.EndTime,
            RecurrencePattern = "Weekly",
            Occurrences = 1
        }));
    }

    [HttpPost("organizer/events/{id:int}/schedule")]
    [ValidateAntiForgeryToken]
    public IActionResult Schedule(int id, [Bind(Prefix = "Form")] ScheduleInput form)
    {
        var guard = RequireOrganizer();
        if (guard is not null)
        {
            return guard;
        }

        var eventRecord = _repository.GetEvent(id);
        if (eventRecord is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(BuildScheduleModel(eventRecord, form));
        }

        _repository.AddSession(id, form);
        TempData["FlashMessage"] = "The event schedule has been updated.";
        return RedirectToAction(nameof(Schedule), new { id });
    }

    [HttpGet("organizer/events/{id:int}/media")]
    public IActionResult Media(int id)
    {
        var guard = RequireOrganizer();
        if (guard is not null)
        {
            return guard;
        }

        var eventRecord = _repository.GetEvent(id);
        if (eventRecord is null)
        {
            return NotFound();
        }

        return View(BuildMediaModel(eventRecord, new MediaUploadInput
        {
            FolderName = eventRecord.MediaFolders.FirstOrDefault()?.Name ?? "General"
        }, new MediaFolderInput()));
    }

    [HttpPost("organizer/events/{id:int}/media")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Media(int id, [Bind(Prefix = "Form")] MediaUploadInput form)
    {
        var guard = RequireOrganizer();
        if (guard is not null)
        {
            return guard;
        }

        var eventRecord = _repository.GetEvent(id);
        if (eventRecord is null)
        {
            return NotFound();
        }

        if (form.UploadedFile is { Length: > 0 } && string.IsNullOrWhiteSpace(form.FileName))
        {
            form.FileName = Path.GetFileName(form.UploadedFile.FileName);
        }

        if (!ModelState.IsValid)
        {
            return View(BuildMediaModel(eventRecord, form, new MediaFolderInput()));
        }

        if (form.UploadedFile is { Length: > 0 })
        {
            try
            {
                var storedAsset = await _assetStorageService.SaveEventAssetAsync(id, form.UploadedFile);
                form.Url = storedAsset.PublicUrl;
                form.FileName = string.IsNullOrWhiteSpace(form.FileName) ? storedAsset.OriginalFileName : form.FileName;
                form.SizeInMb = storedAsset.SizeInMb;
            }
            catch (Exception exception)
            {
                ModelState.AddModelError(nameof(form.UploadedFile), $"Unable to save the uploaded file. {exception.Message}");
                return View(BuildMediaModel(eventRecord, form, new MediaFolderInput()));
            }
        }
        else
        {
            form.FileName = NormalizeExternalFileName(form.FileName, form.Url);
            form.SizeInMb ??= 0.1m;
        }

        _repository.AddMediaAsset(id, form);
        TempData["FlashMessage"] = $"{form.FileName} was added to the media gallery.";
        return RedirectToAction(nameof(Media), new { id });
    }

    [HttpPost("organizer/events/{id:int}/media/folders")]
    [ValidateAntiForgeryToken]
    public IActionResult CreateFolder(int id, [Bind(Prefix = "FolderForm")] MediaFolderInput form)
    {
        var guard = RequireOrganizer();
        if (guard is not null)
        {
            return guard;
        }

        var eventRecord = _repository.GetEvent(id);
        if (eventRecord is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View("Media", BuildMediaModel(eventRecord, new MediaUploadInput
            {
                FolderName = eventRecord.MediaFolders.FirstOrDefault()?.Name ?? "General"
            }, form));
        }

        _repository.AddMediaFolder(id, form);
        TempData["FlashMessage"] = $"Folder \"{form.Name}\" has been created.";
        return RedirectToAction(nameof(Media), new { id });
    }

    [HttpPost("organizer/events/{id:int}/media/{assetId:int}/cover")]
    [ValidateAntiForgeryToken]
    public IActionResult SetCover(int id, int assetId)
    {
        var guard = RequireOrganizer();
        if (guard is not null)
        {
            return guard;
        }

        _repository.SetCoverAsset(id, assetId);
        TempData["FlashMessage"] = "The event cover has been updated.";
        return RedirectToAction(nameof(Media), new { id });
    }

    [HttpGet("organizer/events/{id:int}/assets")]
    public IActionResult Assets(int id)
    {
        var guard = RequireOrganizer();
        if (guard is not null)
        {
            return guard;
        }

        var eventRecord = _repository.GetEvent(id);
        if (eventRecord is null)
        {
            return NotFound();
        }

        return View(new AssetLibraryViewModel
        {
            CurrentUser = CurrentUser,
            Event = eventRecord
        });
    }

    private CreateEventPageViewModel BuildCreateModel(CreateEventInput form) =>
        new()
        {
            CurrentUser = CurrentUser,
            Form = form,
            Categories = Categories
        };

    private SchedulePageViewModel BuildScheduleModel(EventRecord eventRecord, ScheduleInput form) =>
        new()
        {
            CurrentUser = CurrentUser,
            Event = eventRecord,
            Form = form
        };

    private MediaPageViewModel BuildMediaModel(EventRecord eventRecord, MediaUploadInput form, MediaFolderInput folderForm) =>
        new()
        {
            CurrentUser = CurrentUser,
            Event = eventRecord,
            Form = form,
            FolderForm = folderForm
        };

    private static string NormalizeExternalFileName(string fileName, string url)
    {
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            return fileName;
        }

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            var candidate = Path.GetFileName(uri.LocalPath);
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                return candidate;
            }
        }

        return $"asset-{DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture)}";
    }
}
