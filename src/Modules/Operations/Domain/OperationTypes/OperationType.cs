using System.Collections.Generic;
using System.Text.Json;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.OperationTypes;
using Operations.Domain.ClientOperations;

namespace Operations.Domain.OperationTypes;

public sealed class OperationType : Entity
{
    public long OperationTypeId { get; private set; }
    public string Name { get; private set; }
    public int? CategoryId { get; private set; }
    public IncomeEgressNature Nature { get; private set; }
    public Status Status { get; private set; }
    public string External { get; private set; }
    public bool Visible { get; private set; }
    public JsonDocument AdditionalAttributes { get; private set; }
    public string HomologatedCode { get; private set; }

    private readonly List<ClientOperation> _clientOperations = new();
    public IReadOnlyCollection<ClientOperation> ClientOperations => _clientOperations;

    private OperationType()
    {
    }

    public static Result<OperationType> Create(
        string name,
        int? categoryId,
        IncomeEgressNature nature,
        Status status,
        string external,
        bool visible,
        JsonDocument additionalAttributes,
        string homologatedCode
    )
    {
        var operationType = new OperationType
        {
            OperationTypeId = default,
            Name = name,
            CategoryId = categoryId,
            Nature = nature,
            Status = status,
            External = external,
            Visible = visible,
            AdditionalAttributes = additionalAttributes,
            HomologatedCode = homologatedCode
        };

        return Result.Success(operationType);
    }

    public void UpdateDetails(
        string newName,
        int? newCategoryId,
        IncomeEgressNature newNature,
        Status newStatus,
        string newExternal,
        bool newVisible,
        JsonDocument newAdditionalAttributes,
        string newHomologatedCode
    )
    {
        Name = newName;
        CategoryId = newCategoryId;
        Nature = newNature;
        Status = newStatus;
        External = newExternal;
        Visible = newVisible;
        AdditionalAttributes = newAdditionalAttributes;
        HomologatedCode = newHomologatedCode;
    }
}