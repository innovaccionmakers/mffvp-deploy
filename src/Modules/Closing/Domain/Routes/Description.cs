
namespace Closing.Domain.Routes
{
    public struct Description
    {
        public const string
            RunPreclosing = """
                                 **Ejemplo de llamada:**

                                 ```http
                                 POST /FVP/closing/preClosing/RunSimulation
                                 Content-Type: application/json

                                 {
                                   "portfolioId": 1,
                                   "closingDate": "2025-01-07",
                                   "isClosing": false
                                 }
                                 ```

                                 - `portfolioId`: Identificador del portafolio
                                 - `closingDate`: Fecha en que se va a ejecutar el cierre del portafolio
                                 - `isClosing`: Identificador de que los valores aún pueden ser recalculados (false) y cuando no (true)
                                 """
             ;
    }

    public struct RequestBodyDescription
    {
        public const string
            RunPreclosing = "";
    }
}
