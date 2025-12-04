using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

using System.Text.Json;

namespace Common.SharedKernel.Infrastructure.Configuration;

public static class SecretsManagerHelper
{
    public static async Task<string> GetSecretAsync(string secretName, string region, 
        int? commandTimeoutSeconds = null)
    {
        using var client = new AmazonSecretsManagerClient(Amazon.RegionEndpoint.GetBySystemName(region));
        var response = await client.GetSecretValueAsync(new GetSecretValueRequest { SecretId = secretName });

        if (string.IsNullOrWhiteSpace(response.SecretString))
            throw new InvalidOperationException("Secret string is empty.");

        var secretData = JsonSerializer.Deserialize<DbSecret>(response.SecretString);

        if (secretData is null)
            throw new InvalidOperationException("Failed to deserialize secret.");

        var normalizedCommandTimeoutSeconds = NormalizeCommandTimeoutSeconds(commandTimeoutSeconds);

        var connectionString = $"Host={secretData.host};Port={secretData.port};Database=dbfvp;Username={secretData.username};Password={secretData.password};SSL Mode=Require;Trust Server Certificate=true;Command Timeout={normalizedCommandTimeoutSeconds};";

        return connectionString;
    }

    private class DbSecret
    {
        public string dbClusterIdentifier { get; set; }
        public string engine { get; set; }
        public string host { get; set; }
        public string password { get; set; }
        public string port { get; set; }
        public string resourceId { get; set; }
        public string username { get; set; }
    }

    private static int NormalizeCommandTimeoutSeconds(int? configuredSeconds)
    {
        const int defaultSeconds = 30;

        if (!configuredSeconds.HasValue)
        {
            return defaultSeconds;
        }

        return configuredSeconds.Value is >= 1 and <= 3600
            ? configuredSeconds.Value
            : defaultSeconds;
    }
}
