namespace Products.Integrations.Objectives.UpdateObjective;

public static class UpdateObjectiveCommandExtensions
{
    public static bool HasAnyFieldToUpdate(this UpdateObjectiveCommand command)
    {
        return !string.IsNullOrWhiteSpace(command.ObjectiveType) ||
               !string.IsNullOrWhiteSpace(command.ObjectiveName) ||
               !string.IsNullOrWhiteSpace(command.OpeningOffice) ||
               !string.IsNullOrWhiteSpace(command.CurrentOffice) ||
               !string.IsNullOrWhiteSpace(command.Commercial) ||
               !string.IsNullOrWhiteSpace(command.Status);
    }
}