using Operations.Domain.ConfigurationParameters;
using Common.SharedKernel.Domain;
using Operations.Integrations.ConfigurationParameters;
using Common.SharedKernel.Application.Messaging;

namespace Operations.Application.ConfigurationParameters;

public class GetCertificationStatusesQueryHandler(IConfigurationParameterRepository repository) : IQueryHandler<GetCertificationStatusesQuery, IReadOnlyCollection<CertificationStatus>>
{
    public async Task<Result<IReadOnlyCollection<CertificationStatus>>> Handle(GetCertificationStatusesQuery request, CancellationToken cancellationToken)
    {
        var list = await repository.GetCertificationStatusesAsync(cancellationToken);
        return Result.Success(list);
    }
}