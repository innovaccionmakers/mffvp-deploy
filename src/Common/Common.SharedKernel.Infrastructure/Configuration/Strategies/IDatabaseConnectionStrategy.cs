using System.Data.Common;
using Microsoft.Extensions.Configuration;

namespace Common.SharedKernel.Infrastructure.Configuration.Strategies
{
    public interface IDatabaseConnectionStrategy
    {
        DbConnection CreateConnection(string connectionString);
        bool CanConnect(DbConnection connection);
    }
}
