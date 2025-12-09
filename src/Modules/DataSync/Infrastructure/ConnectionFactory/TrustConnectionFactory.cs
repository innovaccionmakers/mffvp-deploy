using Common.SharedKernel.Infrastructure.Configuration;
using DataSync.Infrastructure.ConnectionFactory.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DataSync.Infrastructure.ConnectionFactory;

public sealed class TrustConnectionFactory(IConfiguration configuration) : ITrustConnectionFactory
{
    public async Task<Npgsql.NpgsqlConnection> CreateOpenAsync(CancellationToken ct)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var connectionString = configuration.GetConnectionString("TrustsDatabase");
        if (!string.Equals(env, "Development"))
        {
            var secretName = configuration["AWS:SecretsManager:SecretName"];
            var region = configuration["AWS:SecretsManager:Region"];
            var commandTimeoutSeconds = configuration.GetValue<int?>("CustomSettings:DatabaseTimeouts:CommandTimeoutSeconds") ?? 30;
            connectionString = SecretsManagerHelper.GetSecretAsync(secretName, region, commandTimeoutSeconds).GetAwaiter().GetResult();
        }
        var conn = new Npgsql.NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);
        return conn;
    }
}
