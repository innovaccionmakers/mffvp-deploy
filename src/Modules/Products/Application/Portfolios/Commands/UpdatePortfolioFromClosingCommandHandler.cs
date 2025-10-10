using MediatR;
using Products.Application.Abstractions.Data;
using Products.Domain.Portfolios;
using Products.Integrations.Portfolios.Commands;

internal class UpdatePortfolioFromClosingCommandHandler(
    IPortfolioRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePortfolioFromClosingCommand>
{
    public async Task Handle(UpdatePortfolioFromClosingCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetAsync(request.PortfolioId, cancellationToken);
        if (existing is null)
            throw new InvalidOperationException($"Portafolio {request.PortfolioId} no encontrado.");

        // Aplica sólo la fecha de cierre
       existing.UpdateDetails(
            existing.HomologatedCode,
            existing.Name,
            existing.ShortName,
            existing.ModalityId,
            existing.InitialMinimumAmount,
            existing.AdditionalMinimumAmount,
            request.CloseDate,
            existing.Status
        );

        await repository.UpdateAsync(existing, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
