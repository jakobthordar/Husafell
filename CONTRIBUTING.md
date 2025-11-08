# Contributing to Aspire Clean Architecture

Thank you for your interest in contributing! This document provides guidelines and instructions for contributing to this project.

## Development Setup

1. **Install Prerequisites**
   - .NET 9.0 SDK or higher
   - .NET Aspire workload
   - Your preferred IDE (Visual Studio, Rider, VS Code)

2. **Clone and Build**
   ```bash
   git clone https://github.com/jakobthordar/Husafell.git
   cd Husafell
   dotnet restore
   dotnet build
   ```

3. **Run Tests**
   ```bash
   dotnet test
   ```

## Development Workflow

### 1. Create a Branch

Create a feature branch from `main`:
```bash
git checkout -b feature/your-feature-name
```

Branch naming conventions:
- `feature/` - New features
- `fix/` - Bug fixes
- `refactor/` - Code refactoring
- `docs/` - Documentation updates
- `test/` - Test additions or updates

### 2. Make Changes

- Follow the existing code style (enforced by `.editorconfig`)
- Write meaningful commit messages
- Add tests for new functionality
- Update documentation as needed

### 3. Code Style

This project uses:
- C# 12 language features
- Nullable reference types enabled
- Implicit usings
- File-scoped namespaces where appropriate
- Record types for value objects

Run code formatting before committing:
```bash
dotnet format
```

### 4. Testing

- Write unit tests for domain logic
- Write integration tests for infrastructure
- Ensure all tests pass before submitting
- Aim for high code coverage on business logic

Run tests with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### 5. Commit Guidelines

Write clear, descriptive commit messages:

```
<type>(<scope>): <subject>

<body>

<footer>
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `refactor`: Code refactoring
- `test`: Test additions or updates
- `docs`: Documentation changes
- `chore`: Maintenance tasks

Example:
```
feat(catalog): add multi-language support for work descriptions

Implement LocalizedText value object to support translations
in multiple cultures with fallback logic.

Closes #123
```

### 6. Pull Request Process

1. **Update your branch** with the latest changes from `main`:
   ```bash
   git fetch origin
   git rebase origin/main
   ```

2. **Push your branch**:
   ```bash
   git push origin feature/your-feature-name
   ```

3. **Create a Pull Request** on GitHub with:
   - Clear title and description
   - Reference to related issues
   - Screenshots/examples if applicable
   - Test results

4. **Code Review**:
   - Address reviewer feedback
   - Keep discussion focused and professional
   - Update PR as needed

5. **Merge**:
   - Squash commits if requested
   - Delete branch after merge

## Architecture Guidelines

### Clean Architecture Layers

```
Domain ← Application ← Infrastructure ← API
```

**Rules:**
- Domain has no dependencies
- Application depends only on Domain
- Infrastructure depends on Application and Domain
- API depends on all layers but only references abstractions

### Domain Layer

- **Entities**: Have identity, mutable state
- **Value Objects**: Immutable, defined by attributes
- **Domain Events**: Raised by entities on state changes
- **Aggregates**: Consistency boundaries
- **Domain Services**: When behavior doesn't fit in entities

Guidelines:
- No external dependencies (except System namespaces)
- Business rules live here
- Use guards for validation
- Raise domain events for important state changes

### Application Layer

- **Service Interfaces**: Define contracts
- **DTOs**: Data transfer objects
- **Commands/Queries**: CQRS pattern (if applicable)

Guidelines:
- Define interfaces, not implementations
- Orchestrate domain objects
- Transaction boundaries
- No infrastructure concerns

### Infrastructure Layer

- **Repository Implementations**
- **Data Access**
- **External Service Integrations**

Guidelines:
- Implement application contracts
- Handle data persistence
- Manage external dependencies

### API Layer

- **Controllers/Endpoints**
- **Dependency Injection**
- **Middleware**

Guidelines:
- Thin controllers
- Use DTOs for requests/responses
- Handle HTTP concerns only

## Domain-Driven Design Patterns

### Value Objects

```csharp
public sealed record Email
{
    private Email(string value) => Value = value;

    public string Value { get; }

    public static Email Create(string value)
    {
        // Validation
        return new Email(value);
    }
}
```

### Entities

```csharp
public abstract class Entity<TId>
{
    protected Entity(TId id) => Id = id;

    public TId Id { get; }

    // Equality based on Id
}
```

### Aggregates

```csharp
public class Order : Entity<Guid>
{
    private readonly List<OrderLine> _lines = new();

    public void AddLine(Product product, int quantity)
    {
        // Business rule validation
        _lines.Add(new OrderLine(product, quantity));
        RaiseDomainEvent(new OrderLineAdded(Id, product.Id));
    }
}
```

## Testing Guidelines

### Unit Tests

- Test domain logic in isolation
- Use descriptive test names
- Follow AAA pattern (Arrange, Act, Assert)

```csharp
public class AccessionNumberTests
{
    [Theory]
    [InlineData("inv-001")]
    public void Create_ShouldNormaliseValue(string input)
    {
        // Arrange & Act
        var result = AccessionNumber.Create(input);

        // Assert
        Assert.Equal("INV-001", result.Value);
    }
}
```

### Integration Tests

- Test infrastructure implementations
- Use in-memory databases where possible
- Clean up test data

## Documentation

- Update README.md for user-facing changes
- Write XML documentation for public APIs
- Add inline comments for complex logic
- Update CHANGELOG.md (if exists)

## Security

- Never commit secrets or credentials
- Use user secrets for local development
- Validate all inputs
- Follow OWASP guidelines
- Report security issues privately

## Performance

- Profile before optimizing
- Use async/await appropriately
- Consider caching strategies
- Monitor allocations

## Questions?

If you have questions:
1. Check existing documentation
2. Search existing issues
3. Ask in discussions
4. Create a new issue

## Code of Conduct

- Be respectful and inclusive
- Welcome newcomers
- Focus on constructive feedback
- Assume good intentions

Thank you for contributing!
