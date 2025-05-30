using Common.SharedKernel.Domain;

namespace Products.Application.Abstractions.Services.External;

public interface IDocumentTypeValidator
{
    Task<Result> EnsureExistsAsync(string typeCode, CancellationToken ct);
}