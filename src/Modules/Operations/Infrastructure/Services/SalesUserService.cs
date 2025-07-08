using Operations.Domain.Services;

namespace Operations.Infrastructure.Services;

public class SalesUserService : ISalesUserService
{
    public string GetUser()
    {
        return "Makers";
    }
}