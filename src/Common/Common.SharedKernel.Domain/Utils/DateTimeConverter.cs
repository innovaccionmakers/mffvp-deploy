namespace Common.SharedKernel.Domain.Utils
{
    public static class DateTimeConverter
    {
        public static DateTime ToUtcDateTime(DateTime dateTime)
        {
            return dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
                _ => throw new InvalidOperationException("Fecha inválida")
            };
        }
    }
}