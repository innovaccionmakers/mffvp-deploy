using System.Data.Common;

namespace Common.SharedKernel.Infrastructure.Configuration.Strategies;

public interface IDatabaseConnectionStrategy
{
    DbConnection CreateConnection(string connectionString);
    bool CanConnect(DbConnection connection);
}