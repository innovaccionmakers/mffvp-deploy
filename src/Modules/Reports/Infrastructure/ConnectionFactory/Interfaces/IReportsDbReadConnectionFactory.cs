using Npgsql;

namespace Reports.Infrastructure.ConnectionFactory.Interfaces;

public interface IReportsDbReadConnectionFactory
{
    Task<NpgsqlConnection> CreateOpenAsync(CancellationToken cancellationToken);

}
