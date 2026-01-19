
using Common.SharedKernel.Core.Primitives;

namespace Products.Integrations.Administrators;

public sealed record AdministratorResponse(
    int AdministratorId,
    string Identification,
    int IdentificationTypeId,
    int Digit,
    string Name,
    Status Status,
    string EntityCode,
    int EntityType,
    string SfcEntityCode
);

