using Catalog.Application.Works;

namespace Catalog.Application.Tests;

public class WorkCatalogServiceContractTests
{
    [Fact]
    public void ServiceContract_ShouldBeAbstractedByInterface()
    {
        Assert.True(typeof(IWorkCatalogService).IsInterface);
    }
}
