using Catalog.Domain.Abstractions;
using Catalog.Domain.Works.ValueObjects;

namespace Catalog.Domain.Works;

/// <summary>
/// Represents an asset that is associated with a work, such as an image or document.
/// </summary>
public sealed class Asset : Entity<Guid>
{
    private Asset(Guid id, string fileName, LocalizedText? caption)
        : base(id)
    {
        FileName = fileName;
        Caption = caption;
    }

    /// <summary>
    /// Gets the human-readable name or file name of the asset.
    /// </summary>
    public string FileName { get; private set; }

    /// <summary>
    /// Gets the optional localized caption describing the asset.
    /// </summary>
    public LocalizedText? Caption { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the asset is marked as the primary representation of its work.
    /// </summary>
    public bool IsPrimary { get; private set; }

    /// <summary>
    /// Creates a new <see cref="Asset"/> after validating the provided details.
    /// </summary>
    /// <param name="id">The unique identifier for the asset.</param>
    /// <param name="fileName">The file name or descriptive name of the asset.</param>
    /// <param name="caption">An optional localized caption for the asset.</param>
    /// <returns>A validated <see cref="Asset"/> entity.</returns>
    /// <exception cref="ArgumentException">Thrown when the identifier or file name are invalid.</exception>
    public static Asset Create(Guid id, string fileName, LocalizedText? caption)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Asset identifier cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Asset file name cannot be null, empty, or whitespace.", nameof(fileName));
        }

        var normalizedFileName = fileName.Trim();

        if (normalizedFileName.Length > 256)
        {
            throw new ArgumentException("Asset file name cannot exceed 256 characters.", nameof(fileName));
        }

        return new Asset(id, normalizedFileName, caption);
    }

    /// <summary>
    /// Updates the caption associated with the asset.
    /// </summary>
    /// <param name="caption">The new caption value.</param>
    public void UpdateCaption(LocalizedText? caption) => Caption = caption;

    /// <summary>
    /// Updates the stored file name for the asset.
    /// </summary>
    /// <param name="fileName">The new file name.</param>
    /// <exception cref="ArgumentException">Thrown when the new file name is invalid.</exception>
    public void UpdateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Asset file name cannot be null, empty, or whitespace.", nameof(fileName));
        }

        var normalizedFileName = fileName.Trim();

        if (normalizedFileName.Length > 256)
        {
            throw new ArgumentException("Asset file name cannot exceed 256 characters.", nameof(fileName));
        }

        FileName = normalizedFileName;
    }

    internal void MarkAsPrimary() => IsPrimary = true;

    internal void DemoteFromPrimary() => IsPrimary = false;
}
