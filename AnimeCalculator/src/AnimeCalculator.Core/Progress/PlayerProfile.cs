using AnimeCalculator.Core.Calculator;
using AnimeCalculator.Core.Skins;

namespace AnimeCalculator.Core.Progress;

public sealed record PlayerStats(int Calculations, int DivideByZeroAttempts);

/// <summary>
/// Everything we remember about the player between visits. Serialised to JSON
/// and kept in the browser's localStorage.
/// </summary>
public sealed class PlayerProfile
{
    public HashSet<string> OwnedSkinIds { get; set; } = [SkinCatalog.DefaultSkinId];

    public string SelectedSkinId { get; set; } = SkinCatalog.DefaultSkinId;

    public int Calculations { get; set; }

    public int DivideByZeroAttempts { get; set; }

    /// <summary>How much money the player would have spent if the shop were real.</summary>
    public decimal PretendMoneySpent { get; set; }

    public PlayerStats Stats => new(Calculations, DivideByZeroAttempts);

    public Skin SelectedSkin => SkinCatalog.Find(SelectedSkinId) ?? SkinCatalog.Default;

    public bool Owns(string skinId) => OwnedSkinIds.Contains(skinId);

    /// <summary>
    /// Records what the calculator just did and returns any skins that this unlocked.
    /// </summary>
    public IReadOnlyList<Skin> Record(CalculatorEvent e, decimal? result)
    {
        switch (e)
        {
            case CalculatorEvent.Evaluated:
                Calculations++;
                break;
            case CalculatorEvent.DivideByZero:
                DivideByZeroAttempts++;
                break;
        }

        var stats = Stats;
        var unlocked = SkinCatalog.All
            .Where(s => s.Unlock is not null && !Owns(s.Id) && s.Unlock.IsMet(stats, e, result))
            .ToList();

        foreach (var skin in unlocked) OwnedSkinIds.Add(skin.Id);
        return unlocked;
    }

    public void Grant(IEnumerable<Skin> skins)
    {
        foreach (var skin in skins) OwnedSkinIds.Add(skin.Id);
    }

    public bool Select(string skinId)
    {
        if (!Owns(skinId) || SkinCatalog.Find(skinId) is null) return false;
        SelectedSkinId = skinId;
        return true;
    }

    /// <summary>Fixes up anything odd that came back from storage.</summary>
    public PlayerProfile Normalize()
    {
        OwnedSkinIds ??= [];
        OwnedSkinIds.RemoveWhere(id => SkinCatalog.Find(id) is null);
        OwnedSkinIds.Add(SkinCatalog.DefaultSkinId);
        if (!Owns(SelectedSkinId)) SelectedSkinId = SkinCatalog.DefaultSkinId;
        Calculations = Math.Max(0, Calculations);
        DivideByZeroAttempts = Math.Max(0, DivideByZeroAttempts);
        return this;
    }
}
