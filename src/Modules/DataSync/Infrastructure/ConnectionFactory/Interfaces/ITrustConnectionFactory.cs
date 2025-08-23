namespace DataSync.Infrastructure.ConnectionFactory.Interfaces;

public interface ITrustConnectionFactory
{
    Task<Npgsql.NpgsqlConnection> CreateOpenAsync(CancellationToken ct);
}