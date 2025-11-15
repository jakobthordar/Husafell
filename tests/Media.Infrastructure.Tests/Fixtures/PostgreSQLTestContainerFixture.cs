using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Testcontainers.PostgreSql;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Media.Infrastructure.Tests.Fixtures;

public class PostgreSQLTestContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private readonly ILogger<PostgreSQLTestContainerFixture> _logger;

    public string ConnectionString { get; private set; } = string.Empty;

    public PostgreSQLTestContainerFixture()
    {
        _logger = new ConsoleLogger<PostgreSQLTestContainerFixture>();
        
        _container = new TestcontainersBuilder<PostgreSqlContainer>()
            .WithDatabase(new PostgreSqlContainerConfiguration
            {
                Database = "test_media",
                Username = "test_user",
                Password = "test_password"
            })
            .WithImage("postgres:16-alpine")
            .WithPortBinding(5432, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _container.StartAsync();
            ConnectionString = _container.GetConnectionString();
            _logger.LogInformation("PostgreSQL test container started: {ConnectionString}", ConnectionString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start PostgreSQL test container");
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        try
        {
            await _container.DisposeAsync();
            _logger.LogInformation("PostgreSQL test container disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispose PostgreSQL test container");
        }
    }
}

public class ConsoleLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [{logLevel}] [{typeof(T).Name}] {formatter(state, exception)}");
        if (exception != null)
        {
            Console.WriteLine($"Exception: {exception}");
        }
    }
}