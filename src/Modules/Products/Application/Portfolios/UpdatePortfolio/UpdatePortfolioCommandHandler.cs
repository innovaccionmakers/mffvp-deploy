using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions.Data;
using Products.Domain.Portfolios;
using Products.Integrations.Portfolios;
using Products.Integrations.Portfolios.UpdatePortfolio;

namespace Products.Application.Portfolios;

internal sealed class UpdatePortfolioCommandHandler(
    IPortfolioRepository portfolioRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdatePortfolioCommand, PortfolioResponse>
{
    public async Task<Result<PortfolioResponse>> Handle(UpdatePortfolioCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = await portfolioRepository.GetAsync(request.PortfolioId, cancellationToken);
        if (entity is null) return Result.Failure<PortfolioResponse>(PortfolioErrors.NotFound(request.PortfolioId));

        entity.UpdateDetails(
            request.NewStandardCode,
            request.NewName,
            request.NewShortName,
            request.NewModalityId,
            request.NewInitialMinimumAmount
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new PortfolioResponse(entity.PortfolioId, entity.StandardCode, entity.Name, entity.ShortName,
            entity.ModalityId, entity.InitialMinimumAmount);
    }
}