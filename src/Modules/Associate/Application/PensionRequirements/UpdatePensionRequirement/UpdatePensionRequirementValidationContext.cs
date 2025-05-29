using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Associate.Integrations.PensionRequirements.UpdatePensionRequirement;

namespace Application.PensionRequirements.UpdatePensionRequirement
{
    public class UpdatePensionRequirementValidationContext
    {
        public UpdatePensionRequirementCommand Request { get; }
        public int ActivateId { get; set; }


        public UpdatePensionRequirementValidationContext(UpdatePensionRequirementCommand request, int activateId)
        {
            ActivateId = activateId;        
            Request = request;
        }
    }
}