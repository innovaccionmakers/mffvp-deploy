using Azure;
using Common.SharedKernel.Domain;
using Dapper;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Reports.Application.Reports.DTOs;
using Reports.Domain.BalancesAndMovements;
using Reports.Infrastructure.ConnectionFactory.Interfaces;

namespace Reports.Infrastructure.BalancesAndMovements
{
    internal class BalancesAndMovementsReportRepository(IReportsDbConnectionFactory dbConnectionFactory) : IBalancesAndMovementsReportRepository
    {
        public async Task<IEnumerable<Result<BalancesResponse>>> GetBalancesAsync(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken)
        {
            try
            {
                using var connection = await dbConnectionFactory.CreateOpenAsync(cancellationToken);
                IEnumerable<int> activateIds = new List<int>();
                IEnumerable<TrustYieldRequest> responseTrustYiel = new List<TrustYieldRequest>();
                if (reportRequest.Identification.IsNullOrEmpty())
                {
                    responseTrustYiel = await GetTrustYieldAsync(reportRequest.startDate, reportRequest.endDate, connection, cancellationToken, null);
                    activateIds = responseTrustYiel.Select(x => x.ActivitesId);
                }
                else
                {
                    activateIds = await GetActivateWhitIdentificationAsync(reportRequest.Identification, connection, cancellationToken);
                    responseTrustYiel = await GetTrustYieldAsync(reportRequest.startDate, reportRequest.endDate, connection, cancellationToken, activateIds);
                }

                if (!activateIds.Any())
                    return (IEnumerable<Result<BalancesResponse>>)Task.FromResult(Enumerable.Empty<Result<BalancesResponse>>());

                if (!responseTrustYiel.Any())
                    return (IEnumerable<Result<BalancesResponse>>)Task.FromResult(Enumerable.Empty<Result<BalancesResponse>>());

                var operations = await GetOperationsAsync(activateIds, connection, cancellationToken);
                var persons = await GetPersonsInfoAsync(activateIds, connection, cancellationToken);
                var alternative = await GetAlternativeIdAsync(operations, connection, cancellationToken);
                var products = await GetProductsInfoAsync(responseTrustYiel, alternative, operations, connection, cancellationToken);
                var closingData = await GetClosingDataAsync(reportRequest.startDate, responseTrustYiel, connection, cancellationToken);
                connection.Close();

                var response = BuildBalancesResponse(reportRequest.startDate, reportRequest.endDate, responseTrustYiel, operations, persons, products, closingData);
                return response.Select(response => Result<BalancesResponse>.Success(response));
            }
            catch (Exception ex)
            {
                return (IEnumerable<Result<BalancesResponse>>)Task.FromResult(Enumerable.Empty<Result<BalancesResponse>>());
            }
        }

        public Task<IEnumerable<Result<MovementsResponse>>> GetMovementsAsync(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken)
        {
            return Task.FromResult(Enumerable.Empty<Result<MovementsResponse>>());
        }

        public async Task<IEnumerable<int>> GetActivateWhitIdentificationAsync(string identification, Npgsql.NpgsqlConnection connection, CancellationToken cancellationToken)
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

        public async Task<IEnumerable<TrustYieldRequest>> GetTrustYieldAsync(DateOnly startDate, DateOnly endDate, Npgsql.NpgsqlConnection connection, CancellationToken cancellationToken, IEnumerable<int>? activateId = null)
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

                // Convertir DateOnly a DateTime para compatibilidad con Dapper
                var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
                var endDateTime = endDate.ToDateTime(TimeOnly.MinValue);

                // Validar si la lista tiene elementos (considerando null y vacía)
                var hasActivateIds = activateId != null && activateId.Any();

                if (hasActivateIds)
                {
                    // Usar ANY para arrays en PostgreSQL
                    sql += " AND afiliado_id = ANY(@activateId);";
                    parameters = new
                    {
                        startDate = startDateTime,
                        endDate = endDateTime,
                        activateId = activateId.ToArray()
                    };
                }
                else
                {
                    parameters = new
                    {
                        startDate = startDateTime,
                        endDate = endDateTime
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

        private async Task<IEnumerable<OperationRequest>> GetOperationsAsync(IEnumerable<int> activateIds,
            NpgsqlConnection connection, CancellationToken cancellationToken)
        {
            try
            {
                const string sql = @"
                SELECT 
                    portafolio_id AS PortfolioId,
                    SUM(valor) AS Entry
                FROM operaciones.operaciones_clientes
                WHERE afiliado_id = ANY(@activateIds) 
                AND tipo_operaciones_id = 1
                GROUP BY portafolio_id;";

                var command = new CommandDefinition(sql, new { activateIds = activateIds.ToArray() }, cancellationToken: cancellationToken);
                return await connection.QueryAsync<OperationRequest>(command);

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private async Task<IEnumerable<PersonsRequest>> GetPersonsInfoAsync(IEnumerable<int> activateIds,
            NpgsqlConnection connection, CancellationToken cancellationToken)
        {
            try
            {
                const string sql = @"
                SELECT 
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

        public async Task<IEnumerable<AlternativeRequest>> GetAlternativeIdAsync(IEnumerable<OperationRequest> operations, Npgsql.NpgsqlConnection connection, CancellationToken cancellationToken)
        {
            try
            {
                var portfolioIds = operations.Select(x => x.PortfolioId).Distinct();

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

        private async Task<IEnumerable<ProductsRequest>> GetProductsInfoAsync(IEnumerable<TrustYieldRequest> trustYields, IEnumerable<AlternativeRequest> alternative, IEnumerable<OperationRequest> operations, NpgsqlConnection connection, CancellationToken cancellationToken)
        {
            try
            {
                var objectiveIds = trustYields.Select(x => x.ObjectsId).Distinct();
                var portfolioIds = operations.Select(x => x.PortfolioId).Distinct();
                var alternativeIds = alternative.Select(x => x.AlternativeId).Distinct();

                const string sql = @"SELECT 
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
                                    GROUP BY Objective, Fund, Plan, Alternative, Portfolio;";

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

        private async Task<IEnumerable<CloseRequest>> GetClosingDataAsync(DateOnly startDate,
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
                    previousDate = previousDate.ToDateTime(TimeOnly.MinValue),
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

        private IEnumerable<BalancesResponse> BuildBalancesResponse(
            DateOnly startDate,
            DateOnly endDate,
            IEnumerable<TrustYieldRequest> trustYields,
            IEnumerable<OperationRequest> operations,
            IEnumerable<PersonsRequest> persons,
            IEnumerable<ProductsRequest> products,
            IEnumerable<CloseRequest> closingData)
        {
            try
            {
                var result = new List<BalancesResponse>();

                // Obtener la persona (asumiendo que solo hay una por request)
                var person = persons.FirstOrDefault();
                if (person == null)
                    return result;

                // Crear un response por cada combinación de portafolio/objetivo
                foreach (var product in products)
                {
                    // Encontrar la operación correspondiente a este portafolio
                    var portfolioId = ExtractPortfolioId(product.Portfolio);
                    var operation = operations.FirstOrDefault(op => op.PortfolioId == portfolioId);

                    // Encontrar los trust yields y closing data correspondientes
                    var productTrustYields = trustYields.Where(ty =>
                        ty.ObjectsId.ToString() == ExtractObjectiveId(product.Objective));

                    var productClosingData = closingData.Where(cd =>
                        productTrustYields.Any(ty => ty.TrustYieldId == cd.TrustYieldId));

                    // Calcular balances
                    var initialBalance = productClosingData.Sum(cd => cd.InitialBalance);
                    var entry = operation?.Entry ?? 0m;
                    var outflows = 0m;
                    var yields = productClosingData.Sum(cd => cd.Yields);
                    var sourceWithholding = 0m;
                    var closingBalance = initialBalance + entry - outflows + yields - sourceWithholding;

                    // Extraer TargetID del objetivo (primer parte antes del '-')
                    var targetId = ExtractObjectiveId(product.Objective);

                    var response = new BalancesResponse(
                        StartDate: startDate.ToString("yyyy-MM-dd"),
                        EndDate: endDate.ToString("yyyy-MM-dd"),
                        IdentificationType: person.IdentificationType,
                        Identification: person.Identification,
                        FullName: person.FullName,
                        TargetID: targetId,
                        Target: product.Objective,
                        Fund: product.Fund,
                        Plan: product.Plan,
                        Alternative: product.Alternative,
                        Portfolio: product.Portfolio,
                        InitialBalance: initialBalance.ToString("N2"),
                        Entry: entry.ToString("N2"),
                        Outflows: outflows.ToString("N2"),
                        Returns: yields.ToString("N2"),
                        SourceWithholding: sourceWithholding.ToString("N2"),
                        ClosingBalance: closingBalance.ToString("N2")
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

        //private IEnumerable<MovementsResponse> BuildMovementsResponse(
        //    DateOnly startDate,
        //    DateOnly endDate,
        //    IEnumerable<TrustYieldRequest> trustYields,
        //    IEnumerable<OperationRequest> operations,
        //    IEnumerable<PersonsRequest> persons,
        //    IEnumerable<ProductsRequest> products,
        //    IEnumerable<CloseRequest> closingData)
        //{
        //    try
        //    {
        //        var result = new List<MovementsResponse>();

        //        // Obtener la persona (asumiendo que solo hay una por request)
        //        var person = persons.FirstOrDefault();
        //        if (person == null)
        //            return result;

        //        // Crear un response por cada combinación de portafolio/objetivo
        //        foreach (var product in products)
        //        {
        //            // Encontrar la operación correspondiente a este portafolio
        //            var portfolioId = ExtractPortfolioId(product.Portfolio);
        //            var operation = operations.FirstOrDefault(op => op.PortfolioId == portfolioId);

        //            // Encontrar los trust yields y closing data correspondientes
        //            var productTrustYields = trustYields.Where(ty =>
        //                ty.ObjectsId.ToString() == ExtractObjectiveId(product.Objective));

        //            var productClosingData = closingData.Where(cd =>
        //                productTrustYields.Any(ty => ty.TrustYieldId == cd.TrustYieldId));

        //            // Calcular balances
        //            var initialBalance = productClosingData.Sum(cd => cd.InitialBalance);
        //            var entry = operation?.Entry ?? 0m;
        //            var outflows = 0m;
        //            var yields = productClosingData.Sum(cd => cd.Yields);
        //            var sourceWithholding = 0m;
        //            var closingBalance = initialBalance + entry - outflows + yields - sourceWithholding;

        //            // Extraer TargetID del objetivo (primer parte antes del '-')
        //            var targetId = ExtractObjectiveId(product.Objective);

        //            var response = new MovementsResponse(
        //            );

        //            result.Add(response);
        //        }

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        private int ExtractPortfolioId(string portfolioString)
        {
            if (string.IsNullOrEmpty(portfolioString)) return 0;
            var parts = portfolioString.Split('-');
            return parts.Length > 0 && int.TryParse(parts[0].Trim(), out int id) ? id : 0;
        }

        private string ExtractObjectiveId(string objectiveString)
        {
            if (string.IsNullOrEmpty(objectiveString)) return string.Empty;
            var parts = objectiveString.Split('-');
            return parts.Length > 0 ? parts[0].Trim() : string.Empty;
        }
    }
}