namespace Products.Domain.Routes;

public struct Description
{
    public const string
        GetGoals = """
                             **Ejemplo de llamada (query):**

                             ```http
                             GET /FVP/Product/GetGoals?typeId=C&identification=123456789&status=A
                             ```

                             - `typeId`: C (Ciudadanía)
                             - `identification`: 27577533
                             - `status`: A (Activo)
                             """,
        Goals = """
                **Ejemplo de petición (application/json):**
                ```json
                {
                  \"TipoId\": \"CC\",
                  \"Identificacion\": \"123456789\",
                  \"IdAlternativa\": \"ALT001\",
                  \"TipoObjetivo\": \"Ahorro\",
                  \"NombreObjetivo\": \"Viaje a Cartagena\",
                  \"OficinaApertura\": \"001\",
                  \"OficinaActual\": \"001\",
                  \"Comercial\": \"COM123\"
                }
                ```
                """,
        GetPortfolioById = """
                                 **Ejemplo de llamada:**

                                 ```http
                                 GET /FVP/products/portfolios/GetById?portfolioId=123
                                 ```

                                 - `portfolioId`: Identificador del portafolio (e.g., 123)
                                 """,
        GetAllPortfolios = """
                                 **Ejemplo de llamada:**

                                 ```http
                                 GET /FVP/products/portfolios/GetAllPortfolios
                                 ```
                                 """;
}

public struct RequestBodyDescription
{
    public const string Goals = "";
}
