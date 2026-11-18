namespace AnimeCalculator.Core.Skins;

public enum HairStyle
{
    Twintails,
    Long,
    Bob,
    Ponytail
}

public enum Accessory
{
    None,
    Bow,
    CatEars,
    FoxEars,
    Antenna,
    Headphones,
    Headband,
    StarClip,
    Halo,
    Crown
}

public enum Rarity
{
    Common,
    Rare,
    Epic,
    Legendary,
    Mythic
}

/// <summary>How the mascot character is drawn.</summary>
public sealed record MascotLook(
    HairStyle Hair,
    string HairColor,
    string HairShade,
    string EyeColor,
    string OutfitColor,
    string OutfitAccent,
    Accessory Accessory,
    string AccessoryColor,
    string SkinTone = "#ffe3d3");

/// <summary>Colours for the calculator itself. Each value becomes a CSS custom property.</summary>
public sealed record SkinPalette(
    string Background1,
    string Background2,
    string Panel,
    string Display,
    string DisplayText,
    string Key,
    string KeyText,
    string OperatorKey,
    string OperatorKeyText,
    string Accent,
    string Text);

public sealed record Skin(
    string Id,
    string Name,
    string CharacterName,
    string Tagline,
    Rarity Rarity,
    SkinPalette Palette,
    MascotLook Look,
    decimal? Price = null,
    decimal? OriginalPrice = null,
    UnlockRule? Unlock = null)
{
    /// <summary>Free from the start: no price and no unlock challenge.</summary>
    public bool IsStarter => Price is null && Unlock is null;

    /// <summary>Sold in the (totally fake) shop.</summary>
    public bool IsPremium => Price is not null;

    public bool IsUnlockable => Unlock is not null;
}
