﻿using Operations.Domain.ClientOperations;
using Operations.Integrations.ClientOperations;
using Operations.Presentation.DTOs;
using Products.Domain.PensionFunds;

namespace Operations.Presentation.GraphQL;

public interface IOperationsExperienceQueries
{
    Task<IReadOnlyCollection<TransactionTypeDto>> GetTransactionTypesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OperationTypeDto>> GetOperationTypesAsync(
        int? categoryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CertificationStatusDto>> GetCertificationStatusesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OriginModeDto>> GetOriginModesAsync(
        int originId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CollectionMethodDto>> GetCollectionMethodsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PaymentMethodDto>> GetPaymentMethodsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OriginContributionDto>> GetOriginContributionsAsync(
        CancellationToken cancellationToken = default);

    Task<string> GetWithholdingContingencyAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ClientOperationsByProcessDateDto>> GetClientOperationsByProcessDateAsync(
        DateTime processDate, CancellationToken cancellationToken = default);

}
