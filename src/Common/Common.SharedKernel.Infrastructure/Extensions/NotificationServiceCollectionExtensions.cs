using Amazon.SQS;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Domain.Aws;
using Common.SharedKernel.Infrastructure.NotificationCenter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.SharedKernel.Infrastructure.Extensions;


public static class NotificationServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationCenter(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SqsConfig>(configuration.GetSection("Aws:Sqs"));

        services.AddSingleton<IAmazonSQS>(provider =>
        {
            var sqsConfig = configuration.GetSection("Aws:Sqs").Get<SqsConfig>();
            var region = Amazon.RegionEndpoint.GetBySystemName(sqsConfig?.Region ?? "us-east-1");
            return new AmazonSQSClient(region);
        });

        services.AddScoped<INotificationCenter, NotificationCenter>();

        return services;
    }

    public static IServiceCollection AddNotificationCenter(this IServiceCollection services, SqsConfig sqsConfig)
    {
        services.Configure<SqsConfig>(options =>
        {
            options.QueueUrl = sqsConfig.QueueUrl;
            options.Region = sqsConfig.Region;
            options.VisibilityTimeoutSeconds = sqsConfig.VisibilityTimeoutSeconds;
            options.WaitTimeSeconds = sqsConfig.WaitTimeSeconds;
            options.MaxNumberOfMessages = sqsConfig.MaxNumberOfMessages;
        });

        services.AddSingleton<IAmazonSQS>(provider =>
        {
            var region = Amazon.RegionEndpoint.GetBySystemName(sqsConfig.Region);
            return new AmazonSQSClient(region);
        });

        services.AddScoped<INotificationCenter, NotificationCenter>();

        return services;
    }
}
