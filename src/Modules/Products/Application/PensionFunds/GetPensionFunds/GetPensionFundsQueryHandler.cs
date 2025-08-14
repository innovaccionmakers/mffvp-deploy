using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.PensionFunds;
using Products.Integrations.PensionFunds;
using Products.Integrations.PensionFunds.GetPensionFunds;

namespace Products.Application.PensionFunds.GetPensionFunds
{
    internal sealed class GetPensionFundsQueryHandler(
        IPensionFundRepository repository)
        : IQueryHandler<GetPensionFundsQuery, IReadOnlyCollection<PensionFundsResponse>>
    {
        public async Task<Result<IReadOnlyCollection<PensionFundsResponse>>> Handle(GetPensionFundsQuery request, CancellationToken cancellationToken)
        {
            var entities = await repository.GetAllAsync(cancellationToken);

            var response = entities
                .Select(p => new PensionFundsResponse(
                    p.PensionFundId,
                    p.DocumentTypeId,
                    p.IdentificationNumber,
                    p.Name,
                    p.ShortName,
                    p.Status,
                    p.HomologatedCode
                    ))
                .ToList();

            return Result.Success<IReadOnlyCollection<PensionFundsResponse>>(response);
        }
    }
}
