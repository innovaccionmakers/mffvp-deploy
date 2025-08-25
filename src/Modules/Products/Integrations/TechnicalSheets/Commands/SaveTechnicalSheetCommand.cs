using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using MediatR;
using System.Text.Json.Serialization;

namespace Products.Integrations.TechnicalSheets.Commands;

[AuditLog]
public sealed record SaveTechnicalSheetCommand(
    [property: JsonPropertyName("FechaCierre")]
    DateOnly ClosingDate
) : ICommand<Unit>;
