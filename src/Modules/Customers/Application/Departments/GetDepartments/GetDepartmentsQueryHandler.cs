using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Customers.Domain.Departments;
using Customers.Integrations.Departments.GetDepartments;
using Customers.Integrations.Departments;
using System.Collections.Generic;
using System.Linq;

namespace Customers.Application.Departments.GetDepartments;

internal sealed class GetDepartmentsQueryHandler(
    IDepartmentRepository departmentRepository)
    : IQueryHandler<GetDepartmentsQuery, IReadOnlyCollection<DepartmentResponse>>
{
    public async Task<Result<IReadOnlyCollection<DepartmentResponse>>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var entities = await departmentRepository.GetAllAsync(cancellationToken);
        
        var response = entities
            .Select(e => new DepartmentResponse(
                e.DepartmentId,
                e.Name,
                e.DaneCode,
                e.HomologatedCode))
            .ToList();

        return Result.Success<IReadOnlyCollection<DepartmentResponse>>(response);
    }
}