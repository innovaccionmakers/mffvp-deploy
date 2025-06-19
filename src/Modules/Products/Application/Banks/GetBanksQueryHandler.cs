using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Banks;
using Products.Integrations.Banks;
using System.Text.Json;


namespace Products.Application.Banks;

public class GetBanksQueryHandler(
    IBankRepository repository)
    : IQueryHandler<GetBanksQuery, IReadOnlyCollection<Bank>>
{
    public async Task<Result<IReadOnlyCollection<Bank>>> Handle(
    GetBanksQuery request,
    CancellationToken cancellationToken)
    {
        var result = await repository.GetBanksAsync(cancellationToken);

        return Result.Success(result);
    }

}