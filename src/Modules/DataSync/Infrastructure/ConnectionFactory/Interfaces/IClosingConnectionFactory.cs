namespace DataSync.Infrastructure.ConnectionFactory.Interfaces;

public interface IClosingConnectionFactory
{
    Task<Npgsql.NpgsqlConnection> CreateOpenAsync(CancellationToken ct);
}