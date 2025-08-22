using Associate.Presentation.DTOs;
using MFFVP.BFF.Services.Reports.DepositsReport.Interfaces;
using Operations.Presentation.DTOs;
using Operations.Presentation.GraphQL;
using Products.Domain.PensionFunds;
using Products.Presentation.GraphQL;

namespace MFFVP.BFF.Services.Reports.DepositsReport
{
    public class DepositsReportDataProvider(
        IOperationsExperienceQueries _clientOperationRepository,
        IProductsExperienceQueries _pensionFundRepository,
        PaymentMethodProcessor _processor,
        ILogger<DepositsReportDataProvider> _logger) : IDepositsReportDataProvider
    {
        public async IAsyncEnumerable<DepositsReportModel> GetDataAsync(
            DateTime processDate, CancellationToken cancellationToken)
        {
            const int bufferSize = 100;
            var buffer = new List<DepositsReportModel>(bufferSize); 
            string pensionFunds = string.Empty;
            IReadOnlyCollection<ClientOperationsByProcessDateDto> operations = new List<ClientOperationsByProcessDateDto>();

            try
            {
                pensionFunds = await _pensionFundRepository.GetAllPensionFundsAsync(cancellationToken);
                operations = await _clientOperationRepository.GetClientOperationsByProcessDateAsync(processDate, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener datos para el reporte de depósitos. Fecha: {processDate}", processDate.ToString("yyyy-MM-dd"));
                throw;
            }

            foreach (var operation in operations)
            { 
                var BufferCount = buffer.Count;
                _logger.LogDebug($"Buffer lleno ({BufferCount} registros), liberando registros");

                var _operations = new ClientOperationsDto
                (
                    operation.Amount,
                    operation.CollectionAccount,
                    operation.PaymentMethodDetail,
                    operation.Name
                );

                var (debitRecord, creditRecord) = _processor.ProcessPaymentMethod(_operations, processDate, pensionFunds);
                buffer.Add(debitRecord);
                buffer.Add(creditRecord);

                if (BufferCount >= bufferSize)
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
