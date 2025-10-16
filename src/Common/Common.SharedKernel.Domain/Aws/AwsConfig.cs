namespace Common.SharedKernel.Domain.Aws;

public class AwsConfig
{
    public string Environment { get; set; }
    public string SecretManager { get; set; }
    public string Redis { get; set; }
    public string Region { get; set; }
    public string AttachedS3Bucket { get; set; }
}
