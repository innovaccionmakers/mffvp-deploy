using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Alternatives.CreateAlternative;

public sealed record CreateAlternativeCommand(
    int AlternativeTypeId,
    string Name,
    string Status,
    string Description
) : ICommand<AlternativeResponse>;