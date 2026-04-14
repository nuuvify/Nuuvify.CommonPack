using Microsoft.EntityFrameworkCore;
using Nuuvify.CommonPack.UnitOfWork.Examples;
using Testcontainers.MsSql;
using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.Integration.xTest.Fixtures;

/// <summary>
/// Fixture for integration tests using SQL Server in a Docker container via Testcontainers.
/// Falls back to EF Core InMemory provider when Docker is not available.
/// </summary>
/// <remarks>
/// This fixture attempts to use Testcontainers to spin up a SQL Server container for testing.
/// If Docker is not available, it falls back to EF Core InMemory provider.
///
/// With Docker (SQL Server):
/// - ✅ Case-insensitive filtering works correctly (COLLATE SQL_Latin1_General_CP1_CI_AS)
/// - ✅ Constraints and triggers are enforced
/// - ✅ Behavior identical to production
///
/// Without Docker (InMemory fallback):
/// - ✅ Tests run without Docker dependency
/// - ✅ Case-insensitive filtering handled by ToUpper() in expression tree
/// </remarks>
public sealed class SqlServerDbContextFixture : IAsyncLifetime
{
    private MsSqlContainer? _msSqlContainer;
    private string? _connectionString;
    private DbContextOptions<ExampleDbContext>? _inMemoryOptions;
    private bool _useInMemory;

    public SqlServerDbContextFixture()
    {
        try
        {
            _msSqlContainer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPassword("YourStrong@Passw0rd")
                .WithCleanUp(true)
                .Build();
        }
        catch (ArgumentException)
        {
            // Docker is not available; will fall back to InMemory in InitializeAsync
            _msSqlContainer = null;
        }
    }

    /// <summary>
    /// Initializes the SQL Server container (or InMemory fallback) and creates the database schema.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_msSqlContainer != null)
        {
            try
            {
                await _msSqlContainer.StartAsync();
                _connectionString = _msSqlContainer.GetConnectionString();

                await using var context = CreateContext();
                _ = await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
                return;
            }
            catch
            {
                // Docker start failed; fall back to InMemory
                _msSqlContainer = null;
            }
        }

        _useInMemory = true;
        _inMemoryOptions = new DbContextOptionsBuilder<ExampleDbContext>()
            .UseInMemoryDatabase($"IntegrationTestDb_{Guid.NewGuid()}")
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        await using var inMemoryContext = new ExampleDbContext(_inMemoryOptions);
        _ = await inMemoryContext.Database.EnsureCreatedAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Stops and removes the SQL Server container (if used).
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_msSqlContainer != null)
        {
            await _msSqlContainer.DisposeAsync();
        }
    }

    /// <summary>
    /// Creates a new instance of ExampleDbContext connected to the SQL Server container or InMemory database.
    /// </summary>
    public ExampleDbContext CreateContext(Xunit.Abstractions.ITestOutputHelper? output = null)
    {
        if (_useInMemory)
        {
            if (_inMemoryOptions == null)
            {
                throw new InvalidOperationException(
                    "InMemory options are not available. Ensure InitializeAsync has been called.");
            }
            return new ExampleDbContext(_inMemoryOptions);
        }

        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException(
                "Connection string is not available. Ensure InitializeAsync has been called.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ExampleDbContext>()
            .UseSqlServer(_connectionString)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();

        if (output != null)
        {
            _ = optionsBuilder.LogTo(output.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
        }

        return new ExampleDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Gets the connection string for the SQL Server container.
    /// </summary>
    public string GetConnectionString()
    {
        if (_useInMemory)
        {
            return "InMemory";
        }

        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException(
                "Connection string is not available. Ensure InitializeAsync has been called.");
        }

        return _connectionString;
    }
}
