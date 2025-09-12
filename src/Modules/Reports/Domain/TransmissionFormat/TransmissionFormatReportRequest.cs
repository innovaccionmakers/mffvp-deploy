namespace Reports.Domain.TransmissionFormat
{
    public class TransmissionFormatReportRequest
    {
        public DateTime GenerationDate { get; set; }

        public bool IsValid()
            => GenerationDate != default;
    }
}
