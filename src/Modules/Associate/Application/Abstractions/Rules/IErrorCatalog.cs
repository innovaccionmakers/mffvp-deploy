namespace Associate.Application.Abstractions.Rules;

public interface IErrorCatalog
{
    Task<(int Code, string DefaultMessage)> GetAsync(
        string ruleKey,
        CancellationToken ct);
}