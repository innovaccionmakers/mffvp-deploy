using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Common.SharedKernel.Presentation.Results;

internal static class ApiSuccessBuilder
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propCache = new();

    public static IResult Build<T>(
        T payload,
        string descripcion = "Transacción causada Exitosamente",
        int codigo = 0)
    {
        var dict = new Dictionary<string, object?>
        {
            ["Estado"] = "Exitosa",
            ["CodigoRespuesta"] = codigo,
            ["DescripcionRespuesta"] = descripcion
        };

        var props = _propCache.GetOrAdd(
            typeof(T),
            t => t.GetProperties(BindingFlags.Instance | BindingFlags.Public));

        foreach (var p in props)
        {
            dict[p.Name] = p.GetValue(payload);
        }

        return HttpResults.Json(dict, statusCode: StatusCodes.Status200OK);
    }
}
