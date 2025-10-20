namespace Common.SharedKernel.Domain.Constants;


public static class NotificationTypes
{
    public const string AccountingReport = "Informe Contabilidad";
    public const string Report = "Informe";
    public const string ReportGeneration = "Generacion Informe";
}

public static class NotificationDefaults
{
    public const string Administrator = "Fiduciaria Bancolombia";
    public const string Origin = "MFFVP";
}

public static class NotificationStatuses
{
    public const string Success = "Exitoso";
    public const string Failure = "Fallido";
    public const string Finalized = "Finalizado";
}