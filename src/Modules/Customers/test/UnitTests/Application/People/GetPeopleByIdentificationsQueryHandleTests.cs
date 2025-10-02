using Customers.Integrations.People.GetPeopleByIdentifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customers.test.UnitTests.Application.People
{
    public class GetPeopleByIdentificationsQueryHandleTests
    {
        [Fact]
        public void GetPeopleByIdentificationsResponse_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var identification = "TEST-12345";
            var fullName = "John Doe";

            // Act
            var response = new GetPeopleByIdentificationsResponse(identification, fullName);

            // Assert
            Assert.Equal(identification, response.Identification);
            Assert.Equal(fullName, response.FullName);
        }

        [Fact]
        public void GetPeopleByIdentificationsResponse_WithNullFullName_AllowsNull()
        {
            // Arrange & Act
            var response = new GetPeopleByIdentificationsResponse("ID-001", null);

            // Assert
            Assert.Null(response.FullName);
        }

        [Fact]
        public void GetPeopleByIdentificationsResponse_WithEmptyFullName_AllowsEmptyString()
        {
            // Arrange & Act
            var response = new GetPeopleByIdentificationsResponse("ID-001", string.Empty);

            // Assert
            Assert.Equal(string.Empty, response.FullName);
        }

        [Fact]
        public void GetPeopleByIdentificationsResponse_WithNullIdentification_AllowsNull()
        {
            // Arrange & Act
            var response = new GetPeopleByIdentificationsResponse(null, "John Doe");

            // Assert
            Assert.Null(response.Identification);
        }

        [Fact]
        public void GetPeopleByIdentificationsResponse_WithEmptyIdentification_AllowsEmptyString()
        {
            // Arrange & Act
            var response = new GetPeopleByIdentificationsResponse(string.Empty, "John Doe");

            // Assert
            Assert.Equal(string.Empty, response.Identification);
        }

        [Theory]
        [InlineData("John Doe")]
        [InlineData("Jane Smith")]
        [InlineData("Bob Johnson")]
        [InlineData("María José")]
        [InlineData("Jean-Luc Picard")]
        public void GetPeopleByIdentificationsResponse_WithVariousFullNameFormats_HandlesCorrectly(string fullName)
        {
            // Arrange & Act
            var response = new GetPeopleByIdentificationsResponse("ID-001", fullName);

            // Assert
            Assert.Equal(fullName, response.FullName);
        }

        [Theory]
        [InlineData("1234567890")]
        [InlineData("ABC-DEF-GHI")]
        [InlineData("id_with_underscore")]
        [InlineData("id-with-dash")]
        [InlineData("ID.123.456")]
        [InlineData("PASSPORT-12345")]
        public void GetPeopleByIdentificationsResponse_WithVariousIdentificationFormats_HandlesCorrectly(string identification)
        {
            // Arrange & Act
            var response = new GetPeopleByIdentificationsResponse(identification, "Test Person");

            // Assert
            Assert.Equal(identification, response.Identification);
        }

        [Fact]
        public void GetPeopleByIdentificationsResponse_WithLongFullName_HandlesCorrectly()
        {
            // Arrange
            var longFullName = new string('A', 1000);

            // Act
            var response = new GetPeopleByIdentificationsResponse("ID-001", longFullName);

            // Assert
            Assert.Equal(1000, response.FullName.Length);
        }

        [Fact]
        public void GetPeopleByIdentificationsResponse_WithLongIdentification_HandlesCorrectly()
        {
            // Arrange
            var longIdentification = new string('B', 500);

            // Act
            var response = new GetPeopleByIdentificationsResponse(longIdentification, "Test Person");

            // Assert
            Assert.Equal(500, response.Identification.Length);
        }

        [Fact]
        public void GetPeopleByIdentificationsResponse_WithUnicodeCharacters_HandlesCorrectly()
        {
            // Arrange
            var identification = "1234567890";
            var fullName = "José María López-Ñuñez";

            // Act
            var response = new GetPeopleByIdentificationsResponse(identification, fullName);

            // Assert
            Assert.Equal(identification, response.Identification);
            Assert.Equal(fullName, response.FullName);
        }
    }
}
