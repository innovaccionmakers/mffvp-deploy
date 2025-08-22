using Common.SharedKernel.Core.Primitives;

namespace Customers.Domain.Departments;
public static class DepartmentErrors
{
    public static Error NotFound(int departmentId) =>
        Error.NotFound(
            "Department.NotFound",
            $"The department with identifier {departmentId} was not found"
        );
}