
namespace Closing.Domain.Routes
{
    public struct Description
    {
        public const string
            RunPreclosing = """
                                 **Ejemplo de llamada:**

                                 ```http
                                 POST /FVP/closing/RunSimulation
                                 Content-Type: application/json

                                 {
                                   "IdPortafolio": 1,
                                   "FechaCierre": "2025-07-01"
                                 }
                                 ```

                                 - `IdPortafolio`: Identificador del portafolio
                                 - `FechaCierre`: Fecha en que se va a ejecutar el cierre del portafolio
                                 """,
                  RunClosing = """
                                 **Ejemplo de llamada:**

                                 ```http
                                 POST /FVP/closing/RunClosing
                                 Content-Type: application/json

                                 {
                                   "IdPortafolio": 1,
                                   "FechaCierre": "2025-07-01"
                                 }
                                 ```

                                 - `IdPortafolio`: Identificador del portafolio
                                 - `FechaCierre`: Fecha en que se va a ejecutar el cierre del portafolio
                                 """
             ;
    }

    public struct RequestBodyDescription
    {
        public const string
            RunPreclosing = "", 
            RunClosing = "";
    }
}
