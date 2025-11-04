using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using Reports.Domain.Deposits;
using Reports.Infrastructure.ConnectionFactory.Interfaces;
using System.Text.Json;

namespace Reports.Infrastructure.Deposits
{
    internal class DepositsRepository(
        IReportsDbConnectionFactory dbConnectionFactory,
        ILogger<DepositsRepository> _logger) : IDepositsRepository
    {
        public async Task<IEnumerable<DepositsResponse>> GetDepositsAsync(DepositsRequest reportRequest, CancellationToken cancellationToken)
        {
            try
            {
                var processDate = reportRequest.ProcessDate;
                using var connection = await dbConnectionFactory.CreateOpenAsync(cancellationToken);
                var pensionFunds = await GetAllPensionFundsAsync(connection, cancellationToken);
                var clientOperations = await GetClientOperationsByProcessDateAsync(processDate, connection, cancellationToken);
                connection.Close();

                return BuildDepositsResponse(processDate, pensionFunds, clientOperations);
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<DepositsResponse>();
            }
        }

        public async Task<string> GetAllPensionFundsAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
        {
                var sql = @"SELECT 
	                            nombre Name
                            FROM productos.fondos_voluntarios_pensiones;";

                var command = new CommandDefinition(sql, null, cancellationToken: cancellationToken);

                return await connection.QueryFirstOrDefaultAsync<string>(command);
        }

        public async Task<IEnumerable<ClientOperationsByProcessDateDto>> GetClientOperationsByProcessDateAsync(DateTime processDate, NpgsqlConnection connection, CancellationToken cancellationToken)
        {
            var utcProcessDate = processDate.Kind == DateTimeKind.Unspecified
                                ? DateTime.SpecifyKind(processDate, DateTimeKind.Utc)
                                : processDate.ToUniversalTime();

                const string sql = @"SELECT 
	                                    OC.valor AS Amount,
	                                    IA.cuenta_recaudo AS CollectionAccount,
	                                    IA.detalle_forma_pago AS PaymentMethodDetail,
	                                    T.nombre AS Name
                                    FROM operaciones.operaciones_clientes OC
                                    JOIN operaciones.informacion_auxiliar IA ON IA.operacion_cliente_id = OC.id
                                    JOIN operaciones.tipos_operaciones T ON T.id = OC.tipo_operaciones_id
                                    WHERE OC.fecha_proceso = @utcProcessDate AND OC.estado = 1;";

                var command = new CommandDefinition(sql, new { utcProcessDate }, cancellationToken: cancellationToken);

                return await connection.QueryAsync<ClientOperationsByProcessDateDto>(command);
        }

        private IEnumerable<DepositsResponse> BuildDepositsResponse(DateTime processDate, string pensionFunds, IEnumerable<ClientOperationsByProcessDateDto> clientOperations)
        {
                var result = new List<DepositsResponse>();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                foreach (var operation in clientOperations)
                {
                    var paymentDetail = JsonSerializer.Deserialize<PaymentMethodDetail>(operation.PaymentMethodDetail, options);

                    if (string.IsNullOrWhiteSpace(paymentDetail.TipoCuenta))
                    {
                        _logger.LogError($"Tipo de cuenta no debe estar vacia");

                        //result.AddError(new Error("EXCEPTION", "El dato Tipo de cuenta no debe estar vacío", ErrorType.Failure));
                        return result;
                    }

                    if (!paymentDetail.TipoCuenta.Equals("Ahorros", StringComparison.OrdinalIgnoreCase) &&
                        !paymentDetail.TipoCuenta.Equals("Corriente", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogError($"Tipo de cuenta no válido: {paymentDetail.TipoCuenta}. Debe ser 'Ahorros' o 'Corriente' para el numero de cuenta en el titular origen {paymentDetail.IdTitularOrigen}: {paymentDetail.NumeroCuenta}");

                        //result.AddError(new Error("EXCEPTION", $"Tipo de cuenta no válido: {paymentDetail.TipoCuenta}. Debe ser 'Ahorros' o 'Corriente' en el titular origen {paymentDetail.IdTitularOrigen}", ErrorType.Failure));
                        return result;
                    }

                    if (string.IsNullOrWhiteSpace(paymentDetail.NumeroCuenta))
                    {
                        _logger.LogError($"El número de cuenta es requerido para la operación: {operation}");
                        //result.AddError(new Error("EXCEPTION", "El número de cuenta es requerido", ErrorType.Failure));
                        return result;
                    }

                    var accountType = paymentDetail.TipoCuenta.Equals("Ahorros", StringComparison.OrdinalIgnoreCase) ? "S" : "D";
                    var affiliateTransactionCode = accountType == "S" ? "606" : "7010";
                    var fundsTransactionCode = "2085";

                    // Crear registro de DÉBITO del afiliado
                    var debitRecord = new DepositsResponse
                    (
                        accountType,
                        paymentDetail.NumeroCuenta,
                        affiliateTransactionCode,
                        processDate,
                        operation.Amount,
                        string.Empty,
                        affiliateTransactionCode == "606" || affiliateTransactionCode == "7010" ? "D" : "C",
                        $"{operation.Name} {pensionFunds}",
                        $"{operation.Name} {pensionFunds}",
                        $"{operation.Name} {pensionFunds}",
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        252
                    );
                    result.Add(debitRecord);

                    // Crear registro de CRÉDITO del fondo
                    var creditRecord = new DepositsResponse
                    (
                        "D",
                        operation.CollectionAccount.ToString(),
                        fundsTransactionCode,
                        processDate,
                        operation.Amount,
                        string.Empty,
                        fundsTransactionCode == "2085" ? "C" : "D",
                        $"{operation.Name} {pensionFunds}",
                        $"{operation.Name} {pensionFunds}",
                        $"{operation.Name} {pensionFunds}",
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        252
                    );
                    result.Add(creditRecord);

                    _logger.LogDebug($"Registros creados exitosamente para el numero de cuenta: {paymentDetail.NumeroCuenta}");                    
                }
                
                return result;
        }
        
        public class ClientOperationsByProcessDateDto
        {
            public decimal Amount { get; set; }
            public string CollectionAccount { get; set; }
            public string PaymentMethodDetail { get; set; }
            public string Name { get; set; }
        }

        public class PaymentMethodDetail
        {
            public string TipoCuenta { get; set; }
            public string NumeroCuenta { get; set; }
            public string IdTitularOrigen { get; set; }
        }
    }
}
