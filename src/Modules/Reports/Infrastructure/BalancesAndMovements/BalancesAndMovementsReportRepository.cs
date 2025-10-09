using Common.SharedKernel.Core.Primitives;
using Dapper;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Reports.Application.Reports.DTOs;
using Reports.Domain.BalancesAndMovements;
using Reports.Infrastructure.ConnectionFactory.Interfaces;
using System.Data;

namespace Reports.Infrastructure.BalancesAndMovements
{
    internal class BalancesAndMovementsReportRepository(IReportsDbConnectionFactory dbConnectionFactory) : IBalancesAndMovementsReportRepository
    {
        private const int ActiveLifecycleStatus = (int)LifecycleStatus.Active;

        public async Task<IEnumerable<BalancesResponse>> GetBalancesAsync(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken)
        {
            try
            {
                using var connection = await dbConnectionFactory.CreateOpenAsync(cancellationToken);
                var (activateIds, responseTrustYiel) = await GetTrustYieldDataAsync(reportRequest, connection, cancellationToken);

                if (!activateIds.Any() || !responseTrustYiel.Any())
                    return Enumerable.Empty<BalancesResponse>();

                var operations = await GetOperationsBalancesAsync(activateIds, reportRequest.StartDate, reportRequest.EndDate, connection, cancellationToken);
                var portfolioIds = operations.Select(x => x.PortfolioId).Distinct();
                var persons = await GetPersonsInfoAsync(activateIds, connection, cancellationToken);
                var alternative = await GetAlternativeIdAsync(portfolioIds, connection, cancellationToken);
                var objectsId = responseTrustYiel.Select(x => x.ObjectsId).Distinct();
                var products = await GetProductsInfoAsync(objectsId, alternative, portfolioIds, connection, cancellationToken);
                connection.Close();

                return BuildBalancesResponse(reportRequest.StartDate, reportRequest.EndDate, responseTrustYiel, operations, persons, products);
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<BalancesResponse>();
            }
        }

        public async Task<IEnumerable<MovementsResponse>> GetMovementsAsync(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken)
        {
            try
            {
                using var connection = await dbConnectionFactory.CreateOpenAsync(cancellationToken);
                var operations = await GetOperationsMovementsAsync(reportRequest.StartDate, reportRequest.EndDate, connection, cancellationToken);
                var portfolioIds = operations.Select(x => x.PortfolioId).Distinct();
                var activateIds = operations.Select(x => x.ActiviteId).Distinct();
                var objectsId = operations.Select(x => x.ObjectId).Distinct();
                var persons = await GetPersonsInfoAsync(activateIds, connection, cancellationToken);
                var alternative = await GetAlternativeIdAsync(portfolioIds, connection, cancellationToken);
                var products = await GetProductsInfoAsync(objectsId, alternative, portfolioIds, connection, cancellationToken);
                connection.Close();

                return BuildMovementsResponse(operations, persons, products);
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<MovementsResponse>();
            }
        }

        #region Balances

        public async Task<(IEnumerable<int> activateIds, IEnumerable<TrustYieldRequest> responseTrustYiel)> GetTrustYieldDataAsync(BalancesAndMovementsReportRequest reportRequest, NpgsqlConnection connection, CancellationToken cancellationToken)
        {
            IEnumerable<int> activateIds = new List<int>();
            IEnumerable<TrustYieldRequest> responseTrustYiel = new List<TrustYieldRequest>();

            if (reportRequest.Identification.IsNullOrEmpty())
            {
                responseTrustYiel = await GetTrustYieldAsync(reportRequest.StartDate, reportRequest.EndDate, connection, cancellationToken, null);
                activateIds = responseTrustYiel.Select(x => x.ActivitesId);
            }
            else
            {
                activateIds = await GetActivateWhitIdentificationAsync(reportRequest.Identification, connection, cancellationToken);
                responseTrustYiel = await GetTrustYieldAsync(reportRequest.StartDate, reportRequest.EndDate, connection, cancellationToken, activateIds);
            }

            return (activateIds, responseTrustYiel);
        }

        private async Task<IEnumerable<TrustYieldRequest>> GetTrustYieldAsync(DateTime startDate, DateTime endDate, NpgsqlConnection connection, CancellationToken cancellationToken, IEnumerable<int>? activateId = null)
        {
            try
            {
                var previousDate = startDate.AddDays(-1);
                const string baseSql = @"SELECT 
                                            fd.portafolio_id AS PortfolioId,
                                            fd.afiliado_id AS ActivitesId,
                                            fd.objetivo_id AS ObjectsId,
                                            SUM(CASE WHEN rf.fecha_cierre BETWEEN @startDate AND @endDate THEN rf.rendimientos ELSE 0 END) AS Yields,
                                            SUM(CASE WHEN rf.fecha_cierre = @previousDate THEN rf.saldo_cierre ELSE 0 END) AS InitialBalance,
                                            SUM(CASE WHEN rf.fecha_cierre = @endDate THEN rf.saldo_cierre ELSE 0 END) AS closingBalance
                                        FROM cierre.rendimientos_fideicomisos rf
                                        INNER JOIN fideicomisos.fideicomisos fd ON fd.id = rf.fideicomiso_id ";

                string sql = baseSql;
                object parameters;
                var hasActivateIds = activateId != null && activateId.Any();

                if (hasActivateIds)
                {
                    sql += "WHERE afiliado_id = ANY(@activateId) ";
                    parameters = new
                    {
                        previousDate,
                        startDate,
                        endDate,
                        activateId = activateId.ToArray()
                    };
                }
                else
                {
                    parameters = new
                    {
                        previousDate,
                        startDate,
                        endDate
                    };
                }

                sql += "GROUP BY fd.portafolio_id, fd.afiliado_id, fd.objetivo_id;";
                var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
                return await connection.QueryAsync<TrustYieldRequest>(command);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task<IEnumerable<int>> GetActivateWhitIdentificationAsync(string identification, NpgsqlConnection connection, CancellationToken cancellationToken)
        {
            try
            {
                var sql = @"SELECT 
	                            id afiliado_id
                            FROM afiliados.activacion_afiliados
                            WHERE identificacion = @identification;";

                var command = new CommandDefinition(sql, new { identification }, cancellationToken: cancellationToken);

                return await connection.QueryAsync<int>(command);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task<IEnumerable<OperationBalancesRequest>> GetOperationsBalancesAsync(
            IEnumerable<int> activateIds,
            DateTime startDate,
            DateTime endDate,
            NpgsqlConnection connection,
            CancellationToken cancellationToken)
        {
            try
            {
                const string sql = @"SELECT
                                        OC.portafolio_id AS PortfolioId,
                                        OC.afiliado_id AS ActivitesId,
                                        OC.objetivo_id AS ObjectsId,
                                            SUM(OC.valor) AS Entry
                                    FROM operaciones.operaciones_clientes OC
                                    INNER JOIN operaciones.tipos_operaciones T ON OC.tipo_operaciones_id = t.id
                                    WHERE fecha_proceso::date BETWEEN @startDate AND @endDate AND (T.id = 1 OR T.categoria = 1) AND OC.afiliado_id = ANY(@activateIds) AND OC.estado = @activeStatus
                                    GROUP BY PortfolioId, ActivitesId, ObjectsId;";

                var command = new CommandDefinition(
                    sql,
                    new
                    {
                        activateIds = activateIds.ToArray(),
                        startDate,
                        endDate,
                        activeStatus = ActiveLifecycleStatus
                    },
                    cancellationToken: cancellationToken);
                return await connection.QueryAsync<OperationBalancesRequest>(command);

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private IEnumerable<BalancesResponse> BuildBalancesResponse(
            DateTime startDate,
            DateTime endDate,
            IEnumerable<TrustYieldRequest> trustYields,
            IEnumerable<OperationBalancesRequest> operations,
            IEnumerable<PersonsRequest> persons,
            IEnumerable<ProductsRequest> products)
        {
            try
            {
                var result = new List<BalancesResponse>();
                foreach (var trustYield in trustYields)
                {
                    var operation = operations.FirstOrDefault(op => op.PortfolioId == trustYield.PortfolioId && op.ObjectsId == trustYield.ObjectsId && op.ActivitesId == trustYield.ActivitesId);
                    if (operation == null) continue;
                    var product = products.FirstOrDefault(p => p.PortfolioId == trustYield.PortfolioId && p.PortfolioId == trustYield.PortfolioId);
                    if (product == null) continue;
                    var person = persons.FirstOrDefault(p => p.ActiviteId == trustYield.ActivitesId);
                    if (person == null) continue;

                    var initialBalance = trustYield.InitialBalance;
                    var entry = operation?.Entry ?? 0m;
                    var outflows = 0m;
                    var yields = trustYield.Yields;
                    var sourceWithholding = 0m;
                    var closingBalance = trustYield.ClosingBalance;

                    var response = new BalancesResponse(
                        StartDate: startDate.ToString("yyyy-MM-dd") ?? string.Empty,
                        EndDate: endDate.ToString("yyyy-MM-dd") ?? string.Empty,
                        IdentificationType: person.IdentificationType ?? string.Empty,
                        Identification: person.Identification ?? string.Empty,
                        FullName: person.FullName ?? string.Empty,
                        ObjectiveId: trustYield.ObjectsId,
                        Objective: product.Objective ?? string.Empty,
                        Fund: product.Fund ?? string.Empty,
                        Plan: product.Plan ?? string.Empty,
                        Alternative: product.Alternative ?? string.Empty,
                        Portfolio: product.Portfolio ?? string.Empty,
                        InitialBalance: initialBalance,
                        Entry: entry,
                        Outflows: outflows,
                        Returns: yields,
                        SourceWithholding: sourceWithholding,
                        ClosingBalance: closingBalance
                    );

                    result.Add(response);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion

        #region Movements

        private async Task<IEnumerable<OperationMovementsRequest>> GetOperationsMovementsAsync(DateTime startDate, DateTime endDate, NpgsqlConnection connection, CancellationToken cancellationToken)
        {
            try
            {
                const string sql = @"SELECT
                                        OC.portafolio_id AS PortfolioId,
                                        OC.afiliado_id AS ActiviteId,
                                        OC.objetivo_id AS ObjectId,
                                        OC.id AS Voucher,
                                        OC.fecha_proceso AS ProcessDate,
                                        CONCAT_WS(' - ', T.codigo_homologado, T.nombre) AS TransactionType,
                                        CONCAT_WS(' - ', ST.codigo_homologado, ST.nombre) AS TransactionSubtype,
	                                    OC.valor AS Value,
                                        PC_Tributaria.nombre AS TaxCondition,
                                        IA.retencion_contingente AS ContingentWithholding,
                                        CONCAT_WS(' - ', PC_FormaPago.codigo_homologacion, PC_FormaPago.nombre) AS PaymentMethod
                                    FROM operaciones.operaciones_clientes OC
                                    JOIN operaciones.tipos_operaciones T ON T.id = OC.tipo_operaciones_id
                                    LEFT JOIN operaciones.tipos_operaciones ST ON ST.id = T.categoria
                                    LEFT JOIN operaciones.informacion_auxiliar IA ON IA.operacion_cliente_id = OC.id
                                    LEFT JOIN operaciones.parametros_configuracion PC_Tributaria ON PC_Tributaria.id = IA.condicion_tributaria_id
                                    LEFT JOIN operaciones.parametros_configuracion PC_FormaPago ON PC_FormaPago.id = IA.forma_pago_id
                                    WHERE fecha_aplicacion::date BETWEEN @startDate AND @endDate AND (T.id = 1 OR T.categoria = 1) AND OC.estado = @activeStatus;";

                var command = new CommandDefinition(
                    sql,
                    new
                    {
                        startDate,
                        endDate,
                        activeStatus = ActiveLifecycleStatus
                    },
                    cancellationToken: cancellationToken);
                return await connection.QueryAsync<OperationMovementsRequest>(command);

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private IEnumerable<MovementsResponse> BuildMovementsResponse(
            IEnumerable<OperationMovementsRequest> operations,
            IEnumerable<PersonsRequest> persons,
            IEnumerable<ProductsRequest> products)
        {
            try
            {
                var result = new List<MovementsResponse>();

                foreach (var operation in operations)
                {
                    var product = products.FirstOrDefault(op => op.PortfolioId == operation.PortfolioId && op.ObjectiveId == operation.ObjectId);
                    if (product == null) continue;
                    var person = persons.FirstOrDefault(p => p.ActiviteId == operation?.ActiviteId);
                    if (person == null) continue;

                    var response = new MovementsResponse(
                        ProcesDate: operation.ProcessDate.ToString("yyyy-MM-dd") ?? string.Empty,
                        IdentificationType: person.IdentificationType ?? string.Empty,
                        Identification: person.Identification ?? string.Empty,
                        FullName: person.FullName ?? string.Empty,
                        ObjectiveId: product.ObjectiveId,
                        Objective: product.Objective ?? string.Empty,
                        Fund: product.Fund ?? string.Empty,
                        Plan: product.Plan ?? string.Empty,
                        Alternative: product.Alternative ?? string.Empty,
                        Portfolio: product.Portfolio ?? string.Empty,
                        Voucher: operation.Voucher,
                        TransactionType: operation.TransactionType ?? string.Empty,
                        TransactionSubtype: operation.TransactionSubtype ?? string.Empty,
                        Value: operation.Value,
                        TaxCondition: operation.TaxCondition ?? string.Empty,
                        ContingentWithholding: operation.ContingentWithholding,
                        PaymentMethod: operation.PaymentMethod ?? string.Empty
                    );

                    result.Add(response);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion

        #region Private methods

        private async Task<IEnumerable<PersonsRequest>> GetPersonsInfoAsync(IEnumerable<int> activateIds,
            NpgsqlConnection connection, CancellationToken cancellationToken)
        {
            try
            {
                const string sql = @"SELECT 
                                        AA.id AS ActiviteId,
                                        P.identificacion AS Identification,
                                        CONCAT_WS(' - ', PC.codigo_homologacion, PC.nombre) AS IdentificationType,
                                        P.nombre_completo AS FullName
                                    FROM personas.parametros_configuracion PC
                                    JOIN personas.personas P ON P.tipo_documento_uuid = PC.uuid
                                    JOIN afiliados.activacion_afiliados AA ON AA.identificacion = P.identificacion
                                    WHERE AA.id = ANY(@activateIds);";

                var command = new CommandDefinition(sql, new { activateIds = activateIds.ToArray() }, cancellationToken: cancellationToken);
                return await connection.QueryAsync<PersonsRequest>(command);

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private async Task<IEnumerable<AlternativeRequest>> GetAlternativeIdAsync(IEnumerable<int> portfolioIds, NpgsqlConnection connection, CancellationToken cancellationToken)
        {
            try
            {
                var sql = @"SELECT 
                                ""AlternativeId""
                            FROM productos.alternativas_portafolios
                            WHERE ""PortfolioId"" = ANY(@portfolioIds);";

                var command = new CommandDefinition(sql, new { portfolioIds = portfolioIds.ToArray() }, cancellationToken: cancellationToken);
                return await connection.QueryAsync<AlternativeRequest>(command);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private async Task<IEnumerable<ProductsRequest>> GetProductsInfoAsync(IEnumerable<int> objectiveIds, IEnumerable<AlternativeRequest> alternative, IEnumerable<int> portfolioIds, NpgsqlConnection connection, CancellationToken cancellationToken)
        {
            try
            {
                var alternativeIds = alternative.Select(x => x.AlternativeId).Distinct();

                const string sql = @"SELECT 
                                        P.id AS PortfolioId,
	                                    O.id AS ObjectiveId,
	                                    CONCAT_WS(' - ', O.tipo_objetivo_id, O.nombre) AS Objective,
	                                    CONCAT_WS(' - ', FVP.codigo_homologado, FVP.nombre) AS Fund,
	                                    CONCAT_WS(' - ', PL.codigo_homologado, PL.nombre) AS Plan,
                                        CONCAT_WS(' - ', A.codigo_homologado, A.nombre, PC.nombre) AS Alternative,
                                        CONCAT_WS(' - ', P.codigo_homologacion, P.nombre) AS Portfolio	
                                    FROM productos.portafolios P
                                    JOIN productos.alternativas_portafolios AP ON P.id = AP.""PortfolioId""
                                    JOIN productos.alternativas A ON AP.""AlternativeId"" = A.id
                                    JOIN productos.parametros_configuracion PC ON A.tipo_alternativa_id = PC.id
                                    JOIN productos.objetivos O ON O.alternativa_id = A.id
                                    JOIN productos.planes_fondo PF ON A.planes_fondo_id = PF.id
                                    JOIN productos.fondos_voluntarios_pensiones FVP ON PF.fondo_id = FVP.id
                                    JOIN productos.planes PL ON PF.plan_id = PL.id
                                    WHERE P.id = ANY(@portfolioIds) AND A.tipo_alternativa_id = ANY(@alternativeIds) AND O.id = ANY(@objectiveIds)
                                    GROUP BY PortfolioId, ObjectiveId, Objective, Fund, Plan, Alternative, Portfolio;";

                var parameters = new
                {
                    objectiveIds = objectiveIds.ToArray(),
                    portfolioIds = portfolioIds.ToArray(),
                    alternativeIds = alternativeIds.ToArray()
                };

                var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
                return await connection.QueryAsync<ProductsRequest>(command);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        #endregion
    }
}