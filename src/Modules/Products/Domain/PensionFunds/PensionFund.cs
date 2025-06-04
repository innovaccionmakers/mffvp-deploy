using Common.SharedKernel.Domain;
using Products.Domain.PlanFunds;

namespace Products.Domain.PensionFunds
{
    public sealed class PensionFund : Entity
    {
        public int PensionFundId { get; private set; }
        public int IdentificationTypeId { get; private set; }
        public int IdentificationNumber { get; private set; }
        public string Name { get; private set; }
        public string ShortName { get; private set; }
        public string Status { get; private set; }
        public string HomologatedCode { get; private set; }
        
        public IReadOnlyCollection<PlanFund> PlanFunds { get; private set; } = new List<PlanFund>();

        private PensionFund() { }

        public static Result<PensionFund> Create(
            int identificationTypeId,
            int identificationNumber,
            string name,
            string shortName,
            string status,
            string homologatedCode
        )
        {
            var fund = new PensionFund
            {
                IdentificationTypeId = identificationTypeId,
                IdentificationNumber = identificationNumber,
                Name                 = name,
                ShortName            = shortName,
                Status               = status,
                HomologatedCode      = homologatedCode
            };
            return Result.Success(fund);
        }

        public void UpdateDetails(
            int identificationTypeId,
            int identificationNumber,
            string name,
            string shortName,
            string status,
            string homologatedCode
        )
        {
            IdentificationTypeId = identificationTypeId;
            IdentificationNumber = identificationNumber;
            Name                 = name;
            ShortName            = shortName;
            Status               = status;
            HomologatedCode      = homologatedCode;
        }
    }
}