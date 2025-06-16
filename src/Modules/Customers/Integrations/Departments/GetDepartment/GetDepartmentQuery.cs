using Common.SharedKernel.Application.Messaging;
using System;

namespace Customers.Integrations.Departments.GetDepartment;
public sealed record GetDepartmentQuery(
    string HomologatedCode
) : IQuery<DepartmentResponse>;