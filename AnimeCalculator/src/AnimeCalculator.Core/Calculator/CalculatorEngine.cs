using System.Globalization;

namespace AnimeCalculator.Core.Calculator;

public enum Operator
{
    Add,
    Subtract,
    Multiply,
    Divide
}

/// <summary>What happened as a result of the last key press. The mascot reacts to these.</summary>
public enum CalculatorEvent
{
    None,
    Input,
    Evaluated,
    Cleared,
    DivideByZero,
    Overflow
}

/// <summary>
/// A classic "immediate execution" pocket calculator. Pure logic, no UI,
/// so it can be unit tested without a browser.
/// </summary>
public sealed class CalculatorEngine
{
    public const int MaxDigits = 15;

    private decimal? _accumulator;
    private Operator? _pending;
    private string _entry = "0";
    private bool _startNewEntry = true;
    private Operator? _lastOperator;
    private decimal _lastOperand;

    public string Display { get; private set; } = "0";

    /// <summary>The small line above the display, e.g. "12 ×".</summary>
    public string Expression { get; private set; } = string.Empty;

    public bool HasError { get; private set; }

    /// <summary>The last successful result produced by "=", if any.</summary>
    public decimal? LastResult { get; private set; }

    public CalculatorEvent LastEvent { get; private set; }

    public CalculatorEvent InputDigit(int digit)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(digit, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(digit, 9);
        ResetIfError();

        if (_startNewEntry)
        {
            _entry = digit.ToString(CultureInfo.InvariantCulture);
            _startNewEntry = false;
        }
        else if (CountDigits(_entry) < MaxDigits)
        {
            _entry = _entry switch
            {
                "0" => digit.ToString(CultureInfo.InvariantCulture),
                "-0" => "-" + digit,
                _ => _entry + digit
            };
        }

        Display = _entry;
        return Done(CalculatorEvent.Input);
    }

    public CalculatorEvent InputDecimalPoint()
    {
        ResetIfError();

        if (_startNewEntry)
        {
            _entry = "0.";
            _startNewEntry = false;
        }
        else if (!_entry.Contains('.'))
        {
            _entry += ".";
        }

        Display = _entry;
        return Done(CalculatorEvent.Input);
    }

    public CalculatorEvent SetOperator(Operator op)
    {
        ResetIfError();

        // Pressing two operators in a row just swaps the operator.
        if (_startNewEntry && _pending is not null)
        {
            _pending = op;
            Expression = $"{Format(_accumulator!.Value)} {Symbol(op)}";
            return Done(CalculatorEvent.Input);
        }

        var current = ParseEntry();
        if (_accumulator is null || _pending is null)
        {
            _accumulator = current;
        }
        else
        {
            var outcome = TryApply(_accumulator.Value, _pending.Value, current, out var result);
            if (outcome != CalculatorEvent.None) return Fail(outcome);
            _accumulator = result;
            Display = Format(result);
        }

        _pending = op;
        _startNewEntry = true;
        Expression = $"{Format(_accumulator.Value)} {Symbol(op)}";
        return Done(CalculatorEvent.Input);
    }

    public CalculatorEvent Evaluate()
    {
        if (HasError) return Done(CalculatorEvent.None);

        decimal left;
        Operator op;
        decimal right;

        if (_pending is not null && _accumulator is not null)
        {
            left = _accumulator.Value;
            op = _pending.Value;
            right = ParseEntry();
        }
        else if (_lastOperator is not null)
        {
            // Pressing "=" again repeats the last operation, like a real calculator.
            left = ParseEntry();
            op = _lastOperator.Value;
            right = _lastOperand;
        }
        else
        {
            LastResult = ParseEntry();
            _startNewEntry = true;
            Expression = $"{Format(LastResult.Value)} =";
            return Done(CalculatorEvent.Evaluated);
        }

        var outcome = TryApply(left, op, right, out var result);
        if (outcome != CalculatorEvent.None) return Fail(outcome);

        Expression = $"{Format(left)} {Symbol(op)} {Format(right)} =";
        _lastOperator = op;
        _lastOperand = right;
        _accumulator = null;
        _pending = null;
        _entry = Format(result);
        _startNewEntry = true;
        Display = _entry;
        LastResult = result;
        return Done(CalculatorEvent.Evaluated);
    }

    public CalculatorEvent Clear()
    {
        _accumulator = null;
        _pending = null;
        _lastOperator = null;
        _entry = "0";
        _startNewEntry = true;
        HasError = false;
        Display = "0";
        Expression = string.Empty;
        return Done(CalculatorEvent.Cleared);
    }

    public CalculatorEvent Backspace()
    {
        if (HasError) return Clear();
        if (_startNewEntry) return Done(CalculatorEvent.None);

        _entry = _entry.Length <= 1 || (_entry.Length == 2 && _entry.StartsWith('-'))
            ? "0"
            : _entry[..^1];
        Display = _entry;
        return Done(CalculatorEvent.Input);
    }

    public CalculatorEvent ToggleSign()
    {
        ResetIfError();
        if (_startNewEntry)
        {
            _entry = Display;
            _startNewEntry = false;
        }

        _entry = _entry.StartsWith('-') ? _entry[1..] : "-" + _entry;
        Display = _entry;
        return Done(CalculatorEvent.Input);
    }

    public CalculatorEvent Percent()
    {
        ResetIfError();
        var value = ParseEntry();
        var result = _pending is Operator.Add or Operator.Subtract && _accumulator is not null
            ? _accumulator.Value * value / 100m
            : value / 100m;

        _entry = Format(result);
        _startNewEntry = false;
        Display = _entry;
        return Done(CalculatorEvent.Input);
    }

    public static string Symbol(Operator op) => op switch
    {
        Operator.Add => "+",
        Operator.Subtract => "−",
        Operator.Multiply => "×",
        Operator.Divide => "÷",
        _ => "?"
    };

    public static string Format(decimal value)
    {
        // Round to something that fits the display, then strip trailing zeros.
        var rounded = Math.Round(value, 10, MidpointRounding.AwayFromZero);
        var text = rounded.ToString("0.##########", CultureInfo.InvariantCulture);
        return text == "-0" ? "0" : text;
    }

    private static CalculatorEvent TryApply(decimal left, Operator op, decimal right, out decimal result)
    {
        result = 0;
        if (op == Operator.Divide && right == 0) return CalculatorEvent.DivideByZero;

        try
        {
            result = op switch
            {
                Operator.Add => left + right,
                Operator.Subtract => left - right,
                Operator.Multiply => left * right,
                Operator.Divide => left / right,
                _ => throw new ArgumentOutOfRangeException(nameof(op))
            };
        }
        catch (OverflowException)
        {
            return CalculatorEvent.Overflow;
        }

        return CountDigits(Format(result)) > 28 ? CalculatorEvent.Overflow : CalculatorEvent.None;
    }

    private decimal ParseEntry()
    {
        var text = _startNewEntry ? Display : _entry;
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : 0m;
    }

    private static int CountDigits(string text) => text.Count(char.IsDigit);

    private void ResetIfError()
    {
        if (HasError) Clear();
    }

    private CalculatorEvent Fail(CalculatorEvent reason)
    {
        HasError = true;
        _accumulator = null;
        _pending = null;
        _lastOperator = null;
        _startNewEntry = true;
        Display = reason == CalculatorEvent.DivideByZero ? "Nani?!" : "Too big!";
        Expression = string.Empty;
        return Done(reason);
    }

    private CalculatorEvent Done(CalculatorEvent e)
    {
        LastEvent = e;
        return e;
    }
}
