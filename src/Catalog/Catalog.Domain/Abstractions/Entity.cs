namespace Catalog.Domain.Abstractions;

/// <summary>
/// Provides the base type for domain entities that are identified by a strongly typed identifier.
/// </summary>
/// <typeparam name="TIdentifier">The type used to uniquely identify the entity.</typeparam>
public abstract class Entity<TIdentifier> : IEquatable<Entity<TIdentifier>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TIdentifier}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    /// <exception cref="ArgumentException">Thrown when the identifier is the default value for its type.</exception>
    protected Entity(TIdentifier id)
    {
        if (EqualityComparer<TIdentifier>.Default.Equals(id, default!))
        {
            throw new ArgumentException("Entity identifier cannot be the type's default value.", nameof(id));
        }

        Id = id;
    }

    /// <summary>
    /// Gets the unique identifier for the entity.
    /// </summary>
    public TIdentifier Id { get; }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as Entity<TIdentifier>);

    /// <inheritdoc />
    public bool Equals(Entity<TIdentifier>? other)
    {
        if (other is null)
        {
            return false;
        }

        return EqualityComparer<TIdentifier>.Default.Equals(Id, other.Id);
    }

    /// <inheritdoc />
    public override int GetHashCode() => EqualityComparer<TIdentifier>.Default.GetHashCode(Id!);
}
