namespace Media.Infrastructure.Assets;

using Media.Application.Assets;
using Media.Domain.Assets;

public sealed class InMemoryMediaLibraryService : IMediaLibraryService
{
    private static readonly IReadOnlyCollection<MediaAsset> SeedLibrary = new List<MediaAsset>
    {
        new(Guid.Parse("0f7dc871-90fb-4d55-b725-b99be0d466a2"), "sample-image.png", "image/png")
    };

    public IReadOnlyCollection<MediaAsset> GetLibrary() => SeedLibrary;
}
