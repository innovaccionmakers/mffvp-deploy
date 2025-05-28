using Common.SharedKernel.Domain;

namespace Products.Domain.Offices;

public sealed class Office : Entity
{
    public int OfficeId { get; private set; }
    public string Name { get; private set; }

    private Office()
    {
    }

    public static Result<Office> Create(
        string name
    )
    {
        var office = new Office
        {
            OfficeId = default,

            Name = name
        };

        office.Raise(new OfficeCreatedDomainEvent(office.OfficeId));
        return Result.Success(office);
    }

    public void UpdateDetails(
        string newName
    )
    {
        Name = newName;
    }
}