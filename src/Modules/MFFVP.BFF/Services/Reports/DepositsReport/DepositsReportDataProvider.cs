using MFFVP.BFF.Services.Reports.DepositsReport.Interfaces;
using Operations.Domain.ClientOperations;
using Products.Domain.PensionFunds;

namespace MFFVP.BFF.Services.Reports.DepositsReport
{
    public class DepositsReportDataProvider(
        IClientOperationRepository _clientOperationRepository,
        IPensionFundRepository _pensionFundRepository,
        PaymentMethodProcessor _processor) : IDepositsReportDataProvider
    {
        public async IAsyncEnumerable<DepositsReportModel> GetDataAsync(
            DateTime processDate, CancellationToken cancellationToken)
        {
            const int bufferSize = 100;
            var buffer = new List<DepositsReportModel>(bufferSize);
            var pensionFunds = await _pensionFundRepository.GetAllAsync(cancellationToken);
            var operations = await _clientOperationRepository.GetClientOperationsByProcessDateAsync(processDate, cancellationToken);

            foreach (var operation in operations)
            {
                var (debitRecord, creditRecord) = _processor.ProcessPaymentMethod(operation, pensionFunds);
                buffer.Add(debitRecord);
                buffer.Add(creditRecord);

                if (buffer.Count >= bufferSize)
                {
                    foreach (var item in buffer)
                        yield return item;
                    
                    buffer.Clear();

                    await Task.Delay(10, cancellationToken);
                }
            }

            foreach (var item in buffer)
                yield return item;
            
        }
    }
}
