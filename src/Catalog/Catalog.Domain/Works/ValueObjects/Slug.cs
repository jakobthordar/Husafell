using System.Text.RegularExpressions;

namespace Catalog.Domain.Works.ValueObjects;

/// <summary>
/// Represents a URL-friendly slug for a catalogued work.
/// </summary>
public sealed record Slug
{
    private static readonly Regex ValidPattern = new(
        pattern: "^[a-z0-9]+(?:-[a-z0-9]+)*$",
        options: RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private Slug(string value) => Value = value;

    /// <summary>
    /// Gets the normalized slug value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new <see cref="Slug"/> instance from the provided text, normalising it into a URL-safe representation.
    /// </summary>
    /// <param name="text">The text to convert into a slug.</param>
    /// <returns>A validated slug.</returns>
    /// <exception cref="ArgumentException">Thrown when the supplied text cannot produce a valid slug.</exception>
    public static Slug Create(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Slug source text cannot be null, empty, or whitespace.", nameof(text));
        }

        var normalized = Normalize(text);

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Slug source text must contain at least one letter or number.", nameof(text));
        }

        if (!ValidPattern.IsMatch(normalized))
        {
            throw new ArgumentException("Slugs must contain lowercase alphanumeric characters separated by single hyphens.", nameof(text));
        }

        return new Slug(normalized);
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    private static string Normalize(string text)
    {
        var lowerInvariant = text.Trim().ToLowerInvariant();

        var collapsedWhitespace = Regex.Replace(lowerInvariant, "\\s+", "-");
        var removedInvalidCharacters = Regex.Replace(collapsedWhitespace, "[^a-z0-9-]", string.Empty);
        var singleHyphenated = Regex.Replace(removedInvalidCharacters, "-{2,}", "-");
        var trimmed = singleHyphenated.Trim('-');

        return trimmed;
    }
}
