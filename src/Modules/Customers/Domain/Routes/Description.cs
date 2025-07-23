namespace Customers.Domain.Routes;

public struct Description
{
    public const string
        GetCustomer = """
                             **Ejemplo de llamada:**

                             ```http
                             GET /FVP/Customer/GetCustomer
                             ```
                             """,
        PostCustomer = """
                             **Ejemplo de petición (application/json):**
                             ```json
                             {
                               \"CodigoHomologado\": \"string\",
                               \"TipoIdentificacion\": \"C\",
                               \"Identificacion\": \"12345689\",
                               \"PrimerNombre\": \"Primera\",
                               \"SegundoNombre\": \"\",
                               \"PrimerApellido\": \"Prueba\",
                               \"SegundoApellido\": \"\",
                               \"FechaNacimiento\": \"2025-06-13T17:18:12.576Z\",
                               \"Celular\": \"987654321\",
                               \"Sexo\": \"M\",
                               \"Direccion\": \"Calle\",
                               \"Departamento\": \"91\",
                               \"Municipio\": \"5002\",
                               \"PaisResidencia\": \"1\",
                               \"Email\": \"priemera@prueba.com\",
                               \"ActividadEconomica\": \"10\",
                               \"Declarante\": true,
                               \"PerfilRiesgo\": \"MOD\",
                               \"TipoInversionista\": \"INV\"
                             }
                             ```
                             """;
}

public struct RequestBodyDescription
{
    public const string PostCustomer = "";
}
