using AnimeCalculator.Core.Calculator;
using AnimeCalculator.Core.Progress;

namespace AnimeCalculator.Core.Skins;

/// <summary>A challenge that unlocks a skin for free.</summary>
public abstract record UnlockRule(string Description)
{
    public abstract bool IsMet(PlayerStats stats, CalculatorEvent lastEvent, decimal? lastResult);
}

public sealed record CalculationsAtLeast(int Count)
    : UnlockRule($"Press = {Count} times")
{
    public override bool IsMet(PlayerStats stats, CalculatorEvent lastEvent, decimal? lastResult) =>
        stats.Calculations >= Count;
}

public sealed record ResultEquals(decimal Value, string Hint)
    : UnlockRule(Hint)
{
    public override bool IsMet(PlayerStats stats, CalculatorEvent lastEvent, decimal? lastResult) =>
        lastEvent == CalculatorEvent.Evaluated && lastResult == Value;
}

public sealed record DivideByZeroRule()
    : UnlockRule("Do the forbidden thing (divide by zero)")
{
    public override bool IsMet(PlayerStats stats, CalculatorEvent lastEvent, decimal? lastResult) =>
        stats.DivideByZeroAttempts > 0;
}
