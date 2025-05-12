namespace Trusts.Domain.InputInfos;

public interface IInputInfoRepository
{
    Task<IReadOnlyCollection<InputInfo>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InputInfo?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    void Insert(InputInfo inputinfo);
    void Update(InputInfo inputinfo);
    void Delete(InputInfo inputinfo);
}