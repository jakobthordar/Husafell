using Catalog.Domain.Works.ValueObjects;

namespace Catalog.Domain.Tests.ValueObjects;

public class SlugTests
{
    [Fact]
    public void Create_ShouldNormaliseWhitespaceAndCasing()
    {
        var slug = Slug.Create("  The Quick Brown Fox  ");

        Assert.Equal("the-quick-brown-fox", slug.Value);
    }

    [Fact]
    public void Create_ShouldRemoveUnsupportedCharacters()
    {
        var slug = Slug.Create("Rococo: Elegance & Style!");

        Assert.Equal("rococo-elegance-style", slug.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!!!")]
    public void Create_InvalidValues_ShouldThrow(string rawValue)
    {
        Assert.Throws<ArgumentException>(() => Slug.Create(rawValue));
    }
}
