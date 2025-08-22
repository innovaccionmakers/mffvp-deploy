using Associate.Presentation.DTOs;
using System.Text.Json;

namespace MFFVP.BFF.Services.Reports.DepositsReport
{
    public class PaymentMethodProcessor(ILogger<PaymentMethodProcessor> _logger)
    {
        public (DepositsReportModel debitRecord, DepositsReportModel creditRecord) ProcessPaymentMethod(ClientOperationsDto operation, DateTime processDate, string pensionFunds)
        {
            try
            {
                var paymentDetail = JsonSerializer.Deserialize<PaymentMethodDetail>(operation.PaymentMethodDetail.RootElement.GetRawText());

                if (!paymentDetail.TipoCuenta.Equals("Ahorros", StringComparison.OrdinalIgnoreCase) &&
                    !paymentDetail.TipoCuenta.Equals("Corriente", StringComparison.OrdinalIgnoreCase) &&
                    string.IsNullOrWhiteSpace(paymentDetail.TipoCuenta))
                {
                    _logger.LogError($"Tipo de cuenta no válido: {paymentDetail.TipoCuenta}. Debe ser 'Ahorros' o 'Corriente' para el numero de cuenta: {paymentDetail.NumeroCuenta}",
                        paymentDetail.TipoCuenta, paymentDetail.NumeroCuenta);
                    throw new ArgumentException("Tipo de cuenta no válido. Debe ser 'Ahorros' o 'Corriente'");
                }

                if (string.IsNullOrWhiteSpace(paymentDetail.NumeroCuenta))
                {
                    _logger.LogError($"El número de cuenta es requerido para la operación: {operation}", operation);
                    throw new ArgumentException("El número de cuenta es requerido");
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
                return (debitRecord, creditRecord);
            }
            catch ( Exception ex)
            {
                _logger.LogError(ex, $"Error al procesar método de pago para operación: {operation}.", operation);
                throw;
            }
            
        }
    }

    public class PaymentMethodDetail
    {
        public string TipoCuenta { get; set; }
        public string NumeroCuenta { get; set; }
    }
}
