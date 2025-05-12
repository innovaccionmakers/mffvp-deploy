using System.Text.Json;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Application.Abstractions.Data;
using Trusts.Application.Abstractions.Lookups;
using Trusts.Application.Abstractions.Rules;
using Trusts.Domain.Clients;
using Trusts.Domain.CustomerDeals;
using Trusts.Domain.InputInfos;
using Trusts.Domain.Portfolios;
using Trusts.Domain.TrustOperations;
using Trusts.Domain.Trusts;
using Trusts.Integrations.FullContribution;

namespace Trusts.Application.FullContribution;

internal sealed class CreateFullContributionCommandHandler
    : ICommandHandler<CreateFullContributionCommand, FullContributionResponse>
{
    private const string Workflow = "Trusts.FullContribution.Validation";
    private readonly IClientRepository _clientRepository;
    private readonly ICustomerDealRepository _customerDealRepository;
    private readonly IInputInfoRepository _inputInfoRepository;
    private readonly ILookupService _lookupService;
    private readonly IPortfolioRepository _portfolioRepository;

    private readonly IRuleEvaluator _ruleEvaluator;
    private readonly ITrustOperationRepository _trustOperationRepository;
    private readonly ITrustRepository _trustRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFullContributionCommandHandler(
        IRuleEvaluator ruleEvaluator,
        ILookupService lookupService,
        IClientRepository clientRepository,
        IPortfolioRepository portfolioRepository,
        ITrustRepository trustRepository,
        ITrustOperationRepository trustOperationRepository,
        ICustomerDealRepository customerDealRepository,
        IInputInfoRepository inputInfoRepository,
        IUnitOfWork unitOfWork)
    {
        _ruleEvaluator = ruleEvaluator;
        _lookupService = lookupService;
        _clientRepository = clientRepository;
        _portfolioRepository = portfolioRepository;
        _trustRepository = trustRepository;
        _trustOperationRepository = trustOperationRepository;
        _customerDealRepository = customerDealRepository;
        _inputInfoRepository = inputInfoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FullContributionResponse>> Handle(
        CreateFullContributionCommand cmd,
        CancellationToken ct)
    {
        // 1. Base data + mock lookups
        var client = _clientRepository.Get(cmd.IdentificationType, cmd.IdentificationNumber);
        var portfolio = cmd.PortfolioCode is null
            ? null
            : _portfolioRepository.GetByCode(cmd.PortfolioCode);

        var idTypeExists = _lookupService.CodeExists("IdentificationType", cmd.IdentificationType);
        var originExists = _lookupService.CodeExists("Origin", cmd.OriginCode);
        var originActive = _lookupService.CodeIsActive("Origin", cmd.OriginCode);
        var collectionExists = _lookupService.CodeExists("CollectionMethod", cmd.CollectionMethodCode);
        var collectionActive = _lookupService.CodeIsActive("CollectionMethod", cmd.CollectionMethodCode);
        var paymentExists = _lookupService.CodeExists("PaymentMethod", cmd.PaymentMethodCode);
        var paymentActive = _lookupService.CodeIsActive("PaymentMethod", cmd.PaymentMethodCode);
        var originRequiresCert = _lookupService.OriginRequiresCertification(cmd.OriginCode);

        var ctx = new FullContributionContext(
            cmd,
            client,
            portfolio,
            idTypeExists,
            originExists,
            originActive,
            collectionExists,
            collectionActive,
            paymentExists,
            paymentActive,
            originRequiresCert);

        var (isValid, _, ruleErrors) =
            await _ruleEvaluator.EvaluateAsync(Workflow, ctx, ct);

        if (!isValid)
        {
            var first = ruleErrors.OrderByDescending(r => r.Code).First();
            return Result.Failure<FullContributionResponse>(
                Error.Validation(first.Code, first.Message));
        }

        // 2. Begin transaction
        await using var trx = await _unitOfWork.BeginTransactionAsync(ct);

        // 3. Create CustomerDeal
        var dealResult = CustomerDeal.Create(
            cmd.ExecutionDate,
            0,
            cmd.ObjectiveId,
            0,
            _lookupService.CodeExists("OriginMode", cmd.OriginMode)
                ? _lookupService.CodeIsActive("OriginMode", cmd.OriginMode) ? 1 : 0
                : 0,
            cmd.Amount);

        if (dealResult.IsFailure)
            return Result.Failure<FullContributionResponse>(dealResult.Error);

        var deal = dealResult.Value;
        _customerDealRepository.Insert(deal);

        // 4. Create or get Trust
        var trustResult = Trust.Create(
            0,
            0,
            cmd.ObjectiveId,
            0,
            cmd.Amount,
            0,
            cmd.Amount,
            0,
            0,
            cmd.ContingentWithholding ?? 0);

        if (trustResult.IsFailure)
            return Result.Failure<FullContributionResponse>(trustResult.Error);

        var trust = trustResult.Value;
        _trustRepository.Insert(trust);

        // 5. Create TrustOperation
        var opResult = TrustOperation.Create(
            cmd.Amount,
            deal,
            trust);

        if (opResult.IsFailure)
            return Result.Failure<FullContributionResponse>(opResult.Error);

        var trustOperation = opResult.Value;
        _trustOperationRepository.Insert(trustOperation);

        // 6. Create InputInfo
        var inputResult = InputInfo.Create(
            customerDeal: deal,
            originId: 0,
            collectionMethodId: 0,
            paymentFormId: 0,
            collectionAccount: int.TryParse(cmd.CollectionAccount, out var acct) ? acct : 0,
            paymentFormDetail: JsonDocument.Parse(cmd.PaymentDetails ?? "{}"),
            certificationStatusId: cmd.CertificationStatus == "SI" ? 1 : 0,
            taxConditionId: 0,
            contingentWithholding: (int)(cmd.ContingentWithholding ?? cmd.Amount * 0.07m),
            verifiableMedium: JsonDocument.Parse(cmd.VerifiableMedium ?? "{}"),
            collectionBank: cmd.CollectionBank,
            depositDate: cmd.DepositDate,
            salesUser: cmd.SalesUser,
            city: string.Empty);

        if (inputResult.IsFailure)
            return Result.Failure<FullContributionResponse>(inputResult.Error);

        _inputInfoRepository.Insert(inputResult.Value);

        // 7. Persist & commit
        await _unitOfWork.SaveChangesAsync(ct);
        await trx.CommitAsync(ct);

        // 8. Build response
        var response = new FullContributionResponse(
            trustOperation.TrustOperationId,
            portfolio?.Code ?? "P01",
            portfolio?.Name ?? "Default Portfolio",
            "No Contingent Withholding",
            cmd.ContingentWithholding ?? 0);

        return Result.Success(response);
    }
}