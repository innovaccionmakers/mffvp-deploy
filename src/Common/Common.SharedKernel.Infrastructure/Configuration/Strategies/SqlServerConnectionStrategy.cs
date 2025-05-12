using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace Common.SharedKernel.Infrastructure.Configuration.Strategies;

internal class SqlServerConnectionStrategy : IDatabaseConnectionStrategy
{
    public bool CanConnect(DbConnection connection)
    {
        try
        {
            connection.Open();
            connection.Close();
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

        return new SqlConnection(connectionString);
    }
}