namespace Products.Integrations.Objectives.GetObjectivesByAffiliate;

public sealed class AffiliateObjectiveQueryResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string IdType { get; set; }
    public string Type { get; set; }
    public string HomologatedCodeType { get; set; }
    public string IdPlan { get; set; }
    public string Plan { get; set; }
    public string IdFund { get; set; }
    public string Fund { get; set; }
    public string IdAlternative { get; set; }
    public string Alternative { get; set; }
    public string IdCommercial { get; set; }
    public string Commercial { get; set; }
    public string IdOpeningOffice { get; set; }
    public string OpeningOffice { get; set; }
    public string IdCurrentOffice { get; set; }
    public string CurrentOffice { get; set; }
    public string Status { get; set; }

    public AffiliateObjectiveQueryResponse() { }

    public static AffiliateObjectiveQueryResponse Create(
        int id,
        string name,
        string idType,
        string type,
        string homologatedCodeType,
        string idPlan,
        string plan,
        string idFund,
        string fund,
        string idAlternative,
        string alternative,
        string idCommercial,
        string commercial,
        string idOpeningOffice,
        string openingOffice,
        string idCurrentOffice,
        string currentOffice,
        string status)
    {
        return new AffiliateObjectiveQueryResponse
        {
            Id = id,
            Name = name,
            IdType = idType,
            Type = type,
            HomologatedCodeType = homologatedCodeType,
            IdPlan = idPlan,
            Plan = plan,
            IdFund = idFund,
            Fund = fund,
            IdAlternative = idAlternative,
            Alternative = alternative,
            IdCommercial = idCommercial,
            Commercial = commercial,
            IdOpeningOffice = idOpeningOffice,
            OpeningOffice = openingOffice,
            IdCurrentOffice = idCurrentOffice,
            CurrentOffice = currentOffice,
            Status = status
        };
    }
}