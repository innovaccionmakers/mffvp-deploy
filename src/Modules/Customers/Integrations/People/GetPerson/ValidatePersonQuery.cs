using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Customers.Integrations.People.GetPerson;

public sealed record ValidatePersonQuery(
    [property: HomologScope("TipoDocumento")]
    string DocumentTypeHomologatedCode,
    string IdentificationNumber)
    : IQuery<ValidatePersonResponse>;