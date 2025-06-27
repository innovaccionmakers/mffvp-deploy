using Operations.Presentation.DTOs;

namespace Operations.Presentation.GraphQL;

public interface IOperationsExperienceQueries
{
    Task<IReadOnlyCollection<TransactionTypeDto>> GetTransactionTypesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SubTransactionTypeDto>> GetSubTransactionTypesAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CertificationStatusDto>> GetCertificationStatusesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OriginModeDto>> GetOriginModesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CollectionMethodDto>> GetCollectionMethodsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PaymentMethodDto>> GetPaymentMethodsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OriginContributionDto>> GetOriginContributionsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<BankDto>> GetBanksAsync(CancellationToken cancellationToken = default);
}
