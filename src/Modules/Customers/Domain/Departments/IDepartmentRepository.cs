namespace Customers.Domain.Departments;
public interface IDepartmentRepository
{
    Task<IReadOnlyCollection<Department>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Department?> GetAsync(string HomologatedCode, CancellationToken cancellationToken = default);
    void Insert(Department department);
    void Update(Department department);
    void Delete(Department department);
}