using Common.SharedKernel.Domain;
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
        public async Task<IEnumerable<BalancesResponse>> GetBalancesAsync(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken)
        {
            try
            {
                using var connection = await dbConnectionFactory.CreateOpenAsync(cancellationToken);
                var (activateIds, responseTrustYiel) = await GetTrustYieldDataAsync(reportRequest, connection, cancellationToken);

                if (!activateIds.Any() || !responseTrustYiel.Any())
                    return Enumerable.Empty<BalancesResponse>();

                var operations = await GetOperationsBalancesAsync(activateIds, connection, cancellationToken);
                var portfolioIds = operations.Select(x => x.PortfolioId).Distinct();
                var persons = await GetPersonsInfoAsync(activateIds, connection, cancellationToken);
                var alternative = await GetAlternativeIdAsync(portfolioIds, connection, cancellationToken);
                var products = await GetProductsInfoAsync(responseTrustYiel, alternative, portfolioIds, connection, cancellationToken);
                var closingData = await GetClosingDataAsync(reportRequest.StartDate, responseTrustYiel, connection, cancellationToken);
                connection.Close();

                return BuildBalancesResponse(reportRequest.StartDate, reportRequest.EndDate, responseTrustYiel, operations, persons, products, closingData);
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
                var (activateIds, responseTrustYiel) = await GetTrustYieldDataAsync(reportRequest, connection, cancellationToken);

                if (!activateIds.Any() || !responseTrustYiel.Any())
                    return Enumerable.Empty<MovementsResponse>();

                var operations = await GetOperationsMovementsAsync(reportRequest.StartDate, reportRequest.EndDate, connection, cancellationToken);
                var portfolioIds = operations.Select(x => x.PortfolioId).Distinct();
                var persons = await GetPersonsInfoAsync(activateIds, connection, cancellationToken);
                var alternative = await GetAlternativeIdAsync(portfolioIds, connection, cancellationToken);
                var products = await GetProductsInfoAsync(responseTrustYiel, alternative, portfolioIds, connection, cancellationToken);
                connection.Close();

                return BuildMovementsResponse(operations, persons, products);
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<MovementsResponse>();
            }
        }

        #region Balances

        private async Task<IEnumerable<OperationBalancesRequest>> GetOperationsBalancesAsync(IEnumerable<int> activateIds, NpgsqlConnection connection, CancellationToken cancellationToken)
        {
            try
            {
                const string sql = @"SELECT 
                                        portafolio_id AS PortfolioId,
                                        SUM(valor) AS Entry
                                    FROM operaciones.operaciones_clientes
                                    WHERE afiliado_id = ANY(@activateIds) AND tipo_operaciones_id = 1
                                    GROUP BY PortfolioId;";

                var command = new CommandDefinition(sql, new { activateIds = activateIds.ToArray() }, cancellationToken: cancellationToken);
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
            IEnumerable<ProductsRequest> products,
            IEnumerable<CloseRequest> closingData)
        {
            try
            {
                var result = new List<BalancesResponse>();

                var person = persons.FirstOrDefault();
                if (person == null)
                    return result;

                foreach (var product in products)
                {
                    var operation = operations.FirstOrDefault(op => op.PortfolioId == product.PortfolioId);
                    var productTrustYields = trustYields.Where(ty => ty.ObjectsId == product.ObjectiveId);

                    if (!productTrustYields.Any())
                        continue;

                    var productClosingData = closingData.Where(cd =>
                        productTrustYields.Any(ty => ty.TrustYieldId == cd.TrustYieldId));

                    var initialBalance = productClosingData.Sum(cd => cd.InitialBalance);
                    var entry = operation?.Entry ?? 0m;
                    var outflows = 0m;
                    var yields = productClosingData.Sum(cd => cd.Yields);
                    var sourceWithholding = 0m;
                    var closingBalance = initialBalance + entry - outflows + yields - sourceWithholding;

                    var response = new BalancesResponse(
                        StartDate: startDate.ToString("yyyy-MM-dd") ?? string.Empty,
                        EndDate: endDate.ToString("yyyy-MM-dd") ?? string.Empty,
                        IdentificationType: person.IdentificationType ?? string.Empty,
                        Identification: person.Identification ?? string.Empty,
                        FullName: person.FullName ?? string.Empty,
                        ObjectiveId: product.ObjectiveId,
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
                                    WHERE tipo_operaciones_id = 1 AND fecha_aplicacion::date BETWEEN @startDate AND @endDate;";

                var command = new CommandDefinition(sql, new { startDate, endDate }, cancellationToken: cancellationToken);
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
                var person = persons.FirstOrDefault();
                if (person == null)
                    return result;

                foreach (var product in products)
                {
                    var operation = operations.FirstOrDefault(op => op.PortfolioId == product.PortfolioId);

                    if (operation == null)
                        continue;

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
                        PaymentMethod:  operation.PaymentMethod ?? string.Empty
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

        public async Task<(IEnumerable<int> activateIds, IEnumerable<TrustYieldRequest> responseTrustYiel)>GetTrustYieldDataAsync(BalancesAndMovementsReportRequest reportRequest, NpgsqlConnection connection, CancellationToken cancellationToken)
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

        private async Task<IEnumerable<TrustYieldRequest>> GetTrustYieldAsync(DateTime startDate, DateTime endDate, NpgsqlConnection connection, CancellationToken cancellationToken, IEnumerable<int>? activateId = null)
        {
            try
            {
                const string baseSql = @"SELECT 
                                            id AS TrustYieldId, 
                                            afiliado_id AS ActivitesId,
                                            objetivo_id AS ObjectsId
                                        FROM fideicomisos.fideicomisos
                                        WHERE fecha_creacion::date BETWEEN @startDate AND @endDate";

                string sql = baseSql;
                object parameters;

                // Validar si la lista tiene elementos (considerando null y vacía)
                var hasActivateIds = activateId != null && activateId.Any();

                if (hasActivateIds)
                {
                    // Usar ANY para arrays en PostgreSQL
                    sql += " AND afiliado_id = ANY(@activateId);";
                    parameters = new
                    {
                        startDate = startDate,
                        endDate = endDate,
                        activateId = activateId.ToArray()
                    };
                }
                else
                {
                    parameters = new
                    {
                        startDate = startDate,
                        endDate = endDate
                    };
                }

                var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
                return await connection.QueryAsync<TrustYieldRequest>(command);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task<IEnumerable<PersonsRequest>> GetPersonsInfoAsync(IEnumerable<int> activateIds,
            NpgsqlConnection connection, CancellationToken cancellationToken)
        {
            try
            {
                const string sql = @"SELECT 
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

        private async Task<IEnumerable<CloseRequest>> GetClosingDataAsync(DateTime startDate,
            IEnumerable<TrustYieldRequest> trustYields, NpgsqlConnection connection, CancellationToken cancellationToken)
        {
            try
            {
                var previousDate = startDate.AddDays(-1);
                var fideicomisoIds = trustYields.Select(x => x.TrustYieldId).Distinct();

                const string sql = @"SELECT 
                                        fideicomiso_id AS TrustYieldId,
                                        SUM(saldo_cierre) AS InitialBalance,
                                        rendimientos AS Yields
                                    FROM cierre.rendimientos_fideicomisos
                                    WHERE fecha_cierre::date = @previousDate AND fideicomiso_id = ANY(@fideicomisoIds)
                                    GROUP BY fideicomiso_id, rendimientos;";

                var parameters = new
                {
                    previousDate = previousDate,
                    fideicomisoIds = fideicomisoIds.ToArray()
                };

                var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
                return await connection.QueryAsync<CloseRequest>(command);

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
        private async Task<IEnumerable<ProductsRequest>> GetProductsInfoAsync(IEnumerable<TrustYieldRequest> trustYields, IEnumerable<AlternativeRequest> alternative, IEnumerable<int> portfolioIds, NpgsqlConnection connection, CancellationToken cancellationToken)
        {
            try
            {
                var objectiveIds = trustYields.Select(x => x.ObjectsId).Distinct();
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