using Associate.Presentation.DTOs;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Results;
using MFFVP.BFF.Services.Reports.DepositsReport.Interfaces;
using Operations.Presentation.DTOs;
using Operations.Presentation.GraphQL;
using Products.Presentation.GraphQL;
using Error = Common.SharedKernel.Core.Primitives.Error;

namespace MFFVP.BFF.Services.Reports.DepositsReport
{
    public class DepositsReportDataProvider(
        IOperationsExperienceQueries _clientOperationRepository,
        IProductsExperienceQueries _pensionFundRepository,
        PaymentMethodProcessor _processor,
        ILogger<DepositsReportDataProvider> _logger) : IDepositsReportDataProvider
    {
        public async IAsyncEnumerable<GraphqlResult<DepositsReportModel>> GetDataAsync(
            DateTime processDate, CancellationToken cancellationToken)
        {
            const int bufferSize = 100;
            var buffer = new List<GraphqlResult<DepositsReportModel>>(bufferSize); 
            string pensionFunds = string.Empty;
            IReadOnlyCollection<ClientOperationsByProcessDateDto> operations = new List<ClientOperationsByProcessDateDto>();
            var errorResult = new GraphqlResult<DepositsReportModel>();

            try
            {
                pensionFunds = await _pensionFundRepository.GetAllPensionFundsAsync(cancellationToken);
                operations = await _clientOperationRepository.GetClientOperationsByProcessDateAsync(processDate, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener datos para el reporte de depósitos. Fecha: {processDate}");
                errorResult.AddError(new Error("EXCEPTION", $"Error al obtener datos para el reporte de depósitos. Fecha: {processDate}", ErrorType.Failure));                
            }

            if (errorResult.Errors.Count > 0)
                yield return errorResult;

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
                var result = _processor.ProcessPaymentMethod(_operations, processDate, pensionFunds); 
                
                if (!result.Success)
                {
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError($"Error: {error.Code} - {error.Description}");
                        errorResult.AddError(new Error($"{error.Code}", $"{error.Description}", ErrorType.Failure));
                        yield return errorResult;
                    }
                    yield break;
                }

                var (debitRecord, creditRecord) = result.Data;
                buffer.Add(new GraphqlResult<DepositsReportModel> { Data = debitRecord });
                buffer.Add(new GraphqlResult<DepositsReportModel> { Data = creditRecord });

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
