namespace Catalog.Domain.Abstractions;

/// <summary>
/// Represents an error that occurs when a domain rule is violated.
/// </summary>
public sealed class DomainRuleViolationException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainRuleViolationException"/> class with the specified message.
    /// </summary>
    /// <param name="message">The exception message that describes the rule violation.</param>
    public DomainRuleViolationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainRuleViolationException"/> class with a specified message and inner exception.
    /// </summary>
    /// <param name="message">The exception message that describes the rule violation.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public DomainRuleViolationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
