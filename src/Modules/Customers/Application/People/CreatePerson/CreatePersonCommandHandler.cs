using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Customers.Domain.People;
using Customers.Integrations.People;
using Customers.Application.Abstractions.Data;
using Integrations.People.CreatePerson;
using Customers.Domain.ConfigurationParameters;
using Common.SharedKernel.Application.Rules;
using Customers.Application.Abstractions;
using Application.People;

namespace Customers.Application.People.CreatePerson

{
    internal sealed class CreatePersonCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IRuleEvaluator<CustomersModuleMarker> ruleEvaluator,
        PersonCommandHandlerValidation validator)
        : ICommandHandler<CreatePersonRequestCommand>
    {

        private const string Workflow = "People.Person.ValidationCreateCustomer";

        public async Task<Result> Handle(CreatePersonRequestCommand request, CancellationToken cancellationToken)
        {
            await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
            var validationResults = await validator.ValidateRequestAsync(request, cancellationToken);

            var (isValid, _, ruleErrors) =
                        await ruleEvaluator.EvaluateAsync(Workflow, validationResults, cancellationToken);

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
                validationResults.Uuid,
                request.Identification,
                request.FirstName,
                request.MiddleName,
                request.LastName,
                request.SecondLastName,
                request.BirthDate,
                request.Mobile,
                validationResults.GenderId ?? 0,
                validationResults.CountryId ?? 0,
                validationResults.DepartmentId ?? 0,
                validationResults.MunicipalityId ?? 0,
                request.Email,
                validationResults.EconomicActivityId ?? 0,
                Status.Active,
                request.Address,
                request.Declarant,
                validationResults.InvestorTypeId ?? 0,
                validationResults.RiskProfileId ?? 0
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