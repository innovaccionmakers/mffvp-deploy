using Common.SharedKernel.Application.Messaging;
using People.Integrations.People;

namespace Integrations.People.GetPerson;

public sealed record GetPersonForIdentificationQuery(
    string DocumentType,
    string Identification
    ) : IQuery<PersonResponse>;
