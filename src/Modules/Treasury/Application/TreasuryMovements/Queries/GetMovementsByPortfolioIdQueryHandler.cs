using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Treasury.Domain.TreasuryMovements;
using Treasury.Integrations.TreasuryMovements.Queries;
using Treasury.Integrations.TreasuryMovements.Response;

namespace Treasury.Application.TreasuryMovements.Queries;
internal sealed class GetMovementsByPortfolioIdQueryHandler(
    ITreasuryMovementRepository treasuryMovementRepository) : IQueryHandler<GetMovementsByPortfolioIdQuery, IReadOnlyCollection<GetMovementsByPortfolioIdResponse>>

{
    public async Task<Result<IReadOnlyCollection<GetMovementsByPortfolioIdResponse>>> Handle(GetMovementsByPortfolioIdQuery request, CancellationToken cancellationToken)
    {
        var treasuryMovements = await treasuryMovementRepository.GetReadOnlyTreasuryMovementsByPortfolioAsync(request.PortfolioId, request.ClosingDate, cancellationToken);
        var response = treasuryMovements
        .Select(m => new GetMovementsByPortfolioIdResponse(
            m.ConceptId, 
            m.ConceptName,
            m.Nature, 
            m.AllowsExpense, 
            m.TotalAmount
          ))
        .ToList();

        return response;
    }
}