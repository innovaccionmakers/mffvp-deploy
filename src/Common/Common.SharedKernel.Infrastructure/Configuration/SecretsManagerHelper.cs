using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace Common.SharedKernel.Infrastructure.Configuration;

public static class SecretsManagerHelper
{
    public static async Task<string> GetSecretAsync(string secretName, string region)
    {
        using var client = new AmazonSecretsManagerClient(Amazon.RegionEndpoint.GetBySystemName(region));
        var response = await client.GetSecretValueAsync(new GetSecretValueRequest { SecretId = secretName });
        return response.SecretString;
    }
}
