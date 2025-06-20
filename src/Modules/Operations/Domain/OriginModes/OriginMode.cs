using Common.SharedKernel.Domain;
using Operations.Domain.Origins;

namespace Operations.Domain.OriginModes;

public sealed class OriginMode : Entity
{
    public int Id { get; private set; }
    public int OriginId { get; private set; }
    public int ModalityOriginId { get; private set; }
    
    public Origin Origin { get; private set; }

    private OriginMode() { }

    public static Result<OriginMode> Create(int originId, int modalityOriginId)
    {
        var entity = new OriginMode
        {
            Id = default,
            OriginId = originId,
            ModalityOriginId = modalityOriginId
        };
        return Result.Success(entity);
    }
}