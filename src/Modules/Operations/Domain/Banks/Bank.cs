using Common.SharedKernel.Domain;
using Operations.Domain.AuxiliaryInformations;

namespace Operations.Domain.Banks;

public sealed class Bank : Entity
{
    public int BankId { get; private set; }
    public string Nit { get; private set; }
    public string Name { get; private set; }
    public int CompensationCode { get; private set; }
    public Status Status { get; private set; }
    public string HomologatedCode { get; private set; }
    public int CheckClearingDays { get; private set; }
    
    private readonly List<AuxiliaryInformation> _auxiliaryInformations = new();
    public IReadOnlyCollection<AuxiliaryInformation> AuxiliaryInformations => _auxiliaryInformations;

    private Bank() { }

    public static Result<Bank> Create(
        string nit,
        string name,
        int compensationCode,
        Status status,
        string homologatedCode,
        int checkClearingDays)
    {
        var bank = new Bank
        {
            BankId = default,
            Nit = nit,
            Name = name,
            CompensationCode = compensationCode,
            Status = status,
            HomologatedCode = homologatedCode,
            CheckClearingDays = checkClearingDays
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
}