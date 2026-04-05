namespace PublishRealLiteApi.DTOs
{
    public record TrackDto(Guid Id, Guid ReleaseId, int Position, string Title);
    public record CreateTrackDto(Guid ReleaseId, int Position, string Title);
    public record UpdateTrackDto(int Position, string Title);
}
