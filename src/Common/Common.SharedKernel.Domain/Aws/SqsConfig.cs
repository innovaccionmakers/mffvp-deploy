namespace Common.SharedKernel.Domain.Aws;

public class SqsConfig
{
    public string QueueUrl { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public int VisibilityTimeoutSeconds { get; set; } = 30;
    public int WaitTimeSeconds { get; set; } = 20;
    public int MaxNumberOfMessages { get; set; } = 10;
}
