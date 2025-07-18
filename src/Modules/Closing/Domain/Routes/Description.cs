
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
                                   "IdPortafolio": 1,
                                   "FechaCierre": "2025-07-01",
                                   "EsCierre": false
                                 }
                                 ```

                                 - `IdPortafolio`: Identificador del portafolio
                                 - `FechaCierre`: Fecha en que se va a ejecutar el cierre del portafolio
                                 - `EsCierre`: Identificador de que los valores aún pueden ser recalculados (false) y cuando no (true)
                                 """
             ;
    }

    public struct RequestBodyDescription
    {
        public const string
            RunPreclosing = "";
    }
}
