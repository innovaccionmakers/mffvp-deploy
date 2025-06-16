namespace Customers.Integrations.Departments;

public sealed record DepartmentResponse(
    int DepartmentId,
    string Name,
    int DaneCode,
    string HomologatedCode
);