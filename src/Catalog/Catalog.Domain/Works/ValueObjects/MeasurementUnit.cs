namespace Catalog.Domain.Works.ValueObjects;

/// <summary>
/// Represents the measurement units supported by the <see cref="Dimensions"/> value object.
/// </summary>
public enum MeasurementUnit
{
    /// <summary>
    /// Indicates that the measurement is expressed in millimetres.
    /// </summary>
    Millimetres,

    /// <summary>
    /// Indicates that the measurement is expressed in centimetres.
    /// </summary>
    Centimetres,

    /// <summary>
    /// Indicates that the measurement is expressed in inches.
    /// </summary>
    Inches
}
