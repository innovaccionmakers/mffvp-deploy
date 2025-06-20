using Associate.Domain.Activates;

namespace Integrations.Activates.GetActivateId
{
    public class GetActivateIdValidationContext
    {
        public Activate? Activate { get; }
        public bool DocumentTypeExists { get; set; }

        public GetActivateIdValidationContext(Activate? activate)
        {
            Activate = activate;
        }
    }
}