using Closing.Domain.PortfolioValuations;
using Closing.Integrations.PortfolioValuations.Queries;
using Closing.Integrations.PortfolioValuations.Response;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Closing.Application.PortfolioValuations.Queries;

internal sealed class CheckValuationExistsQueryHandler(
    IPortfolioValuationRepository portfolioValuationRepository
) : IQueryHandler<CheckValuationExistsQuery, CheckValuationExistsResponse>
{
    public async Task<Result<CheckValuationExistsResponse>> Handle(
        CheckValuationExistsQuery request,
        CancellationToken cancellationToken)
    {
        var exists = await portfolioValuationRepository
            .ExistsByClosingDateAsync(request.ClosingDate, cancellationToken);

        var response = new CheckValuationExistsResponse(exists);
        return Result.Success(response, "Consulta realizada correctamente");
    }
}
