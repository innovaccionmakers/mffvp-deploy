using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.ConfigurationParameters;
using Operations.Integrations.ConfigurationParameters;

namespace Operations.Application.ConfigurationParameters;

public class GetPaymentMethodsQueryHandler(
    IConfigurationParameterRepository repository,
) : IQueryHandler<GetPaymentMethodsQuery, IReadOnlyCollection<PaymentMethod>>
{
    public async Task<Result<IReadOnlyCollection<PaymentMethod>>> Handle(GetPaymentMethodsQuery request, CancellationToken cancellationToken)
    {
        var list = await repository.GetPaymentMethodsAsync(cancellationToken);

        return Result.Success(list);
    }
}