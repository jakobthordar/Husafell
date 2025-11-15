using Catalog.Domain.Abstractions;
using Catalog.Domain.Works;
using Catalog.Domain.Works.Events;
using Catalog.Domain.Works.ValueObjects;

namespace Catalog.Domain.Tests.Works;

public class WorkTests
{
    [Fact]
    public void Register_ShouldRaiseWorkRegisteredEvent()
    {
        var work = Work.Register(
            id: Guid.NewGuid(),
            accessionNumber: AccessionNumber.Create("INV-2024-01"),
            title: LocalizedText.Create("en", "Test Work"),
            slug: Slug.Create("test work"));

        var domainEvent = Assert.Single(work.DomainEvents);
        var registeredEvent = Assert.IsType<WorkRegisteredDomainEvent>(domainEvent);
        Assert.Equal(work.Id, registeredEvent.WorkId);
        Assert.Equal("INV-2024-01", registeredEvent.AccessionNumber);
    }

    [Fact]
    public void AddAsset_FirstAsset_ShouldBecomePrimaryAndRaiseEvents()
    {
        var work = CreateWork();
        work.ClearDomainEvents();

        var asset = work.AddAsset(Guid.NewGuid(), "image.jpg", null);

        Assert.True(asset.IsPrimary);
        Assert.Same(asset, work.PrimaryAsset);

        Assert.Collection(
            work.DomainEvents,
            evt =>
            {
                var added = Assert.IsType<AssetAddedToWorkDomainEvent>(evt);
                Assert.True(added.IsPrimary);
            },
            evt => Assert.IsType<WorkPrimaryAssetChangedDomainEvent>(evt));
    }

    [Fact]
    public void AddAsset_WithDuplicateFileName_ShouldThrow()
    {
        var work = CreateWork();
        work.AddAsset(Guid.NewGuid(), "image.jpg", null);

        Assert.Throws<DomainRuleViolationException>(() => work.AddAsset(Guid.NewGuid(), "IMAGE.jpg", null));
    }

    [Fact]
    public void AddAsset_PromotingNewPrimary_ShouldDemoteExistingPrimary()
    {
        var work = CreateWork();
        var first = work.AddAsset(Guid.NewGuid(), "first.jpg", null);
        work.ClearDomainEvents();

        var second = work.AddAsset(Guid.NewGuid(), "second.jpg", null, isPrimary: true);

        Assert.False(first.IsPrimary);
        Assert.True(second.IsPrimary);
        Assert.Same(second, work.PrimaryAsset);

        Assert.Collection(
            work.DomainEvents,
            evt => { var added = Assert.IsType<AssetAddedToWorkDomainEvent>(evt); Assert.True(added.IsPrimary); },
            evt => Assert.IsType<WorkPrimaryAssetChangedDomainEvent>(evt));
    }

    [Fact]
    public void PromoteAssetToPrimary_ShouldRaiseDomainEvent()
    {
        var work = CreateWork();
        work.AddAsset(Guid.NewGuid(), "primary.jpg", null);
        var secondary = work.AddAsset(Guid.NewGuid(), "secondary.jpg", null);
        work.ClearDomainEvents();

        work.PromoteAssetToPrimary(secondary.Id);

        Assert.True(secondary.IsPrimary);
        Assert.Same(secondary, work.PrimaryAsset);
        Assert.Single(work.DomainEvents);
        Assert.IsType<WorkPrimaryAssetChangedDomainEvent>(work.DomainEvents.Single());
    }

    [Fact]
    public void RemovePrimaryAsset_ShouldPromoteAnotherAsset()
    {
        var work = CreateWork();
        var first = work.AddAsset(Guid.NewGuid(), "first.jpg", null);
        var second = work.AddAsset(Guid.NewGuid(), "second.jpg", null);
        work.ClearDomainEvents();

        work.RemoveAsset(first.Id);

        Assert.DoesNotContain(work.Assets, asset => asset.Id == first.Id);
        Assert.True(second.IsPrimary);
        Assert.Collection(
            work.DomainEvents,
            evt => Assert.IsType<AssetRemovedFromWorkDomainEvent>(evt),
            evt => Assert.IsType<WorkPrimaryAssetChangedDomainEvent>(evt));
    }

    [Fact]
    public void UpdateDescription_WhenChanged_ShouldRaiseEvent()
    {
        var work = CreateWork();
        work.ClearDomainEvents();

        work.UpdateDescription(LocalizedText.Create("en", "Updated description"));

        Assert.Single(work.DomainEvents);
        Assert.IsType<WorkDescriptionUpdatedDomainEvent>(work.DomainEvents.Single());
    }

    [Fact]
    public void UpdateDescription_WhenUnchanged_ShouldNotRaiseEvent()
    {
        var description = LocalizedText.Create("en", "Description");
        var work = Work.Register(
            Guid.NewGuid(),
            AccessionNumber.Create("INV-2024-02"),
            LocalizedText.Create("en", "Work"),
            Slug.Create("work"),
            description);

        work.ClearDomainEvents();

        work.UpdateDescription(description);

        Assert.Empty(work.DomainEvents);
    }

    [Fact]
    public void UpdateDimensions_WhenChanged_ShouldRaiseEvent()
    {
        var work = CreateWork();
        work.ClearDomainEvents();

        work.UpdateDimensions(Dimensions.Create(10, 20, null, MeasurementUnit.Millimetres));

        Assert.Single(work.DomainEvents);
        Assert.IsType<WorkDimensionsChangedDomainEvent>(work.DomainEvents.Single());
    }

    [Fact]
    public void UpdateDimensions_WhenUnchanged_ShouldNotRaiseEvent()
    {
        var dimensions = Dimensions.Create(10, 20, null, MeasurementUnit.Centimetres);
        var work = Work.Register(
            Guid.NewGuid(),
            AccessionNumber.Create("INV-2024-03"),
            LocalizedText.Create("en", "Work"),
            Slug.Create("work"),
            dimensions: dimensions);

        work.ClearDomainEvents();

        work.UpdateDimensions(dimensions);

        Assert.Empty(work.DomainEvents);
    }

    [Fact]
    public void Rename_WhenChanged_ShouldRaiseEvent()
    {
        var work = CreateWork();
        work.ClearDomainEvents();

        work.Rename(LocalizedText.Create("en", "New Title"), Slug.Create("new title"));

        Assert.Single(work.DomainEvents);
        var @event = Assert.IsType<WorkRenamedDomainEvent>(work.DomainEvents.Single());
        Assert.Equal("new-title", @event.Slug);
    }

    [Fact]
    public void Rename_WhenUnchanged_ShouldNotRaiseEvent()
    {
        var title = LocalizedText.Create("en", "Title");
        var slug = Slug.Create("title");
        var work = Work.Register(
            Guid.NewGuid(),
            AccessionNumber.Create("INV-2024-04"),
            title,
            slug);

        work.ClearDomainEvents();

        work.Rename(title, slug);

        Assert.Empty(work.DomainEvents);
    }

    private static Work CreateWork() => Work.Register(
        Guid.NewGuid(),
        AccessionNumber.Create("INV-0001"),
        LocalizedText.Create("en", "Sample Work"),
        Slug.Create("sample work"));
}
