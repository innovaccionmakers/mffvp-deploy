namespace Closing.Domain.Rules
{

    public struct WorkflowNames
    {
        public const string 
            PreclosingValidationsBefore = "Closing.Preclosing.RunSimulation.Before",
            ClosingValidations = "Closing.Closing.RunClosing.Before",
            PostclosingValidations = "Closing.Postclosing.Consolidation"
            ;
    }
}
