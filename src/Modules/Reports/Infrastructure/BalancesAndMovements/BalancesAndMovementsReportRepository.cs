using Dapper;
using Reports.Domain.BalancesAndMovements;
using Reports.Infrastructure.ConnectionFactory.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Reports.Infrastructure.BalancesAndMovements
{
    internal class BalancesAndMovementsReportRepository(IReportsDbConnectionFactory dbConnectionFactory) : IBalancesAndMovementsReportRepository
    {
        public async Task<BalancesResponse> GetBalancesAsync(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken)
        {
            using var connection = await dbConnectionFactory.CreateOpenAsync(cancellationToken);
            
            //await GetTrustYieldAsync(reportRequest.startDate, reportRequest.endDate , reportRequest.Identification, cancellationToken);
            //await GetOperationAsync(reportRequest.activateId, cancellationToken);
            //await GetProductsAsync(reportRequest.objectiveId, reportRequest.portfolioId, reportRequest.alternativeId, cancellationToken);
            //await GetCloseAsync(reportRequest.startDate, reportRequest.activateId, cancellationToken);
            //await GetPersonsAsync(reportRequest.Identification, cancellationToken);

            throw new NotImplementedException();
        }

        public Task<MovementsResponse> GetMovementsAsync(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetActivateAsync(string identification, List<int> activateId, CancellationToken cancellationToken)
        {
            var sql = string.Empty;

            if (identification == string.Empty)
            {
                sql = $@"SELECT 
	                        identificacion
                        FROM afiliados.activacion_afiliados
                        WHERE id IN ({activateId});";
            }
            else
            {
                sql = $@"SELECT 
	                        id afiliado_id
                        FROM afiliados.activacion_afiliados
                        WHERE identificacion = {identification};";
            }

            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
            throw new NotImplementedException();
        }

        public async Task<(List<int>, List<int>)> GetTrustYieldAsync(DateOnly startDate, DateOnly endDate, string identification, CancellationToken cancellationToken)
        {
            var sql = string.Empty;

            if (identification == string.Empty)
            {
                sql = $@"SELECT 
	                        afiliado_id,
	                        objetivo_id
                        FROM fideicomisos.fideicomisos
                        WHERE fecha_creacion::date BETWEEN {startDate} AND {endDate};";
            }
            else
            {
                sql = $@"SELECT 
	                        afiliado_id,
	                        objetivo_id
                        FROM fideicomisos.fideicomisos
                        WHERE fecha_creacion::date BETWEEN {startDate} AND {endDate} 
                        AND afiliado_id = {identification};";
            }


            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
            throw new NotImplementedException();
        }

        public async Task GetPersonsAsync(string identification, CancellationToken cancellationToken)
        {
            var sql = $@"SELECT
	                        P.identificacion,
	                        CONCAT_WS(' - ', PC.codigo_homologacion, PC.nombre) Tipoidentificacion,
	                        P.nombre_completo
                        FROM personas.parametros_configuracion PC
                        JOIN personas.personas P ON P.tipo_documento_uuid = PC.uuid
                        WHERE P.identificacion = 'afiliados.identificacion';";

            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
            throw new NotImplementedException();
        }

        public async Task GetOperationAsync(int activateId, CancellationToken cancellationToken)
        {
            var sql = $@"SELECT 
	                        portafolio_id,
	                        valor Entrada
                        FROM operaciones.operaciones_clientes
                        WHERE afiliado_id IN ('fideicomisos.afiliado_id') AND tipo_operaciones_id = 1";

            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
            throw new NotImplementedException();
        }

        public async Task GetProductsAsync(int objectiveId, int portfolioId, int alternativeId, CancellationToken cancellationToken)
        {
            var sql = $@"SELECT
	                        CONCAT_WS(' - ', tipo_objetivo_id, nombre) Objetivo
                        FROM productos.objetivos
                        WHERE id = 'fideicomisos.objetivo_id';

                        SELECT
	                        CONCAT_WS(' - ', codigo_homologado, nombre) Fondo
                        FROM productos.fondos_voluntarios_pensiones;

                        SELECT	
	                        CONCAT_WS(' - ', codigo_homologado, nombre) Plan
                        FROM productos.planes;

                        SELECT 
	                        ""AlternativeId""
                        FROM productos.alternativas_portafolios
                        WHERE ""PortfolioId"" = 'operaciones_clientes.portafolio_id';

                        SELECT 
	                        CONCAT_WS(' - ', A.codigo_homologado, PC.nombre, A.nombre) Alternativa
                        FROM productos.alternativas A
                        JOIN productos.parametros_configuracion PC ON A.codigo_homologado = PC.codigo_homologacion
                        WHERE tipo_alternativa_id = 'alternativa_portafolios.AlternativeId';

                        SELECT 
	                        CONCAT_WS(' - ', p.codigo_homologacion, P.nombre) Portafolio
                        FROM productos.portafolios P
                        JOIN productos.parametros_configuracion PC ON P.codigo_homologacion = PC.codigo_homologacion
                        WHERE id = 'operaciones_clientes.portafolio_id';";

            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
            throw new NotImplementedException();
        }

        public async Task GetCloseAsync(DateOnly startDate, int activateId, CancellationToken cancellationToken)
        {
            var previousDate = startDate.AddDays(-1);
            var sql = $@"SELECT 
	                        saldo_cierre SaldoInicial,
	                        rendimientos
                        FROM cierre.rendimientos_fideicomisos
                        WHERE = {activateId};";

            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
            throw new NotImplementedException();
        }
    }
}
