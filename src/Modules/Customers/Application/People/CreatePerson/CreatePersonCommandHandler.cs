using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Customers.Domain.People;
using Customers.Integrations.People;
using Customers.Application.Abstractions.Data;
using Integrations.People.CreatePerson;
using Common.SharedKernel.Application.Attributes;
using Customers.Domain.ConfigurationParameters;
using Common.SharedKernel.Application.Rules;
using Customers.Application.Abstractions;
using Application.People.CreatePerson;

namespace Customers.Application.People.CreatePerson

{
    internal sealed class CreatePersonCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IConfigurationParameterRepository configurationParameterRepository,
        IRuleEvaluator<CustomersModuleMarker> ruleEvaluator)
        : ICommandHandler<CreatePersonRequestCommand>
    {
        
        private const string Workflow = "Associate.Activates.ValidationCreateCustomer";

        public async Task<Result> Handle(CreatePersonRequestCommand request, CancellationToken cancellationToken)
        {
            await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

            var configurationParameter = await configurationParameterRepository.GetByCodeAndScopeAsync(
            request.IdentificationType, HomologScope.Of<CreatePersonRequestCommand>(c => c.IdentificationType), cancellationToken);
            Guid uuid = configurationParameter == null ? new Guid() : configurationParameter.Uuid;

            Person? existingActivate = await personRepository.GetForIdentificationAsync(uuid, request.Identification, cancellationToken);

            var validationContext = new CreatePersonValidationContext(request, existingActivate!, uuid);

            var (isValid, _, ruleErrors) =
                        await ruleEvaluator.EvaluateAsync(Workflow, validationContext, cancellationToken);

            if (!isValid)
            {
                var first = ruleErrors
                    .OrderByDescending(r => r.Code)
                    .First();

                return Result.Failure(
                    Error.Validation(first.Code, first.Message));
            }

            var result = Person.Create(
                request.HomologatedCode,
                request.Identification,
                request.FirstName,
                request.MiddleName,
                request.LastName,
                request.SecondLastName,
                request.BirthDate,
                request.Mobile,
                1,
                1,
                1,
                1,
                request.Email,
                1,
                request.Address,
                request.Declarant,
                1,
                1
            );

            if (result.IsFailure)
                return Result.Failure<PersonResponse>(result.Error);


            var person = result.Value;

            personRepository.Insert(person);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success("Cliente creado Exitosamente");
        }
    }
}