using Common.SharedKernel.Domain;

namespace Operations.Application.Abstractions.External;

public interface IPersonValidator
{
    Task<Result> ValidateAsync(string idType, string identification, CancellationToken ct);
}