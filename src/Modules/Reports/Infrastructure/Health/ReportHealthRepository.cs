using Dapper;
using Reports.Domain.Health;
using Reports.Infrastructure.ConnectionFactory.Interfaces;

namespace Reports.Infrastructure.Health;

public class ReportHealthRepository(IReportsDbConnectionFactory dbConnectionFactory) : IReportHealthRepository
{
    public async Task<DateTime> GetDatabaseUtcNowAsync(CancellationToken cancellationToken)
    {
        const string sql = "select now() at time zone 'UTC'";
        using var connection = await dbConnectionFactory.CreateOpenAsync(cancellationToken);
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
        return await connection.ExecuteScalarAsync<DateTime>(command);
    }
}
