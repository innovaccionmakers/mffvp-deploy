using Activations.Application.Abstractions.Data;
using Activations.Application.Abstractions.Lookups;
using Activations.Application.Abstractions.Rules;
using Activations.Application.Affiliates.CreateActivation;
using Activations.Domain.Affiliates;
using Activations.Domain.Clients;
using Activations.Integrations.Affiliates;
using Activations.Integrations.Affiliates.CreateActivation;
using Activations.Integrations.MeetsPensionRequirements.CreateMeetsPensionRequirement;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using MediatR;

namespace Activations.Application.Affiliates.CreateAffiliate;

internal sealed class CreateAffiliateCommandHandler(
    IAffiliateRepository affiliateRepository,
    IRuleEvaluator ruleEvaluator,
    ILookupService lookupService,
    IClientRepository _clientRepository,
    IUnitOfWork unitOfWork,
    ISender mediator)
    : ICommandHandler<CreateActivationCommand, AffiliateResponse>
{
    private const string Workflow = "Activations.Activation.Validation";

    public async Task<Result<AffiliateResponse>> Handle(CreateActivationCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var identificationTypeValid = lookupService.CodeExists("IdentificationType", request.IdentificationType);
        var pensionerValid = lookupService.ValidatePensionerStatus(request.Pensioner);
        var requirementsValid =
            lookupService.ValidatePensionRequirements(request.Pensioner, request.MeetsPensionRequirements);
        var datesValid = lookupService.ValidatePensionDates(request.Pensioner, request.MeetsPensionRequirements,
            request.StartDateReqPen, request.EndDateReqPen);
        var existingActivation =
            affiliateRepository.GetByIdTypeAndNumber(request.IdentificationType, request.Identification);
        var client = _clientRepository.Get(request.IdentificationType, request.Identification);

        var validationContext = new ActivationValidationContext(
            request,
            client,
            identificationTypeValid,
            pensionerValid,
            requirementsValid,
            datesValid,
            existingActivation
        );

        var (isValid, _, ruleErrors) =
            await ruleEvaluator.EvaluateAsync(Workflow, validationContext, cancellationToken);

        if (!isValid)
        {
            var first = ruleErrors
                .OrderByDescending(r => r.Code)
                .First();

            return Result.Failure<AffiliateResponse>(
                Error.Validation(first.Code, first.Message));
        }

        var result = Affiliate.Create(
            request.IdentificationType,
            request.Identification,
            request.Pensioner,
            request.MeetsPensionRequirements,
            DateTime.UtcNow
        );

        if (result.IsFailure)
            return Result.Failure<AffiliateResponse>(result.Error);

        var affiliate = result.Value;

        affiliateRepository.Insert(affiliate);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var createMeetsPensionRequirementCommand = new CreateMeetsPensionRequirementCommand(
            affiliate.AffiliateId,
            request.StartDateReqPen ?? DateTime.UtcNow,
            request.EndDateReqPen ?? DateTime.UtcNow,
            DateTime.UtcNow,
            "A"
        );

        await mediator.Send(createMeetsPensionRequirementCommand, cancellationToken);

        return new AffiliateResponse(
            affiliate.AffiliateId,
            affiliate.IdentificationType,
            affiliate.Identification,
            affiliate.Pensioner,
            affiliate.MeetsRequirements,
            affiliate.ActivationDate
        );
    }
}