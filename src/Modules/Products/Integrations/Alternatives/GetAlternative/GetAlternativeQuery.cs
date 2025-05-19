using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Alternatives.GetAlternative;

public sealed record GetAlternativeQuery(
    long AlternativeId
) : IQuery<AlternativeResponse>;