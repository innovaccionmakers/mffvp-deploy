using Common.SharedKernel.Domain;

namespace Customers.Domain.Departments;
public sealed class Department : Entity
{
    public int DepartmentId { get; private set; }
    public string Name { get; private set; }
    public int DaneCode { get; private set; }
    public string HomologatedCode { get; private set; }

    private Department() { }

    public static Result<Department> Create(
        string name, int danecode, string homologatedcode
    )
    {
        var department = new Department
        {
                DepartmentId = new int(),
                Name = name,
                DaneCode = danecode,
                HomologatedCode = homologatedcode,
        };
        department.Raise(new DepartmentCreatedDomainEvent(department.DepartmentId));
        return Result.Success(department);
    }

    public void UpdateDetails(
        string newName, int newDaneCode, string newHomologatedCode
    )
    {
        Name = newName;
        DaneCode = newDaneCode;
        HomologatedCode = newHomologatedCode;
    }
}