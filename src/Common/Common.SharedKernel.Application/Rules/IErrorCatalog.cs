namespace Common.SharedKernel.Application.Rules;

public interface IErrorCatalog<TModule>
{
    Task<(string Code, string Message)> GetAsync(
        Guid ruleUuid,
        CancellationToken cancellationToken = default
    );
}
