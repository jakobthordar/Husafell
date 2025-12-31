using System.Text.RegularExpressions;

namespace Catalog.Domain.Works.ValueObjects;

/// <summary>
/// Represents the registrar-issued accession number for an individual work.
/// </summary>
public sealed record AccessionNumber
{
    private const int MinimumLength = 3;
    private const int MaximumLength = 32;

    private static readonly Regex ValidPattern = new(
        pattern: @"^[A-Z0-9\-\./]+$",
        options: RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private AccessionNumber(string value) => Value = value;

    /// <summary>
    /// Gets the normalized accession number value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new <see cref="AccessionNumber"/> instance after validating the supplied value.
    /// </summary>
    /// <param name="value">The accession number to validate.</param>
    /// <returns>A validated accession number.</returns>
    /// <exception cref="ArgumentException">Thrown when the supplied value does not satisfy the defined format.</exception>
    public static AccessionNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Accession number cannot be null, empty, or composed only of whitespace characters.", nameof(value));
        }

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length is < MinimumLength or > MaximumLength)
        {
            throw new ArgumentException($"Accession numbers must be between {MinimumLength} and {MaximumLength} characters in length.", nameof(value));
        }

        if (!ValidPattern.IsMatch(normalized))
        {
            throw new ArgumentException("Accession numbers may only contain letters, numbers, and the '-', '.', or '/' separators.", nameof(value));
        }

        return new AccessionNumber(normalized);
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}
