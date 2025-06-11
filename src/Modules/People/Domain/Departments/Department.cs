using Common.SharedKernel.Domain;

namespace People.Domain.Departments;

public sealed class Department : Entity
{
    public int DepartmentId { get; private set; }
    public string Name { get; private set; }
    public int DaneCode { get; private set; }
    public string HomologatedCode { get; private set; }
    
    private Department()
    {
    }

    public static Result<Department> Create(string name, int daneCode, string homologatedCode)
    {
        var department = new Department
        {
            DepartmentId = default,
            Name = name,
            DaneCode = daneCode,
            HomologatedCode = homologatedCode
        };
        
        return Result.Success(department);
    }

    public void UpdateDetails(string newName, int newDaneCode, string newHomologatedCode)
    {
        Name = newName;
        DaneCode = newDaneCode;
        HomologatedCode = newHomologatedCode;
    }
}