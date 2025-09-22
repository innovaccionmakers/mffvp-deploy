namespace Reports.Domain.TechnicalSheet;

public class TechnicalSheetReportRequest
{
    public int PortfolioId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public bool IsValid()
    {
        return PortfolioId > 0 &&
               StartDate != default &&
               EndDate != default &&
               StartDate <= EndDate;
    }

    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (PortfolioId <= 0)
        {
            errors.Add("El ID del portafolio debe ser mayor a 0");
        }

        if (StartDate == default)
        {
            errors.Add("La fecha de inicio es requerida");
        }

        if (EndDate == default)
        {
            errors.Add("La fecha de fin es requerida");
        }

        if (StartDate != default && EndDate != default && StartDate > EndDate)
        {
            errors.Add("La fecha de inicio no puede ser mayor a la fecha de fin");
        }

        return errors;
    }
}
