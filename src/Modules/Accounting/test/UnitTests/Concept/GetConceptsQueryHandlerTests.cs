using Accounting.Application.Concept.GetConcepts;
using Accounting.Domain.Concepts;
using Accounting.Integrations.Concept.GetConcepts;
using Common.SharedKernel.Application.Messaging;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Accounting.test.UnitTests.Concept
{
    public class GetConceptsQueryHandlerTests
    {
        private readonly Mock<IConceptsRepository> _mockRepository;
        private readonly Mock<ILogger<GetConceptsQueryHandler>> _mockLogger;
        private readonly GetConceptsQueryHandler _handler;

        public GetConceptsQueryHandlerTests()
        {
            _mockRepository = new Mock<IConceptsRepository>();
            _mockLogger = new Mock<ILogger<GetConceptsQueryHandler>>();
            _handler = new GetConceptsQueryHandler(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_WithValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            var query = new GetConceptsQuery();

            var concepts = new List<Domain.Concepts.Concept>
            {
                Domain.Concepts.Concept.Create(123, "Concepto 1", "DEBIT-1", "CREDIT-1").Value,
                Domain.Concepts.Concept.Create(456, "Concepto 2", "DEBIT-2", "CREDIT-2").Value
            };

            _mockRepository
                .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(concepts);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Count);
            Assert.Equal("Concepto 1", result.Value.First().Name);
            Assert.Equal("Concepto 2", result.Value.Last().Name);
        }

        [Fact]
        public async Task Handle_WithEmptyList_ReturnsEmptyCollection()
        {
            // Arrange
            var query = new GetConceptsQuery();

            _mockRepository
                .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<Domain.Concepts.Concept>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value);
        }
    }
}

