using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using People.Domain.Countries;
using People.Integrations.Countries.DeleteCountry;
using People.Application.Abstractions.Data;

namespace People.Application.Countries.DeleteCountry;

internal sealed class DeleteCountryCommandHandler(
    ICountryRepository countryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCountryCommand>
{
    public async Task<Result> Handle(DeleteCountryCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var country = await countryRepository.GetAsync(request.CountryId, cancellationToken);
        if (country is null)
        {
            return Result.Failure(CountryErrors.NotFound(request.CountryId));
        }
        
        countryRepository.Delete(country);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
}