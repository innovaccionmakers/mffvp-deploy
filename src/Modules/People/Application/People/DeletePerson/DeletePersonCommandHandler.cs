using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using People.Domain.People;
using People.Integrations.People.DeletePerson;
using People.Application.Abstractions.Data;

namespace People.Application.People.DeletePerson;

internal sealed class DeletePersonCommandHandler(
    IPersonRepository personRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeletePersonCommand>
{
    public async Task<Result> Handle(DeletePersonCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var person = await personRepository.GetAsync(request.PersonId, cancellationToken);
        if (person is null)
        {
            return Result.Failure(PersonErrors.NotFound(request.PersonId));
        }
        
        personRepository.Delete(person);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
}