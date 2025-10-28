using Catalog.Infrastructure.Works;

namespace Catalog.Infrastructure.Tests;

public class InMemoryWorkCatalogServiceTests
{
    [Fact]
    public void GetWorks_ShouldReturnSeededWorks()
    {
        var service = new InMemoryWorkCatalogService();

        var works = service.GetWorks();

        Assert.NotEmpty(works);
    }
}
