using Operations.Application.Abstractions.Services.SalesUser;

namespace Operations.Application.SalesUser.Services;

public class SalesUserService : ISalesUserService
{
    public string GetUser()
    {
        return "Makers";
    }
}