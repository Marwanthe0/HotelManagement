using HotelManagement.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Tests;

/// <summary>
/// Creates an isolated in-memory SQLite database per test using the real HotelDbContext,
/// so the tests exercise the actual EF Core queries used by the repositories.
/// </summary>
public sealed class TestDbContext : IDisposable
{
    private readonly SqliteConnection _connection;

    public HotelDbContext Context { get; }

    public TestDbContext()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new HotelDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
