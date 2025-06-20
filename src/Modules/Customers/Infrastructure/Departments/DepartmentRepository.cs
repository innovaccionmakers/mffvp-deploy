using Microsoft.EntityFrameworkCore;
using Customers.Domain.Departments;
using Customers.Infrastructure.Database;

namespace Customers.Infrastructure;

internal sealed class DepartmentRepository(CustomersDbContext context) : IDepartmentRepository
{
    public async Task<IReadOnlyCollection<Department>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Departments.ToListAsync(cancellationToken);
    }

    public async Task<Department?> GetAsync(string HomologatedCode, CancellationToken cancellationToken = default)
    {
        return await context.Departments
            .SingleOrDefaultAsync(x => x.HomologatedCode == HomologatedCode, cancellationToken);
    }

    public void Insert(Department department)
    {
        context.Departments.Add(department);
    }

    public void Update(Department department)
    {
        context.Departments.Update(department);
    }

    public void Delete(Department department)
    {
        context.Departments.Remove(department);
    }
}