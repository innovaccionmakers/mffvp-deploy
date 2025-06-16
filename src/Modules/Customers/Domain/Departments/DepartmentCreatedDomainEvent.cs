using Common.SharedKernel.Domain;

namespace Customers.Domain.Departments;
public sealed class DepartmentCreatedDomainEvent(int departmentId) : DomainEvent
{
    public int DepartmentId { get; } = departmentId;
}