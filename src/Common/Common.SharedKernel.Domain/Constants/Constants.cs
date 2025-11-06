namespace Common.SharedKernel.Domain.Constants;


public static class NotificationTypes
{
    public const string AccountingReport = "Informe Contabilidad";
    public const string Report = "Informe";
    public const string ReportGeneration = "Generando Informe";
    public const string ReportGenerated  = "Informe generado";
    public const string ReportGeneratedError = "Informe no generado";
}

public static class NotificationDefaults
{
    public const string Administrator = "Fiduciaria Bancolombia";
    public const string Origin = "MFFVP";
    public const string MessageGroupId = "1";
    public const string EmailFrom = "NotificacionesMakersFunds@somosmakers.co";
}

public static class NotificationStatuses
{
    public const string Initiated = "Iniciado";
    public const string Success = "Exitoso";
    public const string Failure = "Fallido";
    public const string Finalized = "Finalizado";
}

public static class WorksheetNames
{
    public const string Balances = "Saldos";
    public const string Movements = "Movimientos";
    public const string TechnicalSheet = "Ficha Técnica";
    public const string AccountingInconsistencies = "Inconsistencias Contables";
    public const string Accounting = "Contabilidad";
    public const string ManualFormat = "Formato Manual";
}