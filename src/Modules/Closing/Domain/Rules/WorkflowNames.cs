namespace Closing.Domain.Rules
{

    public struct WorkflowNames
    {
        public const string 
            PreclosingValidations = "Closing.Preclosing.Simulation.BlockingValidations",
            ClosingValidations = "Closing.Closing.RunClosing.Before",
            PostclosingValidations = "Closing.Postclosing.Consolidation"
            ;
    }
}
