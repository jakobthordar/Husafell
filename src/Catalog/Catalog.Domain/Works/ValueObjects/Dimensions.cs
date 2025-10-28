namespace Catalog.Domain.Works.ValueObjects;

/// <summary>
/// Represents the physical dimensions of a catalogued work.
/// </summary>
public sealed record Dimensions
{
    private Dimensions(decimal width, decimal height, decimal? depth, MeasurementUnit unit)
    {
        Width = width;
        Height = height;
        Depth = depth;
        Unit = unit;
    }

    /// <summary>
    /// Gets the width component of the dimensions.
    /// </summary>
    public decimal Width { get; }

    /// <summary>
    /// Gets the height component of the dimensions.
    /// </summary>
    public decimal Height { get; }

    /// <summary>
    /// Gets the optional depth component of the dimensions.
    /// </summary>
    public decimal? Depth { get; }

    /// <summary>
    /// Gets the measurement unit in which the dimensions are expressed.
    /// </summary>
    public MeasurementUnit Unit { get; }

    /// <summary>
    /// Creates a new <see cref="Dimensions"/> instance after validating the supplied values.
    /// The measurements are rounded to two decimal places using <see cref="MidpointRounding.AwayFromZero"/>.
    /// </summary>
    /// <param name="width">The width component of the dimensions.</param>
    /// <param name="height">The height component of the dimensions.</param>
    /// <param name="depth">The optional depth component of the dimensions.</param>
    /// <param name="unit">The unit used to express the measurements.</param>
    /// <returns>A validated <see cref="Dimensions"/> value object.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any of the specified measurements are zero or negative.</exception>
    public static Dimensions Create(decimal width, decimal height, decimal? depth, MeasurementUnit unit)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero.");
        }

        if (depth is not null && depth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(depth), "Depth must be greater than zero when specified.");
        }

        return new Dimensions(
            Math.Round(width, 2, MidpointRounding.AwayFromZero),
            Math.Round(height, 2, MidpointRounding.AwayFromZero),
            depth is null ? null : Math.Round(depth.Value, 2, MidpointRounding.AwayFromZero),
            unit);
    }

    /// <summary>
    /// Returns a textual representation of the dimensions that is suitable for display.
    /// </summary>
    /// <returns>A human-readable representation of the dimensions.</returns>
    public override string ToString()
    {
        var depthSegment = Depth is null ? string.Empty : $" × {Depth.Value}";

        return $"{Width} × {Height}{depthSegment} {Unit}";
    }
}
