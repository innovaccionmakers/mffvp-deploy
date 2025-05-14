using Common.SharedKernel.Application.Messaging;
using System;

namespace Products.Integrations.Alternatives.UpdateAlternative;
public sealed record UpdateAlternativeCommand(
    long AlternativeId,
    int NewAlternativeTypeId,
    string NewName,
    string NewStatus,
    string NewDescription
) : ICommand<AlternativeResponse>;