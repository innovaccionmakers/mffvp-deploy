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
        services.Configure<S3Config>(configuration.GetSection("Aws:S3"));

        services.AddSingleton<IAmazonS3>(provider =>
        {
            var s3Config = configuration.GetSection("Aws:S3").Get<S3Config>();
            var region = Amazon.RegionEndpoint.GetBySystemName(s3Config?.Region ?? "us-east-1");
            return new AmazonS3Client(region);
        });

        services.AddScoped<IFileStorageService, S3FileStorageService>();

        return services;
    }

    public static IServiceCollection AddFileStorageService(this IServiceCollection services, S3Config s3Config)
    {
        services.Configure<S3Config>(options =>
        {
            options.BucketName = s3Config.BucketName;
            options.Region = s3Config.Region;
            options.BasePrefix = s3Config.BasePrefix;
            options.PublicUrlBase = s3Config.PublicUrlBase;
            options.DefaultExpirationHours = s3Config.DefaultExpirationHours;
            options.EnableCors = s3Config.EnableCors;
            options.MaxFileSizeMB = s3Config.MaxFileSizeMB;
        });

        services.AddSingleton<IAmazonS3>(provider =>
        {
            var region = Amazon.RegionEndpoint.GetBySystemName(s3Config.Region);
            return new AmazonS3Client(region);
        });

        services.AddScoped<IFileStorageService, S3FileStorageService>();

        return services;
    }
}
