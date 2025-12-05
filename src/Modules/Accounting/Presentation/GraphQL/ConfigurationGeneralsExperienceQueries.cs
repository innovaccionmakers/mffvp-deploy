using Accounting.Integrations.ConfigurationGenerals.GetConfigurationGeneral;
using Accounting.Presentation.DTOs;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Results;
using MediatR;

namespace Accounting.Presentation.GraphQL
{
    public class ConfigurationGeneralsExperienceQueries(
        ISender mediator) : IConfigurationGeneralsExperienceQueries
    {
        public async Task<GraphqlResult<ConfigurationGeneralDto>> GetConfigurationGeneralsAsync(int portfolioId, CancellationToken cancellationToken = default)
        {
            var result = new GraphqlResult<ConfigurationGeneralDto>();
            try
            {
                var response = await mediator.Send(new GetConfigurationGeneralQuery(portfolioId), cancellationToken);

                if (!response.IsSuccess || response.Value == null)
                {
                    result.AddError(response.Error);
                    return result;
                }

                var configurationGeneral = response.Value;

                var passiveTransactions = new ConfigurationGeneralDto(
                    configurationGeneral.Id,
                    configurationGeneral.AccountingCode,
                    configurationGeneral.CostCenter
                );

                result.Data = passiveTransactions;

                return result;
            }
            catch (Exception ex)
            {
                result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
                return result;
            }
        }
    }
}
