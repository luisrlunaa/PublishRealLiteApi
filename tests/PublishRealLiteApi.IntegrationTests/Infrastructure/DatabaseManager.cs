using Microsoft.Data.SqlClient;
using Respawn;

namespace PublishRealLiteApi.IntegrationTests.Infrastructure;

public class DatabaseManager
{
    private readonly string _connectionString;
    private Respawner _respawner = null!;

    private DatabaseManager(string connectionString)
    {
        _connectionString = connectionString;
    }

    public static async Task<DatabaseManager> CreateAsync(string connectionString)
    {
        var manager = new DatabaseManager(connectionString);
        await manager.InitializeAsync();
        return manager;
    }

    private async Task InitializeAsync()
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            // Preserve migrations history and role definitions between resets
            TablesToIgnore =
            [
                new("__EFMigrationsHistory"),
                new("AspNetRoles"),
                new("AspNetRoleClaims")
            ]
        });
    }

    public async Task ResetAsync()
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await _respawner.ResetAsync(conn);
    }
}
