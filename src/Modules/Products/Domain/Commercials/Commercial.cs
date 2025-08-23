using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

namespace Products.Domain.Commercials;

public sealed class Commercial : Entity
{
    public int CommercialId { get; private set; }
    public string Name { get; private set; }
    public string Prefix { get; private set; }
    public Status Status { get; private set; }
    public string HomologatedCode { get; private set; }

    private Commercial()
    {
    }

    public static Result<Commercial> Create(
        string name,
        Status status,
        string prefix,
        string homologatedCode
    )
    {
        var commercial = new Commercial
        {
            Name = name,
            Status = status,
            Prefix = prefix,
            HomologatedCode = homologatedCode
        };

        commercial.Raise(new CommercialCreatedDomainEvent(commercial.CommercialId));
        return Result.Success(commercial);
    }

    public void UpdateDetails(string name, Status status, string prefix, string homologatedCode)
    {
        Name = name;
        Status = status;
        Prefix = prefix;
        HomologatedCode = homologatedCode;
    }
}