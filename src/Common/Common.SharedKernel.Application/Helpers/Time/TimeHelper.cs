namespace Common.SharedKernel.Application.Helpers.Time;

public static class TimeHelper
{
    public static string GetDuration(DateTime startUtc, DateTime endUtc)
    {
        startUtc = startUtc.ToUniversalTime();
        endUtc = endUtc.ToUniversalTime();

        TimeSpan duration = endUtc - startUtc;

        if (duration.TotalSeconds < 60)
        {
            return $"{Math.Round(duration.TotalSeconds)} segundos";
        }
        else if (duration.TotalMinutes < 60)
        {
            return $"{Math.Round(duration.TotalMinutes)} minutos";
        }
        else if (duration.TotalHours < 24)
        {
            return $"{Math.Round(duration.TotalHours)} horas";
        }
        else
        {
            return $"{Math.Round(duration.TotalDays)} días";
        }
    }
}
