namespace Reports.Domain.Health;

public interface IReportHealthRepository
{
    Task<DateTime> GetDatabaseUtcNowAsync(CancellationToken cancellationToken);
}