using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace Customers.Integrations.Departments.GetDepartments;
public sealed record GetDepartmentsQuery() : IQuery<IReadOnlyCollection<DepartmentResponse>>;