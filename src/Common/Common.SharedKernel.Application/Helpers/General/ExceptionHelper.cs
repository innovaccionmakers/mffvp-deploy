using System.Text;

namespace Common.SharedKernel.Application.Helpers.General
{
    public static class ExceptionHelper
    {
        public static string GetErrorMessage(Exception ex)
        {
            if (ex == null)
                return "An unknown error occurred.";

            var sb = new StringBuilder();

            if (ex is AggregateException aggEx)
            {
                foreach (var inner in aggEx.InnerExceptions)
                {
                    sb.AppendLine(GetErrorMessage(inner));
                }
                return sb.ToString().Trim();
            }

            while (ex != null)
            {
                if (!string.IsNullOrWhiteSpace(ex.Message))
                {
                    sb.AppendLine(ex.Message.Trim());
                }
                ex = ex.InnerException;
            }

            var message = sb.ToString().Trim();
            return string.IsNullOrWhiteSpace(message) ? "An unexpected error occurred." : message;
        }
    }
}
