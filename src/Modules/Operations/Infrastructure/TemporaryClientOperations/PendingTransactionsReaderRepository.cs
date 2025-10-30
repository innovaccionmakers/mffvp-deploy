
using Microsoft.EntityFrameworkCore;
using Operations.Domain.TemporaryClientOperations;
using Operations.Infrastructure.Database;

namespace Operations.Infrastructure.TemporaryClientOperations;

internal sealed class PendingTransactionsReaderRepository(OperationsDbContext context) : IPendingTransactionsReaderRepository
{
    public async Task<IReadOnlyList<PendingContributionRow>> TakePendingBatchWithAuxAsync(
     int portfolioId, int batchSize, CancellationToken cancellationToken)
    {
        const string tag = "PendingTransactionsReaderRepository_TakePendingBatchWithAux_Select";

        FormattableString sql = $@"
                WITH cte AS (
                    SELECT o.id
                    FROM   operaciones.operaciones_clientes_temporal o
                    WHERE  o.portafolio_id = {portfolioId}
                       AND o.procesado = false
                    ORDER BY o.id
                    FOR UPDATE SKIP LOCKED
                    LIMIT {batchSize}
                )
                SELECT
                    o.id                                    AS ""TemporaryClientOperationId"",
                    o.fecha_radicacion                      AS ""RegistrationDate"",
                    o.afiliado_id                           AS ""AffiliateId"",
                    o.objetivo_id                           AS ""ObjectiveId"",
                    o.portafolio_id                         AS ""PortfolioId"",
                    o.valor                                 AS ""Amount"",
                    o.fecha_proceso                         AS ""ProcessDate"",
                    o.tipo_operaciones_id                   AS ""OperationTypeId"",
                    o.fecha_aplicacion                      AS ""ApplicationDate"",
                    o.procesado                             AS ""Processed"",
                    o.fideicomiso_id                        AS ""TrustId"",
                    o.operaciones_cliente_id                AS ""LinkedClientOperationId"",
                    o.estado                                AS ""Status"",
                    o.unidades                              AS ""Units"",
                    o.causal_id                             AS ""CauseId"",
                    a.id                                    AS ""TemporaryAuxiliaryInformationId"",
                    a.origen_id                             AS ""OriginId"",
                    a.metodo_recaudo_id                     AS ""CollectionMethodId"",
                    a.forma_pago_id                         AS ""PaymentMethodId"",
                    a.cuenta_recaudo                        AS ""CollectionAccount"",
                    a.detalle_forma_pago                    AS ""PaymentMethodDetail"",
                    a.estado_certificacion_id               AS ""CertificationStatusId"",
                    a.condicion_tributaria_id               AS ""TaxConditionId"",
                    a.retencion_contingente                 AS ""ContingentWithholding"",
                    a.medio_verificable                     AS ""VerifiableMedium"",
                    a.banco_recaudo                         AS ""CollectionBankId"",
                    a.fecha_consignacion                    AS ""DepositDate"",
                    a.usuario_comercial                     AS ""SalesUser"",
                    a.modalidad_origen_id                   AS ""OriginModalityId"",
                    a.ciudad_id                             AS ""CityId"",
                    a.canal_id                              AS ""ChannelId"",
                    a.usuario_id                            AS ""UserId""
                FROM operaciones.operaciones_clientes_temporal o
                JOIN cte ON cte.id = o.id
                JOIN operaciones.informacion_auxiliar_temporal a
                  ON a.operacion_cliente_temporal_id = o.id
                ORDER BY o.id;";

        return await context.Database
            .SqlQuery<PendingContributionRow>(sql)
            .TagWith(tag)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
