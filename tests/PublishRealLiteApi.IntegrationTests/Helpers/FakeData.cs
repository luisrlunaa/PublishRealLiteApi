using Bogus;
using PublishRealLiteApi.Features.ArtistProfiles;
using PublishRealLiteApi.Features.Releases;
using PublishRealLiteApi.Features.Tracks;
using PublishRealLiteApi.Features.Videos;

namespace PublishRealLiteApi.IntegrationTests.Helpers;

public static class FakeData
{
    private static readonly Faker Faker = new();

    public static string Email() => Faker.Internet.Email();
    public static string Password() => "Password123!";

    public static CreateArtistProfile.Command Artist() => new(
        ArtistName: Faker.Name.FullName(),
        Bio: Faker.Lorem.Sentence(),
        SocialLinksJson: null);

    public static CreateRelease.Command Release(int artistProfileId) => new(
        ArtistProfileId: artistProfileId,
        Title: string.Join(" ", Faker.Lorem.Words(3)),
        ReleaseDate: Faker.Date.Future(),
        Genre: Faker.Music.Genre(),
        Label: Faker.Company.CompanyName(),
        UPC: null,
        ISRC: null,
        LinksJson: null);

    public static CreateTrack.Command Track(Guid releaseId, int position = 1) => new(
        ReleaseId: releaseId,
        Position: position,
        Title: $"Track {Faker.Lorem.Word()}");

    public static CreateVideo.Command Video(int artistProfileId) => new(
        ArtistProfileId: artistProfileId,
        Title: Faker.Lorem.Sentence(3),
        ThumbnailUrl: "https://picsum.photos/400/300",
        VideoUrl: $"https://youtube.com/watch?v={Faker.Random.AlphaNumeric(11)}");
}
