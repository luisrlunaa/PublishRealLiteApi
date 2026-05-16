using FluentValidation;
using PublishRealLiteApi.Services.Interfaces;

namespace PublishRealLiteApi.Features.Uploads;

public static class UploadImage
{
    public record Command(IFormFile File, string Folder = "covers");

    public record Response(string Url, string FileName, long Size);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.File).NotNull();
            RuleFor(x => x.Folder).NotEmpty();
        }
    }

    public class Handler(IStorageService storage)
    {
        public async Task<Response> HandleAsync(Command cmd, CancellationToken ct = default)
        {
            var result = await storage.SaveImageAsync(cmd.File, cmd.Folder);
            return new Response(result.Url, result.FileName, result.Size);
        }
    }
}
