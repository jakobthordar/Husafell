using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Catalog.Domain.Works.ValueObjects;

/// <summary>
/// Represents text that has been translated into one or more cultures.
/// </summary>
public sealed class LocalizedText : IEquatable<LocalizedText>
{
    private readonly Dictionary<string, string> _translations;
    private readonly IReadOnlyDictionary<string, string> _readOnlyTranslations;
    private readonly string _defaultCulture;

    private LocalizedText(Dictionary<string, string> translations, string defaultCulture)
    {
        _translations = translations;
        _readOnlyTranslations = new ReadOnlyDictionary<string, string>(_translations);
        _defaultCulture = defaultCulture;
    }

    /// <summary>
    /// Creates a new <see cref="LocalizedText"/> value object from the supplied translations.
    /// </summary>
    /// <param name="translations">The culture-text pairs that make up the localized string.</param>
    /// <returns>A validated <see cref="LocalizedText"/> value object.</returns>
    /// <exception cref="ArgumentException">Thrown when the translations collection is empty or contains invalid data.</exception>
    public static LocalizedText Create(IEnumerable<(string Culture, string Text)> translations)
    {
        ArgumentNullException.ThrowIfNull(translations);

        var normalizedTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? defaultCulture = null;

        foreach (var (culture, text) in translations)
        {
            if (string.IsNullOrWhiteSpace(culture))
            {
                throw new ArgumentException("Translation culture cannot be null, empty, or whitespace.", nameof(translations));
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Translation text cannot be null, empty, or whitespace.", nameof(translations));
            }

            var cultureInfo = NormalizeCulture(culture);
            var normalizedCulture = cultureInfo.Name;

            if (normalizedTranslations.ContainsKey(normalizedCulture))
            {
                throw new ArgumentException($"A translation for culture '{normalizedCulture}' has already been provided.", nameof(translations));
            }

            normalizedTranslations[normalizedCulture] = text.Trim();
            defaultCulture ??= normalizedCulture;
        }

        if (normalizedTranslations.Count == 0)
        {
            throw new ArgumentException("At least one translation must be supplied.", nameof(translations));
        }

        return new LocalizedText(normalizedTranslations, defaultCulture!);
    }

    /// <summary>
    /// Creates a new <see cref="LocalizedText"/> value object containing a single translation.
    /// </summary>
    /// <param name="culture">The culture for the translation.</param>
    /// <param name="text">The text value.</param>
    /// <returns>A validated <see cref="LocalizedText"/> value object.</returns>
    public static LocalizedText Create(string culture, string text) => Create(new[] { (culture, text) });

    /// <summary>
    /// Gets the default culture associated with the localized text.
    /// </summary>
    public string DefaultCulture => _defaultCulture;

    /// <summary>
    /// Gets the text for the default culture.
    /// </summary>
    public string DefaultText => _translations[_defaultCulture];

    /// <summary>
    /// Gets the collection of translations keyed by their culture names.
    /// </summary>
    public IReadOnlyDictionary<string, string> Translations => _readOnlyTranslations;

    /// <summary>
    /// Retrieves the translation for the specified culture, falling back to the neutral or default culture as required.
    /// </summary>
    /// <param name="culture">The requested culture.</param>
    /// <returns>The localized text corresponding to the requested culture.</returns>
    /// <exception cref="ArgumentException">Thrown when the supplied culture is invalid.</exception>
    public string GetText(string culture)
    {
        var cultureInfo = NormalizeCulture(culture);
        var normalizedCulture = cultureInfo.Name;

        if (_translations.TryGetValue(normalizedCulture, out var specificText))
        {
            return specificText;
        }

        var fallbackCulture = cultureInfo.IsNeutralCulture ? cultureInfo.Name : cultureInfo.Parent.Name;

        if (!string.IsNullOrEmpty(fallbackCulture) && _translations.TryGetValue(fallbackCulture, out var neutralText))
        {
            return neutralText;
        }

        return DefaultText;
    }

    /// <inheritdoc />
    public bool Equals(LocalizedText? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (_translations.Count != other._translations.Count)
        {
            return false;
        }

        foreach (var (key, value) in _translations)
        {
            if (!other._translations.TryGetValue(key, out var otherValue))
            {
                return false;
            }

            if (!string.Equals(value, otherValue, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as LocalizedText);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var key in _translations.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase))
        {
            hash.Add(key, StringComparer.OrdinalIgnoreCase);
            hash.Add(_translations[key], StringComparer.Ordinal);
        }

        return hash.ToHashCode();
    }

    private static CultureInfo NormalizeCulture(string culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            throw new ArgumentException("Culture cannot be null, empty, or whitespace.", nameof(culture));
        }

        try
        {
            return CultureInfo.GetCultureInfo(culture);
        }
        catch (CultureNotFoundException exception)
        {
            throw new ArgumentException($"Culture '{culture}' is not recognised.", nameof(culture), exception);
        }
    }
}
