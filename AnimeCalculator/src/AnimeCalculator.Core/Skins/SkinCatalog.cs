namespace AnimeCalculator.Core.Skins;

/// <summary>
/// Every skin in the game. All characters are original designs made for this app.
/// </summary>
public static class SkinCatalog
{
    public const string DefaultSkinId = "sakura";

    public static IReadOnlyList<Skin> All { get; } =
    [
        new Skin(
            Id: DefaultSkinId,
            Name: "Sakura Default",
            CharacterName: "Momo",
            Tagline: "Your cheerful math buddy. Ganbatte!",
            Rarity: Rarity.Common,
            Palette: new SkinPalette(
                Background1: "#ffd6e7", Background2: "#ffeef6", Panel: "#fff7fb",
                Display: "#ffe4f0", DisplayText: "#7a2650",
                Key: "#ffffff", KeyText: "#8a3a62",
                OperatorKey: "#ff7eb6", OperatorKeyText: "#ffffff",
                Accent: "#ff4f9a", Text: "#5b2140"),
            Look: new MascotLook(HairStyle.Twintails, "#ff9cc7", "#e0679e", "#c2185b",
                "#ffffff", "#ff4f9a", Accessory.Bow, "#ff4f9a")),

        // ---- Unlockable with challenges ----
        new Skin(
            Id: "neko",
            Name: "Neko Nightshift",
            CharacterName: "Kuro",
            Tagline: "Does math at 3am. Nya.",
            Rarity: Rarity.Rare,
            Palette: new SkinPalette(
                Background1: "#1e1433", Background2: "#3a2366", Panel: "#2a1c47",
                Display: "#140d24", DisplayText: "#d9c2ff",
                Key: "#3b2a60", KeyText: "#e9dcff",
                OperatorKey: "#a970ff", OperatorKeyText: "#1e1433",
                Accent: "#c69bff", Text: "#efe6ff"),
            Look: new MascotLook(HairStyle.Long, "#2b2140", "#140e22", "#ffd34d",
                "#3b2a60", "#a970ff", Accessory.CatEars, "#2b2140"),
            Unlock: new CalculationsAtLeast(10)),

        new Skin(
            Id: "kitsune",
            Name: "Kitsune of Answers",
            CharacterName: "Kokona",
            Tagline: "Knows the answer to life, the universe and everything.",
            Rarity: Rarity.Rare,
            Palette: new SkinPalette(
                Background1: "#ffb36b", Background2: "#fff0dc", Panel: "#fffaf3",
                Display: "#fff1e0", DisplayText: "#8c3b00",
                Key: "#ffffff", KeyText: "#a04a10",
                OperatorKey: "#ff7a1a", OperatorKeyText: "#ffffff",
                Accent: "#e85d00", Text: "#5a2600"),
            Look: new MascotLook(HairStyle.Ponytail, "#ff9a3c", "#d96a12", "#b33b00",
                "#fff4e6", "#d32f2f", Accessory.FoxEars, "#ff9a3c"),
            Unlock: new ResultEquals(42, "Get the answer to everything (= 42)")),

        new Skin(
            Id: "glitch",
            Name: "Glitch-chan",
            CharacterName: "Err0r",
            Tagline: "You divided by zero. She was born from the void.",
            Rarity: Rarity.Epic,
            Palette: new SkinPalette(
                Background1: "#001a0f", Background2: "#00331f", Panel: "#00140c",
                Display: "#000a05", DisplayText: "#39ff88",
                Key: "#00261a", KeyText: "#39ff88",
                OperatorKey: "#39ff88", OperatorKeyText: "#001a0f",
                Accent: "#39ff88", Text: "#b6ffd5"),
            Look: new MascotLook(HairStyle.Bob, "#1de9b6", "#00897b", "#39ff88",
                "#0b2e22", "#39ff88", Accessory.Antenna, "#39ff88"),
            Unlock: new DivideByZeroRule()),

        new Skin(
            Id: "lofi",
            Name: "Lo-fi Study Girl",
            CharacterName: "Shizu",
            Tagline: "Beats to calculate and relax to.",
            Rarity: Rarity.Epic,
            Palette: new SkinPalette(
                Background1: "#c9a27e", Background2: "#f3e6d6", Panel: "#fbf4ea",
                Display: "#efe1cf", DisplayText: "#4a3426",
                Key: "#fffaf3", KeyText: "#5c4030",
                OperatorKey: "#8d6e63", OperatorKeyText: "#fffaf3",
                Accent: "#6d4c41", Text: "#3e2a1f"),
            Look: new MascotLook(HairStyle.Bob, "#6d4c41", "#4e342e", "#5d4037",
                "#f6c26b", "#e57373", Accessory.Headphones, "#e57373"),
            Unlock: new CalculationsAtLeast(50)),

        // ---- "Premium" skins in the fake shop ----
        new Skin(
            Id: "idol",
            Name: "Idol Supernova",
            CharacterName: "Hoshimi",
            Tagline: "Center stage of every equation! ☆",
            Rarity: Rarity.Rare,
            Palette: new SkinPalette(
                Background1: "#ffe66d", Background2: "#ff8fd8", Panel: "#fffdf5",
                Display: "#fff5c2", DisplayText: "#7a3d00",
                Key: "#ffffff", KeyText: "#b0306f",
                OperatorKey: "#ff4fb8", OperatorKeyText: "#ffffff",
                Accent: "#ff4fb8", Text: "#5a1c3e"),
            Look: new MascotLook(HairStyle.Twintails, "#ffe066", "#f2b705", "#e91e63",
                "#ff4fb8", "#ffe066", Accessory.StarClip, "#ffd400"),
            Price: 4.99m, OriginalPrice: 19.99m),

        new Skin(
            Id: "mecha-maid",
            Name: "Mecha Maid Unit-07",
            CharacterName: "Unit-07",
            Tagline: "Calculating your order, master. Beep.",
            Rarity: Rarity.Epic,
            Palette: new SkinPalette(
                Background1: "#cfd8e3", Background2: "#f4f7fb", Panel: "#ffffff",
                Display: "#1f2a38", DisplayText: "#7fe3ff",
                Key: "#eef2f7", KeyText: "#2b3a4d",
                OperatorKey: "#2b3a4d", OperatorKeyText: "#7fe3ff",
                Accent: "#00b8e6", Text: "#1f2a38"),
            Look: new MascotLook(HairStyle.Bob, "#dfe6ee", "#9aa8b8", "#00b8e6",
                "#2b3a4d", "#ffffff", Accessory.Headband, "#ffffff"),
            Price: 9.99m, OriginalPrice: 39.99m),

        new Skin(
            Id: "ocean",
            Name: "Ocean Princess",
            CharacterName: "Mizuki",
            Tagline: "Her numbers run deeper than the sea.",
            Rarity: Rarity.Epic,
            Palette: new SkinPalette(
                Background1: "#4fc3f7", Background2: "#e1f5fe", Panel: "#f5fcff",
                Display: "#d7f1ff", DisplayText: "#01426a",
                Key: "#ffffff", KeyText: "#0277bd",
                OperatorKey: "#0288d1", OperatorKeyText: "#ffffff",
                Accent: "#00acc1", Text: "#01304d"),
            Look: new MascotLook(HairStyle.Long, "#4dd0e1", "#0097a7", "#0277bd",
                "#e0f7fa", "#0288d1", Accessory.StarClip, "#ffffff"),
            Price: 14.99m, OriginalPrice: 49.99m),

        new Skin(
            Id: "angel",
            Name: "Celestial Angel",
            CharacterName: "Seraphina",
            Tagline: "Blessed arithmetic. Every answer is divine.",
            Rarity: Rarity.Legendary,
            Palette: new SkinPalette(
                Background1: "#fff8e1", Background2: "#e3f2fd", Panel: "#ffffff",
                Display: "#fffdf2", DisplayText: "#8d6e00",
                Key: "#ffffff", KeyText: "#6d5a00",
                OperatorKey: "#ffd54f", OperatorKeyText: "#5d4600",
                Accent: "#ffb300", Text: "#4a3b00"),
            Look: new MascotLook(HairStyle.Long, "#fffaf0", "#e6d8b8", "#64b5f6",
                "#ffffff", "#ffd54f", Accessory.Halo, "#ffd54f"),
            Price: 49.99m, OriginalPrice: 129.99m),

        new Skin(
            Id: "ojou",
            Name: "Founder's Platinum Ojou-sama",
            CharacterName: "Reika",
            Tagline: "Ohohoho! Only the finest decimals for me.",
            Rarity: Rarity.Mythic,
            Palette: new SkinPalette(
                Background1: "#2b1d0e", Background2: "#6b4e16", Panel: "#1c1309",
                Display: "#0f0a04", DisplayText: "#ffd96b",
                Key: "#33240f", KeyText: "#ffe7a3",
                OperatorKey: "#e6b422", OperatorKeyText: "#2b1d0e",
                Accent: "#ffd96b", Text: "#fff1cc"),
            Look: new MascotLook(HairStyle.Twintails, "#f5c542", "#c9961a", "#8e24aa",
                "#6a1b9a", "#f5c542", Accessory.Crown, "#ffd700"),
            Price: 199.99m, OriginalPrice: 999.99m),
    ];

    public static Skin Default => Get(DefaultSkinId);

    public static Skin Get(string id) =>
        All.FirstOrDefault(s => s.Id == id) ?? throw new KeyNotFoundException($"Unknown skin '{id}'.");

    public static Skin? Find(string? id) => All.FirstOrDefault(s => s.Id == id);
}
