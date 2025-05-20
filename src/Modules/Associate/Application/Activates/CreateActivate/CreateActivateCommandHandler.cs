using Associate.Application.Abstractions.Data;
using Associate.Application.Abstractions.Rules;
using Associate.Domain.Activates;
using Associate.Domain.Clients;
using Associate.Integrations.Activates.CreateActivate;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Associate.Application.Activates.CreateActivate;

internal sealed class CreateActivateCommandHandler(
    IActivateRepository activateRepository,
    IRuleEvaluator ruleEvaluator,
    IClientRepository _clientRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateActivateCommand>
{
    private const string Workflow = "Associate.Activates.Validation";

    public async Task<Result> Handle(CreateActivateCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        bool existingActivate = activateRepository.GetByIdTypeAndNumber(request.IdentificationType, request.Identification);
        Client? client = _clientRepository.Get(request.IdentificationType, request.Identification);

        var validationContext = new ActivateValidationContext(
            request,
            client,
            existingActivate
        );

        var (isValid, _, ruleErrors) =
            await ruleEvaluator.EvaluateAsync(Workflow, validationContext, cancellationToken);

        if (!isValid)
        {
            var first = ruleErrors
                .OrderByDescending(r => r.Code)
                .First();

            return Result.Failure(
                Error.Validation(first.Code, first.Message));
        }

        var result = Activate.Create(
            request.IdentificationType,
            request.Identification,
            request.Pensioner,
            request.MeetsPensionRequirements ?? false,
            DateTime.UtcNow
        );

        if (result.IsFailure)
            return Result.Failure(result.Error);

        var activate = result.Value;

        activateRepository.Insert(activate);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }
}