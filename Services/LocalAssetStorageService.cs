using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Text;

namespace EventRegistrationSystem.Services;

public sealed class LocalAssetStorageService : IAssetStorageService
{
    private readonly IWebHostEnvironment _environment;

    public LocalAssetStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<StoredAssetResult> SaveEventAssetAsync(int eventId, IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0)
        {
            throw new InvalidOperationException("The uploaded file is empty.");
        }

        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var uploadsDirectory = Path.Combine(
            webRootPath,
            "uploads",
            "events",
            eventId.ToString(CultureInfo.InvariantCulture));

        Directory.CreateDirectory(uploadsDirectory);

        var originalFileName = Path.GetFileName(file.FileName);
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var safeBaseName = SanitizeFileName(Path.GetFileNameWithoutExtension(originalFileName));
        var storedFileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N[..8]}-{safeBaseName}{extension}";
        var physicalPath = Path.Combine(uploadsDirectory, storedFileName);

        await using var fileStream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(fileStream, cancellationToken);

        return new StoredAssetResult
        {
            OriginalFileName = originalFileName,
            PublicUrl = $"/uploads/events/{eventId}/{storedFileName}",
            SizeInMb = Math.Round(file.Length / 1024m / 1024m, 2, MidpointRounding.AwayFromZero)
        };
    }

    private static string SanitizeFileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "asset";
        }

        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : '-');
        }

        var sanitized = builder.ToString().Trim('-');
        return string.IsNullOrWhiteSpace(sanitized) ? "asset" : sanitized;
    }
}
