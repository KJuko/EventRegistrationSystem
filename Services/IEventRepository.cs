using EventRegistrationSystem.Models;

namespace EventRegistrationSystem.Services;

public interface IEventRepository
{
    IReadOnlyList<EventRecord> GetPublicEvents();
    IReadOnlyList<EventRecord> GetManagedEvents(CurrentUser currentUser);
    IReadOnlyList<EventRecord> GetRegisteredEvents(string attendeeEmail);
    EventRecord? GetEvent(int id);
    EventRecord CreateEvent(CreateEventInput input, CurrentUser creator);
    bool RegisterAttendee(int eventId, RegistrationInput input);
    void AddSession(int eventId, ScheduleInput input);
    void AddMediaAsset(int eventId, MediaUploadInput input);
    void AddMediaFolder(int eventId, MediaFolderInput input);
    void SetCoverAsset(int eventId, int assetId);
}
