namespace Media.Application.Assets;

using Media.Domain.Assets;

public interface IMediaLibraryService
{
    IReadOnlyCollection<MediaAsset> GetLibrary();
}
