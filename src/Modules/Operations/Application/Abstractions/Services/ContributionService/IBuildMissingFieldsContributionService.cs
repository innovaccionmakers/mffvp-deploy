namespace Operations.Application.Abstractions.Services.ContributionService;

public interface IBuildMissingFieldsContributionService
{
    Task<(DateTime ExecuteDate, string Channel, string SalesUser)> BuildAsync(
        string portfolioId,
        CancellationToken cancellationToken = default);
}