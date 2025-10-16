using Media.Application.Assets;

namespace Media.Application.Tests;

public class MediaLibraryServiceContractTests
{
    [Fact]
    public void ServiceContract_ShouldBeRepresentedByInterface()
    {
        Assert.True(typeof(IMediaLibraryService).IsInterface);
    }
}
