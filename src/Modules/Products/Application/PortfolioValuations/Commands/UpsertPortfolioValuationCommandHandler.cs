

using MediatR;
using Products.Application.Abstractions.Data;
using Products.Domain.PortfolioValuations;
using Products.Integrations.PortfolioValuation.Commands;

namespace Products.Application.PortfolioValuations.Commands;

internal sealed class UpsertPortfolioValuationCommandHandler(
    IPortfolioValuationRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpsertPortfolioValuationCommand>
{
    public async Task Handle(UpsertPortfolioValuationCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByPortfolioIdAsync(request.PortfolioId, cancellationToken);

        if (existing is not null)
        {
            existing.UpdateValuation(
                request.CloseDate,
                request.Value,
                request.Units,
                request.UnitValue,
                request.GrossYieldUnits,
                request.UnitCost,
                request.DailyYield,
                request.IncomingOperations,
                request.OutgoingOperations,
                request.ProcessDate
            );

            await repository.UpdateAsync(existing, cancellationToken);
        }
        else
        {
            var result = PortfolioValuation.Create(
                request.PortfolioId,
                request.CloseDate,
                request.Value,
                request.Units,
                request.UnitValue,
                request.GrossYieldUnits,
                request.UnitCost,
                request.DailyYield,
                request.IncomingOperations,
                request.OutgoingOperations,
                request.ProcessDate
            );

            if (result.IsFailure)
                throw new InvalidOperationException($"Error creando valoracion portafolio dia: {result.Error.Description}");

            await repository.AddAsync(result.Value, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}