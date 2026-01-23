using Accounting.Integrations.ConfigurationGenerals.GetConfigurationGeneral;
using Accounting.Integrations.ConfigurationGenerals.GetConfigurationGenerals;
using Accounting.Presentation.DTOs;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Results;
using MediatR;

namespace Accounting.Presentation.GraphQL
{
    public class ConfigurationGeneralsExperienceQueries(
        ISender mediator) : IConfigurationGeneralsExperienceQueries
    {
        public async Task<GraphqlResult<IReadOnlyCollection<ConfigurationGeneralDto>>> GetConfigurationGeneralsAsync(int? portfolioId, CancellationToken cancellationToken = default)
        {
            if (portfolioId != null)
                return await GetConfigurationGeneral(portfolioId.Value, cancellationToken);

            return await GetConfigurationGenerals(cancellationToken);
        }

        public async Task<GraphqlResult<IReadOnlyCollection<ConfigurationGeneralDto>>> GetConfigurationGeneral(int portfolioId, CancellationToken cancellationToken = default)
        {
            var result = new GraphqlResult<IReadOnlyCollection<ConfigurationGeneralDto>>();
            try
            {
                var listConfigurationGenerals = new List<ConfigurationGeneralDto>();
                var response = await mediator.Send(new GetConfigurationGeneralQuery(portfolioId), cancellationToken);

                if (!response.IsSuccess || response.Value == null)
                {
                    result.AddError(response.Error);
                    return result;
                }
                var configurationGeneral = response.Value;

                var configurationGenerals = new ConfigurationGeneralDto(
                    configurationGeneral.Id,
                    configurationGeneral.AccountingCode,
                    configurationGeneral.CostCenter
                );

                listConfigurationGenerals.Add(configurationGenerals);

                result.Data = listConfigurationGenerals;

                return result;
            }
            catch (Exception ex)
            {
                result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
                return result;
            }
        }


        public async Task<GraphqlResult<IReadOnlyCollection<ConfigurationGeneralDto>>> GetConfigurationGenerals(CancellationToken cancellationToken = default)
        {
            var result = new GraphqlResult<IReadOnlyCollection<ConfigurationGeneralDto>>();
            try
            {
                var response = await mediator.Send(new GetConfigurationGeneralsQuery(), cancellationToken);

                if (!response.IsSuccess || response.Value == null)
                {
                    result.AddError(response.Error);
                    return result;
                }

                var listConfigurationGenerals = response.Value.Select(x => new ConfigurationGeneralDto(
                    x.Id,
                    x.AccountingCode,
                    x.CostCenter
                )).ToList();

                result.Data = listConfigurationGenerals;

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
