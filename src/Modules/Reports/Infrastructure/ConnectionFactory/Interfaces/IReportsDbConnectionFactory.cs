using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Reports.Infrastructure.ConnectionFactory.Interfaces;

public interface IReportsDbConnectionFactory
{
    Task<NpgsqlConnection> CreateOpenAsync(CancellationToken ct);
}