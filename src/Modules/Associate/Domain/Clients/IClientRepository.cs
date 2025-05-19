namespace Associate.Domain.Clients;

public interface IClientRepository
{
    Client? Get(string idType, string idNumber);
}