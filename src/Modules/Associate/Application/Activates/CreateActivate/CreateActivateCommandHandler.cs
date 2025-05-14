using Associate.Application.Abstractions.Data;
using Associate.Domain.Activates;
using Associate.Integrations.Activates;
using Associate.Integrations.Activates.CreateActivate;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Associate.Application.Activates.CreateActivate;

internal sealed class CreateActivateCommandHandler(
    IActivateRepository activateRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateActivateCommand, ActivateResponse>
{
    private const string Workflow = "Associate.Associate.Validation";

    public async Task<Result<ActivateResponse>> Handle(CreateActivateCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var result = Activate.Create(
            request.IdentificationType,
            request.Identification,
            request.Pensioner,
            request.MeetsPensionRequirements,
            DateTime.UtcNow
        );

        if (result.IsFailure)
            return Result.Failure<ActivateResponse>(result.Error);

        var activate = result.Value;

        activateRepository.Insert(activate);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ActivateResponse(
            activate.ActivateId,
            activate.IdentificationType,
            activate.Identification,
            activate.Pensioner,
            activate.MeetsPensionRequirements,
            activate.ActivateDate
        );
    }
}