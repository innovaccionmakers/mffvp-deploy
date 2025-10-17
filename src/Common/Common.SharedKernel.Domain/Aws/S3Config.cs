namespace Common.SharedKernel.Domain.Aws;

public class S3Config
{
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string BasePrefix { get; set; } = "uploads";
    public string? PublicUrlBase { get; set; }
    public int DefaultExpirationHours { get; set; } = 24;
    public bool EnableCors { get; set; } = true;
    public int MaxFileSizeMB { get; set; } = 100;
}
