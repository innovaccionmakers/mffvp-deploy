namespace Operations.Domain.Services;

public interface IBuildMissingFieldsContributionService
{
    Task<(DateTime ExecuteDate, string Channel, string SalesUser)> BuildAsync(
        string portfolioId,
        CancellationToken cancellationToken = default);
}