using Microsoft.EntityFrameworkCore;
using Nuuvify.CommonPack.UnitOfWork.Examples;
using Testcontainers.MsSql;
using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.Integration.xTest.Fixtures;

/// <summary>
/// Fixture for integration tests using SQL Server in a Docker container via Testcontainers.
/// Provides a real SQL Server database with proper collation, constraints, and case-insensitive filtering support.
/// </summary>
/// <remarks>
/// This fixture uses Testcontainers to spin up a SQL Server container for testing.
/// Advantages over In-Memory:
/// - ✅ Case-insensitive filtering works correctly (COLLATE SQL_Latin1_General_CP1_CI_AS)
/// - ✅ Constraints and triggers are enforced
/// - ✅ Behavior identical to production
/// - ✅ Supports all SQL Server features
/// 
/// Requirements:
/// - Docker must be installed and running
/// - Internet connection for first-time SQL Server image download (~1.5 GB)
/// 
/// Performance:
/// - First run: ~30-60 seconds (container startup + image download if needed)
/// - Subsequent runs: ~10-15 seconds (container startup only)
/// </remarks>
public sealed class SqlServerDbContextFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer;
    private string? _connectionString;

    public SqlServerDbContextFixture()
    {
        // Configure SQL Server container with specific settings for testing
        _msSqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("YourStrong@Passw0rd") // Strong password required by SQL Server
            .WithCleanUp(true) // Automatically remove container after tests
            .Build();
    }

    /// <summary>
    /// Initializes the SQL Server container and creates the database schema.
    /// Called once before all tests in the class.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Start the SQL Server container
        await _msSqlContainer.StartAsync();

        // Get connection string from the running container
        _connectionString = _msSqlContainer.GetConnectionString();

        // Create database schema
        await using var context = CreateContext();
        _ = await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Stops and removes the SQL Server container.
    /// Called once after all tests in the class complete.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
    }

    /// <summary>
    /// Creates a new instance of ExampleDbContext connected to the SQL Server container.
    /// Each call creates a new context instance, but all share the same database.
    /// </summary>
    /// <param name="output">Optional xUnit output helper for SQL logging. If provided, EF Core will log SQL queries to test output.</param>
    /// <returns>A new ExampleDbContext instance.</returns>
    public ExampleDbContext CreateContext(Xunit.Abstractions.ITestOutputHelper? output = null)
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException(
                "Connection string is not available. Ensure InitializeAsync has been called.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ExampleDbContext>()
            .UseSqlServer(_connectionString)
            .EnableSensitiveDataLogging() // For better test diagnostics
            .EnableDetailedErrors();

        if (output != null)
        {
            // Log SQL queries to xUnit test output
            _ = optionsBuilder.LogTo(output.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
        }

        return new ExampleDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Gets the connection string for the SQL Server container.
    /// Useful for advanced scenarios or manual database inspection.
    /// </summary>
    public string GetConnectionString()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException(
                "Connection string is not available. Ensure InitializeAsync has been called.");
        }

        return _connectionString;
    }
}
