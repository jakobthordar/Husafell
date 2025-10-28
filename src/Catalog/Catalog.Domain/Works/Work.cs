using System.Linq;
using Catalog.Domain.Abstractions;
using Catalog.Domain.Works.Events;
using Catalog.Domain.Works.ValueObjects;

namespace Catalog.Domain.Works;

/// <summary>
/// Represents an artwork or cultural object that can be catalogued.
/// </summary>
public sealed class Work : AggregateRoot<Guid>
{
    private readonly List<Asset> _assets = new();

    private Work(
        Guid id,
        AccessionNumber accessionNumber,
        LocalizedText title,
        Slug slug,
        LocalizedText? description,
        Dimensions? dimensions)
        : base(id)
    {
        AccessionNumber = accessionNumber;
        Title = title;
        Slug = slug;
        Description = description;
        Dimensions = dimensions;
    }

    /// <summary>
    /// Gets the accession number assigned to the work by the registrar.
    /// </summary>
    public AccessionNumber AccessionNumber { get; private set; }

    /// <summary>
    /// Gets the localized title of the work.
    /// </summary>
    public LocalizedText Title { get; private set; }

    /// <summary>
    /// Gets the optional localized description of the work.
    /// </summary>
    public LocalizedText? Description { get; private set; }

    /// <summary>
    /// Gets the URL-friendly slug associated with the work.
    /// </summary>
    public Slug Slug { get; private set; }

    /// <summary>
    /// Gets the optional physical dimensions of the work.
    /// </summary>
    public Dimensions? Dimensions { get; private set; }

    /// <summary>
    /// Gets the assets that have been associated with the work.
    /// </summary>
    public IReadOnlyCollection<Asset> Assets => _assets.AsReadOnly();

    /// <summary>
    /// Gets the primary asset associated with the work, if one has been designated.
    /// </summary>
    public Asset? PrimaryAsset => _assets.FirstOrDefault(asset => asset.IsPrimary);

    /// <summary>
    /// Registers a new work in the catalog.
    /// </summary>
    /// <param name="id">The unique identifier for the work.</param>
    /// <param name="accessionNumber">The accession number assigned to the work.</param>
    /// <param name="title">The localized title of the work.</param>
    /// <param name="slug">The URL-friendly slug for the work.</param>
    /// <param name="description">An optional localized description for the work.</param>
    /// <param name="dimensions">Optional physical dimensions of the work.</param>
    /// <returns>The newly registered work aggregate.</returns>
    /// <exception cref="ArgumentException">Thrown when the work identifier is empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when required arguments are not supplied.</exception>
    public static Work Register(
        Guid id,
        AccessionNumber accessionNumber,
        LocalizedText title,
        Slug slug,
        LocalizedText? description = null,
        Dimensions? dimensions = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Work identifier cannot be empty.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(accessionNumber);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(slug);

        var work = new Work(id, accessionNumber, title, slug, description, dimensions);

        work.RaiseDomainEvent(new WorkRegisteredDomainEvent(work.Id, work.AccessionNumber.Value));

        return work;
    }

    /// <summary>
    /// Adds a new asset to the work.
    /// </summary>
    /// <param name="assetId">The unique identifier for the asset.</param>
    /// <param name="fileName">The asset's file name.</param>
    /// <param name="caption">An optional caption for the asset.</param>
    /// <param name="isPrimary">A value indicating whether the asset should be marked as primary.</param>
    /// <returns>The asset that was added to the work.</returns>
    /// <exception cref="DomainRuleViolationException">Thrown when a business rule is violated while adding the asset.</exception>
    public Asset AddAsset(Guid assetId, string fileName, LocalizedText? caption = null, bool isPrimary = false)
    {
        var asset = Asset.Create(assetId, fileName, caption);

        if (_assets.Any(existing => string.Equals(existing.FileName, asset.FileName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainRuleViolationException($"An asset with the file name '{asset.FileName}' already exists for work '{Id}'.");
        }

        if (_assets.Count == 0)
        {
            asset.MarkAsPrimary();
        }
        else if (isPrimary)
        {
            foreach (var existing in _assets.Where(a => a.IsPrimary))
            {
                existing.DemoteFromPrimary();
            }

            asset.MarkAsPrimary();
        }

        _assets.Add(asset);

        RaiseDomainEvent(new AssetAddedToWorkDomainEvent(Id, asset.Id, asset.IsPrimary));

        if (asset.IsPrimary)
        {
            RaiseDomainEvent(new WorkPrimaryAssetChangedDomainEvent(Id, asset.Id));
        }

        return asset;
    }

    /// <summary>
    /// Removes an asset from the work.
    /// </summary>
    /// <param name="assetId">The identifier of the asset to remove.</param>
    /// <exception cref="DomainRuleViolationException">Thrown when the specified asset is not associated with the work.</exception>
    public void RemoveAsset(Guid assetId)
    {
        if (assetId == Guid.Empty)
        {
            throw new ArgumentException("Asset identifier cannot be empty.", nameof(assetId));
        }

        var asset = _assets.FirstOrDefault(existing => existing.Id == assetId)
            ?? throw new DomainRuleViolationException($"Asset '{assetId}' does not exist on work '{Id}'.");

        var wasPrimary = asset.IsPrimary;

        _assets.Remove(asset);

        RaiseDomainEvent(new AssetRemovedFromWorkDomainEvent(Id, asset.Id, wasPrimary));

        if (wasPrimary && _assets.Count > 0)
        {
            var replacementPrimary = _assets[0];
            replacementPrimary.MarkAsPrimary();
            RaiseDomainEvent(new WorkPrimaryAssetChangedDomainEvent(Id, replacementPrimary.Id));
        }
    }

    /// <summary>
    /// Promotes the specified asset to become the primary asset for the work.
    /// </summary>
    /// <param name="assetId">The identifier of the asset to promote.</param>
    /// <exception cref="DomainRuleViolationException">Thrown when the asset is not associated with the work.</exception>
    public void PromoteAssetToPrimary(Guid assetId)
    {
        if (assetId == Guid.Empty)
        {
            throw new ArgumentException("Asset identifier cannot be empty.", nameof(assetId));
        }

        var asset = _assets.FirstOrDefault(existing => existing.Id == assetId)
            ?? throw new DomainRuleViolationException($"Asset '{assetId}' does not exist on work '{Id}'.");

        if (asset.IsPrimary)
        {
            return;
        }

        foreach (var existing in _assets.Where(a => a.IsPrimary))
        {
            existing.DemoteFromPrimary();
        }

        asset.MarkAsPrimary();

        RaiseDomainEvent(new WorkPrimaryAssetChangedDomainEvent(Id, asset.Id));
    }

    /// <summary>
    /// Updates the localized description for the work.
    /// </summary>
    /// <param name="description">The new description value.</param>
    public void UpdateDescription(LocalizedText? description)
    {
        if (Equals(Description, description))
        {
            return;
        }

        Description = description;

        RaiseDomainEvent(new WorkDescriptionUpdatedDomainEvent(Id));
    }

    /// <summary>
    /// Updates the physical dimensions associated with the work.
    /// </summary>
    /// <param name="dimensions">The new dimensions value.</param>
    public void UpdateDimensions(Dimensions? dimensions)
    {
        if (Equals(Dimensions, dimensions))
        {
            return;
        }

        Dimensions = dimensions;

        RaiseDomainEvent(new WorkDimensionsChangedDomainEvent(Id, dimensions));
    }

    /// <summary>
    /// Updates the title and slug for the work.
    /// </summary>
    /// <param name="title">The new localized title.</param>
    /// <param name="slug">The new slug.</param>
    public void Rename(LocalizedText title, Slug slug)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(slug);

        if (Title.Equals(title) && Slug.Equals(slug))
        {
            return;
        }

        Title = title;
        Slug = slug;

        RaiseDomainEvent(new WorkRenamedDomainEvent(Id, slug.Value));
    }
}
