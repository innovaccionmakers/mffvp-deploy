using HotChocolate.Language;
using HotChocolate.Types;

namespace Common.SharedKernel.Presentation.GraphQlBinding;

/// <summary>
/// Clase que implementa el tipo escalar DateTime para graphql
/// maneja la serialización y deserialización de fechas entre el formato ISO 8601 y DateTime de .net
/// </summary>
public class DateTimeType : ScalarType<DateTime, StringValueNode>
{
    //constructor que define el nombre del tipo escalar como "DateTime"
    public DateTimeType() : base("DateTime")
    {
    }

    /// <summary>
    /// Parsea el resultado de una operación graphql a un nodo de valor
    /// este método es llamado cuando graphql necesita convertir un resultado de operación a un nodo AST
    /// </summary>
    /// <param name="resultValue">El valor del resultado a parsear</param>
    /// <returns>Un nodo de valor que representa la fecha</returns>
    public override IValueNode ParseResult(object? resultValue)
    {
        return ParseValue(resultValue);
    }

    /// <summary>
    /// Parsea un valor literal de fecha desde una cadena graphql
    /// este método es llamado cuando graphql necesita convertir un valor literal en la consulta a un DateTime
    /// </summary>
    /// <param name="valueSyntax">El nodo que contiene el valor de la fecha como cadena</param>
    /// <returns>Un objeto DateTime parseado</returns>
    /// <exception cref="SerializationException">Se lanza cuando el formato de la fecha es inválido</exception>
    protected override DateTime ParseLiteral(StringValueNode valueSyntax)
    {
        if (DateTime.TryParse(valueSyntax.Value, out var dateTime))
        {
            return dateTime;
        }

        throw new SerializationException(
            "Unable to deserialize string to DateTime",
            this);
    }

    /// <summary>
    /// Serializa un DateTime a formato ISO 8601 con precisión de milisegundos
    /// este método es llamado cuando graphql necesita convertir un DateTime de .net a una cadena
    /// </summary>
    /// <param name="runtimeValue">El valor DateTime a serializar</param>
    /// <returns>Un nodo de valor que contiene la fecha en formato ISO 8601</returns>
    protected override StringValueNode ParseValue(DateTime runtimeValue)
    {
        return new StringValueNode(runtimeValue.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }

    /// <summary>
    /// Intenta serializar un valor de tiempo de ejecución a un valor de resultado
    /// este método es llamado cuando graphql necesita convertir un DateTime a un valor que puede ser enviado al cliente
    /// </summary>
    /// <param name="runtimeValue">El valor a serializar</param>
    /// <param name="resultValue">El valor serializado resultante</param>
    /// <returns>true si la serialización fue exitosa, false en caso contrario</returns>
    public override bool TrySerialize(object? runtimeValue, out object? resultValue)
    {
        if (runtimeValue is DateTime dateTime)
        {
            resultValue = dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            return true;
        }

        resultValue = null;
        return false;
    }

    /// <summary>
    /// Intenta deserializar un valor de resultado a un valor de tiempo de ejecución
    /// este método es llamado cuando graphql necesita convertir un valor recibido del cliente a un DateTime
    /// </summary>
    /// <param name="resultValue">El valor a deserializar</param>
    /// <param name="runtimeValue">El valor DateTime resultante</param>
    /// <returns>true si la deserialización fue exitosa, false en caso contrario</returns>
    public override bool TryDeserialize(object? resultValue, out object? runtimeValue)
    {
        if (resultValue is string stringValue && DateTime.TryParse(stringValue, out var dateTime))
        {
            runtimeValue = dateTime;
            return true;
        }

        runtimeValue = null;
        return false;
    }
}