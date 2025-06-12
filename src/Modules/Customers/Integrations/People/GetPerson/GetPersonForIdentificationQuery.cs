using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Customers.Integrations.People;

namespace Customers.Integrations.People.GetPerson;

public sealed record GetPersonForIdentificationQuery(    
    [property: HomologScope("TipoDocumento")]
    string IdentificationType,
    string Identification
) : IQuery<PersonResponse>;
