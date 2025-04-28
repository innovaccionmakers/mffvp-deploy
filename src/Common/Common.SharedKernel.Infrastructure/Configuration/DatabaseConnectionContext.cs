using System.Data.Common;
using Common.SharedKernel.Infrastructure.Configuration.Strategies;
using Microsoft.Extensions.Configuration;

namespace Common.SharedKernel.Infrastructure.Configuration
{
    public class DatabaseConnectionContext
    {
        private readonly IEnumerable<IDatabaseConnectionStrategy> _strategies;

        public DatabaseConnectionContext(IEnumerable<IDatabaseConnectionStrategy> strategies) => _strategies = strategies;        

        public DbConnection GetConnection(string sqlServerConnectionString, string yugaByteConnectionString)
        {
            var orderedStrategies = _strategies.FirstOrDefault() is YugaByteConnectionStrategy ? _strategies : 
                                    _strategies.OrderByDescending(s => s is YugaByteConnectionStrategy);

            foreach (var strategy in orderedStrategies)
            {
                var connection = strategy switch
                {
                    YugaByteConnectionStrategy => strategy.CreateConnection(yugaByteConnectionString),
                    SqlServerConnectionStrategy => strategy.CreateConnection(sqlServerConnectionString),
                    _ => throw new NotSupportedException($"Estrategia no soportada: {strategy.GetType().Name}")
                };

                if (strategy.CanConnect(connection))
                    return connection;

                connection.Dispose();
            }

            throw new Exception("No se pudo establecer conexión con ninguna base de datos disponible.");
        }
    }
}