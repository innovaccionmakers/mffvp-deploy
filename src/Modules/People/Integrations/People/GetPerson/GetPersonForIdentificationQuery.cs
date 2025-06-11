using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using People.Integrations.People;

namespace Integrations.People.GetPerson;

public sealed record GetPersonForIdentificationQuery(    
    [property: HomologScope("TipoDocumento")]
    string IdentificationType,
    string Identification
) : IQuery<PersonResponse>;