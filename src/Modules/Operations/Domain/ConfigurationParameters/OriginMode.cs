namespace Operations.Domain.ConfigurationParameters;

public class OriginMode
{
    public long OriginModeId { get; private set; }
    public string Name { get; private set; }
    public string HomologatedCode { get; private set; }
    public bool Status { get; private set; }

    private OriginMode()
    {
    }

    public static OriginMode Create(
        long originModeId,
        string name,
        string homologatedCode,
        bool status
    )
    {
        return new OriginMode
        {
            OriginModeId = originModeId,
            Name = name,
            HomologatedCode = homologatedCode,
            Status = status
        };
    }
}
