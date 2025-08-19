namespace Security.Application.Abstractions.Services.Auditing;

public interface IPermissionDescriptionService
{
    string? GetDescriptionByPolicy(string policy);

    string? GetDescriptionByEndpoint(string endpointName, string httpMethod);
}