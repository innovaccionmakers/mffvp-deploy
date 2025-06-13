using Customers.Domain.People;
using Integrations.People.CreatePerson;

namespace Application.People.CreatePerson
{
    public class CreatePersonValidationContext
    {
        CreatePersonRequestCommand Request { get; }
        Person? ExistingPerson { get; }
        Guid Uuid { get; }

        public CreatePersonValidationContext(CreatePersonRequestCommand request, Person? existingPerson, Guid uuid)
        {
            Request = request;
            ExistingPerson = existingPerson;
            Uuid = uuid;
        }
    }
}