using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using People.Domain.Countries;
using People.Integrations.Countries.UpdateCountry;
using People.Integrations.Countries;
using People.Application.Abstractions.Data;

namespace People.Application.Countries;
internal sealed class UpdateCountryCommandHandler(
    ICountryRepository countryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCountryCommand, CountryResponse>
{
    public async Task<Result<CountryResponse>> Handle(UpdateCountryCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = await countryRepository.GetAsync(request.CountryId, cancellationToken);
        if (entity is null)
        {
            return Result.Failure<CountryResponse>(CountryErrors.NotFound(request.CountryId));
        }

        entity.UpdateDetails(
            request.NewName, 
            request.NewShortName, 
            request.NewDaneCode, 
            request.NewStandardCode
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CountryResponse(entity.CountryId, entity.Name, entity.ShortName, entity.DaneCode, entity.StandardCode);
    }
}