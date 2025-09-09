using System;
using System.Threading;
using System.Threading.Tasks;
using Common.SharedKernel.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Reports.Infrastructure.ConnectionFactory.Interfaces;

namespace Reports.Infrastructure.ConnectionFactory;

public sealed class ReportsDbConnectionFactory(IConfiguration configuration) : IReportsDbConnectionFactory
{
    public async Task<NpgsqlConnection> CreateOpenAsync(CancellationToken ct)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var connectionString = configuration.GetConnectionString("ReportsDatabase");
        if (!string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase))
        {
            var secretName = configuration["AWS:SecretsManager:SecretName"];
            var region = configuration["AWS:SecretsManager:Region"];
            connectionString = SecretsManagerHelper.GetSecretAsync(secretName, region).GetAwaiter().GetResult();
        }

        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(ct);
        return connection;
    }
}