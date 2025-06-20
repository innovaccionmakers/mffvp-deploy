using Common.SharedKernel.Application.Messaging;
namespace Associate.Integrations.Associates;

public sealed record class GetAssociatesQuery(
    string? IdentificationType,
    string? SearchBy,
    string? Text) : IQuery<IReadOnlyCollection<AssociateResponse>>;
