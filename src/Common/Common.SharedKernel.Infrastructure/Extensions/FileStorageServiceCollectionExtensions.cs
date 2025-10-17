using Amazon.S3;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Domain.Aws;
using Common.SharedKernel.Infrastructure.FileStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.SharedKernel.Infrastructure.Extensions;

public static class FileStorageServiceCollectionExtensions
{
    public static IServiceCollection AddFileStorageService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAmazonS3>(provider =>
        {
            var s3Config = configuration.GetSection("AWS:S3").Get<S3Config>();
            var region = Amazon.RegionEndpoint.GetBySystemName(s3Config?.Region ?? "us-east-1");
            return new AmazonS3Client(region);
        });

        services.AddScoped<IFileStorageService, S3FileStorageService>();

        return services;
    }
}
