using Catalog.Domain.Works.ValueObjects;

namespace Catalog.Domain.Tests.ValueObjects;

public class AccessionNumberTests
{
    [Theory]
    [InlineData("inv-001")]
    [InlineData("  ab-1234  ")]
    [InlineData("A1B2C3")] 
    public void Create_ShouldNormaliseValue(string rawValue)
    {
        var accessionNumber = AccessionNumber.Create(rawValue);

        Assert.Equal(rawValue.Trim().ToUpperInvariant(), accessionNumber.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("AB")] // too short
    [InlineData("THIS-IDENTIFIER-IS-WAY-TOO-LONG-FOR-THE-ALLOWED-RANGE")]
    [InlineData("invalid#chars")]
    public void Create_InvalidValue_ShouldThrow(string rawValue)
    {
        Assert.Throws<ArgumentException>(() => AccessionNumber.Create(rawValue));
    }
}
