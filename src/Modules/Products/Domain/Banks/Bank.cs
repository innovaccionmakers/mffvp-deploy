using Common.SharedKernel.Domain;

namespace Products.Domain.Banks;

public sealed class Bank : Entity
{
    public int BankId { get; private set; }
    public string Name { get; private set; }

    private Bank()
    {
    }

    public static Result<Bank> Create(string name)
    {
        var bank = new Bank
        {
            BankId = default,
            Name = name
        };

        return Result.Success(bank);
    }

    public static Bank CreateforGraphql(
        int bankId,
        string name
    )
    {
        return new Bank
        {
            BankId = bankId,
            Name = name
        };
    }

    public void UpdateDetails(string newName)
    {
        Name = newName;
    }
}