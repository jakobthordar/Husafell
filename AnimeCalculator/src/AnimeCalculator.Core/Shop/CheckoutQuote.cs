using AnimeCalculator.Core.Skins;

namespace AnimeCalculator.Core.Shop;

public sealed record QuoteLine(string Label, decimal Amount, bool IsJokeFee = false);

/// <summary>
/// The pretend receipt shown at checkout. Nothing here is ever charged:
/// the shop has no payment integration at all.
/// </summary>
public sealed record CheckoutQuote(IReadOnlyList<QuoteLine> Items, IReadOnlyList<QuoteLine> Fees)
{
    public const decimal KawaiiProcessingFee = 2.99m;
    public const decimal SparkleTaxRate = 0.08m;
    public const decimal EmotionalSupportFee = 0.69m;

    public decimal Subtotal => Items.Sum(i => i.Amount);

    public decimal Total => Subtotal + Fees.Sum(f => f.Amount);

    public decimal YouSave { get; init; }

    public static CheckoutQuote For(IEnumerable<Skin> skins)
    {
        var premium = skins.Where(s => s.IsPremium).DistinctBy(s => s.Id).ToList();
        var items = premium.Select(s => new QuoteLine(s.Name, s.Price!.Value)).ToList();

        if (items.Count == 0) return new CheckoutQuote(items, []);

        var subtotal = items.Sum(i => i.Amount);
        var fees = new List<QuoteLine>
        {
            new("Kawaii processing fee", KawaiiProcessingFee, IsJokeFee: true),
            new("Sparkle tax (8%)", Math.Round(subtotal * SparkleTaxRate, 2, MidpointRounding.AwayFromZero), IsJokeFee: true),
            new("Emotional support for your wallet", EmotionalSupportFee, IsJokeFee: true),
        };

        var youSave = premium.Sum(s => (s.OriginalPrice ?? s.Price!.Value) - s.Price!.Value);
        return new CheckoutQuote(items, fees) { YouSave = youSave };
    }
}
