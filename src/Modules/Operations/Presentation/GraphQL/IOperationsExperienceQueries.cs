using Operations.Domain.ClientOperations;
using Operations.Integrations.ClientOperations;
using Operations.Presentation.DTOs;
using Products.Domain.PensionFunds;

namespace Operations.Presentation.GraphQL;

public interface IOperationsExperienceQueries
{
    Task<IReadOnlyCollection<TransactionTypeDto>> GetTransactionTypesAsync(
        IEnumerable<string>? groupLists = null, bool? visible = true,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OperationTypeDto>> GetOperationTypesAsync(
        int? categoryId,
        CancellationToken cancellationToken = default,
        IEnumerable<string>? groupLists = null,
        bool? visible = true);

    Task<IReadOnlyCollection<CertificationStatusDto>> GetCertificationStatusesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OriginModeDto>> GetOriginModesAsync(
        int originId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CollectionMethodDto>> GetCollectionMethodsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PaymentMethodDto>> GetPaymentMethodsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DebitNoteCauseDto>> GetDebitNoteCausesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CancellationClauseDto>> GetCancellationClausesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OriginContributionDto>> GetOriginContributionsAsync(
        CancellationToken cancellationToken = default);

    Task<string> GetWithholdingContingencyAsync(
        CancellationToken cancellationToken = default);

    Task<OperationNdPageDto> GetOperationsNdAsync(
        DateTime startDate,
        DateTime endDate,
        int affiliateId,
        int objectiveId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<OperationVoidPageDto> GetOperationsVoidAsync(
        int affiliateId,
        int objectiveId,
        long operationTypeId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AccTransactionTypesDto>> GetAccTransactionTypesAsync(CancellationToken cancellationToken = default);
}
