using Customers.Domain.People;
using System;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Core.Primitives;

namespace Customers.UnitTests.TestHelpers
{
    public static class PersonObjectMother
    {
        public static Person CreateTestPerson(long id, string firstName, string lastName, Status status = Status.Active)
        {
            var result = Person.Create(
                $"HOM{id}",
                Guid.NewGuid(),
                $"{id}23456789",
                $"FirstName{id}",
                "Middle",
                $"LastName{id}",
                "Second",
                new DateTime(1990, 1, 1).AddDays(id),
                $"{id}234567890",
                1, 1, 1, 1,
                $"user{id}@example.com",
                1,
                Status.Active,
                $"{id} Main St",
                false,
                1, 1);

            return result.IsSuccess ? result.Value : throw new Exception("Failed to create test person");
        }
    }
}