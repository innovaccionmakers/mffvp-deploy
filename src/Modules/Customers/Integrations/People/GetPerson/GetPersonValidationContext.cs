using Customers.Domain.People;
using Customers.Integrations.People.GetPerson;

namespace Integrations.People.GetPerson
{
    public class GetPersonValidationContext
    {
        public Person? Person { get; }
        public bool DocumentTypeExists { get; set; }

        public GetPersonValidationContext(Person? person)
        {
            Person = person;
        }
    }
}