using Accounting.Domain.ConfigurationGenerals;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.test.IntegrationTests.ConfigurationGenerals
{
    public class GeneralConfigurationRepositoryTests
    {
        private readonly Mock<IGeneralConfigurationRepository> _mockRepository;

        public GeneralConfigurationRepositoryTests()
        {
            _mockRepository = new Mock<IGeneralConfigurationRepository>();
        }

        [Fact]
        public async Task GetGeneralConfigurationByPortfolioIdAsync_WithValidPortfolioId_ShouldReturnConfiguration()
        {
            // Arrange
            var portfolioId = 1;
            string accountingCode = "123456";
            string costCenter = "Prueba";
            var expectedConfiguration = CreateTestGeneralConfiguration(portfolioId, accountingCode, costCenter);
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, cancellationToken))
                .ReturnsAsync(expectedConfiguration);

            // Act
            var result = await _mockRepository.Object.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedConfiguration);
            _mockRepository.Verify(x => x.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetGeneralConfigurationByPortfolioIdAsync_WithNonExistingPortfolioId_ShouldReturnNull()
        {
            // Arrange
            var portfolioId = 999;
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, cancellationToken))
                .ReturnsAsync((GeneralConfiguration?)null);

            // Act
            var result = await _mockRepository.Object.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, cancellationToken);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(x => x.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetGeneralConfigurationByPortfolioIdAsync_WithZeroPortfolioId_ShouldHandleGracefully()
        {
            // Arrange
            var portfolioId = 0;
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, cancellationToken))
                .ReturnsAsync((GeneralConfiguration?)null);

            // Act
            var result = await _mockRepository.Object.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, cancellationToken);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(x => x.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetGeneralConfigurationByPortfolioIdAsync_WithCancelledToken_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var portfolioId = 1;
            var cancellationToken = new CancellationToken(canceled: true);

            _mockRepository.Setup(x => x.GetGeneralConfigurationByPortfolioIdAsync(
                It.IsAny<int>(),
                It.Is<CancellationToken>(ct => ct.IsCancellationRequested)))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _mockRepository.Object.GetGeneralConfigurationByPortfolioIdAsync(portfolioId, cancellationToken));
        }

        [Fact]
        public async Task GetGeneralConfigurationsByPortfolioIdsAsync_WithValidPortfolioIds_ShouldReturnConfigurations()
        {
            // Arrange
            var portfolioIds = new List<int> { 1, 2, 3 };
            string accountingCode = "123456";
            string costCenter = "Prueba";
            var expectedConfigurations = new List<GeneralConfiguration>
            {
                CreateTestGeneralConfiguration(1, accountingCode, costCenter),
                CreateTestGeneralConfiguration(2, accountingCode, costCenter),
                CreateTestGeneralConfiguration(3, accountingCode, costCenter)
            };
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetGeneralConfigurationsByPortfolioIdsAsync(
                It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(portfolioIds)),
                cancellationToken))
                .ReturnsAsync(expectedConfigurations);

            // Act
            var result = await _mockRepository.Object.GetGeneralConfigurationsByPortfolioIdsAsync(portfolioIds, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expectedConfigurations);
            _mockRepository.Verify(x => x.GetGeneralConfigurationsByPortfolioIdsAsync(portfolioIds, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetGeneralConfigurationsByPortfolioIdsAsync_WithEmptyPortfolioIds_ShouldReturnEmptyCollection()
        {
            // Arrange
            var emptyPortfolioIds = Enumerable.Empty<int>();
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetGeneralConfigurationsByPortfolioIdsAsync(
                It.Is<IEnumerable<int>>(ids => !ids.Any()),
                cancellationToken))
                .ReturnsAsync(Enumerable.Empty<GeneralConfiguration>());

            // Act
            var result = await _mockRepository.Object.GetGeneralConfigurationsByPortfolioIdsAsync(emptyPortfolioIds, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockRepository.Verify(x => x.GetGeneralConfigurationsByPortfolioIdsAsync(emptyPortfolioIds, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetGeneralConfigurationsByPortfolioIdsAsync_WithNullPortfolioIds_ShouldReturnEmptyCollection()
        {
            // Arrange
            IEnumerable<int> nullPortfolioIds = null;
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetGeneralConfigurationsByPortfolioIdsAsync(
                It.Is<IEnumerable<int>>(ids => ids == null),
                cancellationToken))
                .ReturnsAsync(Enumerable.Empty<GeneralConfiguration>());

            // Act
            var result = await _mockRepository.Object.GetGeneralConfigurationsByPortfolioIdsAsync(nullPortfolioIds, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockRepository.Verify(x => x.GetGeneralConfigurationsByPortfolioIdsAsync(nullPortfolioIds, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetGeneralConfigurationsByPortfolioIdsAsync_WithSinglePortfolioId_ShouldReturnSingleConfiguration()
        {
            // Arrange
            var portfolioIds = new List<int> { 1 };
            string accountingCode = "123456";
            string costCenter = "Prueba";
            var expectedConfigurations = new List<GeneralConfiguration> { CreateTestGeneralConfiguration(1, accountingCode, costCenter) };
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetGeneralConfigurationsByPortfolioIdsAsync(
                It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(portfolioIds)),
                cancellationToken))
                .ReturnsAsync(expectedConfigurations);

            // Act
            var result = await _mockRepository.Object.GetGeneralConfigurationsByPortfolioIdsAsync(portfolioIds, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().PortfolioId.Should().Be(1);
            _mockRepository.Verify(x => x.GetGeneralConfigurationsByPortfolioIdsAsync(portfolioIds, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetGeneralConfigurationsByPortfolioIdsAsync_WithNonExistingPortfolioIds_ShouldReturnEmptyCollection()
        {
            // Arrange
            var portfolioIds = new List<int> { 99, 100 };
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetGeneralConfigurationsByPortfolioIdsAsync(
                It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(portfolioIds)),
                cancellationToken))
                .ReturnsAsync(Enumerable.Empty<GeneralConfiguration>());

            // Act
            var result = await _mockRepository.Object.GetGeneralConfigurationsByPortfolioIdsAsync(portfolioIds, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockRepository.Verify(x => x.GetGeneralConfigurationsByPortfolioIdsAsync(portfolioIds, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetGeneralConfigurationsByPortfolioIdsAsync_WithCancelledToken_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var portfolioIds = new List<int> { 1, 2 };
            var cancellationToken = new CancellationToken(canceled: true);

            _mockRepository.Setup(x => x.GetGeneralConfigurationsByPortfolioIdsAsync(
                It.IsAny<IEnumerable<int>>(),
                It.Is<CancellationToken>(ct => ct.IsCancellationRequested)))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _mockRepository.Object.GetGeneralConfigurationsByPortfolioIdsAsync(portfolioIds, cancellationToken));
        }

        [Fact]
        public void Insert_WithValidGeneralConfiguration_ShouldCompleteSuccessfully()
        {
            // Arrange
            var configuration = CreateTestGeneralConfiguration(1, "123456", "Prueba");

            // Act
            _mockRepository.Object.Insert(configuration);

            // Assert
            _mockRepository.Verify(x => x.Insert(configuration), Times.Once);
        }

        [Fact]
        public void Update_WithValidGeneralConfiguration_ShouldCompleteSuccessfully()
        {
            // Arrange
            var configuration = CreateTestGeneralConfiguration(1, "123456", "Prueba");

            // Act
            _mockRepository.Object.Update(configuration);

            // Assert
            _mockRepository.Verify(x => x.Update(configuration), Times.Once);
        }

        [Fact]
        public void Update_WithModifiedGeneralConfiguration_ShouldCompleteSuccessfully()
        {
            // Arrange
            var originalConfiguration = CreateTestGeneralConfiguration(1, "123456", "Prueba");
            var modifiedConfiguration = CreateTestGeneralConfiguration(1, "123456", "Prueba");

            // Act
            _mockRepository.Object.Update(modifiedConfiguration);

            // Assert
            _mockRepository.Verify(x => x.Update(modifiedConfiguration), Times.Once);
        }

        [Fact]
        public void Delete_WithValidGeneralConfiguration_ShouldCompleteSuccessfully()
        {
            // Arrange
            var configuration = CreateTestGeneralConfiguration(1, "123456", "Prueba");

            // Act
            _mockRepository.Object.Delete(configuration);

            // Assert
            _mockRepository.Verify(x => x.Delete(configuration), Times.Once);
        }

        [Fact]
        public void Delete_WithNewlyCreatedConfiguration_ShouldCompleteSuccessfully()
        {
            // Arrange
            var configuration = CreateTestGeneralConfiguration(1, "123456", "Prueba");

            // Act
            _mockRepository.Object.Delete(configuration);

            // Assert
            _mockRepository.Verify(x => x.Delete(configuration), Times.Once);
        }

        [Fact]
        public void CRUD_Operations_Sequence_ShouldWorkCorrectly()
        {
            // Arrange
            var configuration = CreateTestGeneralConfiguration(1, "123456", "Prueba");

            // Act & Assert - Insert
            _mockRepository.Object.Insert(configuration);
            _mockRepository.Verify(x => x.Insert(configuration), Times.Once);

            // Act & Assert - Update
            _mockRepository.Object.Update(configuration);
            _mockRepository.Verify(x => x.Update(configuration), Times.Once);

            // Act & Assert - Delete
            _mockRepository.Object.Delete(configuration);
            _mockRepository.Verify(x => x.Delete(configuration), Times.Once);
        }

        [Fact]
        public void CRUD_Operations_WithMultipleConfigurations_ShouldWorkCorrectly()
        {
            // Arrange
            var configuration1 = CreateTestGeneralConfiguration(1, "123456", "Prueba");
            var configuration2 = CreateTestGeneralConfiguration(2, "123456", "Prueba");
            var configuration3 = CreateTestGeneralConfiguration(3, "123456", "Prueba");

            // Act & Assert - Insert all
            _mockRepository.Object.Insert(configuration1);
            _mockRepository.Object.Insert(configuration2);
            _mockRepository.Object.Insert(configuration3);
            _mockRepository.Verify(x => x.Insert(It.IsAny<GeneralConfiguration>()), Times.Exactly(3));

            // Act & Assert - Update one
            _mockRepository.Object.Update(configuration2);
            _mockRepository.Verify(x => x.Update(configuration2), Times.Once);

            // Act & Assert - Delete one
            _mockRepository.Object.Delete(configuration3);
            _mockRepository.Verify(x => x.Delete(configuration3), Times.Once);
        }

        private GeneralConfiguration CreateTestGeneralConfiguration(int portfolioId, string accountingCode, string costCenter)
        {
            return GeneralConfiguration.Create(
                portfolioId,
                accountingCode,
                costCenter
            ).Value;
        }
    }
}