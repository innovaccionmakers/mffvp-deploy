namespace Trusts.Application.Abstractions.Rules;

public interface IErrorCatalog
{
    Task<(string Code, string Message)> GetAsync(
        Guid ruleUuid,
        CancellationToken cancellationToken = default
    );
}