using Catalog.Domain.Works.ValueObjects;

namespace Catalog.Domain.Tests.ValueObjects;

public class DimensionsTests
{
    [Fact]
    public void Create_WithValidValues_ShouldReturnDimensions()
    {
        var dimensions = Dimensions.Create(10.123m, 20.456m, 5.789m, MeasurementUnit.Inches);

        Assert.Equal(10.12m, dimensions.Width);
        Assert.Equal(20.46m, dimensions.Height);
        Assert.Equal(5.79m, dimensions.Depth);
        Assert.Equal(MeasurementUnit.Inches, dimensions.Unit);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(10, 0)]
    [InlineData(10, -5)]
    public void Create_WithNonPositiveWidthOrHeight_ShouldThrow(decimal width, decimal height)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Dimensions.Create(width, height, null, MeasurementUnit.Centimetres));
    }

    [Fact]
    public void Create_WithNonPositiveDepth_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Dimensions.Create(10, 20, 0, MeasurementUnit.Centimetres));
    }
}
