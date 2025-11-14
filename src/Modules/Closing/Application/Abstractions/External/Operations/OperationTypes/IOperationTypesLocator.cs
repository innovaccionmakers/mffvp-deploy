
using Common.SharedKernel.Application.Helpers.Serialization;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.OperationTypes;
using System.Text.Json;

namespace Closing.Application.Abstractions.External.Operations.OperationTypes;

public interface IOperationTypesLocator
{
    Task<Result<IReadOnlyCollection<OperationTypeInfo>>> GetAllOperationTypesAsync(
        CancellationToken cancellationToken);
    Task<Result<OperationTypeInfo>> GetOperationTypeByNameAsync(
         string name,
         CancellationToken cancellationToken);

}

public sealed record OperationTypeInfo(
    long OperationTypeId,
    string Name,
    string? Category,
    IncomeEgressNature Nature,
    Status Status,
    string External,
    string HomologatedCode,
    JsonDocument AdditionalAttributes
);
public static class OperationTypeInfoExtensions
{
    private const string GroupListPropertyName = "GrupoLista";

    public static string? GetGroupList(this OperationTypeInfo operationType)
    {
        if (operationType is null)
        {
            return null;
        }

        var value = JsonStringHelper.ExtractString(
            operationType.AdditionalAttributes,
            GroupListPropertyName);

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
public static class OperationTypeGroupHelper
{
    public static bool BelongsToAnyGroup(
        OperationTypeInfo operationType,
        ISet<string> targetGroups,
        IReadOnlyDictionary<long, OperationTypeInfo> typesById)
    {
        // 1) Grupo del propio tipo
        var ownGroup = operationType.GetGroupList();
        if (!string.IsNullOrWhiteSpace(ownGroup) &&
            targetGroups.Contains(ownGroup!))
        {
            return true;
        }

        // 2) Si no tiene grupo, miramos el padre (Category)
        if (string.IsNullOrWhiteSpace(operationType.Category))
        {
            return false;
        }

        if (!long.TryParse(operationType.Category, out var parentId))
        {
            return false;
        }

        if (!typesById.TryGetValue(parentId, out var parentType))
        {
            return false;
        }

        var parentGroup = parentType.GetGroupList();
        return !string.IsNullOrWhiteSpace(parentGroup) &&
               targetGroups.Contains(parentGroup!);
    }
}

