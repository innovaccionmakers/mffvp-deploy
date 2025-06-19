namespace Operations.Domain.ConfigurationParameters;

public class PaymentMethod
{
    public long PaymentMethodId { get; private set; }
    public string Name { get; private set; }
    public bool Status { get; private set; }
    public string HomologatedCode { get; private set; }
    private PaymentMethod()
    {
    }

    public static PaymentMethod Create(
        long paymentMethodId,
        string name,
        bool status,
        string homologatedCode
    )
    {
        return new PaymentMethod
        {
            PaymentMethodId = paymentMethodId,
            Name = name,
            Status = status,
            HomologatedCode = homologatedCode
        };
    }

}
