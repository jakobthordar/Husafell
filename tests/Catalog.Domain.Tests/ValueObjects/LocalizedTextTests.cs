using Catalog.Domain.Works.ValueObjects;

namespace Catalog.Domain.Tests.ValueObjects;

public class LocalizedTextTests
{
    [Fact]
    public void Create_WithValidTranslations_ShouldExposeValues()
    {
        var localized = LocalizedText.Create(new[] { ("en", "Hello"), ("fr", "Bonjour") });

        Assert.Equal("en", localized.DefaultCulture);
        Assert.Equal("Hello", localized.DefaultText);
        Assert.Equal("Bonjour", localized.GetText("fr"));
    }

    [Fact]
    public void GetText_ShouldFallbackToNeutralCulture()
    {
        var localized = LocalizedText.Create(new[] { ("en", "Hello"), ("fr", "Bonjour") });

        var result = localized.GetText("en-GB");

        Assert.Equal("Hello", result);
    }

    [Fact]
    public void GetText_WithUnknownCulture_ShouldReturnDefault()
    {
        var localized = LocalizedText.Create(new[] { ("en", "Hello") });

        var result = localized.GetText("es");

        Assert.Equal("Hello", result);
    }

    [Fact]
    public void Create_WithDuplicateCultures_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => LocalizedText.Create(new[] { ("en", "Hello"), ("en", "Hi") }));
    }

    [Fact]
    public void Create_WithInvalidCulture_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => LocalizedText.Create(new[] { ("invalid-culture", "Hello") }));
    }
}
