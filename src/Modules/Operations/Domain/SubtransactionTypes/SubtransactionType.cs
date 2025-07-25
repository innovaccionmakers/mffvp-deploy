using System.Collections.Generic;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.SubtransactionTypes;
using Operations.Domain.ClientOperations;

namespace Operations.Domain.SubtransactionTypes;

public sealed class SubtransactionType : Entity
{
    public long SubtransactionTypeId { get; private set; }
    public string Name { get; private set; }
    public Guid Category { get; private set; }
    public IncomeEgressNature Nature { get; private set; }
    public Status Status { get; private set; }
    public string External { get; private set; }
    public string HomologatedCode { get; private set; }

    private readonly List<ClientOperation> _clientOperations = new();
    public IReadOnlyCollection<ClientOperation> ClientOperations => _clientOperations;

    private SubtransactionType()
    {
    }

    public static Result<SubtransactionType> Create(
        string name,
        Guid category,
        IncomeEgressNature nature,
        Status status,
        string external,
        string homologatedCode
    )
    {
        var subtransactionType = new SubtransactionType
        {
            SubtransactionTypeId = default,
            Name = name,
            Category = category,
            Nature = nature,
            Status = status,
            External = external,
            HomologatedCode = homologatedCode
        };

        return Result.Success(subtransactionType);
    }

    public void UpdateDetails(
        string newName,
        Guid newCategory,
        IncomeEgressNature newNature,
        Status newStatus,
        string newExternal,
        string newHomologatedCode
    )
    {
        Name = newName;
        Category = newCategory;
        Nature = newNature;
        Status = newStatus;
        External = newExternal;
        HomologatedCode = newHomologatedCode;
    }
}