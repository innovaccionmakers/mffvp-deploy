using Operations.Domain.ClientOperations;
using Products.Domain.PensionFunds;
using System.Text.Json;

namespace MFFVP.BFF.Services.Reports.DepositsReport
{
    public class PaymentMethodProcessor
    {
        public (DepositsReportModel debitRecord, DepositsReportModel creditRecord)
            ProcessPaymentMethod(ClientOperation operation, IReadOnlyCollection<PensionFund> pensionFunds)
        {
            var paymentDetail = JsonSerializer.Deserialize<PaymentMethodDetail>(operation.AuxiliaryInformation.PaymentMethodDetail.RootElement.GetRawText());

            if (string.IsNullOrWhiteSpace(paymentDetail.TipoCuenta))
                throw new ArgumentException("El tipo de cuenta es requerido");

            if (!paymentDetail.TipoCuenta.Equals("Ahorros", StringComparison.OrdinalIgnoreCase) &&
                !paymentDetail.TipoCuenta.Equals("Corriente", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Tipo de cuenta no válido. Debe ser 'Ahorros' o 'Corriente'");

            if (string.IsNullOrWhiteSpace(paymentDetail.NumeroCuenta))
                throw new ArgumentException("El número de cuenta es requerido");

            var accountType = paymentDetail.TipoCuenta.Equals("Ahorros", StringComparison.OrdinalIgnoreCase) ? "S" : "D";
            var affiliateTransactionCode = accountType == "S" ? "606" : "7010";
            var fundsTransactionCode = "2085";

            // Crear registro de DÉBITO del afiliado
            var debitRecord = new DepositsReportModel
            {
                AccountType = accountType,
                AccountNumber = paymentDetail.NumeroCuenta,
                TransactionCode = affiliateTransactionCode,
                EffectiveDate = operation.ProcessDate,
                TransactionValue = operation.Amount,
                Nature = affiliateTransactionCode == "606" || affiliateTransactionCode == "7010" ? "D" : "C",
                Observations = $"{operation.OperationType.Name} {pensionFunds.First().Name}",
                TransactionName = $"{operation.OperationType.Name} {pensionFunds.First().Name}",
                AdditionalInfo = $"{operation.OperationType.Name} {pensionFunds.First().Name}",
                Branch = "252",
                Reference1 = string.Empty,
                Reference2 = string.Empty,
                Reference3 = string.Empty
            };

            // Crear registro de CRÉDITO del fondo
            var creditRecord = new DepositsReportModel
            {
                AccountType = "D", 
                AccountNumber = operation.AuxiliaryInformation.CollectionAccount.ToString(),
                TransactionCode = fundsTransactionCode, 
                EffectiveDate = operation.ProcessDate,
                TransactionValue = operation.Amount,
                Nature = fundsTransactionCode == "2085" ? "C" : "D",
                Observations = $"{operation.OperationType.Name} {pensionFunds.First().Name}",
                TransactionName = $"{operation.OperationType.Name} {pensionFunds.First().Name}",
                AdditionalInfo = $"{operation.OperationType.Name} {pensionFunds.First().Name}",
                Branch = "252",
                Reference1 = string.Empty,
                Reference2 = string.Empty,
                Reference3 = string.Empty,
            };

            return (debitRecord, creditRecord);
        }
    }

    public class PaymentMethodDetail
    {
        public string TipoCuenta { get; set; }
        public string NumeroCuenta { get; set; }
    }
}
