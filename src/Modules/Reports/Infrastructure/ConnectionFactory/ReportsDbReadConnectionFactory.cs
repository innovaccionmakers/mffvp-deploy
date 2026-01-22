using Common.SharedKernel.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Reports.Infrastructure.ConnectionFactory.Interfaces;

namespace Reports.Infrastructure.ConnectionFactory;

public sealed class ReportsDbReadConnectionFactory: IReportsDbReadConnectionFactory
{
    private readonly IConfiguration configuration;
    private readonly Lazy<Task<string>> connectionStringLoader;

    public ReportsDbReadConnectionFactory(IConfiguration configuration)
    {
        this.configuration = configuration;

        connectionStringLoader = new Lazy<Task<string>>(
            valueFactory: LoadConnectionStringAsync,
            isThreadSafe: true);
    }

    public async Task<NpgsqlConnection> CreateOpenAsync(CancellationToken cancellationToken)
    {
        var connectionString = await connectionStringLoader.Value;

        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        return connection;
    }

    private async Task<string> LoadConnectionStringAsync()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var connectionString = configuration.GetConnectionString("ReportsReadOnlyDatabase");

        if (string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString!;
        }

        var secretName = configuration["AWS:SecretsManager:SecretNameReadOnly"];
        var region = configuration["AWS:SecretsManager:Region"];

        return await SecretsManagerHelper.GetSecretAsync(secretName, region);
    }
}
