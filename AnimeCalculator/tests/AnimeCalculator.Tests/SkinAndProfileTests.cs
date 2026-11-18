using AnimeCalculator.Core.Calculator;
using AnimeCalculator.Core.Progress;
using AnimeCalculator.Core.Shop;
using AnimeCalculator.Core.Skins;

namespace AnimeCalculator.Tests;

public class SkinCatalogTests
{
    [Fact]
    public void Ids_are_unique() =>
        Assert.Equal(SkinCatalog.All.Count, SkinCatalog.All.Select(s => s.Id).Distinct().Count());

    [Fact]
    public void Default_skin_is_a_free_starter() => Assert.True(SkinCatalog.Default.IsStarter);

    [Fact]
    public void Every_skin_is_either_starter_unlockable_or_premium()
    {
        foreach (var skin in SkinCatalog.All)
        {
            var kinds = new[] { skin.IsStarter, skin.IsUnlockable, skin.IsPremium }.Count(k => k);
            Assert.True(kinds == 1, $"{skin.Id} should be exactly one kind");
        }
    }

    [Fact]
    public void Has_unlockable_and_premium_skins()
    {
        Assert.Contains(SkinCatalog.All, s => s.IsUnlockable);
        Assert.Contains(SkinCatalog.All, s => s.IsPremium);
    }

    [Fact]
    public void Sale_prices_are_lower_than_original_prices()
    {
        foreach (var skin in SkinCatalog.All.Where(s => s.OriginalPrice is not null))
            Assert.True(skin.Price < skin.OriginalPrice, skin.Id);
    }
}

public class PlayerProfileTests
{
    [Fact]
    public void New_profile_owns_only_the_default_skin()
    {
        var profile = new PlayerProfile();
        Assert.Equal([SkinCatalog.DefaultSkinId], profile.OwnedSkinIds);
        Assert.Equal(SkinCatalog.Default, profile.SelectedSkin);
    }

    [Fact]
    public void Ten_calculations_unlock_neko()
    {
        var profile = new PlayerProfile();
        IReadOnlyList<Skin> unlocked = [];
        for (var i = 0; i < 10; i++) unlocked = profile.Record(CalculatorEvent.Evaluated, 1);

        Assert.Contains(unlocked, s => s.Id == "neko");
        Assert.True(profile.Owns("neko"));
    }

    [Fact]
    public void Nine_calculations_are_not_enough()
    {
        var profile = new PlayerProfile();
        for (var i = 0; i < 9; i++) profile.Record(CalculatorEvent.Evaluated, 1);
        Assert.False(profile.Owns("neko"));
    }

    [Fact]
    public void Getting_42_unlocks_kitsune()
    {
        var profile = new PlayerProfile();
        var unlocked = profile.Record(CalculatorEvent.Evaluated, 42);
        Assert.Single(unlocked, s => s.Id == "kitsune");
    }

    [Fact]
    public void Dividing_by_zero_unlocks_glitch_chan()
    {
        var profile = new PlayerProfile();
        var unlocked = profile.Record(CalculatorEvent.DivideByZero, null);
        Assert.Single(unlocked, s => s.Id == "glitch");
        Assert.Equal(1, profile.DivideByZeroAttempts);
    }

    [Fact]
    public void A_skin_is_only_reported_as_unlocked_once()
    {
        var profile = new PlayerProfile();
        Assert.NotEmpty(profile.Record(CalculatorEvent.Evaluated, 42));
        Assert.DoesNotContain(profile.Record(CalculatorEvent.Evaluated, 42), s => s.Id == "kitsune");
    }

    [Fact]
    public void Premium_skins_are_never_unlocked_by_playing()
    {
        var profile = new PlayerProfile();
        for (var i = 0; i < 500; i++) profile.Record(CalculatorEvent.Evaluated, i);
        Assert.DoesNotContain(SkinCatalog.All.Where(s => s.IsPremium), s => profile.Owns(s.Id));
    }

    [Fact]
    public void Cannot_select_a_skin_you_do_not_own()
    {
        var profile = new PlayerProfile();
        Assert.False(profile.Select("ojou"));
        Assert.Equal(SkinCatalog.DefaultSkinId, profile.SelectedSkinId);

        profile.Grant([SkinCatalog.Get("ojou")]);
        Assert.True(profile.Select("ojou"));
        Assert.Equal("ojou", profile.SelectedSkin.Id);
    }

    [Fact]
    public void Normalize_repairs_a_bad_save()
    {
        var profile = new PlayerProfile
        {
            OwnedSkinIds = ["does-not-exist", "idol"],
            SelectedSkinId = "neko",
            Calculations = -5
        }.Normalize();

        Assert.Equal(["idol", SkinCatalog.DefaultSkinId], profile.OwnedSkinIds.Order());
        Assert.Equal(SkinCatalog.DefaultSkinId, profile.SelectedSkinId);
        Assert.Equal(0, profile.Calculations);
    }

    [Fact]
    public void Survives_a_json_round_trip()
    {
        var profile = new PlayerProfile { Calculations = 12, PretendMoneySpent = 4.99m };
        profile.Grant([SkinCatalog.Get("idol")]);
        profile.Select("idol");

        var json = System.Text.Json.JsonSerializer.Serialize(profile);
        var copy = System.Text.Json.JsonSerializer.Deserialize<PlayerProfile>(json)!.Normalize();

        Assert.Equal("idol", copy.SelectedSkinId);
        Assert.Equal(12, copy.Calculations);
        Assert.Equal(4.99m, copy.PretendMoneySpent);
        Assert.True(copy.Owns("idol"));
    }
}

public class CheckoutQuoteTests
{
    [Fact]
    public void Empty_cart_costs_nothing()
    {
        var quote = CheckoutQuote.For([]);
        Assert.Equal(0m, quote.Total);
        Assert.Empty(quote.Fees);
    }

    [Fact]
    public void Adds_joke_fees_on_top_of_the_subtotal()
    {
        var quote = CheckoutQuote.For([SkinCatalog.Get("idol"), SkinCatalog.Get("mecha-maid")]);

        Assert.Equal(14.98m, quote.Subtotal);
        Assert.All(quote.Fees, f => Assert.True(f.IsJokeFee));
        // 14.98 + 2.99 processing + 1.20 sparkle tax + 0.69 emotional support
        Assert.Equal(19.86m, quote.Total);
        Assert.Equal(15m + 30m, quote.YouSave);
    }

    [Fact]
    public void Ignores_free_skins_and_duplicates()
    {
        var idol = SkinCatalog.Get("idol");
        var quote = CheckoutQuote.For([idol, idol, SkinCatalog.Default, SkinCatalog.Get("neko")]);
        Assert.Single(quote.Items);
        Assert.Equal(4.99m, quote.Subtotal);
    }
}
