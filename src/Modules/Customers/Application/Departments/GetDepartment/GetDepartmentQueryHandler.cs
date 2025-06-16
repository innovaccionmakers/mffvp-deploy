using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Customers.Domain.Departments;
using Customers.Integrations.Departments.GetDepartment;
using Customers.Integrations.Departments;

namespace Customers.Application.Departments.GetDepartment;

internal sealed class GetDepartmentQueryHandler(
    IDepartmentRepository departmentRepository)
    : IQueryHandler<GetDepartmentQuery, DepartmentResponse>
{
    public async Task<Result<DepartmentResponse>> Handle(GetDepartmentQuery request, CancellationToken cancellationToken)
    {
        var department = await departmentRepository.GetAsync(request.HomologatedCode, cancellationToken);

        if (department == null)
            return null;

        var response = new DepartmentResponse(
            department.DepartmentId,
            department.Name,
            department.DaneCode,
            department.HomologatedCode
        );
        return response;
    }
}