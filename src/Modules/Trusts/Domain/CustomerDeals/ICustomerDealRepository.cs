namespace Trusts.Domain.CustomerDeals;

public interface ICustomerDealRepository
{
    Task<IReadOnlyCollection<CustomerDeal>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CustomerDeal?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    void Insert(CustomerDeal customerdeal);
    void Update(CustomerDeal customerdeal);
    void Delete(CustomerDeal customerdeal);
}