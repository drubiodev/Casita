using System.Reflection;
using Dapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Casita.Infrastructure.Persistence;

public class DatabaseInitializer : IHostedService
{
    private const string ScriptsResourcePrefix = "Casita.Infrastructure.SqlScripts.";

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        IDbConnectionFactory connectionFactory,
        ILogger<DatabaseInitializer> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var assembly = typeof(DatabaseInitializer).Assembly;
        var scriptNames = assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith(ScriptsResourcePrefix, StringComparison.Ordinal)
                        && n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        if (scriptNames.Count == 0)
        {
            _logger.LogWarning("No embedded SQL scripts found to initialize the database.");
            return;
        }

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        foreach (var name in scriptNames)
        {
            var sql = await ReadResourceAsync(assembly, name, cancellationToken);
            _logger.LogInformation("Applying SQL script {Script}", name);
            await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task<string> ReadResourceAsync(Assembly assembly, string name, CancellationToken cancellationToken)
    {
        await using var stream = assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Embedded SQL script '{name}' not found.");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
