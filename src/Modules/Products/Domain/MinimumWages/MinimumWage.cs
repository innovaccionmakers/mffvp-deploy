using Common.SharedKernel.Domain;

namespace Products.Domain.MinimumWages
{
    public sealed class MinimumWage : Entity
    {
        public int MinimumWageId { get; private set; }
        public string Year { get; private set; }
        public decimal Value { get; private set; }

        private MinimumWage()
        {            
        }

        public static Result<MinimumWage> Create(string year, decimal value)
        {
            var minimumWage = new MinimumWage
            {
                Year = year,
                Value = value
            };
            return Result.Success(minimumWage);
        }

        public void UpdateDetails(string newYear, decimal newValue)
        {
            Year = newYear;
            Value = newValue;
        }
    }
}
