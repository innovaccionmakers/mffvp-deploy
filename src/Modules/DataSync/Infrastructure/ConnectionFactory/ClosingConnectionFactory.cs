using Common.SharedKernel.Infrastructure.Configuration;
using DataSync.Infrastructure.ConnectionFactory.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DataSync.Infrastructure.ConnectionFactory;

public sealed class ClosingConnectionFactory(IConfiguration configuration) : IClosingConnectionFactory
{
    public async Task<Npgsql.NpgsqlConnection> CreateOpenAsync(CancellationToken ct)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var connectionString = configuration.GetConnectionString("ClosingDatabase");
        if (!string.Equals(env, "Development"))
        {
            var secretName = configuration["AWS:SecretsManager:SecretName"];
            var region = configuration["AWS:SecretsManager:Region"];
            connectionString = SecretsManagerHelper.GetSecretAsync(secretName, region).GetAwaiter().GetResult();
        }
        var conn = new Npgsql.NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);
        return conn;
    }
}