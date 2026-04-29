using EventRegistrationSystem.Models;
using EventRegistrationSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrationSystem.Controllers;

public sealed class EventsController : AppController
{
    private readonly IEventRepository _repository;

    public EventsController(IEventRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("events")]
    public IActionResult Dashboard()
    {
        var registeredEvents = CurrentUser.IsGuest
            ? []
            : _repository.GetRegisteredEvents(CurrentUser.Email);

        var recommendedEvents = _repository
            .GetPublicEvents()
            .Where(eventRecord => registeredEvents.All(registered => registered.Id != eventRecord.Id))
            .Take(3)
            .ToList();

        return View(new UserDashboardViewModel
        {
            CurrentUser = CurrentUser,
            RegisteredEvents = registeredEvents,
            RecommendedEvents = recommendedEvents
        });
    }

    [HttpGet("events/{id:int}")]
    public IActionResult Details(int id)
    {
        var eventRecord = _repository.GetEvent(id);
        if (eventRecord is null)
        {
            return NotFound();
        }

        return View(BuildEventDetailsModel(eventRecord, new RegistrationInput
        {
            FullName = CurrentUser.IsGuest ? string.Empty : CurrentUser.DisplayName,
            Email = CurrentUser.IsGuest ? string.Empty : CurrentUser.Email,
            TicketType = "General Admission"
        }));
    }

    [HttpPost("events/{id:int}/register")]
    [ValidateAntiForgeryToken]
    public IActionResult Register(int id, [Bind(Prefix = "RegistrationForm")] RegistrationInput form)
    {
        var eventRecord = _repository.GetEvent(id);
        if (eventRecord is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View("Details", BuildEventDetailsModel(eventRecord, form));
        }

        var registered = _repository.RegisterAttendee(id, form);
        TempData[registered ? "FlashMessage" : "FlashError"] = registered
            ? $"Registration confirmed for {eventRecord.Title}."
            : "That email is already registered for this event.";

        return RedirectToAction(nameof(Details), new { id });
    }

    private EventDetailsViewModel BuildEventDetailsModel(EventRecord eventRecord, RegistrationInput form)
    {
        var alreadyRegistered = !CurrentUser.IsGuest && eventRecord.Registrations.Any(registration =>
            string.Equals(registration.Email, CurrentUser.Email, StringComparison.OrdinalIgnoreCase));

        return new EventDetailsViewModel
        {
            CurrentUser = CurrentUser,
            Event = eventRecord,
            RegistrationForm = form,
            AlreadyRegistered = alreadyRegistered
        };
    }
}
