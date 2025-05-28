using Common.SharedKernel.Domain;

namespace Products.Domain.Commercials;

public sealed class Commercial : Entity
{
    public int CommercialId { get; private set; }
    public string Name { get; private set; }

    private Commercial()
    {
    }

    public static Result<Commercial> Create(
        string name
    )
    {
        var commercial = new Commercial
        {
            CommercialId = default,

            Name = name
        };

        commercial.Raise(new CommercialCreatedDomainEvent(commercial.CommercialId));
        return Result.Success(commercial);
    }

    public void UpdateDetails(
        string newName
    )
    {
        Name = newName;
    }
}