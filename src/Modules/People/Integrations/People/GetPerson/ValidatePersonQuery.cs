using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace People.Integrations.People.GetPerson;

public sealed record ValidatePersonQuery(
    [property: HomologScope("Tipo documento")]
    string DocumentTypeHomologatedCode,
    string IdentificationNumber)
    : IQuery<ValidatePersonResponse>;