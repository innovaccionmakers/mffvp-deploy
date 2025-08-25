using Associate.Presentation.DTOs;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Results;
using System.Text.Json;
using Error = Common.SharedKernel.Core.Primitives.Error;

namespace MFFVP.BFF.Services.Reports.DepositsReport
{
    public class PaymentMethodProcessor(ILogger<PaymentMethodProcessor> _logger)
    {
        public GraphqlResult<(DepositsReportModel debitRecord, DepositsReportModel creditRecord)> ProcessPaymentMethod(ClientOperationsDto operation, DateTime processDate, string pensionFunds)
        {
            var result = new GraphqlResult<(DepositsReportModel, DepositsReportModel)>();
            try
            {
                var paymentDetail = JsonSerializer.Deserialize<PaymentMethodDetail>(operation.PaymentMethodDetail.RootElement.GetRawText());

                if (string.IsNullOrWhiteSpace(paymentDetail.TipoCuenta))
                {
                    _logger.LogError($"Tipo de cuenta no debe estar vacia");

                    result.AddError(new Error("EXCEPTION", "El dato Tipo de cuenta no debe estar vacío", ErrorType.Failure));
                    return result;
                }

                if (!paymentDetail.TipoCuenta.Equals("Ahorros", StringComparison.OrdinalIgnoreCase) &&
                    !paymentDetail.TipoCuenta.Equals("Corriente", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError($"Tipo de cuenta no válido: {paymentDetail.TipoCuenta}. Debe ser 'Ahorros' o 'Corriente' para el numero de cuenta en el titular origen {paymentDetail.IdTitularOrigen}: {paymentDetail.NumeroCuenta}",
                        paymentDetail.TipoCuenta, paymentDetail.NumeroCuenta, paymentDetail.IdTitularOrigen);

                    result.AddError(new Error("EXCEPTION", $"Tipo de cuenta no válido: {paymentDetail.TipoCuenta}. Debe ser 'Ahorros' o 'Corriente' en el titular origen {paymentDetail.IdTitularOrigen}", ErrorType.Failure));
                    return result;
                }

                if (string.IsNullOrWhiteSpace(paymentDetail.NumeroCuenta))
                {
                    _logger.LogError($"El número de cuenta es requerido para la operación: {operation}", operation); 
                    result.AddError(new Error("EXCEPTION", "El número de cuenta es requerido", ErrorType.Failure));
                    return result;
                }

                var accountType = paymentDetail.TipoCuenta.Equals("Ahorros", StringComparison.OrdinalIgnoreCase) ? "S" : "D";
                var affiliateTransactionCode = accountType == "S" ? "606" : "7010";
                var fundsTransactionCode = "2085";

                // Crear registro de DÉBITO del afiliado
                var debitRecord = new DepositsReportModel
                {
                    AccountType = accountType,
                    AccountNumber = paymentDetail.NumeroCuenta,
                    TransactionCode = affiliateTransactionCode,
                    EffectiveDate = processDate,
                    TransactionValue = operation.Amount,
                    Nature = affiliateTransactionCode == "606" || affiliateTransactionCode == "7010" ? "D" : "C",
                    Observations = $"{operation.Name} {pensionFunds}",
                    TransactionName = $"{operation.Name} {pensionFunds}",
                    AdditionalInfo = $"{operation.Name} {pensionFunds}",
                    Branch = "252",
                    Reference1 = string.Empty,
                    Reference2 = string.Empty,
                    Reference3 = string.Empty
                };

                // Crear registro de CRÉDITO del fondo
                var creditRecord = new DepositsReportModel
                {
                    AccountType = "D",
                    AccountNumber = operation.CollectionAccount.ToString(),
                    TransactionCode = fundsTransactionCode,
                    EffectiveDate = processDate,
                    TransactionValue = operation.Amount,
                    Nature = fundsTransactionCode == "2085" ? "C" : "D",
                    Observations = $"{operation.Name} {pensionFunds}",
                    TransactionName = $"{operation.Name} {pensionFunds}",
                    AdditionalInfo = $"{operation.Name} {pensionFunds}",
                    Branch = "252",
                    Reference1 = string.Empty,
                    Reference2 = string.Empty,
                    Reference3 = string.Empty,
                };

                _logger.LogDebug($"Registros creados exitosamente para el numero de cuenta: {paymentDetail.NumeroCuenta}", paymentDetail.NumeroCuenta);
                result.SetSuccess((debitRecord, creditRecord));
                return result;
            }
            catch ( Exception ex)
            {
                _logger.LogError(ex, $"Error al procesar método de pago para operación: {operation}.", operation);
                result.AddError(new Error("EXCEPTION", $"Error al procesar método de pago para operación: {operation}.", ErrorType.Failure));
                return result;
            }
            
        }
    }

    public class PaymentMethodDetail
    {
        public string TipoCuenta { get; set; }
        public string NumeroCuenta { get; set; }
        public string IdTitularOrigen { get; set; }
    }
}
