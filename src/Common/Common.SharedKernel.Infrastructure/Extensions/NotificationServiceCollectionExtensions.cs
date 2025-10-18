using Amazon.SQS;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Domain.Aws;
using Common.SharedKernel.Infrastructure.NotificationsCenter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.SharedKernel.Infrastructure.Extensions;


public static class NotificationServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationCenter(this IServiceCollection services, IConfiguration configuration)
    {       
        services.Configure<SqsConfig>(configuration.GetSection("AWS:Sqs"));

        services.AddSingleton<IAmazonSQS>(provider =>
        {
            var sqsConfig = configuration.GetSection("AWS:Sqs").Get<SqsConfig>();
            var region = Amazon.RegionEndpoint.GetBySystemName(sqsConfig?.Region ?? "us-east-1");
            return new AmazonSQSClient(region);
        });

        services.AddScoped<INotificationCenter, NotificationCenter>();

        return services;
    }
}
