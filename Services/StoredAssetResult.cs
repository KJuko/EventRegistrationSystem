namespace EventRegistrationSystem.Services;

public sealed class StoredAssetResult
{
    public string OriginalFileName { get; init; } = string.Empty;
    public string PublicUrl { get; init; } = string.Empty;
    public decimal SizeInMb { get; init; }
}
