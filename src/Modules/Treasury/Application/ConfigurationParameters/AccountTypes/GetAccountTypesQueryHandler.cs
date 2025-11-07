using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Treasury.Domain.ConfigurationParameters;
using Treasury.Integrations.ConfigurationParameters.AccountTypes;

namespace Treasury.Application.ConfigurationParameters.AccountTypes;

public class GetAccountTypesQueryHandler(IConfigurationParameterRepository repository)
    : IQueryHandler<GetAccountTypesQuery, IReadOnlyCollection<AccountTypeResponse>>
{
    public async Task<Result<IReadOnlyCollection<AccountTypeResponse>>> Handle(
        GetAccountTypesQuery request,
        CancellationToken cancellationToken)
    {
        var list = await repository.GetActiveConfigurationParametersByTypeAsync(
            ConfigurationParameterType.TipoCuenta,
            cancellationToken);

        var response = list
            .Select(e => new AccountTypeResponse(
                e.ConfigurationParameterId,
                e.Uuid,
                e.Name,
                e.HomologationCode
            ))
            .ToList();

        return Result.Success<IReadOnlyCollection<AccountTypeResponse>>(response);
    }
}

