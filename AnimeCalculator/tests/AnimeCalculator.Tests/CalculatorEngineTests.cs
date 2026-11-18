using AnimeCalculator.Core.Calculator;

namespace AnimeCalculator.Tests;

public class CalculatorEngineTests
{
    private static CalculatorEngine Press(string keys)
    {
        var engine = new CalculatorEngine();
        foreach (var key in keys.Replace(" ", ""))
        {
            _ = key switch
            {
                >= '0' and <= '9' => engine.InputDigit(key - '0'),
                '.' => engine.InputDecimalPoint(),
                '+' => engine.SetOperator(Operator.Add),
                '-' => engine.SetOperator(Operator.Subtract),
                '*' => engine.SetOperator(Operator.Multiply),
                '/' => engine.SetOperator(Operator.Divide),
                '=' => engine.Evaluate(),
                '%' => engine.Percent(),
                'n' => engine.ToggleSign(),
                '<' => engine.Backspace(),
                'C' => engine.Clear(),
                _ => throw new ArgumentException($"Unknown key {key}")
            };
        }
        return engine;
    }

    [Theory]
    [InlineData("2+3=", "5")]
    [InlineData("10-4=", "6")]
    [InlineData("6*7=", "42")]
    [InlineData("1/4=", "0.25")]
    [InlineData("0.1+0.2=", "0.3")]
    [InlineData("2+3*4=", "20")] // immediate execution, like a pocket calculator
    [InlineData("1/3=", "0.3333333333")]
    [InlineData("5-8=", "-3")]
    [InlineData("2.50*2=", "5")]
    public void Evaluates_basic_arithmetic(string keys, string expected)
    {
        Assert.Equal(expected, Press(keys).Display);
    }

    [Fact]
    public void Starts_at_zero() => Assert.Equal("0", new CalculatorEngine().Display);

    [Fact]
    public void Repeated_equals_repeats_last_operation()
    {
        Assert.Equal("11", Press("5+2===").Display);
    }

    [Fact]
    public void Changing_operator_replaces_the_pending_one()
    {
        var engine = Press("8+*2=");
        Assert.Equal("16", engine.Display);
    }

    [Fact]
    public void Operator_then_equals_uses_the_same_number()
    {
        Assert.Equal("10", Press("5+=").Display);
    }

    [Fact]
    public void Shows_running_total_when_chaining()
    {
        var engine = Press("2+3+");
        Assert.Equal("5", engine.Display);
        Assert.Equal("5 +", engine.Expression);
    }

    [Fact]
    public void Expression_shows_the_whole_sum_after_equals()
    {
        Assert.Equal("6 × 7 =", Press("6*7=").Expression);
    }

    [Fact]
    public void Divide_by_zero_is_an_error_and_next_digit_recovers()
    {
        var engine = new CalculatorEngine();
        engine.InputDigit(5);
        engine.SetOperator(Operator.Divide);
        engine.InputDigit(0);

        Assert.Equal(CalculatorEvent.DivideByZero, engine.Evaluate());
        Assert.True(engine.HasError);
        Assert.Equal("Nani?!", engine.Display);

        engine.InputDigit(7);
        Assert.False(engine.HasError);
        Assert.Equal("7", engine.Display);
    }

    [Fact]
    public void Overflow_is_reported_instead_of_crashing()
    {
        var engine = new CalculatorEngine();
        foreach (var _ in Enumerable.Range(0, 15)) engine.InputDigit(9);
        engine.SetOperator(Operator.Multiply);
        foreach (var _ in Enumerable.Range(0, 15)) engine.InputDigit(9);

        Assert.Equal(CalculatorEvent.Overflow, engine.Evaluate());
        Assert.True(engine.HasError);
    }

    [Fact]
    public void Only_one_decimal_point_is_allowed()
    {
        Assert.Equal("1.25", Press("1..2.5").Display);
    }

    [Fact]
    public void Decimal_point_first_adds_leading_zero()
    {
        Assert.Equal("0.5", Press(".5").Display);
    }

    [Fact]
    public void Leading_zeros_are_dropped()
    {
        Assert.Equal("7", Press("0007").Display);
    }

    [Fact]
    public void Entry_is_capped_at_max_digits()
    {
        var engine = Press(new string('1', 20));
        Assert.Equal(CalculatorEngine.MaxDigits, engine.Display.Length);
    }

    [Fact]
    public void Backspace_removes_last_digit()
    {
        Assert.Equal("12", Press("123<").Display);
        Assert.Equal("0", Press("1<<").Display);
    }

    [Fact]
    public void Toggle_sign_negates_entry_and_result()
    {
        Assert.Equal("-5", Press("5n").Display);
        Assert.Equal("5", Press("5nn").Display);
        Assert.Equal("-3", Press("1+2=n").Display);
        Assert.Equal("-12", Press("n12").Display);
    }

    [Fact]
    public void Percent_alone_divides_by_hundred()
    {
        Assert.Equal("0.5", Press("50%").Display);
    }

    [Fact]
    public void Percent_after_add_is_percent_of_the_total()
    {
        Assert.Equal("220", Press("200+10%=").Display);
    }

    [Fact]
    public void Clear_resets_everything()
    {
        var engine = Press("12+3C");
        Assert.Equal("0", engine.Display);
        Assert.Equal(string.Empty, engine.Expression);
        Assert.Equal("4", Press("12+3C4=").Display);
    }

    [Fact]
    public void Evaluate_sets_last_result_and_event()
    {
        var engine = Press("40+2");
        Assert.Equal(CalculatorEvent.Evaluated, engine.Evaluate());
        Assert.Equal(42m, engine.LastResult);
    }

    [Fact]
    public void Typing_after_result_starts_a_new_number()
    {
        Assert.Equal("9", Press("2+2=9").Display);
    }

    [Fact]
    public void Operator_after_result_continues_from_it()
    {
        Assert.Equal("8", Press("2+2=*2=").Display);
    }

    [Theory]
    [InlineData(1.5, "1.5")]
    [InlineData(-0.0, "0")]
    [InlineData(100, "100")]
    [InlineData(2.00000000001, "2")]
    public void Format_trims_noise(double value, string expected)
    {
        Assert.Equal(expected, CalculatorEngine.Format((decimal)value));
    }
}
