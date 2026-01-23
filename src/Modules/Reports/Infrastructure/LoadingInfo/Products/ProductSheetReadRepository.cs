using Dapper;
using Microsoft.Extensions.Options;
using Reports.Domain.LoadingInfo.Products;
using Reports.Infrastructure.Common;
using Reports.Infrastructure.Configuration;
using Reports.Infrastructure.ConnectionFactory.Interfaces;
using System.Runtime.CompilerServices;

namespace Reports.Infrastructure.LoadingInfo.Products;

public sealed class ProductSheetReadRepository : BaseReadRepository, IProductSheetReadRepository
{
    private readonly IReportsDbReadConnectionFactory reportsDbReadConnectionFactory;

    public ProductSheetReadRepository(IReportsDbReadConnectionFactory connectionFactory,
            IOptions<DatabaseTimeoutsOptions> timeoutOptions)
            : base(timeoutOptions)
    {
        reportsDbReadConnectionFactory = connectionFactory;
    }

    public async IAsyncEnumerable<ProductSheetReadRow> ReadProductsAsync(
        long? lastRowVersion,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Consulta incremental basada en row_version de administradores y fondos_voluntarios_pensiones
        const string sql = @"
          SELECT
            adm.id AS ""AdministratorId"",
            COALESCE((pc.metadata ->> 'tipo')::int, 0) AS ""EntityType"",
             adm.codigo_entidad AS ""EntityCode"",
             adm.codigo_entidad_sfc AS ""EntitySfcCode"",
             fvp.cod_negocio_sfc AS ""BusinessCodeSfcFund"",
             pf.fondo_id AS ""FundId""
         FROM productos.administradores adm
         JOIN productos.parametros_configuracion pc
          ON pc.id = adm.tipo_entidad
          AND pc.tipo = 'TipoEntidad'
         JOIN productos.fondos_voluntarios_pensiones fvp
          ON fvp.administrador_id = adm.id
         JOIN productos.planes_fondo pf
          ON pf.fondo_id = fvp.id
         WHERE 
          (@LastRowVersion IS NULL 
          OR adm.row_version > @LastRowVersion 
          OR fvp.row_version > @LastRowVersion);
          ";

        await using var connection =
               await reportsDbReadConnectionFactory.CreateOpenAsync(cancellationToken);

        var rows = connection.Query<ProductSheetReadRow>(
         sql,
        param: new { LastRowVersion = lastRowVersion },
          commandTimeout: CommandTimeoutSeconds,
            buffered: false);

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return row;
        }
    }
}
