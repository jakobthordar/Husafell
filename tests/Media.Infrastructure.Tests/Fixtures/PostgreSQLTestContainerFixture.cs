using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Logging;

namespace Media.Infrastructure.Tests.Fixtures;

public class PostgreSQLTestContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlTestcontainer _container;
    private readonly ILogger<PostgreSQLTestContainerFixture> _logger;

    public string ConnectionString { get; private set; } = string.Empty;

    public PostgreSQLTestContainerFixture()
    {
        _logger = new ConsoleLogger<PostgreSQLTestContainerFixture>();
        
        _container = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "test_media",
                Username = "test_user",
                Password = "test_password"
            })
            .WithImage("postgres:16-alpine")
            .WithPortBinding(5432, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .WithLogger(new TestcontainersLogger(_logger))
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

public class TestcontainersLogger : IContainerLogger
{
    private readonly ILogger _logger;

    public TestcontainersLogger(ILogger logger)
    {
        _logger = logger;
    }

    public void Debug(string message, params object[] args)
    {
        _logger.LogDebug(message, args);
    }

    public void Error(string message, params object[] args)
    {
        _logger.LogError(message, args);
    }

    public void Information(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    public void Verbose(string message, params object[] args)
    {
        _logger.LogTrace(message, args);
    }

    public void Warning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }
}