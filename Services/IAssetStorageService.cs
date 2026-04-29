using Microsoft.AspNetCore.Http;

namespace EventRegistrationSystem.Services;

public interface IAssetStorageService
{
    Task<StoredAssetResult> SaveEventAssetAsync(int eventId, IFormFile file, CancellationToken cancellationToken = default);
}
