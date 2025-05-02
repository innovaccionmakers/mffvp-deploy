using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Contributions.Application.Abstractions.Data;
using Contributions.Application.Abstractions.Lookups;
using Contributions.Application.Abstractions.Rules;
using Contributions.Domain.ClientOperations;
using Contributions.Domain.Clients;
using Contributions.Domain.Portfolios;
using Contributions.Domain.TrustOperations;
using Contributions.Domain.Trusts;
using Contributions.Integrations.FullContribution;

namespace Contributions.Application.FullContribution
{
    internal sealed class CreateFullContributionCommandHandler
    : ICommandHandler<CreateFullContributionCommand, FullContributionResponse>
    {
        private const string Workflow = "Contributions.FullContribution.Validation";

        private readonly IRuleEvaluator _ruleEvaluator;
        private readonly ILookupService _lookupService;
        private readonly IClientRepository _clientRepository;
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly IClientOperationRepository _clientOperationRepository;
        private readonly ITrustRepository _trustRepository;
        private readonly ITrustOperationRepository _trustOperationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateFullContributionCommandHandler(
            IRuleEvaluator ruleEvaluator,
            ILookupService lookupService,
            IClientRepository clientRepository,
            IPortfolioRepository portfolioRepository,
            IClientOperationRepository clientOperationRepository,
            ITrustRepository trustRepository,
            ITrustOperationRepository trustOperationRepository,
            IUnitOfWork unitOfWork)
        {
            _ruleEvaluator = ruleEvaluator;
            _lookupService = lookupService;
            _clientRepository = clientRepository;
            _portfolioRepository = portfolioRepository;
            _clientOperationRepository = clientOperationRepository;
            _trustRepository = trustRepository;
            _trustOperationRepository = trustOperationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<FullContributionResponse>> Handle(
            CreateFullContributionCommand command,
            CancellationToken cancellationToken)
        {
            Client? client = _clientRepository.Get(command.IdentificationType, command.Identification);

            Portfolio? portfolio = command.PortfolioCode is null
                ? null
                : _portfolioRepository.GetByCode(command.PortfolioCode);

            bool idTypeHomologated = _lookupService.CodeExists("IdentificationType", command.IdentificationType);
            bool originExists = _lookupService.CodeExists("Origin", command.OriginCode);
            bool originActive = _lookupService.CodeIsActive("Origin", command.OriginCode);
            bool collectionExists = _lookupService.CodeExists("CollectionMethod", command.CollectionMethodCode);
            bool collectionActive = _lookupService.CodeIsActive("CollectionMethod", command.CollectionMethodCode);
            bool paymentExists = _lookupService.CodeExists("PaymentMethod", command.PaymentMethodCode);
            bool paymentActive = _lookupService.CodeIsActive("PaymentMethod", command.PaymentMethodCode);
            bool originRequiresCertification = _lookupService.OriginRequiresCertification(command.OriginCode);

            var validationContext = new FullContributionContext(
                command,
                client,
                portfolio,
                idTypeHomologated,
                originExists,
                originActive,
                collectionExists,
                collectionActive,
                paymentExists,
                paymentActive,
                originRequiresCertification);

            var (isValid, _, ruleErrors) =
                await _ruleEvaluator.EvaluateAsync(Workflow, validationContext, cancellationToken);

            if (!isValid)
            {
                var first = ruleErrors
                    .OrderByDescending(r => r.Code)
                    .First();

                return Result.Failure<FullContributionResponse>(
                    Error.Validation(first.Code, first.Message));
            }

            await using DbTransaction transaction =
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var clientOpResult = ClientOperation.Create(
                validationContext.Cmd.ExecutionDate,
                affiliateid: 0,
                objectiveid: validationContext.Cmd.ObjectiveId,
                portfolioid: 0,
                transactiontypeid: 1,
                subtransactiontypeid: 1,
                amount: validationContext.Cmd.Amount);

            if (clientOpResult.IsFailure)
                return Result.Failure<FullContributionResponse>(clientOpResult.Error);

            var clientOperation = clientOpResult.Value;
            _clientOperationRepository.Insert(clientOperation);

            var trustResult = Trust.Create(
                affiliateid: 0,
                objectiveid: validationContext.Cmd.ObjectiveId,
                portfolioid: 0,
                totalbalance: validationContext.Cmd.Amount,
                totalunits: null,
                principal: validationContext.Cmd.Amount,
                earnings: 0,
                taxcondition: 0,
                contingentwithholding: validationContext.Cmd.ContingentWithholding ?? 0,
                earningswithholding: 0,
                availablebalance: validationContext.Cmd.Amount);

            if (trustResult.IsFailure)
                return Result.Failure<FullContributionResponse>(trustResult.Error);

            var trust = trustResult.Value;
            _trustRepository.Insert(trust);

            var trustOpResult = TrustOperation.Create(
                validationContext.Cmd.Amount,
                clientOperation,
                trust);

            if (trustOpResult.IsFailure)
                return Result.Failure<FullContributionResponse>(trustOpResult.Error);

            var trustOperation = trustOpResult.Value;
            _trustOperationRepository.Insert(trustOperation);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var response = new FullContributionResponse(
                OperationId: trustOperation.TrustOperationId,
                PortfolioCode: portfolio?.Code ?? "P01",
                PortfolioName: portfolio?.Name ?? "Portafolio Liquidez",
                TaxConditionDescription: "Sin Retención Contingente",
                ContingentWithholdingValue: command.ContingentWithholding ?? 0);

            return Result.Success(response);
        }
    }
}
