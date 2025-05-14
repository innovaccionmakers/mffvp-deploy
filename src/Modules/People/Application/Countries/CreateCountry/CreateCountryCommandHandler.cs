using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using People.Domain.Countries;
using People.Integrations.Countries.CreateCountry;
using People.Integrations.Countries;
using People.Application.Abstractions.Data;

namespace People.Application.Countries.CreateCountry

{
    internal sealed class CreateCountryCommandHandler(
        ICountryRepository countryRepository,
        IUnitOfWork unitOfWork)
        : ICommandHandler<CreateCountryCommand, CountryResponse>
    {
        public async Task<Result<CountryResponse>> Handle(CreateCountryCommand request, CancellationToken cancellationToken)
        {
            await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);


            var result = Country.Create(
                request.Name,
                request.ShortName,
                request.DaneCode,
                request.StandardCode
            );

            if (result.IsFailure)
            {
                return Result.Failure<CountryResponse>(result.Error);
            }

            var country = result.Value;
            
            countryRepository.Insert(country);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new CountryResponse(
                country.CountryId,
                country.Name,
                country.ShortName,
                country.DaneCode,
                country.StandardCode
            );
        }
    }
}