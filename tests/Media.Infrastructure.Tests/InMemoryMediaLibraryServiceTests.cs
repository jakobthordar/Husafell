using Media.Infrastructure.Assets;

namespace Media.Infrastructure.Tests;

public class InMemoryMediaLibraryServiceTests
{
    [Fact]
    public void GetLibrary_ShouldReturnSeededAssets()
    {
        var service = new InMemoryMediaLibraryService();

        var assets = service.GetLibrary();

        Assert.NotEmpty(assets);
    }
}
