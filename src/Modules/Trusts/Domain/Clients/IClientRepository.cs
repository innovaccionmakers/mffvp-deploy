namespace Trusts.Domain.Clients;

public interface IClientRepository
{
    Client? Get(string idType, string idNumber);
}