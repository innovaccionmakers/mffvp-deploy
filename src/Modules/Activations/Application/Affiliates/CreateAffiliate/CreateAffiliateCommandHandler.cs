using System.Data.Common;
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

namespace Activations.Application.Affiliates.CreateAffiliate

{
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

        public async Task<Result<AffiliateResponse>> Handle(CreateActivationCommand request, CancellationToken cancellationToken)
        {
            await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

            bool identificationTypeValid = lookupService.CodeExists("IdentificationType", request.IdentificationType);
            bool pensionerValid = lookupService.ValidatePensionerStatus(request.Pensioner);
            bool requirementsValid = lookupService.ValidatePensionRequirements(request.Pensioner, request.MeetsPensionRequirements);
            bool datesValid = lookupService.ValidatePensionDates(request.Pensioner, request.MeetsPensionRequirements, request.StartDateReqPen, request.EndDateReqPen);
            bool existingActivation = affiliateRepository.GetByIdTypeAndNumber(request.IdentificationType, request.Identification);
            Client? client = _clientRepository.Get(request.IdentificationType, request.Identification);

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
                AffiliateId: affiliate.AffiliateId,
                StartDate: request.StartDateReqPen ?? DateTime.UtcNow,
                ExpirationDate: request.EndDateReqPen ?? DateTime.UtcNow,
                CreationDate: DateTime.UtcNow,
                State: "A"
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
}