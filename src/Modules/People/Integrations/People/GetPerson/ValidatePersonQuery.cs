using Common.SharedKernel.Application.Messaging;

namespace People.Integrations.People.GetPerson;

public sealed record ValidatePersonQuery(string DocumentTypeHomologatedCode, string IdentificationNumber)
    : IQuery<ValidatePersonResponse>;