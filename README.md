# Aspire Clean Architecture

A modern .NET 9 application built with Clean Architecture principles and .NET Aspire orchestration.

## Architecture

This solution follows Clean Architecture with Domain-Driven Design (DDD) patterns:

```
├── src/
│   ├── AppHost/                    # .NET Aspire orchestration host
│   ├── ServiceDefaults/            # Shared service configuration
│   ├── Catalog/                    # Catalog bounded context
│   │   ├── Catalog.Domain/         # Domain entities, value objects, and events
│   │   ├── Catalog.Application/    # Application services and contracts
│   │   ├── Catalog.Infrastructure/ # Data access and external integrations
│   │   └── Catalog.Api/            # REST API endpoints
│   └── Media/                      # Media bounded context
│       ├── Media.Domain/
│       ├── Media.Application/
│       ├── Media.Infrastructure/
│       └── Media.Api/
└── tests/                          # Unit and integration tests
```

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or higher
- [.NET Aspire workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling)

## Quick Start

### 1. Install .NET 9 SDK

**Linux/macOS:**
```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0
export PATH="$HOME/.dotnet:$PATH"
```

**Windows (PowerShell):**
```powershell
Invoke-WebRequest -Uri https://dot.net/v1/dotnet-install.ps1 -OutFile dotnet-install.ps1
.\dotnet-install.ps1 -Channel 9.0
```

**Verify installation:**
```bash
dotnet --version
```

### 2. Install .NET Aspire Workload

```bash
dotnet workload update
dotnet workload install aspire
```

### 3. Restore Dependencies

```bash
dotnet restore AspireCleanArchitecture.sln
```

### 4. Build the Solution

```bash
dotnet build AspireCleanArchitecture.sln
```

### 5. Run Tests

```bash
dotnet test AspireCleanArchitecture.sln
```

### 6. Run the Application

```bash
cd src/AppHost
dotnet run
```

The Aspire dashboard will be available at the URL shown in the console output (typically `http://localhost:15000`).

## Project Structure

### Domain Layer
- **Entities**: Aggregate roots with unique identities
- **Value Objects**: Immutable objects defined by their attributes
- **Domain Events**: Events raised by domain entities
- **No external dependencies**

### Application Layer
- **Service Contracts**: Interfaces defining application operations
- **DTOs**: Data transfer objects for API contracts
- **Depends only on Domain layer**

### Infrastructure Layer
- **Implementations**: Concrete implementations of application contracts
- **Data Access**: Repository patterns and database contexts
- **External Services**: Third-party integrations

### API Layer
- **REST Endpoints**: HTTP API endpoints
- **Dependency Injection**: Service configuration
- **Middleware**: Cross-cutting concerns

## Development

### Code Quality

**Format code:**
```bash
dotnet format AspireCleanArchitecture.sln
```

**Check for security vulnerabilities:**
```bash
dotnet list package --vulnerable --include-transitive
```

### Running Individual Services

**Catalog API:**
```bash
cd src/Catalog/Catalog.Api
dotnet run
```

**Media API:**
```bash
cd src/Media/Media.Api
dotnet run
```

## Key Features

### Catalog Module
- Work registration and management
- Multi-language support via `LocalizedText`
- Accession number validation
- Dimension tracking with units
- Asset management (images, files)

### Media Module
- Media library management
- File storage and retrieval

### Cross-Cutting Concerns
- Domain events pattern
- Value object validation
- Clean separation of concerns
- Comprehensive test coverage

## Domain Concepts

### Value Objects
- **AccessionNumber**: Registrar-issued identifier (e.g., "INV-0001")
- **LocalizedText**: Multi-culture text support
- **Dimensions**: Physical measurements with units
- **Slug**: URL-friendly identifiers

### Aggregates
- **Work**: Catalog work with assets and metadata

## Testing

Tests are organized by layer and module:

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Catalog.Domain.Tests/

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## CI/CD

This project includes a GitHub Actions workflow that:
- Builds the solution
- Runs all tests
- Performs code quality checks
- Scans for security vulnerabilities
- Uploads test results and coverage reports

See `.github/workflows/ci.yml` for details.

## Configuration

### NuGet Package Sources

Package sources are configured in `nuget.config`. The default configuration uses nuget.org.

### Environment Variables

- `DOTNET_CLI_TELEMETRY_OPTOUT`: Set to `1` to disable telemetry
- `ASPNETCORE_ENVIRONMENT`: Set to `Development`, `Staging`, or `Production`

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Code Style

- Enable nullable reference types
- Use implicit usings
- Follow C# naming conventions
- Write XML documentation for public APIs
- Use record types for value objects
- Prefer immutability

## Troubleshooting

### Build Errors

**Problem**: Cannot find .NET 9 SDK
```
Solution: Ensure .NET 9 SDK is installed and in your PATH
```

**Problem**: Missing Aspire packages
```bash
Solution: Install the Aspire workload
dotnet workload install aspire
```

**Problem**: NuGet restore fails
```
Solution: Clear NuGet cache and restore
dotnet nuget locals all --clear
dotnet restore
```

### Runtime Errors

**Problem**: Connection refused to services
```
Solution: Ensure all required services are running via AppHost
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Resources

- [.NET 9 Documentation](https://learn.microsoft.com/dotnet/)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Clean Architecture Guide](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/tags/domain%20driven%20design.html)
