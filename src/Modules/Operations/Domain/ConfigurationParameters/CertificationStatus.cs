namespace Operations.Domain.ConfigurationParameters;

public class CertificationStatus
{
    public long CertificationStatusId { get; set; }
    public string Name { get; set; }
    public string HomologatedCode { get; set; }
    public bool Status { get; set; }

    public CertificationStatus()
    {
        
    }

    public static CertificationStatus Create(
        long certificationStatusId,
        string name,
        string homologatedCode,
        bool status)
    {
        return new CertificationStatus
        {
            CertificationStatusId = certificationStatusId,
            Name = name,
            HomologatedCode = homologatedCode,
            Status = status
        };
    }
}
