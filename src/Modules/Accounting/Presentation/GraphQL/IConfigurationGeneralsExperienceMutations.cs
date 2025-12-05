using Accounting.Presentation.ConfigurationGenerals.CreateConfigurationGeneral;
using Accounting.Presentation.ConfigurationGenerals.DeleteConfigurationGeneral;
using Accounting.Presentation.ConfigurationGenerals.UpdateConfigurationGeneral;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;

namespace Accounting.Presentation.GraphQL
{
    public interface IConfigurationGeneralsExperienceMutations
    {
        Task<GraphqlResult> CreateConfiguracionGeneralAsync(CreateConfigurationGeneralInput input, IValidator<CreateConfigurationGeneralInput> validator, CancellationToken cancellationToken = default);
        Task<GraphqlResult> UpdateConfiguracionGeneralAsync(UpdateConfigurationGeneralInput input, IValidator<UpdateConfigurationGeneralInput> validator, CancellationToken cancellationToken = default);
        Task<GraphqlResult> DeleteConfiguracionGeneralAsync(DeleteConfigurationGeneralInput input, IValidator<DeleteConfigurationGeneralInput> validator, CancellationToken cancellationToken = default);
    }
}
