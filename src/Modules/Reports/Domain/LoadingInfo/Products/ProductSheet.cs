using Common.SharedKernel.Domain;

namespace Reports.Domain.LoadingInfo.Products;

public sealed class ProductSheet : Entity
{
    public long Id { get; private set; }

    public int AdministratorId { get; private set; }

    public int EntityType { get; private set; }

    public string EntityCode { get; private set; } = null!;

    public string EntitySfcCode { get; private set; } = null!;

    public int BusinessCodeSfcFund { get; private set; }

    public int FundId { get; private set; }

    public static Result<ProductSheet> Create(
        long id,
        int administratorId,
        int entityType,
        string entityCode,
        string entitySfcCode,
        int businessCodeSfcFund,
        int fundId)
    {
        var sheet = new ProductSheet
        {
            Id = id,
            AdministratorId = administratorId,
            EntityType = entityType,
            EntityCode = entityCode,
            EntitySfcCode = entitySfcCode,
            BusinessCodeSfcFund = businessCodeSfcFund,
            FundId = fundId
        };

        return Result.Success(sheet);
    }
}