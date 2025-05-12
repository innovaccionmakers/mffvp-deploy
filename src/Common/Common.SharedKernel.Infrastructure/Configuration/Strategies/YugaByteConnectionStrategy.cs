using System.Data.Common;
using Npgsql;

namespace Common.SharedKernel.Infrastructure.Configuration.Strategies;

internal class YugaByteConnectionStrategy : IDatabaseConnectionStrategy
{
    public bool CanConnect(DbConnection connection)
    {
        try
        {
            var originalConnectionString = connection.ConnectionString;

            connection.Open();
            connection.Close();

            // Verificar si la cadena cambió
            if (connection.ConnectionString != originalConnectionString)
                connection.ConnectionString = originalConnectionString;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public DbConnection CreateConnection(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        return new NpgsqlConnection(connectionString);
    }
}